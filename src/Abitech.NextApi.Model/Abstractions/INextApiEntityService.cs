using System.Threading.Tasks;
using Abitech.NextApi.Model.Filtering;
using Abitech.NextApi.Model.Paged;

namespace Abitech.NextApi.Model.Abstractions
{
    /// <summary>
    /// Basic interface of entity service
    /// </summary>
    /// <typeparam name="TDto"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    public interface INextApiEntityService<TDto, TKey> where TDto : class
    {
        /// <summary>
        /// Creates an entity from dto and saves it
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task<TDto> Create(TDto entity);

        /// <summary>
        /// Deletes entity by key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Task Delete(TKey key);

        /// <summary>
        /// Patches an entity and returns instance mapped to DTO
        /// </summary>
        /// <param name="key">entity id</param>
        /// <param name="patch">patch entity</param>
        /// <returns></returns>
        Task<TDto> Update(TKey key, TDto patch);

        /// <summary>
        /// Get entities by paged request
        /// </summary>
        /// <param name="request">Request data</param>
        /// <returns>Paged list of entities</returns>
        Task<PagedList<TDto>> GetPaged(PagedRequest request);

        /// <summary>
        /// Get entity by id
        /// </summary>
        /// <param name="key">entity id</param>
        /// <param name="expand">expand references aka Include</param>
        /// <returns>entity</returns>
        Task<TDto> GetById(TKey key, string[] expand = null);

        /// <summary>
        /// Get array of entities by id
        /// </summary>
        /// <param name="keys">array of entity ids</param>
        /// <param name="expand">expand references aka Include</param>
        /// <returns>array of entities</returns>
        Task<TDto[]> GetByIds(TKey[] keys, string[] expand = null);

        /// <summary>
        /// Get count of items by filter
        /// </summary>
        /// <param name="filter">Filter object</param>
        /// <returns>Count of items</returns>
        /// <remarks>When filter is null, all items count returned</remarks>
        Task<int> Count(Filter filter = null);
    }
}
