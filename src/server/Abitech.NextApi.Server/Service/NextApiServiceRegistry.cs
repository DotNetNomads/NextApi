using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Abitech.NextApi.Server.Service
{
    /// <summary>
    /// Provides list of all registered nextapi services
    /// </summary>
    public class NextApiServiceRegistry
    {
        /// <inheritdoc />
        public NextApiServiceRegistry(IDictionary<string, ServiceInformation> registeredServices)
        {
            _registeredServices = registeredServices;
        }

        private readonly IDictionary<string, ServiceInformation> _registeredServices;

        /// <summary>
        /// Tries to Resolve service info by name
        /// </summary>
        /// <param name="name">Name of service</param>
        /// <param name="serviceInfo">Out argument. With service info</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">When name is null or empty</exception>
        public bool TryResolveServiceInfo(string name, out ServiceInformation serviceInfo)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
            return _registeredServices.TryGetValue(name, out serviceInfo);
        }
    }

    /// <summary>
    /// Information about NextApiService implementation
    /// </summary>
    public class ServiceInformation
    {

        /// <summary>
        /// System type for service
        /// </summary>
        public Type ServiceType { get; }

        /// <summary>
        /// Indicates that service requires authorized user
        /// </summary>
        public bool RequiresAuthorization { get; set; } = true;

        /// <summary>
        /// Indicates that service allows execution without checking permissions
        /// (by default, if method doesn't have defined permissions)
        /// </summary>
        public bool AllowByDefault { get; set; } = true;

        /// <summary>
        /// Information about per-method permissions
        /// </summary>
        public Dictionary<string, object> MethodsPermissionInfo { get; }
    }
}
