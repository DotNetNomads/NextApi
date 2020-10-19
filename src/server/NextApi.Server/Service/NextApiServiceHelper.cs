using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using NextApi.Common;
using NextApi.Common.Abstractions;
using NextApi.Common.Serialization;
using NextApi.Server.Base;

namespace NextApi.Server.Service
{
    /// <summary>
    /// Some useful utils for NextApi
    /// </summary>
    public static class NextApiServiceHelper
    {
        /// <summary>
        /// Resolves service method for service type
        /// </summary>
        /// <param name="serviceType">Type of service</param>
        /// <param name="methodName">Method name</param>
        /// <returns>MethodInfo that represents requested method</returns>
        public static MethodInfo GetServiceMethod(Type serviceType, string methodName)
        {
            var methods = serviceType.GetMethods();
            return methods.FirstOrDefault(m =>
                m.Name.ToLower().Equals(methodName) &&
                m.MemberType == MemberTypes.Method &&
                m.IsPublic &&
                !m.IsStatic);
        }

        /// <summary>
        /// Resolves parameters for method call from NextApiCommand
        /// </summary>
        /// <param name="methodInfo">Information about method</param>
        /// <param name="command">Information about NextApi call</param>
        /// <returns>Array of parameters</returns>
        /// <exception cref="Exception">when parameter is not exist in command</exception>
        public static object[] ResolveMethodParameters(MethodInfo methodInfo, NextApiCommand command)
        {
            var paramValues = new List<object>();
            foreach (var parameter in methodInfo.GetParameters())
            {
                var paramName = parameter.Name;

                var arg = command.Args.Cast<INamedNextApiArgument>().FirstOrDefault(d => d.Name == paramName);

                switch (arg)
                {
                    case null when !parameter.IsOptional:
                        throw new Exception($"Parameter with {paramName} is not exist in request");
                    // adding placeholder for optional param.
                    // See: https://stackoverflow.com/questions/9977719/invoke-a-method-with-optional-params-via-reflection
                    case null:
                        paramValues.Add(Type.Missing);
                        continue;
                    case NextApiJsonArgument nextApiJsonArgument:
                        var argType = parameter.ParameterType;
                        var deserializedValue = nextApiJsonArgument.Value?.ToObject(argType,
                            JsonSerializer.Create(SerializationUtils.GetJsonConfig()));
                        paramValues.Add(deserializedValue);
                        break;
                    case NextApiArgument nextApiArgument:
                        paramValues.Add(nextApiArgument.Value);
                        break;
                }
            }

            return paramValues.ToArray();
        }

        /// <summary>
        /// Extension method for MethodInfo class. For detecting async attribute.
        /// </summary>
        /// <param name="methodInfo">Method info</param>
        /// <returns><c>true</c> if the method is async</returns>
        private static bool IsAsyncMethod(this MethodInfo methodInfo)
        {
            var asyncAttr = typeof(AsyncStateMachineAttribute);
            var attribute = (AsyncStateMachineAttribute)methodInfo.GetCustomAttribute(asyncAttr);
            return (attribute != null);
        }

        /// <summary>
        /// Calls service and returns value
        /// </summary>
        /// <param name="methodInfo">MethodInfo</param>
        /// <param name="serviceInstance">Service instance</param>
        /// <param name="arguments">Method arguments</param>
        /// <returns>dynamic result of method call</returns>
        /// <remarks>Do not use <c>async void</c> for service methods. See: https://docs.microsoft.com/en-us/dotnet/csharp/async</remarks>
        public static async Task<object> CallService(MethodInfo methodInfo, INextApiService serviceInstance,
            object[] arguments)
        {
            var returnType = methodInfo.ReturnType;
            var taskType = typeof(Task);
            var voidType = typeof(void);
            var isAsyncMethod = methodInfo.IsAsyncMethod();

            // async void is not supported!!!
            if (returnType == voidType && isAsyncMethod)
                throw new Exception("Invalid definition of method");

            object result = null;
            if (returnType == taskType)
            {
                await (Task)methodInfo.Invoke(serviceInstance, arguments);
            }
            else if (taskType.IsAssignableFrom(returnType))
            {
                result = (object)await (dynamic)methodInfo.Invoke(serviceInstance, arguments);
            }
            else if (returnType == voidType)
            {
                methodInfo.Invoke(serviceInstance, arguments);
            }
            else
            {
                result = methodInfo.Invoke(serviceInstance, arguments);
            }

            return result;
        }

        /// <summary>
        /// Create and return error response for client
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static NextApiResponse CreateNextApiErrorResponse(
            NextApiErrorCode code, string message)
        {
            var error =
                new NextApiError(code.ToString(), new Dictionary<string, object> {{"message", message}});
            return new NextApiResponse(null, error, false);
        }

        /// <summary>
        /// Get all error messages from Exception
        /// </summary>
        /// <param name="exception">Exception instance</param>
        /// <returns>Returns all messages from Exception and its inners</returns>
        public static string GetAllMessagesFromException(this Exception exception)
        {
            var currentException = exception;
            var messageText = new StringBuilder();
            do
            {
                messageText.Append($"{currentException.Message} \n");
            } while ((currentException = currentException.InnerException) != null);

            return messageText.ToString();
        }

        /// <summary>
        /// Create and return exception response for client
        /// </summary>
        /// <param name="exception">Any exception</param>
        /// <returns></returns>
        public static NextApiResponse CreateNextApiExceptionResponse(Exception exception)
        {
            var message = exception.GetAllMessagesFromException();
            string code;
            var parameters = new Dictionary<string, object>();
            if (exception is NextApiException nextApiException)
            {
                code = nextApiException.Code;
                if (nextApiException.Parameters != null)
                {
                    parameters = nextApiException.Parameters;
                }
            }
            else
            {
                code = NextApiErrorCode.Unknown.ToString();
            }

            if (!parameters.ContainsKey("message"))
            {
                parameters.Add(
                    "message", message);
            }

            var error = new NextApiError(code, parameters);
            return new NextApiResponse(null, error, false);
        }

        /// <summary>
        /// Wrapper for sending json to client
        /// </summary>
        /// <param name="response"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static async Task SendJson(this HttpResponse response, object data)
        {
            response.StatusCode = 200;
            response.ContentType = "application/json";
            var encoded = JsonConvert.SerializeObject(data, SerializationUtils.GetJsonConfig());
            await response.WriteAsync(encoded);
        }

        /// <summary>
        /// Wrapper for sending byte array to client
        /// </summary>
        /// <param name="response"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static async Task SendByteArray(this HttpResponse response, byte[] data)
        {
            response.StatusCode = 200;
            response.ContentType = "application/octet-stream";
            using var memoryStream = new MemoryStream(data);
            await memoryStream.CopyToAsync(response.Body);
        }

        /// <summary>
        /// Wrapper for sending file response to client
        /// </summary>
        /// <param name="response"></param>
        /// <param name="fileInfo"></param>
        /// <returns></returns>
        public static async Task SendNextApiFileResponse(this HttpResponse response, NextApiFileResponse fileInfo)
        {
            response.StatusCode = 200;
            response.ContentType = fileInfo.MimeType;
            response.Headers.Add("content-disposition",
                $"attachment; filename={WebUtility.UrlEncode(fileInfo.FileName)}");
            using (fileInfo)
            {
                await fileInfo.CopyToAsync(response.Body);
            }
        }
    }
}
