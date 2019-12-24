using System.Threading.Tasks;
using Abitech.NextApi.Client;
using Abitech.NextApi.Server.Tests.Base;
using Abitech.NextApi.TestClient;
using Xunit;

namespace Abitech.NextApi.Server.Tests
{
    public class SystemTests : NextApiTest<TestApplication, INextApiClient>
    {
        public SystemTests()
        {
        }

        [Theory]
        [InlineData(NextApiTransport.Http)]
        [InlineData(NextApiTransport.SignalR)]
        public async Task TestSupportedPermissions(NextApiTransport transport)
        {
            var client = App.ResolveClient(null, transport);

            var permissions = await client.SupportedPermissions();
            Assert.Equal(new[] {"permission1", "permission2"}, permissions);
        }

        [Theory]
        [InlineData(NextApiTransport.Http)]
        [InlineData(NextApiTransport.SignalR)]
        public void TestNextApiUserAccessor(NextApiTransport transport)
        {
            // case: not authorized
            {
                var service = App.ResolveService<ITestService>(null, transport);

                var userId = service.GetCurrentUser();
                Assert.Null(userId);
            }
            // case: authorized as user 1
            {
                var service = App.ResolveService<ITestService>("1", transport);
                var userId = service.GetCurrentUser();
                Assert.Equal(1, userId.Value);
            }
            // case: authorized as user 2
            {
                var service = App.ResolveService<ITestService>("2", transport);
                var userId = service.GetCurrentUser();
                Assert.Equal(2, userId.Value);
            }
        }
    }
}
