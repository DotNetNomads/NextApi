using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Abitech.NextApi.Model;
using MessagePack;
using MessagePack.Resolvers;
using Microsoft.AspNetCore.Http.Connections.Client;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Abitech.NextApi.Client
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
    }

    /// <summary>
    /// NextApi client
    /// </summary>
    public class NextApiClient : INextApiClient
    {
        public NextApiTransport TransportType { get; }

        #region SuportedPermissions

        private string[] _supportedPermissions;

        /// <summary>
        /// List of supported permissions (awaitable)
        /// </summary>
        public async Task<string[]> SupportedPermissions()
        {
            if (_supportedPermissions != null)
                return _supportedPermissions;
            var connection = await GetConnection();
            _supportedPermissions = await connection.InvokeAsync<string[]>("GetSupportedPermissions");
            return _supportedPermissions;
        }

        #endregion

        /// <summary>
        /// NextApi server url
        /// </summary>
        protected readonly string Url;

        /// <summary>
        /// Access token provider
        /// </summary>
        protected readonly INextApiAccessTokenProvider TokenProvider;

        /// <summary>
        /// Reconnect automatically
        /// </summary>
        protected bool ReconnectAutomatically;

        /// <summary>
        /// Delay between reconnect tries
        /// </summary>
        protected int ReconnectDelayMs;

        private HubConnection _connection;
        private HttpClient _httpClient;

        /// <summary>
        /// Initializes NextApi client
        /// </summary>
        /// <param name="url">NextApi servers url</param>
        /// <param name="tokenProvider">Provides accessKey factory</param>
        /// <param name="transportType">Option to set transport type for this client</param>
        /// <param name="reconnectAutomatically">Reconnect when connection fails</param>
        /// <param name="reconnectDelayMs">Delay between connection fail and trying to reconnect</param>
        /// <remarks>Transport type automatically changed by client in case of request with NextApiFileArgument</remarks>
        public NextApiClient(
            string url,
            INextApiAccessTokenProvider tokenProvider,
            NextApiTransport transportType = NextApiTransport.SignalR,
            bool reconnectAutomatically = true,
            int reconnectDelayMs = 5000)
        {
            TransportType = transportType;
            Url = url ?? throw new ArgumentNullException(nameof(url));
            TokenProvider = tokenProvider;
            ReconnectAutomatically = reconnectAutomatically;
            ReconnectDelayMs = reconnectDelayMs;
        }

        /// <summary>
        /// Initializes client (when first request)
        /// </summary>
        /// <returns></returns>
        protected virtual HubConnection InitializeClientSignalR()
        {
            var connection = new HubConnectionBuilder()
                .WithUrl(Url, ConnectionOptionsConfig)
                .AddMessagePackProtocol(options =>
                {
                    options.FormatterResolvers = new List<IFormatterResolver>
                        {TypelessContractlessStandardResolver.Instance};
                })
                .Build();

            if (ReconnectAutomatically)
                connection.Closed += async error =>
                {
                    await Task.Delay(ReconnectDelayMs);
                    await connection.StartAsync();
                };
            return connection;
        }

        /// <summary>
        /// Gets HTTP client instance for current NextApi client
        /// </summary>
        /// <returns></returns>
        protected virtual HttpClient GetHttpClient()
        {
            return _httpClient ?? (_httpClient = new HttpClient());
        }

        /// <summary>
        /// Invoke method of specific service and return result
        /// </summary>
        /// <param name="serviceName">Name of NextApi service</param>
        /// <param name="serviceMethod">Method of service</param>
        /// <param name="arguments">Arguments for method</param>
        /// <typeparam name="T">Execution result</typeparam>
        /// <returns></returns>
        public async Task<T> Invoke<T>(string serviceName, string serviceMethod, params INextApiArgument[] arguments)
        {
            var command = PrepareCommand(serviceName, serviceMethod, arguments);
            if (TransportType == NextApiTransport.Http || arguments.Any(a => a is NextApiFileArgument))
            {
                return await InvokeHttp<T>(command);
            }

            return await InvokeSignalR<T>(command);
        }

        private async Task<T> InvokeSignalR<T>(NextApiCommand command)
        {
            var connection = await GetConnection();
            return await connection.InvokeAsync<T>("ExecuteCommand", command);
        }

        private async Task<T> InvokeHttp<T>(NextApiCommand command)
        {
            var client = GetHttpClient();
            if (TokenProvider != null)
            {
                var token = await TokenProvider.ResolveToken();
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            }

            var form = new MultipartFormDataContent
            {
                {new StringContent(command.Service), "Service"},
                {new StringContent(command.Method), "Method"},
                {new StringContent(JsonConvert.SerializeObject(command.Args)), "Args"}
            };
            var response = await client.PostAsync($"{Url}/http", form);
            var data = await response.Content.ReadAsStringAsync();
            return ;
        }

        private async Task InvokeSignalR(NextApiCommand command)
        {
            var connection = await GetConnection();
            await connection.InvokeAsync("ExecuteCommand", command);
        }

        private async Task InvokeHttp(NextApiCommand command)
        {
        }

        /// <summary>
        /// Invoke method of specific service
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="serviceMethod"></param>
        /// <param name="arguments"></param>
        /// <returns>nothing</returns>
        public async Task Invoke(string serviceName, string serviceMethod, params INextApiArgument[] arguments)
        {
            var command = PrepareCommand(serviceName, serviceMethod, arguments);
            if (TransportType == NextApiTransport.Http || arguments.Any(a => a is NextApiFileArgument))
            {
                await InvokeHttp(command);
                return;
            }

            await InvokeSignalR(command);
        }

        private NextApiCommand PrepareCommand(string serviceName, string serviceMethod, INextApiArgument[] arguments)
        {
            return new NextApiCommand
            {
                Args = arguments,
                Method = serviceMethod,
                Service = serviceName
            };
        }

        /// <summary>
        /// Configures http connection
        /// </summary>
        /// <param name="options"></param>
        protected virtual void ConnectionOptionsConfig(HttpConnectionOptions options)
        {
            if (TokenProvider != null)
            {
                options.AccessTokenProvider = TokenProvider.ResolveToken;
            }
        }

        /// <summary>
        /// Resolves connection to NextApi server (via SignalR)
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        protected async Task<HubConnection> GetConnection()
        {
            if (_connection == null)
            {
                _connection = InitializeClientSignalR();
            }

            if (_connection.State != HubConnectionState.Disconnected)
            {
                return _connection;
            }

            try
            {
                await _connection.StartAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Problem with NextApi connection occurred. Try later. ({Url})", ex);
            }

            return _connection;
        }
    }

    /// <summary>
    /// Supported transport types for NextApi
    /// </summary>
    public enum NextApiTransport
    {
        /// <summary>
        /// SignalR
        /// </summary>
        SignalR,

        /// <summary>
        /// HTTP
        /// </summary>
        Http
    }
}
