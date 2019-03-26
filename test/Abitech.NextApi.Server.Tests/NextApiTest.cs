using System;
using System.Net.Http;
using System.Threading.Tasks;
using Abitech.NextApi.Client;
using Abitech.NextApi.Server.Tests.Common;
using Microsoft.AspNetCore.Http.Connections.Client;
using Microsoft.AspNetCore.TestHost;

namespace Abitech.NextApi.Server.Tests
{
    public class NextApiTest
    {
#pragma warning disable 1998
        protected async Task<NextApiClient> GetClient()
#pragma warning restore 1998
        {
            if (_client != null) return _client;

            var handler = _server.CreateHandler();
            _client = new NextApiClientForTests(
                "ws://localhost/nextapi", null, handler);

            return _client;
        }

        private NextApiClient _client;
        protected IServiceProvider ServiceProvider;
        private readonly TestServer _server;

        public NextApiTest()
        {
            var factory = new ServerFactory();
            factory.CreateClient();
            _server = factory.Server;
            ServiceProvider = _server.Host.Services;
        }
    }

    public class NextApiClientForTests : NextApiClient
    {
        private readonly HttpMessageHandler _messageHandler;

        public NextApiClientForTests(string url, INextApiAccessTokenProvider tokenProvider,
            HttpMessageHandler messageHandler,
            bool reconnectAutomatically = true, int reconnectDelayMs = 5000) : base(url, tokenProvider,
            reconnectAutomatically, reconnectDelayMs)
        {
            _messageHandler = messageHandler;
        }

        protected override void ConnectionOptionsConfig(HttpConnectionOptions options)
        {
            base.ConnectionOptionsConfig(options);
            if (_messageHandler != null)
            {
                options.HttpMessageHandlerFactory = _ => _messageHandler;
            }
        }
    }
}
