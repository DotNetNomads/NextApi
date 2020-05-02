using System.Net.Http;
using System.Threading.Tasks;
using NextApi.Common;
using NextApi.Common.Event;

namespace NextApi.Client
{
    /// <summary>
    /// NextApi client
    /// </summary>
    public interface INextApiClient
    {
        /// <summary>
        /// Invoke method of specific service and return result
        /// </summary>
        /// <param name="serviceName">Name of NextApi service</param>
        /// <param name="serviceMethod">Method of service</param>
        /// <param name="arguments">Arguments for method</param>
        /// <typeparam name="T">Execution result</typeparam>
        /// <returns></returns>
        Task<T> Invoke<T>(string serviceName, string serviceMethod, params INextApiArgument[] arguments);

        /// <summary>
        /// Invoke method of specific service
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="serviceMethod"></param>
        /// <param name="arguments"></param>
        /// <returns>nothing</returns>
        Task Invoke(string serviceName, string serviceMethod, params INextApiArgument[] arguments);

        /// <summary>
        /// List of supported permissions
        /// </summary>
        Task<string[]> SupportedPermissions();

        /// <summary>
        /// Get specific NextApi event
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        TEvent GetEvent<TEvent>() where TEvent : INextApiEvent, new();

        /// <summary>
        /// Can be used in integration test purposes
        /// </summary>
        HttpMessageHandler MessageHandler { get; set; }
    }
}
