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
    public interface INextApiRepository<T, TKey> where T : class
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
        void Update(T entity);
        /// <summary>
        /// Deletes item by entity instance
        /// </summary>
        /// <param name="entity">entity instance</param>
        void Delete(T entity);
        /// <summary>
        /// Deletes items by condition
        /// </summary>
        /// <param name="where">delete condition</param>
        void Delete(Expression<Func<T, bool>> where);
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
        /// Given a entity and id, return true if entity's key property equals id.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Expression<Func<T, bool>> KeyPredicate(TKey key);
        /// <summary>
        /// Given a entity and ids, return true if entity's key property contains in current array.
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        Expression<Func<T, bool>> KeyPredicate(TKey[] keys);
    }
}
