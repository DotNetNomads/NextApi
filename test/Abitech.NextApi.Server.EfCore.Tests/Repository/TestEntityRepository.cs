using System;
using Abitech.NextApi.Server.EfCore.DAL;
using Abitech.NextApi.Server.EfCore.Tests.Entity;

namespace Abitech.NextApi.Server.EfCore.Tests.Repository
{
    public class TestEntityRepository: EfCoreRepository<TestEntity, Guid>
    {
        public TestEntityRepository(INextApiDbContext dbContext) : base(dbContext)
        {
        }
    }
}
