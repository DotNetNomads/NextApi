using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using NextApi.Common.Abstractions;
using NextApi.Common.Abstractions.DAL;
using NextApi.Common.DTO;
using NextApi.Common.Paged;
using NextApi.Common.Tree;

namespace NextApi.Server.Entity
{
    /// <summary>
    /// Basic realization for NextApi entity service of Tree-type entities
    /// </summary>
    /// <typeparam name="TDto"></typeparam>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TParentKey"></typeparam>
    public class NextApiTreeEntityService<TDto, TEntity, TKey, TParentKey> : NextApiEntityService<TDto, TEntity, TKey>,
        INextApiTreeEntityService<TDto, TKey, TParentKey>
        where TDto : class, ITreeEntityDto<TKey, TParentKey> where TEntity : class, ITreeEntity<TKey, TParentKey>
    {
        private readonly IMapper _mapper;
        private readonly IRepo<TEntity, TKey> _repository;

        /// <inheritdoc />
        public NextApiTreeEntityService(IUnitOfWork unitOfWork, IMapper mapper, IRepo<TEntity, TKey> repository) :
            base(unitOfWork, mapper, repository)
        {
            _mapper = mapper;
            _repository = repository;
        }

        /// <inheritdoc />
        public virtual async Task<PagedList<TreeItem<TDto>>> GetTree(TreeRequest<TParentKey> request)
        {
            var entitiesQuery = _repository.GetAll();
            entitiesQuery = await BeforeGet(entitiesQuery);
            
            var rootQuery = entitiesQuery.Where(e=> e.ParentId.Equals(request.ParentId));
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
                .Select(e => new {Entity = e, ChildrenCount = entitiesQuery.Count(ce => e.Id.Equals(ce.ParentId))}));
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
    }
}
