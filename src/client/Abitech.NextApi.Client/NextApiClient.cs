using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Abitech.NextApi.Client.Base;
using Abitech.NextApi.Common;
using Abitech.NextApi.Common.Event;
using MessagePack;
using MessagePack.Resolvers;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Http.Connections.Client;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Abitech.NextApi.Client
{
    /// <summary>
    /// NextApi client
    /// </summary>
    public class NextApiClient : INextApiClient
    {
        /// <summary>
        /// Can be used in integration test purposes
        /// </summary>
        public HttpMessageHandler MessageHandler { get; set; }

        /// <summary>
        /// Transport type for NextApi requests
        /// </summary>
        protected NextApiTransport TransportType { get; }

        #region SuportedPermissions

        private string[] _supportedPermissions;

        /// <summary>
        /// List of supported permissions (awaitable)
        /// </summary>
        public async Task<string[]> SupportedPermissions()
        {
            if (_supportedPermissions != null)
                return _supportedPermissions;
            switch (TransportType)
            {
                case NextApiTransport.SignalR:
                    var connection = await GetConnection();
                    _supportedPermissions = await connection.InvokeAsync<string[]>("GetSupportedPermissions");
                    break;
                case NextApiTransport.Http:
                    _supportedPermissions = await GetPermissionsHttp();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

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

        #region Events

        private readonly Dictionary<string, INextApiEvent> _eventRegistry = new Dictionary<string, INextApiEvent>();

        private TEvent GetEventInternal<TEvent>() where TEvent : INextApiEvent, new()
        {
            var eventName = typeof(TEvent).Name;
            lock (_eventRegistry)
            {
                TEvent eventHandler;
                if (_eventRegistry.ContainsKey(eventName))
                    eventHandler = (TEvent)_eventRegistry[eventName];
                else
                {
                    eventHandler = new TEvent();
                    _eventRegistry.Add(eventName, eventHandler);
                }

                return eventHandler;
            }
        }

#pragma warning disable 1998
        private async Task ProcessNextApiEvent(object[] arg)
#pragma warning restore 1998
        {
            if (arg == null || arg.Length < 1)
                return;
            var message = (NextApiEventMessage)arg[0];

            INextApiEvent handler;
            lock (_eventRegistry)
            {
                if (!_eventRegistry.ContainsKey(message.EventName))
                    return;

                handler = _eventRegistry[message.EventName];
            }

            handler.Publish(message.Data);
        }

        #endregion

        #region SignalR

        /// <summary>
        /// Initializes client (when first request)
        /// </summary>
        /// <returns></returns>
        protected virtual HubConnection InitializeClientSignalR()
        {
            var connection = new HubConnectionBuilder()
                .WithUrl(Url,
                    ConnectionOptionsConfig)
                .AddMessagePackProtocol(options =>
                {
                    options.FormatterResolvers = new List<IFormatterResolver>
                    {
                        TypelessContractlessStandardResolver.Instance
                    };
                })
                .Build();

            if (ReconnectAutomatically)
                connection.Closed += async error =>
                {
                    await Task.Delay(ReconnectDelayMs);
                    await connection.StartAsync();
                };
            connection.On("NextApiEvent", new[] {typeof(NextApiEventMessage)}, ProcessNextApiEvent);
            return connection;
        }

        private async Task<T> InvokeSignalR<T>(NextApiCommand command)
        {
            var connection = await GetConnection();
            command.Args = command.Args.Where(arg => arg is NextApiArgument).ToArray();
            NextApiResponse response;
            try
            {
                response = await connection.InvokeAsync<NextApiResponse>("ExecuteCommand", command);
            }
            catch (Exception e)
            {
                throw new NextApiException(NextApiErrorCode.SignalRError, e.Message);
            }

            if (response.Error != null)
                throw NextApiClientUtils.NextApiException(response.Error);

            return (T)response.Data;
        }

        /// <summary>
        /// Configures http connection
        /// </summary>
        /// <param name="options"></param>
        protected virtual void ConnectionOptionsConfig(HttpConnectionOptions options)
        {
            if (MessageHandler != null)
            {
                options.HttpMessageHandlerFactory = _ => MessageHandler;
            }

            if (TokenProvider != null) options.AccessTokenProvider = TokenProvider.ResolveToken;
        }

        /// <summary>
        /// Resolves connection to NextApi server (via SignalR)
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        protected async Task<HubConnection> GetConnection()
        {
            if (_connection == null) _connection = InitializeClientSignalR();

            if (_connection.State != HubConnectionState.Disconnected)
                return _connection;

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

        #endregion

        #region HTTP

        private async Task<string[]> GetPermissionsHttp()
        {
            using var client = await GetHttpClient();
            var response = await client.GetAsync($"{Url}/http/permissions");
            response.EnsureSuccessStatusCode();

            var stringResp = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<string[]>(stringResp, NextApiClientUtils.GetJsonConfig());
        }

        /// <summary>
        /// Gets HTTP client instance for current NextApi client
        /// </summary>
        /// <returns></returns>
        protected virtual async Task<HttpClient> GetHttpClient()
        {
            var client = new HttpClient(MessageHandler ?? new HttpClientHandler());
            if (TokenProvider == null)
                return client;

            var token = await TokenProvider.ResolveToken();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            return client;
        }

        private async Task<T> InvokeHttp<T>(NextApiCommand command)
        {
            var form = NextApiClientUtils.PrepareRequestForm(command);
            HttpResponseMessage response;
            try
            {
                using var client = await GetHttpClient();
                response = await client.PostAsync($"{Url}/http", form);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                throw new NextApiException(NextApiErrorCode.HttpError, ex.Message);
            }

            // check that response can processed as json
            if (!response.Content.Headers.ContentType.MediaType.Contains("application/json"))
            {
                if (typeof(T) != typeof(NextApiFileResponse))
                    throw new NextApiException(
                        NextApiErrorCode.IncorrectRequest,
                        "Please specify correct return type for this request. Use NextApiFileResponse."
                    );

                try
                {
                    return await NextApiClientUtils.ProcessNextApiFileResponse(response) as dynamic;
                }
                catch (Exception ex)
                {
                    throw new NextApiException(NextApiErrorCode.HttpError, ex.Message);
                }
            }

            // process as normal nextapi response
            var data = await response.Content.ReadAsStringAsync();
            var result =
                JsonConvert.DeserializeObject<NextApiResponseJsonWrapper<T>>(data, NextApiClientUtils.GetJsonConfig());
            if (!result.Success)
                throw NextApiClientUtils.NextApiException(result.Error);

            return result.Data;
        }

        private async Task<NextApiFileResponse> ProcessNextApiFileResponse(HttpResponseMessage response)
        {
            var content = response.Content;
            var fileName = WebUtility.UrlDecode(content.Headers.ContentDisposition.FileName);
            var mimeType = content.Headers.ContentType.MediaType;
            var stream = await content.ReadAsStreamAsync();
            return new NextApiFileResponse(fileName, stream, mimeType);
        }

        #endregion


        /// <inheritdoc />
        public async Task<T> Invoke<T>(string serviceName, string serviceMethod, params INextApiArgument[] arguments)
        {
            var command = NextApiClientUtils.PrepareCommand(serviceName, serviceMethod, arguments);
            if (TransportType == NextApiTransport.Http || arguments.Any(a => a is NextApiFileArgument) ||
                typeof(T) == typeof(NextApiFileResponse))
                return await InvokeHttp<T>(command);

            return await InvokeSignalR<T>(command);
        }


        /// <inheritdoc />
        public async Task Invoke(string serviceName, string serviceMethod, params INextApiArgument[] arguments)
        {
            var command = NextApiClientUtils.PrepareCommand(serviceName, serviceMethod, arguments);
            if (TransportType == NextApiTransport.Http || arguments.Any(a => a is NextApiFileArgument))
            {
                await InvokeHttp<object>(command);
                return;
            }

            await InvokeSignalR<object>(command);
        }

        /// <inheritdoc />
        public TEvent GetEvent<TEvent>() where TEvent : INextApiEvent, new() => GetEventInternal<TEvent>();
    }
}
