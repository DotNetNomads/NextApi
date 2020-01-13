using Abitech.NextApi.Client;
using Abitech.NextApi.Common.Abstractions;
using Abitech.NextApi.TestServer.DTO;

namespace Abitech.NextApi.TestClient
{
    public interface ITestTreeItemService : INextApiTreeEntityService<TestTreeItemDto, int, int?>
    {
    }

    public class TestTreeItemService : NextApiTreeEntityService<TestTreeItemDto, int, int?, INextApiClient>,
        ITestTreeItemService
    {
        public TestTreeItemService(INextApiClient client) : base(client, "TestTreeItem")
        {
        }
    }
}
