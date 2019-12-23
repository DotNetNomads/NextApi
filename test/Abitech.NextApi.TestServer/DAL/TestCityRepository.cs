using System;
using Abitech.NextApi.Common.Abstractions;
using Abitech.NextApi.Server.EfCore.DAL;
using Abitech.NextApi.TestServer.Model;

namespace Abitech.NextApi.TestServer.DAL
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
