using System;
using Abitech.NextApi.Server.EfCore.DAL;
using Abitech.NextApi.Server.EfCore.Tests.Base;
using Abitech.NextApi.Server.EfCore.Tests.Entity;

namespace Abitech.NextApi.Server.EfCore.Tests.Repository
{
    public class TestColumnChangesRepo: NextApiRepository<TestColumnChangesEnabledEntity, Guid, TestDbContext>
    {
        public TestColumnChangesRepo(TestDbContext dbContext) : base(dbContext)
        {
        }
    }
}
