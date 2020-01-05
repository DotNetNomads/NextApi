using System.Threading.Tasks;
using Abitech.NextApi.Common.DTO;
using Abitech.NextApi.Common.Paged;
using Abitech.NextApi.Common.Tree;

namespace Abitech.NextApi.Common.Abstractions
{
    /// <summary>
    /// Basic interface for entity service of Tree-type entities
    /// </summary>
    /// <typeparam name="TDto"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    public interface INextApiTreeEntityService<TDto, TKey> : INextApiEntityService<TDto, TKey> where TDto : class, IEntityDto<TKey>
    {
        /// <summary>
        /// Get entity tree by specific request
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<PagedList<TreeItem<TDto>>> GetTree(TreeRequest request);
    }
}
