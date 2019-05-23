using System;
using System.Linq;
using System.Threading.Tasks;
using Abitech.NextApi.Model;
using Abitech.NextApi.Server.Attributes;
using Abitech.NextApi.Server.Request;
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
        private readonly INextApiRequest _request;

        /// <summary>
        /// Initialize Hub
        /// </summary>
        /// <param name="serviceProvider">Used for service location</param>
        /// <param name="permissionProvider">Used for users validation by permissions</param>
        /// <param name="hubContext">Hub context</param>
        /// <param name="options">NextApi options</param>
        /// <param name="nextApiUserAccessor">Instance of INextApiUserAccessor</param>
        /// <param name="request">Instance of INextApiRequest</param>
        public NextApiHub(IServiceProvider serviceProvider,
            INextApiPermissionProvider permissionProvider,
            IHubContext<NextApiHub> hubContext,
            NextApiServicesOptions options, INextApiUserAccessor nextApiUserAccessor, INextApiRequest request)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _permissionProvider = permissionProvider ?? throw new ArgumentNullException(nameof(permissionProvider));
            _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _nextApiUserAccessor = nextApiUserAccessor ?? throw new ArgumentNullException(nameof(nextApiUserAccessor));
            _request = request ?? throw new ArgumentNullException(nameof(request));
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
            // set current request info
            _request.ClientContext = Context;
            _request.HubContext = _hubContext;

            if (string.IsNullOrWhiteSpace(command.Service))
                return NextApiServiceHelper.CreateNextApiErrorResponse(NextApiErrorCode.ServiceIsNotFound,
                    "Service name is not provided");
            if (string.IsNullOrWhiteSpace(command.Method))
                return NextApiServiceHelper.CreateNextApiErrorResponse(NextApiErrorCode.OperationIsNotFound,
                    "Operation name is not provided");

            var serviceType = NextApiServiceHelper.GetServiceType(command.Service);
            if (serviceType == null)
            {
                return NextApiServiceHelper.CreateNextApiErrorResponse(NextApiErrorCode.ServiceIsNotFound,
                    $"Service with name {command.Service} is not found");
            }

            // service access validation
            var isAnonymousService = _options.AnonymousByDefault ||
                                     NextApiServiceHelper.IsServiceOnlyForAnonymous(serviceType);
            var userAuthorized = Context.User.Identity.IsAuthenticated;
            if (!isAnonymousService && !userAuthorized)
                return NextApiServiceHelper.CreateNextApiErrorResponse(NextApiErrorCode.ServiceIsOnlyForAuthorized,
                    "This service available only for authorized users");

            var methodInfo = NextApiServiceHelper.GetServiceMethod(serviceType, command.Method);
            if (methodInfo == null)
            {
                return NextApiServiceHelper.CreateNextApiErrorResponse(NextApiErrorCode.OperationIsNotFound,
                    $"Method with name {command.Method} is not found in service {command.Service}");
            }

            // method access validation
            var attribute = methodInfo.GetCustomAttributes(typeof(NextApiAuthorizeAttribute), false)
                .FirstOrDefault();
            if (attribute is NextApiAuthorizeAttribute permissionAuthorizeAttribute)
            {
                if (!await _permissionProvider.HasPermission(Context.User, permissionAuthorizeAttribute.Permission))
                    return NextApiServiceHelper.CreateNextApiErrorResponse(NextApiErrorCode.OperationIsNotAllowed,
                        "This operation is not allowed for current user");
            }

            var methodParameters = NextApiServiceHelper.ResolveMethodParameters(methodInfo, command);
            var serviceInstance = (NextApiService)_serviceProvider.GetService(serviceType);
            try
            {
                var response = await NextApiServiceHelper.CallService(methodInfo, serviceInstance, methodParameters);
                return new NextApiResponse(response);
            }
            catch (Exception ex)
            {
                return NextApiServiceHelper.CreateNextApiExceptionResponse(ex);
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
