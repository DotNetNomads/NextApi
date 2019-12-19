using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Abitech.NextApi.Common.Entity;

namespace Abitech.NextApi.Server.Entity
{
    /// <summary>
    /// Base interface for entity repo
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <typeparam name="TKey">Entity key type</typeparam>
    public interface INextApiRepository<T, TKey> : INextApiRepository where T : class, IEntity<TKey>
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

        /// <summary>
        /// Returns first or default item by query
        /// </summary>
        /// <param name="query">Query instance</param>
        /// <returns>Entity instance</returns>
        Task<T> FirstOrDefaultAsync(IQueryable<T> query);

        /// <summary>
        /// Returns results as list by query
        /// </summary>
        /// <param name="query">Query instance</param>
        /// <returns>Entities as list</returns>
        Task<List<TResult>> ToListAsync<TResult>(IQueryable<TResult> query);

        /// <summary>
        /// Returns results as array by query
        /// </summary>
        /// <param name="query">Query instance</param>
        /// <returns>Entities as array</returns>
        Task<TRequestItem[]> ToArrayAsync<TRequestItem>(IQueryable<TRequestItem> query);

        /// <summary>
        /// Returns count of entities by query
        /// </summary>
        /// <param name="query">Query instance</param>
        /// <returns>Count of entities</returns>
        Task<int> CountAsync<TRequestItem>(IQueryable<TRequestItem> query);

        /// <summary>
        /// Returns true if set contains entities that matched by query and filter expression
        /// </summary>
        /// <param name="query">Query instance</param>
        /// <param name="filterExpression">Filter expression</param>
        /// <remarks>If filterExpression null, just checks that set has any element by query</remarks>
        /// <returns></returns>
        Task<bool> AnyAsync<TRequestItem>(IQueryable<TRequestItem> query, Expression<Func<TRequestItem, bool>> filterExpression = null);

        /// <summary>
        /// Expand entity from expand string (aka Include)
        /// </summary>
        /// <param name="source">Entity source</param>
        /// <param name="expand">Expand string array</param>
        IQueryable<T> Expand(IQueryable<T> source, string[] expand);
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
        /// Returns entity by id
        /// </summary>
        /// <param name="id">id</param>
        /// <returns>entity</returns>
        Task<object> GetByIdAsync(object id);
    }
}
