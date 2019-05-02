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
    public class NextApiTest : IClassFixture<ServerFactory>
    {
        protected ServerFactory Factory;
#pragma warning disable 1998
        protected async Task<NextApiClient> GetClient(NextApiTransport transport, string token = null)
#pragma warning restore 1998
        {
            var handler = Factory.Server.CreateHandler();
            return new NextApiClientForTests(
                "ws://localhost/nextapi",
                token != null ? new TestTokenProvider(token) : null,
                handler, transport);
        }

        public NextApiTest(ServerFactory factory)
        {
            Factory = factory;

            if (Factory.Server == null)
            {
                Factory.CreateClient().Dispose();
            }
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

        protected override async Task<HttpClient> GetHttpClient()
        {
            var client = _messageHandler != null ? new HttpClient(_messageHandler) : new HttpClient();
            if (TokenProvider == null)
            {
                return client;
            }

            var token = await TokenProvider.ResolveToken();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            return client;
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
