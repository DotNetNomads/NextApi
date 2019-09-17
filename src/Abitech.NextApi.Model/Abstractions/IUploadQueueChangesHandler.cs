using System.Threading.Tasks;

namespace Abitech.NextApi.Model.Abstractions
{
    /// <summary>
    /// Base interface for upload queue changes handlers
    /// </summary>
    /// <typeparam name="TEntity">Entity type</typeparam>
    public interface IUploadQueueChangesHandler<TEntity> : IUploadQueueChangesHandler
        where TEntity: class
    {
        /// <summary>
        /// This method is called right before an entity is deleted through it's repository
        /// </summary>
        /// <param name="entity">Entity instance that is to be deleted</param>
        /// <returns>void or an exception is thrown, in which scenario - do not delete this entity instance</returns>
        Task OnBeforeDelete(TEntity entity);
        
        /// <summary>
        /// This method is called right before an entity is updated through it's repository
        /// </summary>
        /// <param name="originalEntity">Entity instance that is to be updated</param>
        /// <param name="columnName">Column name that is to be updated</param>
        /// <param name="newValue">New value</param>
        /// <returns>void or an exception is thrown, in which scenario - do not update this entity instance</returns>
        Task OnBeforeUpdate(TEntity originalEntity, string columnName, object newValue);
        
        /// <summary>
        /// This method is called right before an entity is created through it's repository
        /// </summary>
        /// <param name="entityToCreate">Entity instance that is to be created</param>
        /// <returns>void or an exception is thrown, in which scenario - do not create this entity instance</returns>
        Task OnBeforeCreate(TEntity entityToCreate);
        
        /// <summary>
        /// This method is called right after an entity is deleted through it's repository
        /// </summary>
        /// <param name="entity">Entity instance that has been deleted</param>
        /// <returns>void</returns>
        Task OnAfterDelete(TEntity entity);
        
        /// <summary>
        /// This method is called right after an entity is updated through it's repository
        /// </summary>
        /// <param name="originalEntity">Entity instance that has been updated</param>
        /// <param name="columnName">Column name that is to be updated</param>
        /// <param name="newValue">New value</param>
        /// <returns>void</returns>
        Task OnAfterUpdate(TEntity originalEntity, string columnName, object newValue);
        
        /// <summary>
        /// This method is called right after an entity is created through it's repository
        /// </summary>
        /// <param name="entity">Entity instance that has been created</param>
        /// <returns>void</returns>
        Task OnAfterCreate(TEntity entity);
    }
    
    /// <summary>
    /// Non-generic base interface for upload queue changes handlers
    /// </summary>
    public interface IUploadQueueChangesHandler
    {
        /// <summary>
        /// This method is called right before an entity is deleted through it's repository
        /// </summary>
        /// <param name="entity">Entity instance that is to be deleted</param>
        /// <returns>void or an exception is thrown, in which scenario - do not delete this entity instance</returns>
        Task OnBeforeDelete(object entity);
        
        /// <summary>
        /// This method is called right before an entity is updated through it's repository
        /// </summary>
        /// <param name="originalEntity">Entity instance that is to be updated</param>
        /// <param name="columnName">Column name that is to be updated</param>
        /// <param name="newValue">New value</param>
        /// <returns>void or an exception is thrown, in which scenario - do not update this entity instance</returns>
        Task OnBeforeUpdate(object originalEntity, string columnName, object newValue);
        
        /// <summary>
        /// This method is called right before an entity is created through it's repository
        /// </summary>
        /// <param name="entityToCreate">Entity instance that is to be created</param>
        /// <returns>void or an exception is thrown, in which scenario - do not create this entity instance</returns>
        Task OnBeforeCreate(object entityToCreate);
        
        /// <summary>
        /// This method is called right after an entity is deleted through it's repository
        /// </summary>
        /// <param name="entity">Entity instance that has been deleted</param>
        /// <returns>void</returns>
        Task OnAfterDelete(object entity);
        
        /// <summary>
        /// This method is called right after an entity is updated through it's repository
        /// </summary>
        /// <param name="updatedEntity">Entity instance that has been updated</param>
        /// <param name="columnName">Column name that is to be updated</param>
        /// <param name="newValue">New value</param>
        /// <returns>void</returns>
        Task OnAfterUpdate(object updatedEntity, string columnName, object newValue);
        
        /// <summary>
        /// This method is called right after an entity is created through it's repository
        /// </summary>
        /// <param name="entity">Entity instance that has been created</param>
        /// <returns>void</returns>
        Task OnAfterCreate(object entity);
        
        /// <summary>
        /// This method is called right after all changes are commited by the unit of work
        /// </summary>
        /// <returns>void</returns>
        Task OnCommit();
    }
}
