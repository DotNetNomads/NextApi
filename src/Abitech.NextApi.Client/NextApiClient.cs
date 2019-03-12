using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Abitech.NextApi.Model;
using MessagePack;
using MessagePack.Resolvers;
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
    }

    /// <summary>
    /// NextApi client
    /// </summary>
    public class NextApiClient : INextApiClient
    {
        private HubConnection _connection;
        private readonly HttpMessageHandler _messageHandler;
        private bool _reconnectAutomatically = true;
        private int _reconnectDelayMs = 5000;

        /// <summary>
        /// Initializes NextApi client (for testing)
        /// </summary>
        /// <param name="url"></param>
        /// <param name="accessKey"></param>
        /// <param name="messageHandler"></param>
        public NextApiClient(string url, Func<Task<string>> accessKey,
            HttpMessageHandler messageHandler)
        {
            _messageHandler = messageHandler ?? throw new ArgumentNullException(nameof(messageHandler));
            InitializeClient(url, accessKey);
        }

        /// <summary>
        /// Initializes NextApi client
        /// </summary>
        /// <param name="url">NextApi servers url</param>
        /// <param name="accessKey">Function provides accessKey factory</param>
        /// <param name="reconnectAutomatically">Reconnect when connection fails</param>
        /// <param name="reconnectDelayMs">Delay between connection fail and trying to reconnect</param>
        public NextApiClient(string url, Func<Task<string>> accessKey,
            bool reconnectAutomatically = true, int reconnectDelayMs = 5000)
        {
            _reconnectAutomatically = reconnectAutomatically;
            _reconnectDelayMs = reconnectDelayMs;
            InitializeClient(url, accessKey);
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


        private void InitializeClient(string url, Func<Task<string>> accessKey)
        {
            if (url == null)
            {
                throw new ArgumentNullException(nameof(url));
            }

            _connection = new HubConnectionBuilder()
                .WithUrl(url, o =>
                {
                    // for testing purposes
                    if (_messageHandler != null)
                    {
                        o.HttpMessageHandlerFactory = _ => _messageHandler;
                    }

                    if (accessKey != null)
                    {
                        o.AccessTokenProvider = accessKey;
                    }
                })
                .AddMessagePackProtocol(options =>
                {
                    options.FormatterResolvers = new List<IFormatterResolver>
                        {TypelessContractlessStandardResolver.Instance};
                })
                .Build();

            if (_reconnectAutomatically)
                _connection.Closed += async error =>
                {
                    await Task.Delay(_reconnectDelayMs);
                    await _connection.StartAsync();
                };
        }

        private async Task<HubConnection> GetConnection()
        {
            if (_connection.State == HubConnectionState.Disconnected)
            {
                try
                {
                    await _connection.StartAsync();
                }
                catch (Exception ex)
                {
                    throw new Exception("Problem with NextApi connection occurred. Try later.", ex);
                }
            }

            return _connection;
        }
    }
}
