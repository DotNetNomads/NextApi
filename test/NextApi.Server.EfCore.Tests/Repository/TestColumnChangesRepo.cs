using System;
using NextApi.Server.EfCore.DAL;
using NextApi.Server.EfCore.Tests.Entity;

namespace NextApi.Server.EfCore.Tests.Repository
{
    public class TestColumnChangesRepo: EfCoreRepository<TestColumnChangesEnabledEntity, Guid>
    {
        public TestColumnChangesRepo(INextApiDbContext dbContext) : base(dbContext)
        {
        }
    }
}
