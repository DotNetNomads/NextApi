using Abitech.NextApi.Common.Abstractions;
using Abitech.NextApi.Server.EfCore.DAL;
using Abitech.NextApi.TestServer.Model;

namespace Abitech.NextApi.TestServer.DAL
{
    public interface ITestUserRepository : INextApiRepository<TestUser, int>
    {
        
    }
    public class TestUserRepository : NextApiRepository<TestUser, int>, ITestUserRepository
    {
        public TestUserRepository(INextApiDbContext dbContext) : base(dbContext)
        {
        }
    }
}
