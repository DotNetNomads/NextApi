using Abitech.NextApi.Server.EfCore.DAL;
using Abitech.NextApi.Server.EfCore.Tests.Base;
using Abitech.NextApi.Server.EfCore.Tests.Entity;

namespace Abitech.NextApi.Server.EfCore.Tests.Repository
{
    public class TestEntityPredicatesRepository: NextApiRepository<TestEntityKeyPredicate, string>
    {
        public TestEntityPredicatesRepository(INextApiDbContext dbContext) : base(dbContext)
        {
        }
    }
}
