using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Abitech.NextApi.Model;
using MessagePack;
using MessagePack.Resolvers;
using Microsoft.AspNetCore.Http.Connections.Client;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
        Task<T> Invoke<T>(string serviceName, string serviceMethod, params NextApiArgument[] arguments);

        /// <summary>
        /// Invoke method of specific service
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="serviceMethod"></param>
        /// <param name="arguments"></param>
        /// <returns>nothing</returns>
        Task Invoke(string serviceName, string serviceMethod, params NextApiArgument[] arguments);

        /// <summary>
        /// List of supported permissions
        /// </summary>
        Task<string[]> SupportedPermissions { get; }
    }

    /// <summary>
    /// NextApi client
    /// </summary>
    public class NextApiClient : INextApiClient
    {
        #region SuportedPermissions

        private string[] _supportedPermissions;

        /// <summary>
        /// List of supported permissions (awaitable)
        /// </summary>
        public Task<string[]> SupportedPermissions => LoadSupportedPermissions();

        private async Task<string[]> LoadSupportedPermissions()
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

        /// <summary>
        /// Initializes NextApi client
        /// </summary>
        /// <param name="url">NextApi servers url</param>
        /// <param name="tokenProvider">Provides accessKey factory</param>
        /// <param name="reconnectAutomatically">Reconnect when connection fails</param>
        /// <param name="reconnectDelayMs">Delay between connection fail and trying to reconnect</param>
        public NextApiClient(string url, INextApiAccessTokenProvider tokenProvider,
            bool reconnectAutomatically = true, int reconnectDelayMs = 5000)
        {
            Url = url ?? throw new ArgumentNullException(nameof(url));
            TokenProvider = tokenProvider;
            ReconnectAutomatically = reconnectAutomatically;
            ReconnectDelayMs = reconnectDelayMs;
        }

        /// <summary>
        /// Initializes client (when first request)
        /// </summary>
        /// <returns></returns>
        protected virtual HubConnection InitializeClient()
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
        /// Invoke method of specific service and return result
        /// </summary>
        /// <param name="serviceName">Name of NextApi service</param>
        /// <param name="serviceMethod">Method of service</param>
        /// <param name="arguments">Arguments for method</param>
        /// <typeparam name="T">Execution result</typeparam>
        /// <returns></returns>
        public async Task<T> Invoke<T>(string serviceName, string serviceMethod, params NextApiArgument[] arguments)
        {
            var command = PrepareCommand(serviceName, serviceMethod, arguments);
            var connection = await GetConnection();
            return await connection.InvokeAsync<T>("ExecuteCommand", command);
        }

        /// <summary>
        /// Invoke method of specific service
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="serviceMethod"></param>
        /// <param name="arguments"></param>
        /// <returns>nothing</returns>
        public async Task Invoke(string serviceName, string serviceMethod, params NextApiArgument[] arguments)
        {
            var command = PrepareCommand(serviceName, serviceMethod, arguments);
            var connection = await GetConnection();
            await connection.InvokeAsync("ExecuteCommand", command);
        }

        private NextApiCommand PrepareCommand(string serviceName, string serviceMethod, NextApiArgument[] arguments)
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
        /// Resolves connection to NextApi server
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        protected async Task<HubConnection> GetConnection()
        {
            if (_connection == null)
            {
                _connection = InitializeClient();
            }

            if (_connection.State == HubConnectionState.Disconnected)
            {
                try
                {
                    await _connection.StartAsync();
                }
                catch (Exception ex)
                {
                    throw new Exception($"Problem with NextApi connection occurred. Try later. ({Url})", ex);
                }
            }

            return _connection;
        }
    }
}
