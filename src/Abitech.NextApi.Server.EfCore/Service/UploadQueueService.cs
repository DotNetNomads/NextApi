using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Abitech.NextApi.Model.Abstractions;
using Abitech.NextApi.Model.UploadQueue;
using Abitech.NextApi.Server.EfCore.Model.Base;
using Abitech.NextApi.Server.Entity;
using Abitech.NextApi.Server.Service;
using Microsoft.AspNetCore.SignalR.Internal;
using Newtonsoft.Json;

namespace Abitech.NextApi.Server.EfCore.Service
{
    /// <summary>
    /// Abstract implementation of UploadQueueService
    /// <para>Derive from this class and register repositories in the constructor</para>
    /// <para>You must call <see cref="NextApiEfCoreExtensions.AddColumnChangesLogger{TDbContext}"/></para>
    /// </summary>
    public abstract class UploadQueueService<TUnitOfWork> : NextApiService, IUploadQueueService
        where TUnitOfWork : class, INextApiUnitOfWork
    {
        private static readonly List<(Type modelType, Type repoType)> _repositoryList = new List<(Type modelType, Type repoType)>();
        
        private readonly IColumnChangesLogger _columnChangesLogger;
        private readonly TUnitOfWork _unitOfWork;
        private readonly IServiceProvider _serviceProvider;
        
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1);

        protected UploadQueueService(
            IColumnChangesLogger columnChangesLogger,
            TUnitOfWork unitOfWork,
            IServiceProvider serviceProvider)
        {
            _columnChangesLogger = columnChangesLogger;
            _unitOfWork = unitOfWork;
            _serviceProvider = serviceProvider;
        }
        
        /// <summary>
        /// Use this method to register repositories (that implement INextApiRepository) allowed in UploadQueue process
        /// </summary>
        /// <param name="modelName">This repository's generic model's name</param>
        /// <typeparam name="TModel">Model type for this repository</typeparam>
        /// <typeparam name="TRepository">Repository type</typeparam>
        /// <exception cref="ArgumentNullException">If modelName is null or whitespace</exception>
        /// <exception cref="ArgumentException">If repository type does not implement INextApiRepository</exception>
        protected void RegisterRepository<TModel, TRepository>() 
            where TModel : class, IRowGuidEnabled
            where TRepository : class, INextApiRepository
        {
            var modelType = typeof(TModel);

            var alreadyAdded = _repositoryList
                                   .FirstOrDefault(tuple => tuple.modelType.FullName == modelType.FullName) != (null, null);
            if (alreadyAdded)
                return;
            
            var repoType = typeof(TRepository);
            
            var genericArguments = repoType.AllBaseTypes().Concat(repoType.GetInterfaces()).SelectMany(type => type.GetGenericArguments());
            var notForThisRepo = genericArguments.FirstOrDefault(type => type.IsAssignableFrom(modelType)) == null;
            if (notForThisRepo)
                throw new Exception($"Repo {repoType.Name} is not for {modelType.Name}");
            
            _repositoryList.Add((modelType, repoType));
        }

        public virtual async Task<IDictionary<Guid, UploadQueueResult>> ProcessAsync(IList<UploadQueueDto> uploadQueue)
        {
            _columnChangesLogger.LoggingEnabled = false;
            
            var resultDict = new ConcurrentDictionary<Guid, UploadQueueResult>();
            var taskList = new List<Task>();

            var groups = uploadQueue.GroupBy(dto => dto.EntityName);
            foreach (var group in groups)
            {
                taskList.Add(ProcessByEntityName(resultDict, group));
            }
            
            await Task.WhenAll(taskList);
            
            await _unitOfWork.CommitAsync();
            
            foreach (var operation in uploadQueue)
            {
                if (resultDict.Any(pair => pair.Key == operation.Id)) continue;

                resultDict.TryAdd(operation.Id, new UploadQueueResult{ Error = UploadQueueError.Unknown });
            }
            
            _columnChangesLogger.LoggingEnabled = true;
            return resultDict;
        }
        
