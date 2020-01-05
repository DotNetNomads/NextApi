using Abitech.NextApi.Client;
using Abitech.NextApi.Common.Abstractions;
using Abitech.NextApi.TestServer.DTO;
using Abitech.NextApi.TestServer.Model;

namespace Abitech.NextApi.TestClient
{
    public interface ITestTreeItemService : INextApiTreeEntityService<TestTreeItemDto, int>
    {
    }

    public class TestTreeItemService : NextApiTreeEntityService<TestTreeItemDto, int, INextApiClient>,
        ITestTreeItemService
    {
        public TestTreeItemService(INextApiClient client) : base(client, "TestTreeItem")
        {
        }
    }
}
