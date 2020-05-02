using System.Threading.Tasks;
using NextApi.Common;
using NextApi.Common.Abstractions;
using NextApi.Common.DTO;
using NextApi.Common.Paged;
using NextApi.Common.Tree;

namespace NextApi.Client
{
    /// <summary>
    /// Basic implementation of NextApi tree entity service client
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TClient"></typeparam>
    /// <typeparam name="TParentKey"></typeparam>
    public abstract class
        NextApiTreeEntityService<TEntity, TKey, TParentKey, TClient> : NextApiEntityService<TEntity, TKey, TClient>,
            INextApiTreeEntityService<TEntity, TKey, TParentKey>
        where TClient : class, INextApiClient where TEntity : class, ITreeEntityDto<TKey, TParentKey>
    {
        /// <inheritdoc />
        protected NextApiTreeEntityService(TClient client, string serviceName) : base(client, serviceName)
        {
        }

        /// <inheritdoc />
        public async Task<PagedList<TreeItem<TEntity>>> GetTree(TreeRequest<TParentKey> request) =>
            await InvokeService<PagedList<TreeItem<TEntity>>>(nameof(GetTree),
                new NextApiArgument(nameof(request), request));
    }
}
