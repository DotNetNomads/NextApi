using System;
using System.Net.Http;
using System.Threading.Tasks;
using Abitech.NextApi.Client;
using Abitech.NextApi.Server.Tests.Common;
using Microsoft.AspNetCore.Http.Connections.Client;
using Microsoft.AspNetCore.TestHost;
using Xunit;

namespace Abitech.NextApi.Server.Tests
{
    public class NextApiTest
    {
#pragma warning disable 1998
        protected async Task<NextApiClient> GetClient(string token = null)
#pragma warning restore 1998
        {
            var handler = _server.CreateHandler();
            return new NextApiClientForTests(
                "ws://localhost/nextapi",
                token != null ? new TestTokenProvider(token) : null,
                handler);
        }

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
            NextApiTransport transport = NextApiTransport.Http,
            bool reconnectAutomatically = true, int reconnectDelayMs = 5000) : base(url, tokenProvider, transport,
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

        protected override HttpClient GetHttpClient()
        {
            return _messageHandler != null ? new HttpClient(_messageHandler) : new HttpClient();
        }
    }

    public class TestTokenProvider : INextApiAccessTokenProvider
    {
        private string _token;

        public TestTokenProvider(string token)
        {
            _token = token;
        }

        public async Task<string> ResolveToken()
        {
            return _token;
        }
    }
}
