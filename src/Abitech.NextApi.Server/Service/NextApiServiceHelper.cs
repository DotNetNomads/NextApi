using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Abitech.NextApi.Model;
using Abitech.NextApi.Server.Attributes;
using MessagePack;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Abitech.NextApi.Server.Service
{
    /// <summary>
    /// Some useful utils for NextApi
    /// </summary>
    public static class NextApiServiceHelper
    {
        private static List<Type> _services;

        /// <summary>
        /// Locate every type that extends NextApiService
        /// </summary>
        /// <returns>Returns list of types that extends NextApiService</returns>
        public static List<Type> FindAllServices()
        {
            if (_services != null) return _services;

            var baseType = typeof(NextApiService);
            _services = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes())
                .Where(x => baseType.IsAssignableFrom(x) && !x.IsAbstract)
                .ToList();
            return _services;
        }

        /// <summary>
        /// Resolves service type by name
        /// </summary>
        /// <param name="name">Name of service</param>
        /// <returns>Type of service</returns>
        /// <exception cref="ArgumentNullException">When name is null or empty</exception>
        public static Type GetServiceType(string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            return FindAllServices().FirstOrDefault(t =>
                t.Name.Equals($"{name}Service"));
        }

        /// <summary>
        /// Resolves service method for service type
        /// </summary>
        /// <param name="serviceType">Type of service</param>
        /// <param name="methodName">Method name</param>
        /// <param name="signature">Method signature</param>
        /// <returns>MethodInfo that represents requested method</returns>
        public static MethodInfo GetServiceMethod(Type serviceType,  string methodName, Type[] signature = null)
        {
            MethodInfo result = null;
            if (signature == null)
            {
                var methods = serviceType.GetMethods();
                result = methods.FirstOrDefault(m =>
                    m.Name.Equals(methodName) &&
                    m.MemberType == MemberTypes.Method &&
                    m.IsPublic &&
                    !m.IsStatic);
            }
            else
            {
                var m = serviceType.GetMethod(methodName, signature);
                if (m != null && m.MemberType == MemberTypes.Method && m.IsPublic && !m.IsStatic)
                    result = m;
            }
            
            return result;
        }

        /// <summary>
        /// Checks the service for NextApiAnonymousAttribute
        /// </summary>
        /// <param name="serviceType">Type of service</param>
        /// <returns>Returns <c>true</c> if the service contains anonymous attribute</returns>
        public static bool IsServiceOnlyForAnonymous(Type serviceType)
        {
            return serviceType.GetCustomAttributes().Any(a => a.GetType() == typeof(NextApiAnonymousAttribute));
        }

        /// <summary>
        /// Resolves parameters for method call from NextApiCommand
        /// </summary>
        /// <param name="methodInfo">Information about method</param>
        /// <param name="parameters">Method parameters (Key - param name; Value - param value)</param>
        /// <returns>Array of parameters</returns>
        /// <exception cref="Exception">when parameter is not exist in command</exception>
        public static object[] ResolveMethodParameters(MethodInfo methodInfo, Dictionary<string, object> parameters)
        {
            var paramValues = new List<object>();
            foreach (var parameter in methodInfo.GetParameters())
            {
                var paramName = parameter.Name;

                var res = parameters.TryGetValue(paramName, out var value);

                if (!res)
                {
                    if (!parameter.IsOptional)
                        throw new Exception($"Parameter with {paramName} is not exist in request");

                    // adding placeholder for optional param.
                    // See: https://stackoverflow.com/questions/9977719/invoke-a-method-with-optional-params-via-reflection
                    paramValues.Add(Type.Missing);
                    continue;
                }
                
                paramValues.Add(value);
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
            var attribute = (AsyncStateMachineAttribute) methodInfo.GetCustomAttribute(asyncAttr);
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
        public static async Task<object> CallService(object serviceInstance, MethodInfo methodInfo,
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
                await (Task) methodInfo.Invoke(serviceInstance, arguments);
            }
            else if (taskType.IsAssignableFrom(returnType))
            {
                result = (object) await (dynamic) methodInfo.Invoke(serviceInstance, arguments);
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
    }
}
