using System.Threading.Tasks;
using Abitech.NextApi.Model.Abstractions;

namespace Abitech.NextApi.Server.EfCore.Service
{
    public abstract class UploadQueueChangesHandler<TEntity> : IUploadQueueChangesHandler<TEntity> 
        where TEntity : class
    {
        public virtual async Task OnBeforeDelete(TEntity entity)
        {
        }

        public virtual async Task OnBeforeUpdate(TEntity originalEntity, string columnName, object newValue)
        {
        }

        public virtual async Task OnBeforeCreate(TEntity entityToCreate)
        {
        }

        public virtual async Task OnAfterDelete(TEntity entity)
        {
        }

        public virtual async Task OnAfterUpdate(TEntity originalEntity, string columnName, object newValue)
        {
        }

        public virtual async Task OnAfterCreate(TEntity entity)
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
    }
}
