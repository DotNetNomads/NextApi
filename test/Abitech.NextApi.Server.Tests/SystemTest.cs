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
    }
}
