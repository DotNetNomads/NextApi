using NextApi.TestClient;
using NextApi.TestServer;
using NextApi.Client;
using NextApi.Common.Serialization;
using NextApi.Testing;
using NextApi.Testing.Security;

namespace NextApi.Server.Tests.Base
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

        protected override INextApiClient ClientBuilder(TestTokenProvider tokenProvider, NextApiTransport transport, SerializationType httpSerializationType = SerializationType.Json) =>
            new NextApiClient("ws://localhost/nextapi", tokenProvider, transport)
            {
                HttpSerializationType = httpSerializationType
            };

        public TestApplication()
        {
//            LogLevel = LogLevel.Debug;
        }
    }
}