        private async Task ProcessByEntityName(ConcurrentDictionary<Guid, UploadQueueResult> resultDict, IGrouping<string, UploadQueueDto> groupByEntityName)
        {
            var groupByEntityNameList = groupByEntityName.ToList();
            
            try
            {
                var entityName = groupByEntityName.Key;
                if (string.IsNullOrWhiteSpace(entityName))
                    throw new ArgumentNullException(entityName);
                
                // Resolve repo
                var (modelType, repoType) = _repositoryList.FirstOrDefault(tuple => tuple.modelType.Name == entityName);
                if (repoType == null)
                    throw new Exception($"No repo for {entityName}");
                
                var repoInstance = (INextApiRepository) _serviceProvider.GetService(repoType);
                
                // Process by row guid
                var rowGuidGrouping = groupByEntityNameList.GroupBy(dto => dto.EntityRowGuid);
                await Task.WhenAll(rowGuidGrouping.Select(ProcessByRowGuid));
                
                #region Nested method ProcessByRowGuid
                
                async Task ProcessByRowGuid(IGrouping<Guid, UploadQueueDto> groupByRowGuid)
                {
                    var rowGuid = groupByRowGuid.Key;
                    
                    // IGrouping is IEnumerable, so it cant be enumerated multiple times
                    var groupByRowGuidList = groupByRowGuid.ToList();
                    
                    #region Reject all operations if create and delete ops are in the same batch

                    if (groupByRowGuidList.Any(dto => dto.OperationType == OperationType.Create)
                        && groupByRowGuidList.Any(dto => dto.OperationType == OperationType.Delete))
                    {
                        // silently reject everything
                        var result = new UploadQueueResult
                        {
                            Error = UploadQueueError.NoError
                        };
                        foreach (var operation in groupByRowGuidList)
                        {
                            resultDict.AddOrUpdate(operation.Id, result, (guid, b) => result);
                        }
                        return;
                    }

                    #endregion

                    // If create and update are in the same batch,
                    // DbContext.Add is called after all updates are applied
                    var createAndUpdateInSameBatch = groupByRowGuidList.Any(dto => dto.OperationType == OperationType.Create)
                                                     && groupByRowGuidList.Any(dto => dto.OperationType == OperationType.Update);
                    
                    dynamic entityInstance = null;
                    
                    #region Get entity instance from repo, if exists
                    
                    try
                    {
                        // result of get method - entity instance
                        await _lock.WaitAsync();
                        var result = await repoInstance.GetByRowGuid(rowGuid);
                        _lock.Release();
                        entityInstance = Convert.ChangeType(result, modelType);
                    }
                    catch (Exception e)
                    {
                        _lock.Release();
                        Console.WriteLine(e);
                        entityInstance = null;
                    }
                    
                    #endregion
                    
                    #region Process delete operations

                    var deleteList = groupByRowGuidList.Where(dto => dto.OperationType == OperationType.Delete
                                                                     && dto.EntityRowGuid == rowGuid).ToList();
                    
                    if (entityInstance != null && deleteList.Count > 0)
                    {
                        var result = new UploadQueueResult();
                        try
                        {
                            // Delete entity
                            await _lock.WaitAsync();
                            await repoInstance.DeleteAsync(entityInstance);
                            _lock.Release();
                            
                            result.Error = UploadQueueError.NoError;
                        }
                        catch (Exception e)
                        {
                            _lock.Release();
                            Console.WriteLine(e);
                            result.Error = UploadQueueError.Exception;
                            result.Extra = e.Message;
                        }
                        finally
                        {
                            foreach (var deleteOperation in deleteList)
                            {
                                resultDict.AddOrUpdate(deleteOperation.Id, result, (guid, b) => result);
                            }
                        }
                    }
                    else
                    {
                        var result = new UploadQueueResult
                        {
                            Error = UploadQueueError.EntityDoesNotExist
                        };
                        foreach (var deleteOperation in deleteList)
                        {
                            resultDict.AddOrUpdate(deleteOperation.Id, result, (guid, b) => result);
                        }
                    }

                    #endregion
                    
                    #region Process create operations
                    
                    // should only be one create operation for this rowguid
                    var createList = groupByRowGuidList.Where(dto => dto.OperationType == OperationType.Create 
                                                                     && dto.EntityRowGuid == rowGuid).ToList();
                    
                    // get only first create operation for this rowguid
                    var createOperation = createList.FirstOrDefault();

                    if (createOperation != null)
                    {
                        var createResult = new UploadQueueResult();
                        try
                        {
                            if (entityInstance == null)
                            {
                                entityInstance = JsonConvert.DeserializeObject((string)createOperation.NewValue, modelType);

                                // Add entity, if only create operation is in the batch
                                if (!createAndUpdateInSameBatch)
                                {
                                    await _lock.WaitAsync();
                                    await repoInstance.AddAsync(entityInstance);
                                    _lock.Release();
                                }
                                
                                createResult.Error = UploadQueueError.NoError;
                            }
                            else
                            {
                                createResult.Error = UploadQueueError.EntityAlreadyExists;
                            }
                        }
                        catch (Exception e)
                        {
                            _lock.Release();
                            Console.WriteLine(e);
                            entityInstance = null;
                            createResult.Error = UploadQueueError.Exception;
                            createResult.Extra = e.Message;
                        }
                        finally
                        {
                            resultDict.AddOrUpdate(createOperation.Id, createResult, (guid, b) => createResult);
                        }
                    }
                    
                    // Put the rest of create ops (if any) into the result dict
                    if (createList.Count > 1)
                    {
                        var multipleCreateOpsResult = new UploadQueueResult
                        {
                            Error = UploadQueueError.OnlyOneCreateOperationAllowed
                        };
                    
                        foreach (var createOp in createList)
                        {
                            if (resultDict.ContainsKey(createOp.Id)) continue;

                            resultDict.TryAdd(createOp.Id, multipleCreateOpsResult);
                        }
                    }
                    
                    #endregion
                    
                    #region Process update operations
                    
                    var updateList = groupByRowGuidList.Where(dto => dto.OperationType == OperationType.Update 
                                                                     && dto.EntityRowGuid == rowGuid).ToList();

                    if (entityInstance != null && updateList.Count > 0)
                    {
                        // if update operation occured before the last change, then reject it
                        // to remove from list inside the loop, iterate backwards
                        for (int i = updateList.Count - 1; i >= 0; i--)
                        {
                            var updateOperation = updateList[i];

                            DateTimeOffset? lastChange = null;

                            try
                            {
                                await _lock.WaitAsync();
                                lastChange = await _columnChangesLogger.GetLastChange(updateOperation.EntityName,
                                    updateOperation.ColumnName, updateOperation.EntityRowGuid);
                                _lock.Release();
                            }
                            catch (Exception e)
                            {
                                _lock.Release();
                                Console.WriteLine(e);
                            }
                            
                            if (!lastChange.HasValue) continue;

                            if (lastChange.Value > updateOperation.OccuredAt)
                            {
                                updateList.Remove(updateOperation); // reject
                                var result = new UploadQueueResult
                                {
                                    Error = UploadQueueError.OutdatedChange
                                };
                                resultDict.AddOrUpdate(updateOperation.Id, result, (guid, b) => result);
                            }
                        }

                        try
                        {
                            if (updateList.Count > 0)
                            {
                                var rejected = UploadQueueActions.ApplyModifications(entityInstance, updateList) 
                                    as Dictionary<Guid, Exception>;
                                
                                // Add entity, if create and update ops are in the same batch
                                if (createAndUpdateInSameBatch)
                                {
                                    await _lock.WaitAsync();
                                    await repoInstance.AddAsync(entityInstance);
                                    _lock.Release();
                                }
                                // Else just update existing entity
                                else
                                {
                                    await _lock.WaitAsync();
                                    await repoInstance.UpdateAsync(entityInstance);
                                    _lock.Release();
                                }
    
                                // Put result into result dict
                                var updateTasks = updateList.Select(ProcessUpdate);
                                await Task.WhenAll(updateTasks);
                                
                                async Task ProcessUpdate(UploadQueueDto updateOperation)
                                {
                                    if (rejected == null) return;
                                    
                                    var result = new UploadQueueResult();
                                    if (rejected.ContainsKey(updateOperation.Id))
                                    {
                                        var exception = rejected[updateOperation.Id];
                                        result.Error = UploadQueueError.Exception;
                                        result.Extra = exception;
                                    }
                                    else
                                    {
                                        result.Error = UploadQueueError.NoError;

                                        try
                                        {
                                            // Set last change time to the one that came from the client
                                            _lock.Wait();
                                            await _columnChangesLogger.SetLastChange(
                                                updateOperation.EntityName,
                                                updateOperation.ColumnName,
                                                updateOperation.EntityRowGuid,
                                                updateOperation.OccuredAt);
                                            _lock.Release();
                                        }
                                        catch (Exception e)
                                        {
                                            _lock.Release();
                                            Console.WriteLine(e);
                                        }
                                    }
    
                                    resultDict.AddOrUpdate(updateOperation.Id, result, (guid, b) => result);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            _lock.Release();
                            Console.WriteLine(e);
                            var result = new UploadQueueResult
                            {
                                Error = UploadQueueError.Exception,
                                Extra = e.Message
                            };
                            foreach (var updateOperation in updateList)
                            {
                                resultDict.AddOrUpdate(updateOperation.Id, result, (guid, b) => result);
                            }
                        }
                    }
                    else
                    {
                        var result = new UploadQueueResult
                        {
                            Error = UploadQueueError.EntityDoesNotExist
                        };
                        foreach (var updateOperation in updateList)
                        {
                            resultDict.AddOrUpdate(updateOperation.Id, result, (guid, b) => result);
                        }
                    }
                    
                    #endregion
                }
                
                #endregion
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                var result = new UploadQueueResult
                {
                    Error = UploadQueueError.Exception,
                    Extra = e.Message
                };
                foreach (var operation in groupByEntityNameList)
                {
                    resultDict.AddOrUpdate(operation.Id, result, (guid, b) => result);
                }
            }
        }
    }
}
