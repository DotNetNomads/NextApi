using Abitech.NextApi.Client;
using Abitech.NextApi.TestClient;
using Abitech.NextApi.Testing;
using Abitech.NextApi.TestServer;
using Microsoft.Extensions.Logging;

namespace Abitech.NextApi.Server.Tests.Base
{
    public class FakeStartup : Startup
    {
    }

    public class TestApplication : NextApiApplication<FakeStartup, INextApiClient>
    {
        public TestApplication()
        {
            LogLevel = LogLevel.Critical;
        }

        protected override ServiceRegistrationMaster GetClientServiceRegistry() =>
            new ServiceRegistrationMaster()
                .Add<ITestService, TestService>()
                .Add<ITestUserService, TestUserService>()
                .Add<ITestTreeItemService, TestTreeItemService>()
                .Add<ITestUploadQueueService, TestUploadQueueService>();

        protected override INextApiClient ClientBuilder(TestTokenProvider tokenProvider, NextApiTransport transport) =>
            new NextApiClient("http://localhost/nextapi", tokenProvider, transport);
    }
}
