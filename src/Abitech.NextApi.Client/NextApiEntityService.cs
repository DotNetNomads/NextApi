using System.Linq;
using System.Threading.Tasks;
using Abitech.NextApi.Model;
using Abitech.NextApi.Model.Abstractions;
using Abitech.NextApi.Model.Filtering;
using Abitech.NextApi.Model.Paged;
using Abitech.NextApi.Model.Tree;

namespace Abitech.NextApi.Client
{
    /// <summary>
    /// Basic implementation of NextApi Entity service client
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TClient"></typeparam>
    public abstract class NextApiEntityService<TEntity, TKey, TClient> : NextApiService<TClient>,
        INextApiEntityService<TEntity, TKey>, INextApiEntityService
        where TEntity : class
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
        protected NextApiArgument KeyArgument(TKey key)
        {
            return new NextApiArgument {Name = "key", Value = key};
        }

        /// <inheritdoc />
        public async Task<TEntity> Create(TEntity entity)
        {
            return await InvokeService<TEntity>("Create", new NextApiArgument {Name = "entity", Value = entity});
        }

        /// <inheritdoc />
        public async Task Delete(TKey key)
        {
            await InvokeService("Delete", KeyArgument(key));
        }

        /// <inheritdoc />
        public async Task<TEntity> Update(TKey key, TEntity patch)
        {
            return await InvokeService<TEntity>(
                "Update",
                KeyArgument(key),
                new NextApiArgument {Name = "patch", Value = patch}
            );
        }

        /// <inheritdoc />
        public async Task<object> Create(object entity)
        {
            return await Create((TEntity)entity);
        }

        /// <inheritdoc />
        public async Task Delete(object key)
        {
            await Delete((TKey)key);
        }

        /// <inheritdoc />
        public async Task<object> Update(object key, object patch)
        {
            return await Update((TKey)key, (TEntity)patch);
        }

        /// <inheritdoc />
        public async Task<object> GetByIdNonGeneric(object key, string[] expand = null)
        {
            return await GetById((TKey)key, expand);
        }

        /// <inheritdoc />
        public async Task<object[]> GetByIdsNonGeneric(object[] keys, string[] expand = null)
        {
            return await GetByIds(keys.Cast<TKey>().ToArray(), expand);
        }
        
        /// <inheritdoc />
        public async Task<PagedList<TEntity>> GetPaged(PagedRequest request)
        {
            return await InvokeService<PagedList<TEntity>>("GetPaged",
                new NextApiArgument() {Name = "request", Value = request});
        }


        /// <inheritdoc />
        public async Task<TEntity> GetById(TKey key, string[] expand = null)
        {
            return await InvokeService<TEntity>("GetById", KeyArgument(key),
                new NextApiArgument() {Name = "expand", Value = expand});
        }


        /// <inheritdoc />
        public async Task<TEntity[]> GetByIds(TKey[] keys, string[] expand = null)
        {
            return await InvokeService<TEntity[]>("GetByIds", new NextApiArgument() {Name = "keys", Value = keys},
                new NextApiArgument() {Name = "expand", Value = expand});
        }

        /// <inheritdoc />
        public async Task<int> Count(Filter filter = null)
        {
            return await InvokeService<int>(nameof(Count), new NextApiArgument(nameof(filter), filter));
        }

        public async Task<int[]> GetIdsByFilter(Filter filter = null)
        {
            return await InvokeService<int[]>(nameof(GetIdsByFilter), new NextApiArgument(nameof(filter), filter));
        }

        /// <inheritdoc />
        public async Task<PagedList<TreeItem<TEntity>>> GetTree(TreeRequest request)
        {
            return await InvokeService<PagedList<TreeItem<TEntity>>>(nameof(GetTree),
                new NextApiArgument(nameof(request), request));
        }
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
#pragma warning restore 1591

        Task<int[]> GetIdsByFilter(Filter filter = null);
    }
}
