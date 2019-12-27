using System;
using Abitech.NextApi.Server.EfCore.DAL;
using Abitech.NextApi.Server.EfCore.Tests.Base;
using Abitech.NextApi.Server.EfCore.Tests.Entity;

namespace Abitech.NextApi.Server.EfCore.Tests.Repository
{
    public class TestColumnChangesRepo: EfCoreRepository<TestColumnChangesEnabledEntity, Guid>
    {
        public TestColumnChangesRepo(INextApiDbContext dbContext) : base(dbContext)
        {
        }
    }
}
