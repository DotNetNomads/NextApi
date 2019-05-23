using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
using Newtonsoft.Json.Converters;

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

        #region SignalR

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

        private async Task<T> InvokeSignalR<T>(NextApiCommand command)
        {
            var connection = await GetConnection();
            command.Args = command.Args.Where(arg => arg is NextApiArgument).ToArray();
            try
            {
                var response = await connection.InvokeAsync<NextApiResponse>("ExecuteCommand", command);
                if (response.Error != null)
                {
                    throw NextApiException(response.Error);
                }

                return (T)response.Data;
            }
            catch (Exception e)
            {
                throw new NextApiException(NextApiErrorCode.SignalRError, e.Message);
            }
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

        #endregion

        #region HTTP

        private async Task<string[]> GetPermissionsHttp()
        {
            HttpResponseMessage response;
            using (var client = await GetHttpClient())
            {
                response = await client.GetAsync($"{Url}/http/permissions");
                response.EnsureSuccessStatusCode();
            }

            var stringResp = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<string[]>(stringResp, GetJsonConfig());
        }

        /// <summary>
        /// Gets HTTP client instance for current NextApi client
        /// </summary>
        /// <returns></returns>
        protected virtual async Task<HttpClient> GetHttpClient()
        {
            var client = new HttpClient();
            if (TokenProvider == null)
            {
                return client;
            }

            var token = await TokenProvider.ResolveToken();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            return client;
        }

        private MultipartFormDataContent PrepareRequestForm(NextApiCommand command)
        {
            var args = command.Args.Where(arg => arg is NextApiArgument).ToArray();
            var form = new MultipartFormDataContent
            {
                {new StringContent(command.Service), "Service"},
                {new StringContent(command.Method), "Method"},
                {new StringContent(JsonConvert.SerializeObject(args, GetJsonConfig())), "Args"}
            };
            // send files
            var fileArgs = command.Args.Where(arg => arg is NextApiFileArgument).Cast<NextApiFileArgument>().ToArray();
            var i = 0;
            foreach (var nextApiFileArgument in fileArgs)
            {
                var stream = nextApiFileArgument.FileDataStream ??
                             new FileStream(nextApiFileArgument.FilePath, FileMode.Open);
                var fileName = nextApiFileArgument.FileName ??
                               Path.GetFileName(nextApiFileArgument.FilePath) ?? "noname.bin";
                var name = nextApiFileArgument.FileId;
                form.Add(new StreamContent(stream), name, fileName);
                i++;
            }

            return form;
        }

        class NextApiResponseJsonWrapper<TDataType>
        {
            public bool Success { get; set; }
            public TDataType Data { get; set; }
            public NextApiError Error { get; set; }
        }

        private async Task<T> InvokeHttp<T>(NextApiCommand command)
        {
            var form = PrepareRequestForm(command);
            HttpResponseMessage response;
            try
            {
                using (var client = await GetHttpClient())
                {
                    response = await client.PostAsync($"{Url}/http", form);
                    response.EnsureSuccessStatusCode();
                }
            }
            catch (Exception ex)
            {
                throw new NextApiException(NextApiErrorCode.HttpError, ex.Message);
            }

            // trying to detect file response
            if (response.Content.Headers.ContentType.MediaType == "application/octet-stream")
            {
                if (typeof(T) != typeof(NextApiFileResponse))
                {
                    throw new NextApiException(
                        NextApiErrorCode.IncorrectRequest,
                        "Please specify correct return type for this request. Use NextApiFileResponse."
                    );
                }

                try
                {
                    return await ProcessNextApiFileResponse(response) as dynamic;
                }
                catch (Exception ex)
                {
                    throw new NextApiException(NextApiErrorCode.HttpError, ex.Message);
                }
            }

            // process as normal nextapi response
            var data = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<NextApiResponseJsonWrapper<T>>(data, GetJsonConfig());
            if (!result.Success)
            {
                throw NextApiException(result.Error);
            }

            return result.Data;
        }

        private async Task<NextApiFileResponse> ProcessNextApiFileResponse(HttpResponseMessage response)
        {
            var content = response.Content;
            var fileName = content.Headers.ContentDisposition.FileName;
            var stream = await content.ReadAsStreamAsync();
            return new NextApiFileResponse(fileName, stream);
        }

        private JsonSerializerSettings GetJsonConfig()
        {
            return new JsonSerializerSettings
            {
                Converters = new JsonConverter[] {new StringEnumConverter()}
            };
        }

        #endregion

        /// <inheritdoc />
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
            if (TransportType == NextApiTransport.Http || arguments.Any(a => a is NextApiFileArgument) ||
                typeof(T) == typeof(NextApiFileResponse))
            {
                return await InvokeHttp<T>(command);
            }

            return await InvokeSignalR<T>(command);
        }

        private NextApiException NextApiException(NextApiError error)
        {
            var message = error.Parameters["message"];
            return new NextApiException(error.Code, $"{error.Code} {message}", error.Parameters);
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
                await InvokeHttp<object>(command);
                return;
            }

            await InvokeSignalR<object>(command);
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
