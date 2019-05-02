using System.Linq;
using System.Threading.Tasks;
using Abitech.NextApi.Client;
using Abitech.NextApi.Model.Filtering;
using Abitech.NextApi.Server.Entity;
using Abitech.NextApi.Server.Tests.Common;
using Abitech.NextApi.Server.Tests.Filtering;
using Xunit;

namespace Abitech.NextApi.Server.Tests
{
    public class SystemTests : NextApiTest
    {
        [Theory]
        [InlineData(NextApiTransport.Http)]
        [InlineData(NextApiTransport.SignalR)]
        public async Task TestSupportedPermissions(NextApiTransport transport)
        {
            var client = await GetClient(transport);

            var permissions = await client.SupportedPermissions();
            Assert.Equal(new[] {"permission1", "permission2"}, permissions);
        }

        [Theory]
        [InlineData(NextApiTransport.Http)]
        [InlineData(NextApiTransport.SignalR)]
        public async Task TestNextApiUserAccessor(NextApiTransport transport)
        {
            // case: not authorized
            {
                var client = await GetClient(transport);

                var userId = await client.Invoke<int?>("Test", "GetCurrentUser");
                Assert.Null(userId);
            }
            // case: authorized as user 1
            {
                var client = await GetClient(transport, "1");
                var userId = await client.Invoke<int?>("Test", "GetCurrentUser");
                Assert.Equal(1, userId.Value);
            }
            // case: authorized as user 2
            {
                var client = await GetClient(transport, "2");
                var userId = await client.Invoke<int?>("Test", "GetCurrentUser");
                Assert.Equal(2, userId.Value);
            }
        }

        [Fact]
        public async Task FilterTests()
        {
            var data = TestSource.GetData();

            // filter: entity => entity.ReferenceModel.Name.Contains("Model") &&
            //                   (entity.Number == 1 || entity.Number == 2) &&
            //                   (new [] {5,6,10}).Contains("Number")
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
