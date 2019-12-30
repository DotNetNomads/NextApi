using System;
using System.Collections.Generic;
using Abitech.NextApi.Common.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Abitech.NextApi.Server.Common
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
        public NextApiServiceBuilder AddService<TServiceType>(string serviceName, bool automaticallyRegister = true)
            where TServiceType : class, INextApiService
        {
            if (string.IsNullOrWhiteSpace(serviceName))
                throw new InvalidOperationException("Please provide name for service");

            var toLowerServiceName = serviceName.ToLower();
            if (_serviceRegistry.ContainsKey(toLowerServiceName))
                throw new InvalidOperationException(
                    $"Service with name {toLowerServiceName} already added to NextApi server");
            var info = new ServiceInformation {ServiceType = typeof(TServiceType)};
            _serviceRegistry.Add(toLowerServiceName, info);
            if (automaticallyRegister)
            {
                ServiceCollection.AddTransient<TServiceType>();
            }

            return new NextApiServiceBuilder(info);
        }
    }
}
