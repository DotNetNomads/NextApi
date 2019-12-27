using Abitech.NextApi.Server.EfCore.DAL;
using Abitech.NextApi.Server.EfCore.Tests.Base;
using Abitech.NextApi.Server.EfCore.Tests.Entity;

namespace Abitech.NextApi.Server.EfCore.Tests.Repository
{
    public class TestAuditEntityRepository: EfCoreRepository<TestAuditEntity, int>
    {
        public TestAuditEntityRepository(INextApiDbContext dbContext) : base(dbContext)
        {
        }
    }
}
