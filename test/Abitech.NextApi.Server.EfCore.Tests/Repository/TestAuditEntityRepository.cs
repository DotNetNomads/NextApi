using Abitech.NextApi.Server.EfCore.DAL;
using Abitech.NextApi.Server.EfCore.Tests.Base;
using Abitech.NextApi.Server.EfCore.Tests.Entity;

namespace Abitech.NextApi.Server.EfCore.Tests.Repository
{
    public class TestAuditEntityRepository: NextApiRepository<TestAuditEntity, int, TestDbContext>
    {
        public TestAuditEntityRepository(TestDbContext dbContext) : base(dbContext)
        {
        }
    }
}
