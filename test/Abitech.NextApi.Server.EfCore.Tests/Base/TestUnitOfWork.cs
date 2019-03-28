using Abitech.NextApi.Server.EfCore.DAL;

namespace Abitech.NextApi.Server.EfCore.Tests.Base
{
    public class TestUnitOfWork: NextApiUnitOfWork<TestDbContext>
    {
        public TestUnitOfWork(TestDbContext context) : base(context)
        {
        }
    }
}
