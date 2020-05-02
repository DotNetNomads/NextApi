using NextApi.Client;
using NextApi.Common.Abstractions;
using NextApi.TestServer.DTO;

namespace NextApi.TestClient
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
