using Abitech.NextApi.Client;
using Abitech.NextApi.Common.Abstractions;
using Abitech.NextApi.TestServer.DTO;
using Abitech.NextApi.TestServer.Model;

namespace Abitech.NextApi.TestClient
{
    public interface ITestTreeItemService : INextApiEntityService<TestTreeItemDto, int>
    {
    }

    public class TestTreeItemService : NextApiEntityService<TestTreeItemDto, int, INextApiClient>, ITestTreeItemService
    {
        public TestTreeItemService(INextApiClient client) : base(client, "TestTreeItem")
        {
        }
    }
}
