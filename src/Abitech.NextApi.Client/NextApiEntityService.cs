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
    /// <typeparam name="TClient"></typeparam>
    public abstract class NextApiEntityService<TEntity, TKey, TClient> : NextApiService<TClient>,
        INextApiEntityService<TEntity, TKey>
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


        /// <inheritdoc />
        public async Task<TEntity[]> GetByIds(TKey[] keys, string[] expand = null)
        {
            return await InvokeService<TEntity[]>("GetByIds", new NextApiArgument()
                {
                    Name = "keys",
                    Value = keys
                },
                new NextApiArgument()
                {
                    Name = "expand",
                    Value = expand
                });
        }
    }
}
