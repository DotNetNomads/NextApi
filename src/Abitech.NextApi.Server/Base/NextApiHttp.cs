using System;
using System.Linq;
using System.Reflection;
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
        private readonly INextApiUserAccessor _userAccessor;
        private readonly INextApiRequest _request;
        private readonly NextApiServicesOptions _options;
        private readonly INextApiPermissionProvider _permissionProvider;
        private readonly IServiceProvider _serviceProvider;

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
            var errorResponse = ValidateRequest(context, out var methodInfo, out var form, out var serviceType);
            if (errorResponse != null)
            {
                await context.Response.SendJson(errorResponse);
                return;
            }

            // method access validation
            var attribute = methodInfo.GetCustomAttributes(typeof(NextApiAuthorizeAttribute), false)
                .FirstOrDefault();
            if (attribute is NextApiAuthorizeAttribute permissionAuthorizeAttribute)
            {
                if (!await _permissionProvider.HasPermission(context.User, permissionAuthorizeAttribute.Permission))
                {
                    var response = NextApiServiceHelper.CreateNextApiErrorResponse(
                        NextApiErrorCode.OperationIsNotAllowed,
                        "Operation is not allowed for current user");
                    await context.Response.SendJson(response);
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
                await context.Response.SendJson(
                    NextApiServiceHelper.CreateNextApiErrorResponse(NextApiErrorCode.IncorrectRequest, ex.Message)
                );
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

                var jsonResponse = new NextApiResponse(response);
                await context.Response.SendJson(jsonResponse);
            }
            catch (Exception ex)
            {
                await context.Response.SendJson(
                    NextApiServiceHelper.CreateNextApiExceptionResponse(ex)
                );
            }
        }

        private NextApiResponse ValidateRequest(HttpContext context, out MethodInfo methodInfo,
            out IFormCollection form, out Type serviceType)
        {
            methodInfo = null;
            serviceType = null;
            form = context.Request.Form;

            if (form == null || form.Count < 0)
            {
                return NextApiServiceHelper.CreateNextApiErrorResponse(NextApiErrorCode.IncorrectRequest,
                    "Incorrect request fields");
            }

            _userAccessor.User = context.User;
            _request.FilesFromClient = form.Files;

            var serviceName = form["Service"].FirstOrDefault();
            if (serviceName == null)
            {
                return NextApiServiceHelper.CreateNextApiErrorResponse(NextApiErrorCode.ServiceIsNotFound,
                    "Service name is not provided");
            }

            var methodName = form["Method"].FirstOrDefault();
            if (methodName == null)
            {
                return NextApiServiceHelper.CreateNextApiErrorResponse(NextApiErrorCode.OperationIsNotFound,
                    "Operation is not provided");
            }

            serviceType = NextApiServiceHelper.GetServiceType(serviceName);
            if (serviceType == null)
            {
                return NextApiServiceHelper.CreateNextApiErrorResponse(NextApiErrorCode.ServiceIsNotFound,
                    "Service is not found in service collection");
            }

            // service access validation
            var isAnonymousService = _options.AnonymousByDefault ||
                                     NextApiServiceHelper.IsServiceOnlyForAnonymous(serviceType);
            var userAuthorized = context.User.Identity.IsAuthenticated;
            if (!isAnonymousService && !userAuthorized)
            {
                return NextApiServiceHelper.CreateNextApiErrorResponse(NextApiErrorCode.ServiceIsOnlyForAuthorized,
                    null);
            }

            methodInfo = NextApiServiceHelper.GetServiceMethod(serviceType, methodName);
            if (methodInfo == null)
            {
                return NextApiServiceHelper.CreateNextApiErrorResponse(NextApiErrorCode.OperationIsNotFound,
                    "Operation not found in requested service");
            }

            return null;
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
