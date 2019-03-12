using System.Threading.Tasks;
using Abitech.NextApi.Model;
using Abitech.NextApi.Model.Abstractions;
using Abitech.NextApi.Model.Paged;

namespace Abitech.NextApi.Client
{
    /// <summary>
    /// Basic implementation of NextApi Entity service client
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    public class NextApiEntityService<TEntity, TKey> : INextApiEntityService<TEntity, TKey>
        where TEntity : class
    {
        /// <summary>
        /// NextApi client
        /// </summary>
        /// <returns></returns>
        private readonly INextApiClient _nextApiClient;

        /// <summary>
        /// Service name
        /// </summary>
        private readonly string _serviceName;

        /// <summary>
        /// Initializes client for entity service
        /// </summary>
        /// <param name="nextApiClient"></param>
        /// <param name="serviceName"></param>
        public NextApiEntityService(NextApiClient nextApiClient, string serviceName)
        {
            _nextApiClient = nextApiClient;
            _serviceName = serviceName;
        }

        /// <summary>
        /// Invoke service method
        /// </summary>
        /// <param name="method">Method name</param>
        /// <param name="arguments">Method arguments</param>
        /// <typeparam name="T">Execution result type</typeparam>
        /// <returns>Execution result</returns>
        protected async Task<T> InvokeService<T>(string method, params NextApiArgument[] arguments)
        {
            return await _nextApiClient.Invoke<T>(_serviceName, method, arguments);
        }

        /// <summary>
        /// Invoke service method
        /// </summary>
        /// <param name="method">Method name</param>
        /// <param name="arguments">Method arguments</param>
        protected async Task InvokeService(string method, params NextApiArgument[] arguments)
        {
            await _nextApiClient.Invoke(_serviceName, method, arguments);
        }

        /// <summary>
        /// Returns NextApi argument for key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        protected NextApiArgument KeyArgument(TKey key)
        {
            return new NextApiArgument
            {
                Name = "key",
                Value = key
            };
        }

        /// <inheritdoc />
        public async Task<TEntity> Create(TEntity entity)
        {
            return await InvokeService<TEntity>("Create", new NextApiArgument
            {
                Name = "entity",
                Value = entity
            });
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
                new NextApiArgument
                {
                    Name = "patch",
                    Value = patch
                }
            );
        }

        /// <inheritdoc />
        public async Task<PagedList<TEntity>> GetPaged(PagedRequest request)
        {
            return await InvokeService<PagedList<TEntity>>("GetPaged", new NextApiArgument()
            {
                Name = "request",
                Value = request
            });
        }

        /// <inheritdoc />
        public async Task<TEntity> GetById(TKey key, string[] expand = null)
        {
            return await InvokeService<TEntity>("GetById", KeyArgument(key),
                new NextApiArgument()
                {
                    Name = "expand",
                    Value = expand
                });
        }
    }
}
