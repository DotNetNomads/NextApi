using System;
using System.Threading.Tasks;
using Abitech.NextApi.Common.Entity;

namespace Abitech.NextApi.Server.UploadQueue.ChangeTracking
{
    /// <inheritdoc />
    public abstract class UploadQueueChangesHandler<TEntity> : IUploadQueueChangesHandler<TEntity>
        where TEntity : class, IEntity<Guid>
    {
#pragma warning disable 1998
        /// <inheritdoc />
        public virtual async Task OnBeforeDelete(TEntity entity)
#pragma warning restore 1998
        {
        }

#pragma warning disable 1998
        /// <inheritdoc />
        public virtual async Task OnBeforeUpdate(TEntity originalEntity, string columnName, object newValue)
#pragma warning restore 1998
        {
        }

#pragma warning disable 1998
        /// <inheritdoc />
        public virtual async Task OnBeforeCreate(TEntity entityToCreate)
#pragma warning restore 1998
        {
        }

#pragma warning disable 1998
        /// <inheritdoc />
        public virtual async Task OnAfterDelete(TEntity entity)
#pragma warning restore 1998
        {
        }

#pragma warning disable 1998
        /// <inheritdoc />
        public virtual async Task OnAfterUpdate(TEntity originalEntity, string columnName, object newValue)
#pragma warning restore 1998
        {
        }

#pragma warning disable 1998
        /// <inheritdoc />
        public virtual async Task OnAfterCreate(TEntity entity)
#pragma warning restore 1998
        {
        }

        /// <inheritdoc />
        public Task OnBeforeDelete(object entity)
        {
            return OnBeforeDelete((TEntity)entity);
        }

        /// <inheritdoc />
        public Task OnBeforeUpdate(object originalEntity, string columnName, object newValue)
        {
            return OnBeforeUpdate((TEntity)originalEntity, columnName, newValue);
        }

        /// <inheritdoc />
        public Task OnBeforeCreate(object entityToCreate)
        {
            return OnBeforeCreate((TEntity)entityToCreate);
        }

        /// <inheritdoc />
        public Task OnAfterDelete(object entity)
        {
            return OnAfterDelete((TEntity)entity);
        }

        /// <inheritdoc />
        public Task OnAfterUpdate(object updatedEntity, string columnName, object newValue)
        {
            return OnAfterUpdate((TEntity)updatedEntity, columnName, newValue);
        }

        /// <inheritdoc />
        public Task OnAfterCreate(object entity)
        {
            return OnAfterCreate((TEntity)entity);
        }

#pragma warning disable 1998
        /// <inheritdoc />
        public virtual async Task OnCommit()
#pragma warning restore 1998
        {
        }
    }
}
