using NextApi.Server.EfCore.DAL;
using NextApi.Server.EfCore.Tests.Entity;

namespace NextApi.Server.EfCore.Tests.Repository
{
    public class TestSoftDeletableRepository : EfCoreRepository<TestSoftDeletableEntity, int>
    {
        public TestSoftDeletableRepository(INextApiDbContext dbContext) : base(dbContext)
        {
        }

        public void EnableSoftDeletable(bool enable)
        {
            this.SoftDeleteEnabled = enable;
        }
    }
}
