using Abitech.NextApi.Client;
using Abitech.NextApi.TestClient;
using Abitech.NextApi.Testing;
using Abitech.NextApi.Testing.Security;
using Abitech.NextApi.TestServer;
using Microsoft.Extensions.Logging;

namespace Abitech.NextApi.Server.Tests.Base
{
    public class FakeStartup : Startup
    {
    }

    public class TestApplication : NextApiApplication<FakeStartup, INextApiClient>
    {
        protected override ServiceRegistrationMaster GetClientServiceRegistry() =>
            new ServiceRegistrationMaster()
                .Add<ITestService, TestService>()
                .Add<ITestUserService, TestUserService>()
                .Add<ITestTreeItemService, TestTreeItemService>()
                .Add<ITestUploadQueueService, TestUploadQueueService>();

        protected override INextApiClient ClientBuilder(TestTokenProvider tokenProvider, NextApiTransport transport) =>
            new NextApiClient("ws://localhost/nextapi", tokenProvider, transport);

        public TestApplication()
        {
            LogLevel = LogLevel.Debug;
        }
    }
}
