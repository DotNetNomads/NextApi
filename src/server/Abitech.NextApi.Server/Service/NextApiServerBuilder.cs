using System;
using System.Collections.Generic;

namespace Abitech.NextApi.Server.Service
{
    /// <summary>
    /// Main options for NextApiServices
    /// </summary>
    public class NextApiServerBuilder
    {
        private readonly IDictionary<string, ServiceInformation> _serviceRegistry = new Dictionary<string, ServiceInformation>();

        /// <summary>
        /// Disable permission validation for all services
        /// </summary>
        public bool DisablePermissionValidation { get; set; } = false;

        /// <summary>
        /// Size in bytes for single message (actual for SignalR mode). Default is 256KB 
        /// </summary>
        public long MaximumReceiveMessageSize { get; set; } = 256000;

        /// <summary>
        /// Add service information to NextApi server
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="info"></param>
        public void AddServiceInfo(string serviceName, ServiceInformation info)
        {
            if (string.IsNullOrWhiteSpace(serviceName))
                throw new InvalidOperationException("Please provide name for service");

            var toLowerServiceName = serviceName.ToLower();
            if (_serviceRegistry.ContainsKey(toLowerServiceName))
                throw new InvalidOperationException(
                    $"Service with name {toLowerServiceName} already added to NextApi server");
            _serviceRegistry.Add(toLowerServiceName, info);
        }
    }
}
