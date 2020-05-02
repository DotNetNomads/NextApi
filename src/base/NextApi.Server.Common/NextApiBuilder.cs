using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using NextApi.Common.Abstractions;

namespace NextApi.Server.Common
{
    /// <summary>
    /// Main options for NextApiServices
    /// </summary>
    public class NextApiBuilder
    {
        /// <summary>
        /// Accessor to current Service collection instance
        /// </summary>
        public IServiceCollection ServiceCollection { get; }

        /// <summary>
        /// Current service-collection instance
        /// </summary>
        private readonly IDictionary<string, ServiceInformation> _serviceRegistry =
            new Dictionary<string, ServiceInformation>();

        /// <summary>
        /// Accessor to ServiceRegistry
        /// </summary>
        public IDictionary<string, ServiceInformation> ServiceRegistry => _serviceRegistry;

        /// <summary>
        /// Builder that executed in NextApi server initialization process
        /// </summary>
        /// <param name="serviceCollection">Instance of current service collection</param>
        public NextApiBuilder(IServiceCollection serviceCollection)
        {
            ServiceCollection = serviceCollection;
        }

        /// <summary>
        /// Disable permission validation for all services
        /// </summary>
        public bool DisablePermissionValidation { get; set; } = false;

        /// <summary>
        /// Size in bytes for single message (actual for SignalR mode). Default is 256KB 
        /// </summary>
        public long MaximumReceiveMessageSize { get; set; } = 256000;

        /// <summary>
        /// Add service to NextApi server
        /// </summary>
        /// <param name="serviceName">Name for service</param>
        /// <param name="automaticallyRegister">Register service in DI automatically</param>
        public NextApiServiceBuilder AddService<TServiceType>(string serviceName = null,
            bool automaticallyRegister = true)
            where TServiceType : class, INextApiService
        {
            var serviceType = typeof(TServiceType);
            var toLowerServiceName = serviceName?.ToLower() ?? ResolveServiceName(serviceType);
            if (_serviceRegistry.ContainsKey(toLowerServiceName))
                throw new InvalidOperationException(
                    $"Service with name {toLowerServiceName} already added to NextApi server");
            var info = new ServiceInformation {ServiceType = serviceType};
            _serviceRegistry.Add(toLowerServiceName, info);
            if (automaticallyRegister)
            {
                ServiceCollection.AddTransient<TServiceType>();
            }

            return new NextApiServiceBuilder(info);
        }

        private static string ResolveServiceName(MemberInfo serviceType)
        {
            var typeName = serviceType.Name;
            if (!typeName.EndsWith("Service"))
            {
                throw new Exception(
                    @"The Dynamic Service Name Resolver requires a class that has name ending with `Service`.
                Please correct the class name or set service name manually.");
            }

            return typeName.Substring(0, typeName.Length - 7).ToLower();
        }
    }
}
