using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abitech.NextApi.Common.Abstractions;
using Abitech.NextApi.Server.UploadQueue.ChangeTracking;
using Abitech.NextApi.UploadQueue.Common.Abstractions;
using Abitech.NextApi.UploadQueue.Common.Entity;
using Abitech.NextApi.UploadQueue.Common.UploadQueue;
using Newtonsoft.Json;

namespace Abitech.NextApi.Server.UploadQueue.Service
{
    /// <summary>
    /// Abstract implementation of UploadQueueService
    /// <para>Derive from this class and register repositories in the constructor</para>
    /// <para>You must call <see cref="UploadQueueServerExtensions.AddColumnChangesLogger{TDbContext}"/></para>
    /// </summary>
    public abstract class UploadQueueService : IUploadQueueService
    {
        private readonly IColumnChangesLogger _columnChangesLogger;
        private readonly INextApiUnitOfWork _unitOfWork;
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Assembly that contains UploadQueue models (should be implemented)
        /// </summary>
        protected abstract string UploadQueueModelsAssemblyName { get; }

        private static IDictionary<string, (Type entityType, Type repoType)> _wellKnownUploadQueueRepoTypes;

        private readonly List<IUploadQueueChangesHandler> _changesHandlers = new List<IUploadQueueChangesHandler>();

        /// <inheritdoc />
        protected UploadQueueService(
            IColumnChangesLogger columnChangesLogger,
            INextApiUnitOfWork unitOfWork,
            IServiceProvider serviceProvider)
        {
            _columnChangesLogger = columnChangesLogger;
            _unitOfWork = unitOfWork;
            _serviceProvider = serviceProvider;
            // collect information about UploadQueue model types
            CollectUploadQueueEntitiesInfo();
        }

        private void CollectUploadQueueEntitiesInfo()
        {
            if (_wellKnownUploadQueueRepoTypes != null)
                return;

            if (string.IsNullOrWhiteSpace(UploadQueueModelsAssemblyName))
                throw new ArgumentNullException(
                    $"You should implement {nameof(UploadQueueModelsAssemblyName)} correctly!");
            var uploadQueueModelsAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == UploadQueueModelsAssemblyName);
            if (uploadQueueModelsAssembly == null)
                throw new InvalidOperationException(
                    $@"Assembly with name: {UploadQueueModelsAssemblyName} is not found in the current AppDomain. 
Please specify correct assembly name!");

            var baseInterfaceType = typeof(IUploadQueueEntity);
            var baseRepoType = typeof(INextApiRepository<,>);
            _wellKnownUploadQueueRepoTypes = uploadQueueModelsAssembly.GetTypes()
                .Where(t => baseInterfaceType.IsAssignableFrom(t))
                .Select(entityType =>
                {
                    var repositoryType = baseRepoType.MakeGenericType(entityType, typeof(Guid));
                    return (entityType.Name, entityType, repositoryType);
                }).ToDictionary(k => k.Name, v => (v.entityType, v.repositoryType));
        }

        private (Type entityType, Type repoType) ResolveEntityTypeInfoByName(string entityName)
        {
            if (string.IsNullOrWhiteSpace(entityName))
                throw new ArgumentNullException(nameof(entityName));
            // Resolve info
            if (!_wellKnownUploadQueueRepoTypes.TryGetValue(entityName, out var info))
                throw new Exception($"No information about repo and type for {entityName}");
            return info;
        }

        /// <inheritdoc />
        public virtual async Task<Dictionary<Guid, UploadQueueResult>> ProcessAsync(IList<UploadQueueDto> uploadQueue)
        {
            _columnChangesLogger.LoggingEnabled = false;

            var resultDict = new Dictionary<Guid, (UploadQueueDto, UploadQueueResult)>();

            var entitiesGroups = from queueList in uploadQueue.AsQueryable()
                group queueList by queueList.EntityName
                into entityGroup
                select new {EntityName = entityGroup.Key, Items = entityGroup.ToList()};
            foreach (var entityGroup in entitiesGroups)
            {
                try
                {
                    await ProcessByEntityName(resultDict, entityGroup.EntityName, entityGroup.Items);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    var result = new UploadQueueResult(UploadQueueError.Exception, e.Message);
                    foreach (var operation in entityGroup.Items)
                    {
                        AddOrUpdate(resultDict, operation, result);
                    }
                }
            }

            await _unitOfWork.CommitAsync();

            foreach (var changesHandler in _changesHandlers)
                await changesHandler.OnCommit();

            foreach (var dto in uploadQueue)
            {
                if (resultDict.Any(pair => pair.Key == dto.Id)) continue;
                // if not in result dict, then it was not processed by entity name
                resultDict.Add(dto.Id,
                    (dto, new UploadQueueResult(UploadQueueError.Exception, $"No repo for {dto.EntityName}")));
            }

            _columnChangesLogger.LoggingEnabled = true;
            return resultDict.ToDictionary(k => k.Key, v => v.Value.Item2);
        }

