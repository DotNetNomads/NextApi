using System.Linq;
using System.Threading.Tasks;
using NextApi.Common;
using NextApi.Common.Abstractions;
using NextApi.Common.DTO;
using NextApi.Common.Filtering;
using NextApi.Common.Paged;

namespace NextApi.Client
{
    /// <summary>
    /// Basic implementation of NextApi Entity service client
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TClient"></typeparam>
    public abstract class NextApiEntityService<TEntity, TKey, TClient> : NextApiService<TClient>,
        INextApiEntityService<TEntity, TKey>, INextApiEntityService
        where TEntity : class, IEntityDto<TKey>
        where TClient : class, INextApiClient
    {
        /// <summary>
        /// Initializes client for entity service
        /// </summary>
        /// <param name="client"></param>
        /// <param name="serviceName"></param>
        protected NextApiEntityService(TClient client, string serviceName) : base(client, serviceName)
        {
        }

        /// <summary>
        /// Returns NextApi argument for key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        protected NextApiArgument KeyArgument(TKey key) => new NextApiArgument {Name = "key", Value = key};

        /// <inheritdoc />
        public async Task<TEntity> Create(TEntity entity) =>
            await InvokeService<TEntity>("Create", new NextApiArgument {Name = "entity", Value = entity});

        /// <inheritdoc />
        public async Task Delete(TKey key) => await InvokeService("Delete", KeyArgument(key));

        /// <inheritdoc />
        public async Task<TEntity> Update(TKey key, TEntity patch) =>
            await InvokeService<TEntity>(
                "Update",
                KeyArgument(key),
                new NextApiArgument {Name = "patch", Value = patch}
            );

        /// <inheritdoc />
        public async Task<object> Create(object entity) => await Create((TEntity)entity);

        /// <inheritdoc />
        public async Task Delete(object key) => await Delete((TKey)key);

        /// <inheritdoc />
        public async Task<object> Update(object key, object patch) => await Update((TKey)key, (TEntity)patch);

        /// <inheritdoc />
        public async Task<object> GetByIdNonGeneric(object key, string[] expand = null) =>
            await GetById((TKey)key, expand);

        /// <inheritdoc />
        public async Task<object[]> GetByIdsNonGeneric(object[] keys, string[] expand = null) =>
            await GetByIds(keys.Cast<TKey>().ToArray(), expand);

        /// <inheritdoc />
        public async Task<PagedList<TEntity>> GetPaged(PagedRequest request) =>
            await InvokeService<PagedList<TEntity>>("GetPaged",
                new NextApiArgument() {Name = "request", Value = request});


        /// <inheritdoc />
        public async Task<TEntity> GetById(TKey key, string[] expand = null) =>
            await InvokeService<TEntity>("GetById", KeyArgument(key),
                new NextApiArgument() {Name = "expand", Value = expand});


        /// <inheritdoc />
        public async Task<TEntity[]> GetByIds(TKey[] keys, string[] expand = null) =>
            await InvokeService<TEntity[]>("GetByIds", new NextApiArgument() {Name = "keys", Value = keys},
                new NextApiArgument() {Name = "expand", Value = expand});

        /// <inheritdoc />
        public async Task<int> Count(Filter filter = null) =>
            await InvokeService<int>(nameof(Count), new NextApiArgument(nameof(filter), filter));

        /// <inheritdoc />
        public async Task<bool> Any(Filter filter = null) =>
            await InvokeService<bool>(nameof(Any), new NextApiArgument(nameof(filter), filter));

        async Task<object[]> INextApiEntityService.GetIdsByFilter(Filter filter)
        {
            return (await GetIdsByFilter(filter))
                .Cast<object>()
                .ToArray();
        }

        /// <inheritdoc />
        public async Task<TKey[]> GetIdsByFilter(Filter filter = null) =>
            await InvokeService<TKey[]>(nameof(GetIdsByFilter), new NextApiArgument(nameof(filter), filter));
    }

    /// <summary>
    /// Non-generic variant for entity service
    /// </summary>
    public interface INextApiEntityService
    {
        /// <summary>
        /// Creates an entity from dto and saves it
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task<object> Create(object entity);

        /// <summary>
        /// Deletes entity by key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Task Delete(object key);

        /// <summary>
        /// Patches an entity and returns instance mapped to DTO
        /// </summary>
        /// <param name="key">entity id</param>
        /// <param name="patch">patch entity</param>
        /// <returns></returns>
        Task<object> Update(object key, object patch);

        /// <summary>
        /// Gets entity by its Id
        /// </summary>
        /// <param name="key">entity id</param>
        /// <param name="expand"></param>
        /// <returns></returns>
        Task<object> GetByIdNonGeneric(object key, string[] expand = null);

        /// <summary>
        /// Gets entities by their Ids
        /// </summary>
        /// <param name="key">entity id</param>
        /// <param name="expand"></param>
        /// <returns></returns>
        Task<object[]> GetByIdsNonGeneric(object[] key, string[] expand = null);

#pragma warning disable 1591
        Task<int> Count(Filter filter = null);

        Task<bool> Any(Filter filter = null);

        Task<object[]> GetIdsByFilter(Filter filter = null);
#pragma warning restore 1591
    }
}
