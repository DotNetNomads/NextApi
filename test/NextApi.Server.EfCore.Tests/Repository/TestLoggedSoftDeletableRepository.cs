using System.Threading.Tasks;
using NextApi.Server.EfCore.DAL;
using NextApi.Server.EfCore.Tests.Entity;

namespace NextApi.Server.EfCore.Tests.Repository
{
    public class TestLoggedSoftDeletableRepository : EfCoreRepository<TestLoggedSoftDeletableEntity, int>
    {
        public TestLoggedSoftDeletableRepository(INextApiDbContext dbContext) : base(dbContext)
        {
        }

        public void EnableSoftDeletable(bool enable)
        {
            this.SoftDeleteEnabled = enable;
        }
    }
}
