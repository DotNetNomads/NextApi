using Abitech.NextApi.Server.EfCore.DAL;
using Abitech.NextApi.Server.EfCore.Tests.Base;
using Abitech.NextApi.Server.EfCore.Tests.Entity;
using Abitech.NextApi.Server.Entity;

namespace Abitech.NextApi.Server.EfCore.Tests.Repository
{
    public class TestColumnChangesRepo: NextApiRepository<TestColumnChangesEnabledEntity, int, TestDbContext>
    {
        public TestColumnChangesRepo(TestDbContext dbContext) : base(dbContext)
        {
        }
    }
}
