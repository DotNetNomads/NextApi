using Abitech.NextApi.Server.EfCore.DAL;
using Abitech.NextApi.Server.Tests.EntityService.Model;

namespace Abitech.NextApi.Server.Tests.EntityService.DAL
{
    public class TestCityRepository : NextApiRepository<TestCity, int, TestDbContext>
    {
        public TestCityRepository(TestDbContext dbContext) : base(dbContext)
        {
        }
    }
}
