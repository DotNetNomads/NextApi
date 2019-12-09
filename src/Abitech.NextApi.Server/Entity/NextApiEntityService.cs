using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Abitech.NextApi.Model;
using Abitech.NextApi.Model.Abstractions;
using Abitech.NextApi.Model.Filtering;
using Abitech.NextApi.Model.Paged;
using Abitech.NextApi.Model.Tree;
using Abitech.NextApi.Server.Base;
using Abitech.NextApi.Server.Entity.Model;
using Abitech.NextApi.Server.Service;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Abitech.NextApi.Server.Entity
{
    /// <summary>
    /// Basic abstractions for NextApi entity service
    /// </summary>
    /// <typeparam name="TDto">Type of dto</typeparam>
    /// <typeparam name="TEntity">Type of entity</typeparam>
    /// <typeparam name="TKey">Type of entity key</typeparam>
    /// <typeparam name="TUnitOfWork">Type of unit of work</typeparam>
    /// <typeparam name="TRepo"></typeparam>
    public abstract class NextApiEntityService<TDto, TEntity, TKey, TRepo, TUnitOfWork> : NextApiService,
        INextApiEntityService<TDto, TKey>
        where TDto : class
        where TEntity : class, IEntity<TKey>
        where TRepo : class, INextApiRepository<TEntity, TKey>
        where TUnitOfWork : class, INextApiUnitOfWork
    {
        private readonly TUnitOfWork _unitOfWork;
        private readonly TRepo _repository;
        private readonly IMapper _mapper;

        /// <summary>
        /// Save changes to database automatically
        /// </summary>
        protected bool AutoCommit { get; set; } = true;

        /// <summary>
        /// Initializes instance of entity service 
        /// </summary>
        /// <param name="unitOfWork">Unit of work, used when saving data</param>
        /// <param name="mapper">Used for mapping operations</param>
        /// <param name="repository">Used for data access</param>
        protected NextApiEntityService(TUnitOfWork unitOfWork, IMapper mapper,
            TRepo repository)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }


        /// <inheritdoc />
        public virtual async Task<TDto> Create(TDto entity)
        {
            var entityFromDto = _mapper.Map<TDto, TEntity>(entity);
            await BeforeCreate(entityFromDto);
            await _repository.AddAsync(entityFromDto);
            await AfterCreate(entityFromDto);
            await CommitAsync();
            var insertedEntity = await _repository.GetByIdAsync(entityFromDto.Id);
            return _mapper.Map<TEntity, TDto>(insertedEntity);
        }


        /// <inheritdoc />
        public virtual async Task Delete(TKey key)
        {
            var entity = await _repository.GetByIdAsync(key);
            if (entity == null)
            {
                throw new NextApiException(NextApiErrorCode.EntityIsNotExist,
                    $"Entity with id {key.ToString()} is not found!", new Dictionary<string, object> {{"id", key}});
            }

            await BeforeDelete(entity);
            await _repository.DeleteAsync(entity);
            await AfterDelete(entity);
            await CommitAsync();
        }


        /// <inheritdoc />
        public virtual async Task<TDto> Update(TKey key, TDto patch)
        {
            var entity = await _repository.GetByIdAsync(key);
            if (entity == null)
                throw new NextApiException(NextApiErrorCode.EntityIsNotExist,
                    $"Entity with id {key.ToString()} is not found!", new Dictionary<string, object> {{"id", key}});
            await BeforeUpdate(entity, patch);
            NextApiUtils.PatchEntity(patch, entity);
            await _repository.UpdateAsync(entity);
            await AfterUpdate(entity);
            await CommitAsync();
            var updatedEntity = await _repository.GetByIdAsync(entity.Id);
            return _mapper.Map<TEntity, TDto>(updatedEntity);
        }

        /// <inheritdoc />
        public virtual async Task<PagedList<TDto>> GetPaged(PagedRequest request)
        {
            var entitiesQuery = _repository.GetAll().Expand(request.Expand);
            // apply filter
            var filterExpression = request.Filter?.ToLambdaFilter<TEntity>();
            if (filterExpression != null)
            {
                entitiesQuery = entitiesQuery.Where(filterExpression);
            }

            entitiesQuery = await BeforeGet(entitiesQuery);
            var totalCount = entitiesQuery.Count();
            if (request.Skip != null)
                entitiesQuery = entitiesQuery.Skip(request.Skip.Value);
            if (request.Take != null)
                entitiesQuery = entitiesQuery.Take(request.Take.Value);
            var entities = await entitiesQuery.ToListAsync();
            return new PagedList<TDto>
            {
                Items = _mapper.Map<List<TEntity>, List<TDto>>(entities), TotalItems = totalCount
            };
        }

        /// <inheritdoc />
        public virtual async Task<TDto> GetById(TKey key, string[] expand = null)
        {
            var entityQuery = _repository
                .GetAll()
                .Where(e => e.Id.Equals(key))
                .Expand(expand);
            entityQuery = await BeforeGet(entityQuery);
            var entity = await entityQuery.FirstOrDefaultAsync();
            if (entity == null)
                throw new NextApiException(NextApiErrorCode.EntityIsNotExist,
                    $"Entity with id {key.ToString()} is not found!", new Dictionary<string, object> {{"id", key}});
            return _mapper.Map<TEntity, TDto>(entity);
        }

        /// <summary>
        /// Implementation of GetByIds(get array of entities)
        /// </summary>
        /// <param name="keys"></param>
        /// <param name="expand"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public virtual async Task<TDto[]> GetByIds(TKey[] keys, string[] expand = null)
        {
            var entitiesQuery = _repository
                .GetAll()
                .Where(e => keys.Contains(e.Id))
                .Expand(expand);
            entitiesQuery = await BeforeGet(entitiesQuery);
            var entities = await entitiesQuery.ToArrayAsync();
            if (entities == null)
                throw new NextApiException(NextApiErrorCode.EntitiesIsNotExist,
                    $"Entities with keys {string.Join(",", keys)} is not found!",
                    new Dictionary<string, object> {{"keys", keys}});

            return _mapper.Map<TEntity[], TDto[]>(entities);
        }

        /// <inheritdoc />
        public async virtual Task<int> Count(Filter filter = null)
        {
            var entitiesQuery = _repository.GetAll();
            // apply filter
            var filterExpression = filter?.ToLambdaFilter<TEntity>();
            if (filterExpression != null)
            {
                entitiesQuery = entitiesQuery.Where(filterExpression);
            }

            return await entitiesQuery.CountAsync();
        }

        /// <inheritdoc />
        public virtual async Task<bool> Any(Filter filter = null)
        {
            var entitiesQuery = _repository.GetAll();
            // apply filter
            var filterExpression = filter?.ToLambdaFilter<TEntity>();
            return filterExpression == null
                ? await entitiesQuery.AnyAsync()
                : await entitiesQuery.AnyAsync(filterExpression);
        }

        /// <inheritdoc />
        public virtual async Task<TKey[]> GetIdsByFilter(Filter filter = null)
        {
            var entitiesQuery = _repository.GetAll();
            // apply filter
            var filterExpression = filter?.ToLambdaFilter<TEntity>();
            if (filterExpression != null)
            {
                entitiesQuery = entitiesQuery.Where(filterExpression);
            }

            return await entitiesQuery.Select(e => e.Id).ToArrayAsync();
        }

        /// <inheritdoc />
        public virtual async Task<PagedList<TreeItem<TDto>>> GetTree(TreeRequest request)
        {
            var parentPredicate = await ParentPredicate(request.ParentId);
            if (parentPredicate == null)
            {
                // in case entity doesn't support tree operations
                throw new NextApiException(NextApiErrorCode.OperationIsNotSupported,
                    "Please provide ParentPredicate and SelectTreeChunk implementations");
            }

            var entitiesQuery = _repository.GetAll();
            entitiesQuery = await BeforeGet(entitiesQuery);
            var rootQuery = entitiesQuery.Where(parentPredicate);
            var totalCount = 0;

            if (request.PagedRequest != null)
            {
                var filterExpression = request.PagedRequest.Filter?.ToLambdaFilter<TEntity>();
                if (filterExpression != null)
                {
                    rootQuery = rootQuery.Where(filterExpression);
                }

                totalCount = rootQuery.Count();
                if (request.PagedRequest.Skip != null)
                    rootQuery = rootQuery.Skip(request.PagedRequest.Skip.Value);
                if (request.PagedRequest.Take != null)
                    rootQuery = rootQuery.Take(request.PagedRequest.Take.Value);
                if (request.PagedRequest.Expand != null)
                    rootQuery = rootQuery.Expand(request.PagedRequest.Expand);
            }

            var treeChunk = await rootQuery
                .Select(SelectTreeChunk(entitiesQuery)).ToArrayAsync();
            var output = treeChunk.Select(chunkItem =>
                new TreeItem<TDto>
                {
                    ChildrenCount = chunkItem.ChildrenCount, Entity = _mapper.Map<TEntity, TDto>(chunkItem.Entity)
                }).ToList();

            return new PagedList<TreeItem<TDto>>
            {
                Items = output, TotalItems = totalCount == 0 ? output.Count : totalCount
            };
        }

        /// <summary>
        /// Used in select operation for GetTree method
        /// </summary>
        /// <param name="query">Query for additional calculations</param>
        /// <returns>Select lambda</returns>
        protected virtual Expression<Func<TEntity, TreeItem<TEntity>>> SelectTreeChunk(IQueryable<TEntity> query)
        {
            // in case entity doesn't support tree operations
            throw new NextApiException(NextApiErrorCode.OperationIsNotSupported,
                "Please provide SelectTreeChunk implementation");
        }

        /// <summary>
        /// Commits changes in data repository
        /// </summary>
        /// <returns></returns>
        protected async Task CommitAsync()
        {
            if (AutoCommit)
            {
                await _unitOfWork.CommitAsync();
                await AfterCommit();
            }
        }


        #region hooks

        /// <summary>
        /// Hook: runs before entity added to data repository
        /// </summary>
        /// <param name="entity">Entity instance</param>
        /// <returns></returns>
#pragma warning disable 1998
        protected virtual async Task BeforeCreate(TEntity entity)
#pragma warning restore 1998
        {
        }

        /// <summary>
        /// Hook: runs after entity added to data repository
        /// </summary>
        /// <remarks>Should call CommitAsync in
        /// case you want to work with saved entity</remarks>
        /// <param name="entity">Entity instance</param>
        /// <returns></returns>
#pragma warning disable 1998
        protected virtual async Task AfterCreate(TEntity entity)
#pragma warning restore 1998
        {
        }

        /// <summary>
        /// Hook: runs before entity updated in data repository
        /// </summary>
        /// <param name="entity">Original entity instance</param>
        /// <param name="patch">Patch for original entity</param>
        /// <returns></returns>
#pragma warning disable 1998
        protected virtual async Task BeforeUpdate(TEntity entity, TDto patch)
#pragma warning restore 1998
        {
        }

        /// <summary>
        /// Hook: runs after entity updated in data repository
        /// </summary>
        /// <param name="entity">Patched entity instance</param>
        /// <remarks>Should call CommitAsync in
        /// case you want to work with saved entity</remarks>
        /// <returns></returns>
#pragma warning disable 1998
        protected virtual async Task AfterUpdate(TEntity entity)
#pragma warning restore 1998
        {
        }

        /// <summary>
        /// Hook: runs before entity deleted from data repository
        /// </summary>
        /// <param name="entity">Entity instance for delete</param>
        /// <returns></returns>
#pragma warning disable 1998
        protected virtual async Task BeforeDelete(TEntity entity)
#pragma warning restore 1998
        {
        }

        /// <summary>
        /// Hook: runs after entity deleted from data repository
        /// </summary>
        /// <param name="entity">Deleted entity instance</param>
        /// <remarks>Should call CommitAsync in case you finalize delete operation</remarks>
        /// <returns></returns>
#pragma warning disable 1998
        protected virtual async Task AfterDelete(TEntity entity)
#pragma warning restore 1998
        {
        }

        /// <summary>
        /// Hook: runs after CommitAsync is called
        /// </summary>
        /// <returns></returns>
#pragma warning disable 1998
        protected virtual async Task AfterCommit()
#pragma warning restore 1998
        {
        }

        /// <summary>
        /// Hook: runs before data loaded from repository
        /// </summary>
        /// <param name="query">Prepared query to repo</param>
        /// <returns>Modified query to repo</returns>
#pragma warning disable 1998
        protected virtual async Task<IQueryable<TEntity>> BeforeGet(IQueryable<TEntity> query)
#pragma warning restore 1998
        {
            return query;
        }

        /// <summary>
        /// Returns parent predicate usable in GetTree method
        /// </summary>
        /// <returns>Predicate for matching parent id for the entity</returns>
#pragma warning disable 1998
        protected virtual async Task<Expression<Func<TEntity, bool>>> ParentPredicate(object parentId)
#pragma warning restore 1998
        {
            // GetTree disabled by default
            return null;
        }

        #endregion
    }
}
