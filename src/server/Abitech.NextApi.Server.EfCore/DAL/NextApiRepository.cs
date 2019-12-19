using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Abitech.NextApi.Common.Entity;
using Abitech.NextApi.Server.EfCore.Entity;
using Abitech.NextApi.Server.Entity;
using Microsoft.EntityFrameworkCore;

namespace Abitech.NextApi.Server.EfCore.DAL
{
    /// <summary>
    /// Basic implementation of entity repository
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <typeparam name="TKey">Entity key type</typeparam>
    /// <typeparam name="TDbContext">DbContext type</typeparam>
    public abstract class NextApiRepository<T, TKey, TDbContext> : INextApiRepository<T, TKey>
        where T : class, IEntity<TKey>
        where TDbContext : class, INextApiDbContext
    {
        private readonly TDbContext _context;
        private readonly DbSet<T> _dbset;
        private readonly bool _isSoftDeleteSupported;

        /// <summary>
        /// Indicates that soft-delete enabled for this repo
        /// </summary>
        protected bool SoftDeleteEnabled { get; set; } = true;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbContext"></param>
        protected NextApiRepository(TDbContext dbContext)
        {
            _context = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _dbset = _context.Set<T>();
            _isSoftDeleteSupported = typeof(ISoftDeletableEntity).IsAssignableFrom(typeof(T));
        }

        /// <summary>
        /// Adds entity to dbset
        /// </summary>
        /// <param name="entity">entity instance</param>
        /// <returns></returns>
        public virtual async Task AddAsync(T entity) => await _dbset.AddAsync(entity);

        /// <summary>
        /// Updates entity by instance
        /// </summary>
        /// <param name="entity">entity instance</param>
#pragma warning disable 1998
        public virtual async Task UpdateAsync(T entity)
#pragma warning restore 1998
        {
            AttachIfDetached(entity);
            _context.Entry(entity).State = EntityState.Modified;
        }

        private void AttachIfDetached(T entity)
        {
            // find local copy if exists and detach it
            var entityId = entity.Id;
            var local = _dbset.Local.FirstOrDefault(e => e.Id.Equals(entityId));
            // return if instances is same (in local dbset)
            if (local != null && ReferenceEquals(entity, local))
            {
                return;
            }

            // first detach old instance from local db set
            if (local != null)
            {
                _context.Entry(local).State = EntityState.Detached;
            }

            // attach provided instance
            _dbset.Attach(entity);
        }

        /// <summary>
        /// Deletes item by entity instance
        /// </summary>
        /// <param name="entity">entity instance</param>
        public virtual async Task DeleteAsync(T entity)
        {
            if (_isSoftDeleteSupported && SoftDeleteEnabled)
            {
                ((ISoftDeletableEntity)entity).IsRemoved = true;
                await UpdateAsync(entity);
            }
            else
            {
                AttachIfDetached(entity);
                _dbset.Remove(entity);
            }
        }

        /// <summary>
        /// Deletes items by condition
        /// </summary>
        /// <param name="where">delete condition</param>
        public virtual async Task DeleteAsync(Expression<Func<T, bool>> where)
        {
            var objects = GetAll().Where(where).AsEnumerable();
            foreach (var obj in objects)
                await DeleteAsync(obj);
        }

        /// <summary>
        /// Returns entity by id
        /// </summary>
        /// <param name="id">id</param>
        /// <returns>entity</returns>
        public virtual async Task<T> GetByIdAsync(TKey id) =>
            await GetAll().FirstOrDefaultAsync(item => item.Id.Equals(id));

        /// <summary>
        /// Returns entities by ids
        /// </summary>
        /// <param name="ids">ids</param>
        /// <returns></returns>
        public virtual async Task<T[]> GetByIdsAsync(TKey[] ids) =>
            await GetAll().Where(items => ids.Contains(items.Id)).ToArrayAsync();


        /// <summary>
        /// Returns first or default item by query
        /// </summary>
        /// <param name="query">Query instance</param>
        /// <returns>Entity instance</returns>
        public Task<T> FirstOrDefaultAsync(IQueryable<T> query) => query.FirstOrDefaultAsync();

        /// <summary>
        /// Returns results as list by query
        /// </summary>
        /// <param name="query">Query instance</param>
        /// <returns>Entities as list</returns>
        public Task<List<TRequestItem>> ToListAsync<TRequestItem>(IQueryable<TRequestItem> query) =>
            query.ToListAsync();

        /// <summary>
        /// Returns results as array by query
        /// </summary>
        /// <param name="query">Query instance</param>
        /// <returns>Entities as array</returns>
        public Task<TRequestItem[]> ToArrayAsync<TRequestItem>(IQueryable<TRequestItem> query) => query.ToArrayAsync();

        /// <summary>
        /// Returns count of entities by query
        /// </summary>
        /// <param name="query">Query instance</param>
        /// <returns>Count of entities</returns>
        public Task<int> CountAsync<TRequestItem>(IQueryable<TRequestItem> query) => query.CountAsync();

        /// <summary>
        /// Returns true if set contains entities that matched by query and filter expression
        /// </summary>
        /// <param name="query">Query instance</param>
        /// <param name="filterExpression">Filter expression</param>
        /// <remarks>If filterExpression null, just checks that set has any element by query</remarks>
        /// <returns></returns>
        public Task<bool> AnyAsync<TRequestItem>(IQueryable<TRequestItem> query,
            Expression<Func<TRequestItem, bool>> filterExpression = null) =>
            filterExpression != null ? query.AnyAsync(filterExpression) : query.AnyAsync();

        public IQueryable<T> Expand(IQueryable<T> source, string[] expand) =>
            expand == null
                ? source
                : expand.Aggregate(source, (current, expandNode) => current.Include(expandNode));

        /// <summary>
        /// Returns all entities
        /// </summary>
        /// <returns></returns>
        public virtual IQueryable<T> GetAll()
        {
            var query = _dbset.AsQueryable();
            if (_isSoftDeleteSupported && SoftDeleteEnabled)
            {
                query = query.Where(i => !((ISoftDeletableEntity)i).IsRemoved);
            }

            return query.AsNoTracking();
        }


        /// <summary>
        /// Returns entity using where expression
        /// </summary>
        /// <param name="where">Filter expression</param>
        /// <returns>Filtered entity with includes</returns>
        public async Task<T> GetAsync(Expression<Func<T, bool>> where) => await GetAll().FirstOrDefaultAsync<T>(@where);

        public async Task AddAsync(object entity) => await AddAsync((T)entity);

        public async Task UpdateAsync(object entity) => await UpdateAsync((T)entity);

        public async Task DeleteAsync(object entity) => await DeleteAsync((T)entity);

        public async Task<object> GetByIdAsync(object id) => await GetByIdAsync((TKey)id);
    }
}
