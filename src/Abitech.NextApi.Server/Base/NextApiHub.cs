using System;
using System.Linq;
using System.Threading.Tasks;
using Abitech.NextApi.Model;
using Abitech.NextApi.Server.Attributes;
using Abitech.NextApi.Server.Security;
using Abitech.NextApi.Server.Service;
using Microsoft.AspNetCore.SignalR;
using Hub = Microsoft.AspNetCore.SignalR.Hub;

namespace Abitech.NextApi.Server.Base
{
    /// <inheritdoc />
    /// <summary>
    /// Main Hub. Entry point for all NextApi calls via SignalR
    /// </summary>
    public class NextApiHub : Hub
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly INextApiPermissionProvider _permissionProvider;
        private readonly IHubContext<NextApiHub> _hubContext;
        private readonly NextApiServicesOptions _options;
        private readonly INextApiUserAccessor _nextApiUserAccessor;

        /// <summary>
        /// Initialize Hub
        /// </summary>
        /// <param name="serviceProvider">Used for service location</param>
        /// <param name="permissionProvider">Used for users validation by permissions</param>
        /// <param name="hubContext">Hub context</param>
        /// <param name="options">NextApi options</param>
        /// <param name="nextApiUserAccessor"></param>
        public NextApiHub(IServiceProvider serviceProvider,
            INextApiPermissionProvider permissionProvider,
            IHubContext<NextApiHub> hubContext,
            NextApiServicesOptions options, INextApiUserAccessor nextApiUserAccessor)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _permissionProvider = permissionProvider ?? throw new ArgumentNullException(nameof(permissionProvider));
            _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _nextApiUserAccessor = nextApiUserAccessor ?? throw new ArgumentNullException(nameof(nextApiUserAccessor));
        }

        /// <summary>
        /// Entry point of service calls
        /// </summary>
        /// <param name="command">Information about service call</param>
        /// <returns>Response from called service</returns>
        /// <exception cref="Exception">When security issues, or service issues</exception>
        public async Task<dynamic> ExecuteCommand(NextApiCommand command)
        {
            // set current nextapi user
            _nextApiUserAccessor.User = Context.User;
            
            if (string.IsNullOrWhiteSpace(command.Service))
                throw new Exception("Service name is not provided");
            if (string.IsNullOrWhiteSpace(command.Method))
                throw new Exception("Method name is not provided");

            var serviceType = NextApiServiceHelper.GetServiceType(command.Service)
                              ?? throw new Exception(
                                  $"Service with name {command.Service} is not found");

            // service access validation
            var isAnonymousService = _options.AnonymousByDefault ||
                                     NextApiServiceHelper.IsServiceOnlyForAnonymous(serviceType);
            var userAuthorized = Context.User.Identity.IsAuthenticated;
            if (!isAnonymousService && !userAuthorized)
                throw new Exception("This service available only for authorized users");

            var methodInfo = NextApiServiceHelper.GetServiceMethod(serviceType, command)
                             ?? throw new Exception(
                                 $"Method with name {command.Method} is not found in service {command.Service}");

            // method access validation
            var attribute = methodInfo.GetCustomAttributes(typeof(NextApiAuthorizeAttribute), false)
                .FirstOrDefault();
            if (attribute is NextApiAuthorizeAttribute permissionAuthorizeAttribute)
            {
                if (!await _permissionProvider.HasPermission(Context.User, permissionAuthorizeAttribute.Permission))
                    throw new Exception("This method disabled for current user");
            }

            var methodParameters = NextApiServiceHelper.ResolveMethodParameters(methodInfo, command);
            var serviceInstance = (NextApiService)_serviceProvider.GetService(serviceType);
            serviceInstance.ClientContext = Context;
            serviceInstance.HubContext = _hubContext;
            try
            {
                return await NextApiServiceHelper.CallService(methodInfo, serviceInstance, methodParameters);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Returns list of supported permissions
        /// </summary>
        /// <returns></returns>
        public string[] GetSupportedPermissions()
        {
            return _permissionProvider.SupportedPermissions;
        }
    }
}
