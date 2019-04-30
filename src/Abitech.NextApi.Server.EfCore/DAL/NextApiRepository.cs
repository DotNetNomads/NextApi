using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Abitech.NextApi.Server.EfCore.Model.Base;
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
        where T : class
        where TDbContext : class, INextApiDbContext
    {
        private readonly TDbContext _context;
        private readonly DbSet<T> _dbset;
        private readonly bool _isIEntity;
        private readonly bool _isSoftDeleteSupported;
        private readonly bool _isRowGuidSupported;

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
            _isIEntity = typeof(IEntity<TKey>).IsAssignableFrom(typeof(T));
            _isSoftDeleteSupported = typeof(ISoftDeletableEntity).IsAssignableFrom(typeof(T));
            _isRowGuidSupported = typeof(IRowGuidEnabled).IsAssignableFrom(typeof(T));
        }

        /// <summary>
        /// Adds entity to dbset
        /// </summary>
        /// <param name="entity">entity instance</param>
        /// <returns></returns>
        public virtual async Task AddAsync(T entity)
        {
            await _dbset.AddAsync(entity);
        }

        /// <summary>
        /// Updates entity by instance
        /// </summary>
        /// <param name="entity">entity instance</param>
        public virtual async Task UpdateAsync(T entity)
        {
            // find local copy if exists and detach it
            var entityId = GetEntityId(entity);
            var local = _dbset.Local.FirstOrDefault(KeyPredicate(entityId).Compile());
            if (local != null)
            {
                _context.Entry(local).State = EntityState.Detached;
            }

            // attach updated entity
            _dbset.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
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
                _dbset.Remove(entity);
            }
        }

        /// <summary>
        /// Deletes items by condition
        /// </summary>
        /// <param name="where">delete condition</param>
        public virtual async Task DeleteAsync(Expression<Func<T, bool>> where)
        {
            var objects = _dbset.Where(where).AsEnumerable();
            foreach (var obj in objects)
                await DeleteAsync(obj);
        }

        /// <summary>
        /// Returns entity by id
        /// </summary>
        /// <param name="id">id</param>
        /// <returns>entity</returns>
        public virtual async Task<T> GetByIdAsync(TKey id)
        {
            return await GetAll().FirstOrDefaultAsync(KeyPredicate(id));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public virtual async Task<T[]> GetByIdsAsync(TKey[] ids)
        {
            return await GetAll().Where(KeyPredicate(ids)).ToArrayAsync();
        }

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
        /// Given a entity and id, return true if entity's key property equals id.
        /// </summary>
        /// <param name="id"></param>
        /// <exception cref="NotSupportedException">In case entity doesn't implements IEntity</exception>
        /// <returns></returns>
        public virtual Expression<Func<T, bool>> KeyPredicate(TKey id)
        {
            if (!_isIEntity)
            {
                throw new NotSupportedException(
                    "Default implementation of KeyPredicate method supports only entities that implements IEntity<TKey>." +
                    "Override KeyPredicate for your entity type.");
            }

            return entity => (entity as IEntity<TKey>).Id.Equals(id);
        }

        /// <summary>
        /// Given a entity, returns id value of entity
        /// </summary>
        /// <param name="entity"></param>
        /// <returns>Entity id</returns>
        /// <exception cref="NotSupportedException">In case entity doesn't implements IEntity</exception>
        public virtual TKey GetEntityId(T entity)
        {
            if (!_isIEntity)
            {
                throw new NotSupportedException(
                    "Default implementation of GetEntityId method supports only entity that implements IEntity<TKey>." +
                    "Override GetEntityId for your entity type.");
            }

            return ((IEntity<TKey>)entity).Id;
        }


        /// <inheritdoc />
        public virtual Expression<Func<T, bool>> KeyPredicate(TKey[] keys)
        {
            if (!_isIEntity)
            {
                throw new NotSupportedException(
                    "Default implementation of KeyPredicate method supports only entities that implements IEntity<TKey>." +
                    "Override KeyPredicate for your entity type.");
            }

            return entity => keys.Contains((entity as IEntity<TKey>).Id);
        }

        /// <summary>
        /// Returns entity using where expression
        /// </summary>
        /// <param name="where">Filter expression</param>
        /// <returns>Filtered entity with includes</returns>
        public async Task<T> GetAsync(Expression<Func<T, bool>> where)
        {
            return await GetAll().FirstOrDefaultAsync<T>(where);
        }

        public async Task AddAsync(object entity)
        {
            await AddAsync((T)entity);
        }

        public async Task UpdateAsync(object entity)
        {
            await UpdateAsync((T)entity);
        }

        public async Task DeleteAsync(object entity)
        {
            await DeleteAsync((T)entity);
        }

        public async Task<object> GetByRowGuid(Guid rowGuid)
        {
            if (!_isRowGuidSupported)
                throw new Exception("RowGuid is not supported");

            return await GetAsync(arg => ((IRowGuidEnabled)arg).RowGuid == rowGuid);
        }
    }
}
