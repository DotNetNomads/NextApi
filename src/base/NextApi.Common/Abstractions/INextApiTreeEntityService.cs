using System.Threading.Tasks;
using NextApi.Common.DTO;
using NextApi.Common.Paged;
using NextApi.Common.Tree;

namespace NextApi.Common.Abstractions
{
    /// <summary>
    /// Basic interface for entity service of Tree-type entities
    /// </summary>
    /// <typeparam name="TDto"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TParentKey"></typeparam>
    public interface INextApiTreeEntityService<TDto, TKey, TParentKey> : INextApiEntityService<TDto, TKey>
        where TDto : class, ITreeEntityDto<TKey, TParentKey>
    {
        /// <summary>
        /// Get entity tree by specific request
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<PagedList<TreeItem<TDto>>> GetTree(TreeRequest<TParentKey> request);
    }
}
