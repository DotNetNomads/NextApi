using System;
using System.Linq;
using System.Linq.Expressions;
using Abitech.NextApi.Server.EfCore.DAL;
using Abitech.NextApi.Server.Entity;
using Abitech.NextApi.Server.Tests.EntityService.Model;

namespace Abitech.NextApi.Server.Tests.EntityService.DAL
{
    public class TestCityRepository : NextApiRepository<TestCity, int, TestDbContext>, ITestCityRepository
    {
        public TestCityRepository(TestDbContext dbContext) : base(dbContext)
        {
        }

        public override int GetEntityId(TestCity entity)
        {
            return entity.CityId;
        }

        public override Expression<Func<TestCity, bool>> KeyPredicate(int id)
        {
            return entity => entity.CityId == id;
        }

        public override Expression<Func<TestCity, bool>> KeyPredicate(int[] keys)
        {
            return entity => keys.Contains(entity.CityId);
        }
    }

    public interface ITestCityRepository : INextApiRepository<TestCity, int>
    {
    }
}
