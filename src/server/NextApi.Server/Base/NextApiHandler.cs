using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NextApi.Common;
using NextApi.Common.Abstractions;
using NextApi.Common.Abstractions.Security;
using NextApi.Server.Service;

namespace NextApi.Server.Base
{
    /// <summary>
    /// Base handler for all NextApi requests
    /// </summary>
    public class NextApiHandler
    {
        private readonly INextApiUserAccessor _nextApiUserAccessor;
        private readonly IServiceProvider _serviceProvider;
        private readonly INextApiPermissionProvider _permissionProvider;
        private readonly ILogger<NextApiHandler> _logger;
        private readonly NextApiServiceRegistry _serviceRegistry;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nextApiUserAccessor">Accessor to User information</param>
        /// <param name="serviceProvider"></param>
        /// <param name="permissionProvider"></param>
        /// <param name="logger">Logger</param>
        /// <param name="serviceRegistry"></param>
        public NextApiHandler(INextApiUserAccessor nextApiUserAccessor,
            IServiceProvider serviceProvider, INextApiPermissionProvider permissionProvider,
            ILogger<NextApiHandler> logger, NextApiServiceRegistry serviceRegistry)
        {
            _nextApiUserAccessor = nextApiUserAccessor;
            _serviceProvider = serviceProvider;
            _permissionProvider = permissionProvider;
            _logger = logger;
            _serviceRegistry = serviceRegistry;
        }

        /// <summary>
        /// Entry point of service calls
        /// </summary>
        /// <param name="command">Information about service call</param>
        /// <returns>Response from called service</returns>
        /// <exception cref="Exception">When security issues, or service issues</exception>
        public async Task<INextApiResponse> ExecuteCommand(NextApiCommand command)
        {
            _logger.LogDebug("NextApi request received...");
            _logger.LogDebug($"NextApi/User ID: {_nextApiUserAccessor.SubjectId}");
            _logger.LogDebug($"NextApi/Service: {command.Service}");
            _logger.LogDebug($"NextApi/Method: {command.Method}");
            _logger.LogDebug(
                $"NextApi/Args: {(command.Args == null ? "no" : JsonConvert.SerializeObject(command.Args))}");

            if (string.IsNullOrWhiteSpace(command.Service))
                return NextApiServiceHelper.CreateNextApiErrorResponse(NextApiErrorCode.ServiceIsNotFound,
                    "Service name is not provided");
            if (string.IsNullOrWhiteSpace(command.Method))
                return NextApiServiceHelper.CreateNextApiErrorResponse(NextApiErrorCode.OperationIsNotFound,
                    "Operation name is not provided");

            // translating all to lower case
            command.Service = command.Service.ToLower();
            command.Method = command.Method.ToLower();

            if (!_serviceRegistry.TryResolveServiceInfo(command.Service, out var serviceInfo))
            {
                _logger.LogDebug($"NextApi/Result: service is not found.");
                return NextApiServiceHelper.CreateNextApiErrorResponse(NextApiErrorCode.ServiceIsNotFound,
                    $"Service with name {command.Service} is not found");
            }

            // service access validation
            var userAuthorized = _nextApiUserAccessor.User.Identity.IsAuthenticated;
            if (serviceInfo.RequiresAuthorization && !userAuthorized)
            {
                _logger.LogDebug($"NextApi/Result: service available only for authorized users.");
                return NextApiServiceHelper.CreateNextApiErrorResponse(NextApiErrorCode.ServiceIsOnlyForAuthorized,
                    "This service available only for authorized users");
            }

            var methodInfo = NextApiServiceHelper.GetServiceMethod(serviceInfo.ServiceType, command.Method);
            if (methodInfo == null)
            {
                _logger.LogDebug($"NextApi/Result: method is not found.");
                return NextApiServiceHelper.CreateNextApiErrorResponse(NextApiErrorCode.OperationIsNotFound,
                    $"Method with name {command.Method} is not found in service {command.Service}");
            }

            // method access validation
            var hasPermission = false;
            if (serviceInfo.MethodsPermissionInfo.TryGetValue(command.Method, out var permissionName))
            {
                try
                {
                    hasPermission = await _permissionProvider.HasPermission(_nextApiUserAccessor.User, permissionName);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"NextApi/Error: when checking permissions. {ex.Message}", ex);
                    return NextApiServiceHelper.CreateNextApiErrorResponse(NextApiErrorCode.Unknown,
                        $"Error when checking permissions. {ex.Message}");
                }
            }

            if (!hasPermission && !serviceInfo.AllowByDefault)
            {
                _logger.LogDebug("NextApi/Result: method is not allowed for current user.");
                return NextApiServiceHelper.CreateNextApiErrorResponse(NextApiErrorCode.OperationIsNotAllowed,
                    "This operation is not allowed for current user");
            }

            object[] methodParameters;
            try
            {
                methodParameters = NextApiServiceHelper.ResolveMethodParameters(methodInfo, command);
            }
            catch (Exception e)
            {
                _logger.LogError($"NextApi/Error: when resolving method params. {e.Message}", e);
                return NextApiServiceHelper.CreateNextApiErrorResponse(NextApiErrorCode.IncorrectRequest,
                    $"Error when parsing arguments for method. Please send correct arguments.");
            }

            var serviceInstance = (INextApiService)_serviceProvider.GetService(serviceInfo.ServiceType);
            try
            {
                var result = await NextApiServiceHelper.CallService(methodInfo, serviceInstance, methodParameters);

                _logger.LogDebug(
                    $@"NextApi/Result: {(result is NextApiFileResponse file ? $"file {file.FileName}"
                        : JsonConvert.SerializeObject(result))}");

                return result is INextApiResponse nextApiResponse
                    ? nextApiResponse
                    : new NextApiResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"NextApi/Error: {ex.GetAllMessagesFromException()}", ex);
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
