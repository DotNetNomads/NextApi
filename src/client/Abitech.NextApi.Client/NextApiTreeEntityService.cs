using System.Threading.Tasks;
using Abitech.NextApi.Common;
using Abitech.NextApi.Common.Abstractions;
using Abitech.NextApi.Common.DTO;
using Abitech.NextApi.Common.Paged;
using Abitech.NextApi.Common.Tree;

namespace Abitech.NextApi.Client
{
    /// <summary>
    /// Basic implementation of NextApi tree entity service client
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TClient"></typeparam>
    public abstract class
        NextApiTreeEntityService<TEntity, TKey, TClient> : NextApiEntityService<TEntity, TKey, TClient>,
            INextApiTreeEntityService<TEntity, TKey>
        where TClient : class, INextApiClient where TEntity : class, IEntityDto<TKey>
    {
        /// <inheritdoc />
        protected NextApiTreeEntityService(TClient client, string serviceName) : base(client, serviceName)
        {
        }

        /// <inheritdoc />
        public async Task<PagedList<TreeItem<TEntity>>> GetTree(TreeRequest request) =>
            await InvokeService<PagedList<TreeItem<TEntity>>>(nameof(GetTree),
                new NextApiArgument(nameof(request), request));
    }
}
