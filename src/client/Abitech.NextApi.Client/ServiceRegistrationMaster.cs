using System;
using System.Collections.Generic;

namespace Abitech.NextApi.Client
{
    /// <summary>
    /// Helps to register services in IoC container
    /// </summary>
    public class ServiceRegistrationMaster
    {
        private readonly List<ServiceInfo> _registeredServices = new List<ServiceInfo>();

        /// <summary>
        /// Add service to register
        /// </summary>
        /// <typeparam name="TInterface"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        /// <returns></returns>
        public ServiceRegistrationMaster Add<TInterface, TImplementation>()
            where TImplementation : class, TInterface
        {
            _registeredServices.Add(new ServiceInfo()
            {
                InterfaceType = typeof(TInterface), ImplementationType = typeof(TImplementation)
            });
            return this;
        }

        /// <summary>
        /// Register all services manually.
        /// </summary>
        /// <param name="serviceRegistrationHandler"></param>
        public void ManualRegistration(Action<ServiceInfo> serviceRegistrationHandler)
        {
            foreach (var service in _registeredServices)
            {
                serviceRegistrationHandler(service);
            }
        }
    }

    /// <summary>
    /// Contains service info
    /// </summary>
    public struct ServiceInfo
    {
        /// <summary>
        /// Service interface type
        /// </summary>
        public Type InterfaceType { get; set; }

        /// <summary>
        /// Service implementation type
        /// </summary>
        public Type ImplementationType { get; set; }
    }
}
