using Abitech.NextApi.Common.Abstractions;
using Abitech.NextApi.Common.Abstractions.DAL;
using Abitech.NextApi.Server.EfCore.DAL;
using Abitech.NextApi.TestServer.Model;

namespace Abitech.NextApi.TestServer.DAL
{
    public interface ITestUserRepository : IRepo<TestUser, int>
    {
        
    }
    public class TestUserRepository : EfCoreRepository<TestUser, int>, ITestUserRepository
    {
        public TestUserRepository(INextApiDbContext dbContext) : base(dbContext)
        {
        }
    }
}
