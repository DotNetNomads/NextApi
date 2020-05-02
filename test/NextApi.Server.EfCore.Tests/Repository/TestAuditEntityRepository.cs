using NextApi.Server.EfCore.DAL;
using NextApi.Server.EfCore.Tests.Entity;

namespace NextApi.Server.EfCore.Tests.Repository
{
    public class TestAuditEntityRepository: EfCoreRepository<TestAuditEntity, int>
    {
        public TestAuditEntityRepository(INextApiDbContext dbContext) : base(dbContext)
        {
        }
    }
}
