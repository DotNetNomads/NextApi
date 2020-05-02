using NextApi.Common.Abstractions.DAL;
using NextApi.Server.EfCore.DAL;
using NextApi.TestServer.Model;

namespace NextApi.TestServer.DAL
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
