using Abitech.NextApi.Server.EfCore.DAL;
using Abitech.NextApi.TestServer.Model;

namespace Abitech.NextApi.TestServer.DAL
{
    public class TestTreeItemRepository: NextApiRepository<TestTreeItem, int, ITestDbContext>
    {
        public TestTreeItemRepository(ITestDbContext dbContext) : base(dbContext)
        {
        }
    }
}
