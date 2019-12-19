using System;
using Abitech.NextApi.Server.EfCore.DAL;
using Abitech.NextApi.Server.EfCore.Tests.Base;
using Abitech.NextApi.Server.EfCore.Tests.Entity;

namespace Abitech.NextApi.Server.EfCore.Tests.Repository
{
    public class TestEntityRepository: NextApiRepository<TestEntity, Guid, TestDbContext>
    {
        public TestEntityRepository(TestDbContext dbContext) : base(dbContext)
        {
        }
    }
}
