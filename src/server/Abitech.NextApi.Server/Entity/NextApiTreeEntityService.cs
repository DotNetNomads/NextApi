using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Abitech.NextApi.Common.Abstractions;
using Abitech.NextApi.Common.Abstractions.DAL;
using Abitech.NextApi.Common.DTO;
using Abitech.NextApi.Common.Paged;
using Abitech.NextApi.Common.Tree;
using AutoMapper;

namespace Abitech.NextApi.Server.Entity
{
    /// <summary>
    /// Basic realization for NextApi entity service of Tree-type entities
    /// </summary>
    /// <typeparam name="TDto"></typeparam>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    public class NextApiTreeEntityService<TDto, TEntity, TKey> : NextApiEntityService<TDto, TEntity, TKey>,
        INextApiTreeEntityService<TDto, TKey>
        where TDto : class, IEntityDto<TKey> where TEntity : class, ITreeEntity<TKey>
    {
        private static readonly MethodInfo _equalsMethod =
            typeof(object).GetMethod("Equals", new[] {typeof(object)});

        private static readonly MethodInfo _countWithPredicate = typeof(Queryable).GetMethods()
            .First(m => m.Name == "Count" && m.GetParameters().Length == 2);

        private readonly IMapper _mapper;
        private readonly IRepo<TEntity, TKey> _repository;

        /// <inheritdoc />
        public NextApiTreeEntityService(IUnitOfWork unitOfWork, IMapper mapper, IRepo<TEntity, TKey> repository) :
            base(unitOfWork, mapper, repository)
        {
            _mapper = mapper;
            _repository = repository;
        }

        //        protected override Expression<Func<TestTreeItem, TreeItem<TestTreeItem>>> SelectTreeChunk(
//            IQueryable<TestTreeItem> query) =>
//            entity =>
//                new TreeItem<TestTreeItem>
//                {
//                    Entity = entity, ChildrenCount = query.Count(childEntity => childEntity.ParentId == entity.Id)
//                };

        /// <summary>
        /// Used in select operation for GetTree method
        /// </summary>
        /// <param name="query">Query for additional calculations</param>
        /// <returns>Select lambda</returns>
        protected virtual Expression<Func<TEntity, TreeItem<TEntity>>> SelectTreeChunk(IQueryable<TEntity> query)
        {
            var entityType = typeof(TEntity);

            var mainParameter = Expression.Parameter(entityType, "entity");

            var treeItemType = typeof(TreeItem<TEntity>);
            var propertyEntity = treeItemType.GetProperty("Entity");
            var propertyChildrenCount = treeItemType.GetProperty("ChildrenCount");

            var ctorTreeItem = Expression.New(treeItemType);
            var entityPropertyAssigment = Expression.Bind(propertyEntity, mainParameter);
            var queryConst = Expression.Constant(query);
            var generatedCountMethod = _countWithPredicate.MakeGenericMethod(entityType);
            var childrenCountAssigment = Expression.Bind(propertyChildrenCount,
                Expression.Call(generatedCountMethod, queryConst, ParentPredicateCount(mainParameter)));

            return Expression.Lambda<Func<TEntity, TreeItem<TEntity>>>(
                Expression.MemberInit(ctorTreeItem, entityPropertyAssigment, childrenCountAssigment), mainParameter);
        }

        private static MemberExpression ExpandNullableProperty(Expression parameter, string propertyName,
            out Type valueType)
        {
            var property = Expression.Property(parameter, propertyName);
            valueType = Nullable.GetUnderlyingType(property.Type) != null
                ? typeof(Nullable<>).MakeGenericType(typeof(TKey))
                : typeof(TKey);

            return property;
        }

        /// <summary>
        /// Returns parent predicate usable in GetTree method
        /// </summary>
        /// <returns>Predicate for matching parent id for the entity</returns>
        protected virtual Expression<Func<TEntity, bool>> ParentPredicate(object parentId)
        {
            var parameter = Expression.Parameter(typeof(TEntity), "entity");
            var parentIdProperty = ExpandNullableProperty(parameter, "ParentId", out var valueType);
            // FIXME: 
//            valueType.IsAssignableFrom(parentId)
            var rightValue = Expression.Constant(parentId, valueType);
            var predicateExpression = Expression.Equal(parentIdProperty, rightValue);
            return Expression.Lambda<Func<TEntity, bool>>(predicateExpression, parameter);
        }

        /// <summary>
        /// Returns parent predicate usable in children count operation.
        /// </summary>
        /// <param name="parentParameter">Parent parameter</param>
        /// <returns></returns>
        protected virtual Expression<Func<TEntity, bool>> ParentPredicateCount(ParameterExpression parentParameter)
        {
            var parameter = Expression.Parameter(typeof(TEntity), "childrenEntity");
            var parentIdProperty = ExpandNullableProperty(parameter, "ParentId", out var valueType);

            var predicateExpression =
                Expression.Equal(parentIdProperty,
                    Expression.Convert(Expression.Property(parentParameter, "Id"), valueType));
            return Expression.Lambda<Func<TEntity, bool>>(predicateExpression, parameter);
        }

        /// <inheritdoc />
        public virtual async Task<PagedList<TreeItem<TDto>>> GetTree(TreeRequest request)
        {
            var parentPredicate = ParentPredicate(request.ParentId);

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


//        protected override async Task<Expression<Func<TestTreeItem, bool>>> ParentPredicate(object parentId)
//#pragma warning restore 1998
//        {
//            if (parentId == null)
//            {
//                return entity => entity.ParentId == null;
//            }
//
//            var converted = Convert.ToInt32(parentId);
//            return entity => entity.ParentId == converted;
//        }
//
    }
}
