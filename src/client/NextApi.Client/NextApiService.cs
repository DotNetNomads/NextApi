using System.Threading.Tasks;
using NextApi.Common;
using NextApi.Common.Abstractions;

namespace NextApi.Client
{
    /// <summary>
    /// Base implementation for NextApi service-proxy
    /// </summary>
    public abstract class NextApiService<TClient>: INextApiService where TClient: class, INextApiClient
    {
        /// <summary>
        /// NextApi client
        /// </summary>
        /// <returns></returns>
        protected readonly TClient Client;

        /// <summary>
        /// Service name
        /// </summary>
        protected readonly string ServiceName;

        /// <inheritdoc />
        protected NextApiService(TClient client, string serviceName)
        {
            Client = client;
            ServiceName = serviceName;
        }
        
        /// <summary>
        /// Invoke service method
        /// </summary>
        /// <param name="method">Method name</param>
        /// <param name="arguments">Method arguments</param>
        /// <typeparam name="T">Execution result type</typeparam>
        /// <returns>Execution result</returns>
        protected async Task<T> InvokeService<T>(string method, params INextApiArgument[] arguments)
        {
            return await Client.Invoke<T>(ServiceName, method, arguments);
        }

        /// <summary>
        /// Invoke service method
        /// </summary>
        /// <param name="method">Method name</param>
        /// <param name="arguments">Method arguments</param>
        protected async Task InvokeService(string method, params INextApiArgument[] arguments)
        {
            await Client.Invoke(ServiceName, method, arguments);
        }
    }
}
