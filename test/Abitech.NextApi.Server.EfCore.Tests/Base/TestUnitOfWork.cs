using Abitech.NextApi.Server.EfCore.DAL;
using Abitech.NextApi.Server.Event;

namespace Abitech.NextApi.Server.EfCore.Tests.Base
{
    public class TestUnitOfWork: NextApiUnitOfWork<TestDbContext>
    {
        public TestUnitOfWork(TestDbContext context, INextApiEventManager eventManager) : base(context, eventManager)
        {
        }
    }
}