        private async Task ProcessByEntityName(
            Dictionary<Guid, (UploadQueueDto, UploadQueueResult)> resultDict,
            string entityName,
            IList<UploadQueueDto> entityNameGroupingList)
        {
            var (entityType, repoType) = ResolveEntityTypeInfoByName(entityName);
            var repoInstance = (INextApiRepository)_serviceProvider.GetService(repoType);
            // Resolve changes handler
            var changesHandlerType = typeof(IUploadQueueChangesHandler<>);
            var genericChangesHandlerType = changesHandlerType.MakeGenericType(entityType);
            var changesHandlerInstance =
                (IUploadQueueChangesHandler)_serviceProvider.GetService(genericChangesHandlerType);
            if (changesHandlerInstance != null)
                _changesHandlers.Add(changesHandlerInstance);

            // Process by row guid
            var rowGuidGroupings = entityNameGroupingList.GroupBy(dto => dto.EntityRowGuid);
            foreach (var rowGuidGrouping in rowGuidGroupings)
            {
                var rowGuid = rowGuidGrouping.Key;
                var rowGuidGroupingList = rowGuidGrouping.ToList();
                try
                {
                    await ProcessByRowGuid(rowGuid, rowGuidGroupingList);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    var result = new UploadQueueResult(UploadQueueError.Exception, e.Message);
                    foreach (var operation in rowGuidGroupingList)
                    {
                        AddOrUpdate(resultDict, operation, result);
                    }
                }
            }

            #region Nested method ProcessByRowGuid

            async Task ProcessByRowGuid(Guid rowGuid, IList<UploadQueueDto> rowGuidGroupingList)
            {
                // Reject all operations silently if create and delete ops are in the same batch
                if (rowGuidGroupingList.Any(dto => dto.OperationType == OperationType.Create)
                    && rowGuidGroupingList.Any(dto => dto.OperationType == OperationType.Delete))
                {
                    var result = new UploadQueueResult(UploadQueueError.NoError);
                    foreach (var operation in rowGuidGroupingList)
                    {
                        AddOrUpdate(resultDict, operation, result);
                    }

                    return;
                }

                // If create and update are in the same batch,
                // Repo.AddAsync is called after all updates are applied
                var createAndUpdateInSameBatch =
                    rowGuidGroupingList.Any(dto => dto.OperationType == OperationType.Create)
                    && rowGuidGroupingList.Any(dto => dto.OperationType == OperationType.Update);

                dynamic entityInstance = null;

                // Get entity instance from repo, if exists
                try
                {
                    // result of get method - entity instance
                    var result = await repoInstance.GetByIdAsync(rowGuid);
                    entityInstance = Convert.ChangeType(result, entityType);
                }
                catch
                {
                    entityInstance = null;
                }

                // Process delete operations
                var deleteList = rowGuidGroupingList.Where(dto => dto.OperationType == OperationType.Delete
                                                                  && dto.EntityRowGuid == rowGuid).ToList();

                if (entityInstance != null && deleteList.Count > 0)
                {
                    var result = new UploadQueueResult(UploadQueueError.NoError);
                    try
                    {
                        // Delete entity
                        if (changesHandlerInstance != null)
                            await changesHandlerInstance.OnBeforeDelete(entityInstance);
                        await repoInstance.DeleteAsync(entityInstance);
                        try
                        {
                            if (changesHandlerInstance != null)
                                await changesHandlerInstance.OnAfterDelete(entityInstance);
                        }
                        catch
                        {
                            // ignored
                        }

                        entityInstance = null;
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
                            AddOrUpdate(resultDict, deleteOperation, result);
                        }
                    }
                }
                else
                {
                    var result = new UploadQueueResult(UploadQueueError.EntityDoesNotExist);
                    foreach (var deleteOperation in deleteList)
                    {
                        AddOrUpdate(resultDict, deleteOperation, result);
                    }
                }

                // Process create operations
                // should only be one create operation for this rowguid
                var createList = rowGuidGroupingList.Where(dto => dto.OperationType == OperationType.Create
                                                                  && dto.EntityRowGuid == rowGuid).ToList();

                // get only first create operation for this rowguid
                var firstCreateOperation = createList.FirstOrDefault();

                async Task CreateEntityAsync(dynamic entityInstanceArg)
                {
                    if (changesHandlerInstance != null)
                        await changesHandlerInstance.OnBeforeCreate(entityInstanceArg);
                    await repoInstance.AddAsync(entityInstanceArg);
                    try
                    {
                        if (changesHandlerInstance != null)
                            await changesHandlerInstance.OnAfterCreate(entityInstanceArg);
                    }
                    catch
                    {
                        // ignored
                    }
                }

                if (firstCreateOperation != null)
                {
                    var createResult = new UploadQueueResult(UploadQueueError.NoError);
                    try
                    {
                        if (entityInstance == null)
                        {
                            entityInstance =
                                JsonConvert.DeserializeObject((string)firstCreateOperation.NewValue, entityType);

                            // Create entity, if only create operation is in the batch
                            if (!createAndUpdateInSameBatch)
                                await CreateEntityAsync(entityInstance);
                        }
                        else
                            createResult.Error = UploadQueueError.EntityAlreadyExists;
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
                        AddOrUpdate(resultDict, firstCreateOperation, createResult);
                    }
                }

                // Put the rest of create ops (if any) into the result dict
                if (createList.Count > 1)
                {
                    var multipleCreateOpsResult = new UploadQueueResult(UploadQueueError.OnlyOneCreateOperationAllowed);
                    foreach (var createOp in createList)
                    {
                        if (resultDict.ContainsKey(createOp.Id)) continue;
                        AddOrUpdate(resultDict, createOp, multipleCreateOpsResult);
                    }
                }

                // Process update operations
                var updateList = rowGuidGroupingList.Where(dto => dto.OperationType == OperationType.Update
                                                                  && dto.EntityRowGuid == rowGuid).ToList();

                if (entityInstance != null && updateList.Count > 0)
                {
                    // if update operation occured before the last change, then reject it
                    // to remove from list inside the loop, iterate backwards
                    for (int i = updateList.Count - 1; i >= 0; i--)
                    {
                        var updateOperation = updateList[i];
                        var lastChange = await _columnChangesLogger.GetLastChange(updateOperation.EntityName,
                            updateOperation.ColumnName, updateOperation.EntityRowGuid);

                        if (lastChange.HasValue && lastChange.Value > updateOperation.OccuredAt)
                        {
                            updateList.Remove(updateOperation); // reject
                            var result = new UploadQueueResult(UploadQueueError.OutdatedChange);
                            AddOrUpdate(resultDict, updateOperation, result);
                        }
                        else
                        {
                            try
                            {
                                if (changesHandlerInstance != null)
                                    await changesHandlerInstance.OnBeforeUpdate(entityInstance,
                                        updateOperation.ColumnName, updateOperation.NewValue);
                            }
                            catch (Exception e)
                            {
                                updateList.Remove(updateOperation); // reject
                                var result = new UploadQueueResult(UploadQueueError.Exception, e.Message);
                                AddOrUpdate(resultDict, updateOperation, result);
                            }
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
                                await CreateEntityAsync(entityInstance);
                            // Else just update existing entity
                            else
                                await repoInstance.UpdateAsync(entityInstance);

                            foreach (var updateOperation in updateList)
                            {
                                var result = new UploadQueueResult(UploadQueueError.NoError);
                                if (rejected != null && rejected.ContainsKey(updateOperation.Id))
                                {
                                    var exception = rejected[updateOperation.Id];
                                    result.Error = UploadQueueError.Exception;
                                    result.Extra = exception.Message;
                                }
                                else
                                {
                                    // Set last change time to the one that came from the client
                                    await _columnChangesLogger.SetLastChange(
                                        updateOperation.EntityName,
                                        updateOperation.ColumnName,
                                        updateOperation.EntityRowGuid,
                                        updateOperation.OccuredAt);

                                    try
                                    {
                                        if (!createAndUpdateInSameBatch && changesHandlerInstance != null)
                                            changesHandlerInstance.OnAfterUpdate(entityInstance,
                                                updateOperation.ColumnName, updateOperation.NewValue);
                                    }
                                    catch
                                    {
                                        // ignored
                                    }
                                }

                                AddOrUpdate(resultDict, updateOperation, result);
                            }
                        }
                        else if (createAndUpdateInSameBatch)
                            await CreateEntityAsync(entityInstance);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        var result = new UploadQueueResult(UploadQueueError.Exception, e.Message);
                        if (createAndUpdateInSameBatch)
                        {
                            AddOrUpdate(resultDict, firstCreateOperation, result);
                        }

                        foreach (var updateOperation in updateList)
                        {
                            AddOrUpdate(resultDict, updateOperation, result);
                        }
                    }
                }
                else if (entityInstance == null)
                {
                    var result = new UploadQueueResult(UploadQueueError.EntityDoesNotExist);
                    foreach (var updateOperation in updateList)
                    {
                        AddOrUpdate(resultDict, updateOperation, result);
                    }
                }
            }

            #endregion
        }

        private void AddOrUpdate(
            Dictionary<Guid, (UploadQueueDto, UploadQueueResult)> resultDict,
            UploadQueueDto uploadQueueDto,
            UploadQueueResult result)
        {
            var exists = resultDict.TryGetValue(uploadQueueDto.Id, out _);
            if (exists)
                resultDict[uploadQueueDto.Id] = (uploadQueueDto, result);
            else
                resultDict.Add(uploadQueueDto.Id, (uploadQueueDto, result));
        }
    }
}
