using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Abitech.NextApi.Common;
using Abitech.NextApi.Common.Abstractions;
using Abitech.NextApi.Common.DTO;
using Abitech.NextApi.Common.Entity;
using Abitech.NextApi.Common.Filtering;
using Abitech.NextApi.Common.Paged;
using Abitech.NextApi.Common.Tree;
using Abitech.NextApi.Server.Base;
using Abitech.NextApi.Server.Service;
using AutoMapper;

namespace Abitech.NextApi.Server.Entity
{
    /// <summary>
    /// Basic abstractions for NextApi entity service
    /// </summary>
    /// <typeparam name="TDto">Type of dto</typeparam>
    /// <typeparam name="TEntity">Type of entity</typeparam>
    /// <typeparam name="TKey">Type of entity key</typeparam>
    public abstract class NextApiEntityService<TDto, TEntity, TKey> : INextApiEntityService<TDto, TKey>
        where TDto : class, IEntityDto<TKey>
        where TEntity : class, IEntity<TKey>
    {
        private readonly INextApiUnitOfWork _unitOfWork;
        private readonly INextApiRepository<TEntity, TKey> _repository;
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
        protected NextApiEntityService(INextApiUnitOfWork unitOfWork, IMapper mapper,
            INextApiRepository<TEntity, TKey> repository)
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
            var entitiesQuery = _repository.Expand(_repository.GetAll(), request.Expand);
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
            var entities = await _repository.ToListAsync(entitiesQuery);
            return new PagedList<TDto>
            {
                Items = _mapper.Map<List<TEntity>, List<TDto>>(entities), TotalItems = totalCount
            };
        }

        /// <inheritdoc />
        public virtual async Task<TDto> GetById(TKey key, string[] expand = null)
        {
            var entityQuery = _repository.Expand(_repository
                .GetAll()
                .Where(e => e.Id.Equals(key)), expand);
            entityQuery = await BeforeGet(entityQuery);
            var entity = await _repository.FirstOrDefaultAsync(entityQuery);
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
            var entitiesQuery = _repository.Expand(_repository
                .GetAll()
                .Where(e => keys.Contains(e.Id)), expand);
            entitiesQuery = await BeforeGet(entitiesQuery);
            var entities = await _repository.ToArrayAsync(entitiesQuery);
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

            return await _repository.CountAsync(entitiesQuery);
        }

        /// <inheritdoc />
        public virtual async Task<bool> Any(Filter filter = null)
        {
            var entitiesQuery = _repository.GetAll();
            // apply filter
            var filterExpression = filter?.ToLambdaFilter<TEntity>();
            return await _repository.AnyAsync(entitiesQuery, filterExpression);
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

            return await _repository.ToArrayAsync(entitiesQuery.Select(e => e.Id));
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
                    rootQuery = _repository.Expand(rootQuery, request.PagedRequest.Expand);
            }

            var treeChunk = await _repository.ToArrayAsync(rootQuery
                .Select(SelectTreeChunk(entitiesQuery)));
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
