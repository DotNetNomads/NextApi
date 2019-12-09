using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Abitech.NextApi.Server.Entity
{
    /// <summary>
    /// Base interface for entity repo
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <typeparam name="TKey">Entity key type</typeparam>
    public interface INextApiRepository<T, TKey> : INextApiRepository where T : class
    {
        /// <summary>
        /// Adds entity to dbset
        /// </summary>
        /// <param name="entity">entity instance</param>
        /// <returns></returns>
        Task AddAsync(T entity);
        /// <summary>
        /// Updates entity by instance
        /// </summary>
        /// <param name="entity">entity instance</param>
        Task UpdateAsync(T entity);
        /// <summary>
        /// Deletes item by entity instance
        /// </summary>
        /// <param name="entity">entity instance</param>
        Task DeleteAsync(T entity);
        /// <summary>
        /// Deletes items by condition
        /// </summary>
        /// <param name="where">delete condition</param>
        Task DeleteAsync(Expression<Func<T, bool>> where);
        /// <summary>
        /// Returns entity by id
        /// </summary>
        /// <param name="id">id</param>
        /// <returns>entity</returns>
        Task<T> GetByIdAsync(TKey id);

        /// <summary>
        /// Returns entity using where expression
        /// </summary>
        /// <param name="where">Filter expression</param>
        /// <returns>Filtered entity with includes</returns>
        Task<T> GetAsync(Expression<Func<T, bool>> where);
        /// <summary>
        /// Returns all entities
        /// </summary>
        /// <returns></returns>
        IQueryable<T> GetAll();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        Task<T[]> GetByIdsAsync(TKey[] ids);
    }
    
    /// <summary>
    /// Base interface for INextApiRepository repo (used generally in non-generic code)
    /// </summary>
    public interface INextApiRepository
    {
        /// <summary>
        /// Adds entity to dbset
        /// </summary>
        /// <param name="entity">entity instance as object</param>
        /// <returns></returns>
        Task AddAsync(object entity);
        /// <summary>
        /// Updates entity by instance
        /// </summary>
        /// <param name="entity">entity instance as object</param>
        Task UpdateAsync(object entity);
        /// <summary>
        /// Deletes item by entity instance
        /// </summary>
        /// <param name="entity">entity instance as object</param>
        Task DeleteAsync(object entity);
        /// <summary>
        /// Get entity by row guid
        /// </summary>
        /// <param name="rowGuid">Entity row guid</param>
        Task<object> GetByRowGuid(Guid rowGuid);
    }
}
