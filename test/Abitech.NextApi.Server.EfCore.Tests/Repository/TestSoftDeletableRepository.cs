using Abitech.NextApi.Server.EfCore.DAL;
using Abitech.NextApi.Server.EfCore.Tests.Base;
using Abitech.NextApi.Server.EfCore.Tests.Entity;

namespace Abitech.NextApi.Server.EfCore.Tests.Repository
{
    public class TestSoftDeletableRepository : NextApiRepository<TestSoftDeletableEntity, int, TestDbContext>
    {
        public TestSoftDeletableRepository(TestDbContext dbContext) : base(dbContext)
        {
        }

        public void EnableSoftDeletable(bool enable)
        {
            this.SoftDeleteEnabled = enable;
        }
    }
}
