namespace Abitech.NextApi.Server.Common
{
    /// <summary>
    /// Builder for NextApiService
    /// </summary>
    public class NextApiServiceBuilder
    {
        private readonly ServiceInformation _serviceInformation;

        /// <inheritdoc />
        public NextApiServiceBuilder(ServiceInformation serviceInformation)
        {
            _serviceInformation = serviceInformation;
        }

        /// <summary>
        /// Allow service execution for anonymous users
        /// </summary>
        /// <returns></returns>
        public void AllowToGuests()
        {
            _serviceInformation.RequiresAuthorization = false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public NextApiServiceBuilder DenyAll()
        {
            _serviceInformation.AllowByDefault = false;
            return this;
        }

        /// <summary>
        /// Setup permission for specific NextApi entity service method
        /// </summary>
        /// <param name="methodType">Entity service method</param>
        /// <param name="permission">Permission value</param>
        /// <returns></returns>
        public NextApiServiceBuilder WithPermission(
            Methods methodType,
            object permission)
        {
            _serviceInformation.MethodsPermissionInfo.Add(methodType.ToString(), permission);
            return this;
        }

        /// <summary>
        /// Setup permission for specific NextApi service method
        /// </summary>
        /// <param name="methodName">Service method name</param>
        /// <param name="permission"></param>
        /// <returns></returns>
        public NextApiServiceBuilder WithPermission(
            string methodName,
            object permission)
        {
            _serviceInformation.MethodsPermissionInfo.Add(methodName, permission);
            return this;
        }
    }

    /// <summary>
    /// Default methods for NextApi entity service
    /// </summary>
    public enum Methods
    {
        /// <summary>
        /// Get an entity by id
        /// </summary>
        GetById,

        /// <summary>
        /// Get entities by ids
        /// </summary>
        GetByIds,

        /// <summary>
        /// Get entities as PagedList
        /// </summary>
        GetPaged,

        /// <summary>
        /// Count entities
        /// </summary>
        Count,

        /// <summary>
        /// Get entities ids by filter
        /// </summary>
        GetIdsByFilter,

        /// <summary>
        /// Get entities tree
        /// </summary>
        GetTree,

        /// <summary>
        /// Get Any for entities
        /// </summary>
        Any,

        /// <summary>
        /// Update an entity
        /// </summary>
        Update,

        /// <summary>
        /// Delete an entity
        /// </summary>
        Delete,

        /// <summary>
        /// Create an entity
        /// </summary>
        Create
    }
}
