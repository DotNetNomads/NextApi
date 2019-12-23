using Abitech.NextApi.Server.EfCore.DAL;
using Abitech.NextApi.TestServer.Model;

namespace Abitech.NextApi.TestServer.DAL
{
    public class TestUserRepository : NextApiRepository<TestUser, int, TestDbContext>
    {
        public TestUserRepository(TestDbContext dbContext) : base(dbContext)
        {
        }
    }
}
