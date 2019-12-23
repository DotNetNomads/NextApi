using Abitech.NextApi.Client;
using Abitech.NextApi.Server.Tests;
using Abitech.NextApi.TestClient;
using Abitech.NextApi.Testing;
using Abitech.NextApi.TestServer;

namespace Abitech.NextApi.TestServerCore
{
    public class TestApplication : NextApiApplication<Startup, INextApiClient>
    {
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
