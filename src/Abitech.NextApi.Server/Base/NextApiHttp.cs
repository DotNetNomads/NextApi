using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Abitech.NextApi.Model;
using Abitech.NextApi.Server.Attributes;
using Abitech.NextApi.Server.Request;
using Abitech.NextApi.Server.Security;
using Abitech.NextApi.Server.Service;
using Microsoft.AspNetCore.Http;

namespace Abitech.NextApi.Server.Base
{
    /// <summary>
    /// Request processor from HTTP (in case we don't want to use SignalR as transport)
    /// </summary>
    public class NextApiHttp
    {
        private INextApiUserAccessor _userAccessor;
        private INextApiRequest _request;
        private NextApiServicesOptions _options;
        private INextApiPermissionProvider _permissionProvider;
        private IServiceProvider _serviceProvider;

        /// <inheritdoc />
        public NextApiHttp(INextApiUserAccessor userAccessor, INextApiRequest request, NextApiServicesOptions options,
            INextApiPermissionProvider permissionProvider, IServiceProvider serviceProvider)
        {
            _userAccessor = userAccessor ?? throw new ArgumentNullException(nameof(userAccessor));
            _request = request ?? throw new ArgumentNullException(nameof(request));
            _options = options;
            _permissionProvider = permissionProvider ?? throw new ArgumentNullException(nameof(permissionProvider));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        /// <summary>
        /// Entry point for HTTP requests
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task ProcessRequestAsync(HttpContext context)
        {
            var form = context.Request.Form;
            if (form == null || form.Count < 0)
            {
                await context.Response.SendNextApiError(NextApiErrorCode.IncorrectRequest,
                    new Tuple<string, object>("message", "Incorrect request fields"));
                return;
            }

            _userAccessor.User = context.User;
            _request.FilesFromClient = form.Files;

            var serviceName = form["Service"].FirstOrDefault();
            if (serviceName == null)
            {
                await context.Response.SendNextApiError(NextApiErrorCode.ServiceIsNotFound,
                    new Tuple<string, object>("message", "Service name is not provided"));
                return;
            }

            var methodName = form["Method"].FirstOrDefault();
            if (methodName == null)
            {
                await context.Response.SendNextApiError(NextApiErrorCode.OperationIsNotFound,
                    new Tuple<string, object>("message", "Operation is not provided"));
                return;
            }

            var serviceType = NextApiServiceHelper.GetServiceType(serviceName);
            if (serviceType == null)
            {
                await context.Response.SendNextApiError(NextApiErrorCode.ServiceIsNotFound,
                    new Tuple<string, object>("message", "Service is not found in service collection"));
                return;
            }

            // service access validation
            var isAnonymousService = _options.AnonymousByDefault ||
                                     NextApiServiceHelper.IsServiceOnlyForAnonymous(serviceType);
            var userAuthorized = context.User.Identity.IsAuthenticated;
            if (!isAnonymousService && !userAuthorized)
            {
                await context.Response.SendNextApiError(NextApiErrorCode.ServiceIsOnlyForAuthorized);
                return;
            }

            var methodInfo = NextApiServiceHelper.GetServiceMethod(serviceType, methodName);
            if (methodInfo == null)
            {
                await context.Response.SendNextApiError(NextApiErrorCode.OperationIsNotFound,
                    new Tuple<string, object>("message", "Operation not found in requested service"));
                return;
            }

            // method access validation
            var attribute = methodInfo.GetCustomAttributes(typeof(NextApiAuthorizeAttribute), false)
                .FirstOrDefault();
            if (attribute is NextApiAuthorizeAttribute permissionAuthorizeAttribute)
            {
                if (!await _permissionProvider.HasPermission(context.User, permissionAuthorizeAttribute.Permission))
                {
                    await context.Response.SendNextApiError(NextApiErrorCode.OperationIsNotAllowed);
                    return;
                }
            }

            object[] methodParams;
            try
            {
                methodParams = NextApiServiceHelper.ResolveMethodParametersJson(methodInfo, form["Args"]);
            }
            catch (Exception ex)
            {
                await context.Response.SendNextApiError(NextApiErrorCode.IncorrectRequest,
                    new Tuple<string, object>("message", ex.Message));
                return;
            }

            var serviceInstance = (NextApiService)_serviceProvider.GetService(serviceType);
            try
            {
                var response = await NextApiServiceHelper.CallService(methodInfo, serviceInstance, methodParams);
                if (response is NextApiFileResponse fileResponse)
                {
                    await context.Response.SendNextApiFileResponse(fileResponse);
                    return;
                }

                await context.Response.SendNextApiResponse(response);
            }
            catch (Exception ex)
            {
                var message = ex.Message;
                await context.Response.SendNextApiError(NextApiErrorCode.Unknown,
                    new Tuple<string, object>("message", message));
            }
        }

        /// <summary>
        /// Returns list of supported permissions
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task GetSupportedPermissions(HttpContext context)
        {
            var permissions = _permissionProvider.SupportedPermissions;
            await context.Response.SendJson(permissions);
        }
    }
}
