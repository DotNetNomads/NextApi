using Abitech.NextApi.Server.EfCore.DAL;
using Abitech.NextApi.TestServer.Model;

namespace Abitech.NextApi.TestServer.DAL
{
    public class TestTreeItemRepository: NextApiRepository<TestTreeItem, int, TestDbContext>
    {
        public TestTreeItemRepository(TestDbContext dbContext) : base(dbContext)
        {
        }
    }
}
