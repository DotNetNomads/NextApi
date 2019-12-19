using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Abitech.NextApi.Server.EfCore.DAL;
using Abitech.NextApi.Server.Entity;
using Abitech.NextApi.Server.Tests.EntityService.Model;
using Microsoft.EntityFrameworkCore;

namespace Abitech.NextApi.Server.Tests.EntityService.DAL
{
    public class TestTreeItemRepository: NextApiRepository<TestTreeItem, int, TestDbContext>
    {
        public TestTreeItemRepository(TestDbContext dbContext) : base(dbContext)
        {
        }
    }
}
