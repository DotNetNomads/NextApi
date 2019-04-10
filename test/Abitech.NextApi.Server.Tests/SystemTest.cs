using System.Threading.Tasks;
using Xunit;

namespace Abitech.NextApi.Server.Tests.System
{
    public class SystemTests : NextApiTest
    {
        [Fact]
        public async Task TestSupportedPermissions()
        {
            var client = await GetClient();

            var permissions = await client.SupportedPermissions;
            Assert.Equal(new[] {"permission1", "permission2"}, permissions);
        }

        [Fact]
        public async Task TestNextApiUserAccessor()
        {
            // case: not authorized
            {
                var client = await GetClient();

                var userId = await client.Invoke<int?>("Test", "GetCurrentUser");
                Assert.Null(userId);
            }
            // case: authorized as user 1
            {
                var client = await GetClient("1");
                var userId = await client.Invoke<int?>("Test", "GetCurrentUser");
                Assert.Equal(1, userId.Value);
            }
            // case: authorized as user 2
            {
                var client = await GetClient("2");
                var userId = await client.Invoke<int?>("Test", "GetCurrentUser");
                Assert.Equal(2, userId.Value);
            }
        }
    }
}
