using NextApi.Server.EfCore.DAL;
using NextApi.Server.EfCore.Tests.Entity;

namespace NextApi.Server.EfCore.Tests.Repository
{
    public class TestEntityPredicatesRepository: EfCoreRepository<TestEntityKeyPredicate, string>
    {
        public TestEntityPredicatesRepository(INextApiDbContext dbContext) : base(dbContext)
        {
        }
    }
}
