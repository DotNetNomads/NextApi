using System;
using System.Linq.Expressions;
using Abitech.NextApi.Common.Abstractions;

namespace Abitech.NextApi.Server.Service
{
    /// <summary>
    /// Builder for NextApiService
    /// </summary>
    /// <typeparam name="TServiceType">Service type</typeparam>
    public class NextApiServiceBuilder<TServiceType>
        where TServiceType : INextApiService
    {
        private ServiceInformation _serviceInformation;

        /// <inheritdoc />
        public NextApiServiceBuilder(ServiceInformation serviceInformation)
        {
            _serviceInformation = serviceInformation;
        }
        
        /// <summary>
        /// Allow service execution for anonymous users
        /// </summary>
        /// <returns></returns>
        public NextApiServiceBuilder<TServiceType> AllowToGuests()
        {
            _serviceInformation.RequiresAuthorization = false;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public NextApiServiceBuilder<TServiceType> DenyAll()
        {
            _serviceInformation.AllowByDefault = false;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="method"></param>
        /// <param name="permission"></param>
        /// <returns></returns>
        public NextApiServiceBuilder<TServiceType> WithPermission(Expression<Action<TServiceType>> method,
            object permission)
        {
            return this;
        }
    }
}
