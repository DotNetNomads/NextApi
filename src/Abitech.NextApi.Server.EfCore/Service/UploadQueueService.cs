using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
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
    public abstract class UploadQueueService<TUnitOfWork> : NextApiService
        where TUnitOfWork : class, INextApiUnitOfWork
    {
        private readonly Dictionary<string, Type> _repositoryDictionary = new Dictionary<string, Type>();
        
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
        /// <typeparam name="T">Repository type</typeparam>
        /// <exception cref="ArgumentNullException">If modelName is null or whitespace</exception>
        /// <exception cref="ArgumentException">If repository type does not implement INextApiRepository</exception>
        protected void RegisterRepository<T>(string modelName)
        {
            if (string.IsNullOrWhiteSpace(modelName))
                throw new ArgumentNullException(nameof(modelName));

            var repoType = typeof(T);

            if (!repoType.IsAssignableToGenericType(typeof(INextApiRepository<,>)))
                throw new ArgumentException($"Type {repoType.Name} must implement INextApiRepository<>");
            
            _repositoryDictionary.Add(modelName, repoType);
        }

        public async Task<ConcurrentDictionary<Guid, UploadQueueResult>> ProcessAsync(IList<UploadQueueDto> uploadQueue)
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
            
            await _unitOfWork.Commit();
            
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
                
                #region Resolve repo
            
                var res = _repositoryDictionary.TryGetValue(entityName, out var repoType);
                if (!res)
                    throw new Exception($"No repo for {entityName}");

                Type repoGenericModelType = null;
                var genericArguments = repoType.AllBaseTypes().SelectMany(type => type.GetGenericArguments());
                foreach (var genericType in genericArguments)
                {
                    if (genericType.Name != entityName) continue;
                    if (genericType.IsAssignableFrom(typeof(IGuidEntity<>)) 
                        || genericType.IsAssignableFrom(typeof(IRowGuidEnabled)))
                        continue;
                    
                    repoGenericModelType = genericType;
                    break;
                }

                if (repoGenericModelType == null)
                    throw new Exception($"Repo {repoType.Name} is not for {entityName}");
                
                var repoInstance = _serviceProvider.GetService(repoType);
                var methodSignature = new[] { repoGenericModelType };
                MethodInfo getEntityMethodInfo = null;
                MethodInfo createMethodInfo = null;
                MethodInfo updateMethodInfo = null;
                MethodInfo deleteMethodInfo = null;

                const string getEntityMethodName = nameof(INextApiRepository<object, object>.GetAsync);
                var funcType = typeof(Func<,>).MakeGenericType(repoGenericModelType, typeof(bool));
                var expressionType = typeof(Expression<>).MakeGenericType(funcType);
                getEntityMethodInfo = NextApiServiceHelper
                    .GetServiceMethod(repoType, getEntityMethodName, new[] { expressionType });

                if (groupByEntityNameList.Any(dto => dto.OperationType == OperationType.Create))
                {
                    const string createMethodName = nameof(INextApiRepository<object, object>.AddAsync);
                    createMethodInfo = NextApiServiceHelper
                        .GetServiceMethod(repoType, createMethodName, methodSignature);
                }
                
                if (groupByEntityNameList.Any(dto => dto.OperationType == OperationType.Update))
                {
                    const string updateMethodName = nameof(INextApiRepository<object, object>.Update);
                    updateMethodInfo = NextApiServiceHelper
                        .GetServiceMethod(repoType, updateMethodName, methodSignature);
                }

                if (groupByEntityNameList.Any(dto => dto.OperationType == OperationType.Delete))
                {
                    const string deleteMethodName = nameof(INextApiRepository<object, object>.Delete);
                    deleteMethodInfo = NextApiServiceHelper
                        .GetServiceMethod(repoType, deleteMethodName, methodSignature);
                }
                
                #endregion
                
                var taskList = new List<Task>();
                var rowGuidGrouping = groupByEntityNameList.GroupBy(dto => dto.EntityRowGuid);
                foreach (var group in rowGuidGrouping)
                {
                    taskList.Add(ProcessByRowGuid(group));
                }

                await Task.WhenAll(taskList);
                
                #region Nested method ProcessByRowGuid
                
                async Task ProcessByRowGuid(IGrouping<Guid, UploadQueueDto> groupByRowGuid)
                {
                    var rowGuid = groupByRowGuid.Key;
                    
                    // IGrouping is IEnumerable, so it cant be enumerated multiple times
                    var groupByRowGuidList = groupByRowGuid.ToList();
                    
                    #region Check if create and delete ops are in the same batch

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

                    bool createAndUpdateInSameBatch;

                    #region Check if create and update ops are in the same batch

                    createAndUpdateInSameBatch = groupByRowGuidList.Any(dto => dto.OperationType == OperationType.Create)
                                                 && groupByRowGuidList.Any(dto => dto.OperationType == OperationType.Update);

                    #endregion
                    
                    dynamic entityInstance = null;
                    
                    #region Get entity instance from repo, if exists
                    
                    try
                    {
                        var where = UploadQueueServiceHelper.GetEqualsExpression(
                            repoGenericModelType,
                            nameof(IGuidEntity<object>.RowGuid),
                            rowGuid);
                        var parameters = NextApiServiceHelper.ResolveMethodParameters(getEntityMethodInfo, 
                            new Dictionary<string, object>
                            {
                                { "where", where }
                            });
                                
                        // result of get method - entity instance
                        _lock.Wait();
                        var result = await NextApiServiceHelper.CallService(repoInstance, getEntityMethodInfo, parameters);
                        _lock.Release();
                        
                        entityInstance = Convert.ChangeType(result, repoGenericModelType);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        entityInstance = null;
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
                                entityInstance = JsonConvert.DeserializeObject((string)createOperation.NewValue,
                                    repoGenericModelType);
                                var parameters = NextApiServiceHelper
                                    .ResolveMethodParameters(createMethodInfo, new Dictionary<string, object>
                                    {
                                        {"entity", entityInstance}
                                    });

                                // Add entity, if only create operation is in the batch
                                if (!createAndUpdateInSameBatch)
                                {
                                    _lock.Wait();
                                    await NextApiServiceHelper.CallService(repoInstance, createMethodInfo, parameters);
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
                        var lastChangeTaskArray = new Task<DateTimeOffset?>[updateList.Count];
                        for (int i = 0; i < updateList.Count; i++)
                        {
                            var updateOperation = updateList[i];
                            _lock.Wait();
                            lastChangeTaskArray[i] = _columnChangesLogger.GetLastChange(updateOperation.EntityName,
                                updateOperation.ColumnName, updateOperation.EntityRowGuid);
                            _lock.Release();
                        }

                        await Task.WhenAll(lastChangeTaskArray);

                        // to remove from list inside the loop, iterate backwards
                        for (int i = updateList.Count - 1; i >= 0; i--)
                        {
                            var updateOperation = updateList[i];
                            
                            var lastChange = await lastChangeTaskArray[i];
                            
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
                        
                                var parameters = NextApiServiceHelper.ResolveMethodParameters(updateMethodInfo, 
                                    new Dictionary<string, object>
                                    {
                                        { "entity", entityInstance }
                                    });
                                
                                // Add entity, if create and update ops are in the same batch
                                if (createAndUpdateInSameBatch)
                                {
                                    _lock.Wait();
                                    await NextApiServiceHelper.CallService(repoInstance, createMethodInfo, parameters);
                                    _lock.Release();
                                }
                                // Else just update existing entity
                                else
                                {
                                    _lock.Wait();
                                    await NextApiServiceHelper.CallService(repoInstance, updateMethodInfo, parameters);
                                    _lock.Release();
                                }
    
                                // Put result into result dict
                                Parallel.ForEach(updateList, async updateOperation =>
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
                                        
                                        // Set last change time to the one that came from the client
                                        _lock.Wait();
                                        await _columnChangesLogger.SetLastChange(
                                            updateOperation.EntityName,
                                            updateOperation.ColumnName,
                                            updateOperation.EntityRowGuid,
                                            updateOperation.OccuredAt);
                                        _lock.Release();
                                    }
    
                                    resultDict.AddOrUpdate(updateOperation.Id, result, (guid, b) => result);
                                });
                            }
                        }
                        catch (Exception e)
                        {
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
                    
                    #region Process delete operations

                    var deleteList = groupByRowGuidList.Where(dto => dto.OperationType == OperationType.Delete
                                                                     && dto.EntityRowGuid == rowGuid).ToList();
                    
                    if (entityInstance != null && deleteList.Count > 0)
                    {
                        var result = new UploadQueueResult();
                        try
                        {
                            var parameters = NextApiServiceHelper.ResolveMethodParameters(deleteMethodInfo,
                                new Dictionary<string, object>
                                {
                                    {"entity", entityInstance}
                                });

                            // Delete entity
                            _lock.Wait();
                            await NextApiServiceHelper.CallService(repoInstance, deleteMethodInfo, parameters);
                            _lock.Release();
                            
                            result.Error = UploadQueueError.NoError;
                        }
                        catch (Exception e)
                        {
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
