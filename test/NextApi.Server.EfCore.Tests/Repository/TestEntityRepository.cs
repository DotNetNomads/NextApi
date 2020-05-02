using System;
using NextApi.Server.EfCore.DAL;
using NextApi.Server.EfCore.Tests.Entity;

namespace NextApi.Server.EfCore.Tests.Repository
{
    public class TestEntityRepository: EfCoreRepository<TestEntity, Guid>
    {
        public TestEntityRepository(INextApiDbContext dbContext) : base(dbContext)
        {
        }
    }
}
