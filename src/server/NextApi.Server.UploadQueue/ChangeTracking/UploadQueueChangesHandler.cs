using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NextApi.Common.Entity;
using NextApi.UploadQueue.Common.UploadQueue;

namespace NextApi.Server.UploadQueue.ChangeTracking
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
        public virtual async Task OnBeforeUpdate(TEntity originalEntity, IList<UploadQueueDto> updateList)
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
        public virtual async Task OnAfterUpdate(TEntity originalEntity)
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
        public Task OnBeforeUpdate(object originalEntity, IList<UploadQueueDto> updateList)
        {
            return OnBeforeUpdate((TEntity)originalEntity, updateList);
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
        public Task OnAfterUpdate(object updatedEntity)
        {
            return OnAfterUpdate((TEntity)updatedEntity);
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
