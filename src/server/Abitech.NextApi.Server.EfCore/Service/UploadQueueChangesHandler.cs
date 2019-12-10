using System.Threading.Tasks;
using Abitech.NextApi.Model.Abstractions;

namespace Abitech.NextApi.Server.EfCore.Service
{
    public abstract class UploadQueueChangesHandler<TEntity> : IUploadQueueChangesHandler<TEntity> 
        where TEntity : class
    {
#pragma warning disable 1998
        public virtual async Task OnBeforeDelete(TEntity entity)
#pragma warning restore 1998
        {
        }

#pragma warning disable 1998
        public virtual async Task OnBeforeUpdate(TEntity originalEntity, string columnName, object newValue)
#pragma warning restore 1998
        {
        }

#pragma warning disable 1998
        public virtual async Task OnBeforeCreate(TEntity entityToCreate)
#pragma warning restore 1998
        {
        }

#pragma warning disable 1998
        public virtual async Task OnAfterDelete(TEntity entity)
#pragma warning restore 1998
        {
        }

#pragma warning disable 1998
        public virtual async Task OnAfterUpdate(TEntity originalEntity, string columnName, object newValue)
#pragma warning restore 1998
        {
        }

#pragma warning disable 1998
        public virtual async Task OnAfterCreate(TEntity entity)
#pragma warning restore 1998
        {
        }

        public Task OnBeforeDelete(object entity)
        {
            return OnBeforeDelete((TEntity) entity);
        }

        public Task OnBeforeUpdate(object originalEntity, string columnName, object newValue)
        {
            return OnBeforeUpdate((TEntity)originalEntity, columnName, newValue);
        }

        public Task OnBeforeCreate(object entityToCreate)
        {
            return OnBeforeCreate((TEntity)entityToCreate);
        }

        public Task OnAfterDelete(object entity)
        {
            return OnAfterDelete((TEntity)entity);
        }

        public Task OnAfterUpdate(object updatedEntity, string columnName, object newValue)
        {
            return OnAfterUpdate((TEntity)updatedEntity, columnName, newValue);
        }

        public Task OnAfterCreate(object entity)
        {
            return OnAfterCreate((TEntity)entity);
        }

#pragma warning disable 1998
        public virtual async Task OnCommit()
#pragma warning restore 1998
        {
        }
    }
}
