using System;
using Abitech.NextApi.Server.EfCore.DAL;
using Abitech.NextApi.Server.Entity;
using Abitech.NextApi.Server.Tests.EntityService.Model;

namespace Abitech.NextApi.Server.Tests.EntityService.DAL
{
    public class TestCityRepository : NextApiRepository<TestCity, Guid, TestDbContext>, ITestCityRepository
    {
        public TestCityRepository(TestDbContext dbContext) : base(dbContext)
        {
        }
    }

    public interface ITestCityRepository : INextApiRepository<TestCity, Guid>
    {
    }
}
