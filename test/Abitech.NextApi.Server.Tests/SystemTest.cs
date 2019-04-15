using System.Linq;
using System.Threading.Tasks;
using Abitech.NextApi.Model.Filtering;
using Abitech.NextApi.Server.Entity;
using Abitech.NextApi.Server.Tests.Filtering;
using Xunit;

namespace Abitech.NextApi.Server.Tests.System
{
    public class SystemTests : NextApiTest
    {
        [Fact]
        public async Task TestSupportedPermissions()
        {
            var client = await GetClient();

            var permissions = await client.SupportedPermissions();
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

        [Fact]
        public async Task FilterTests()
        {
            var data = TestSource.GetData();

            var filter = new FilterBuilder()
                .Contains("ReferenceModel.Name", "Model")
                .Or(f => f
                    .MoreThan<int>("Number", 1)
                    .LessThan<int>("Number", 2)
                )
                .In<int>("Number", new[] {5, 6, 10})
                .Build();

            var expression = filter.ToLambdaFilter<TestModel>();

            var filtered = data.Where(expression).ToList();

            Assert.True(filtered.Count == 3);
            Assert.True(filtered.All(e => e.Number == 5 || e.Number == 6 || e.Number == 10));
        }
    }
}
