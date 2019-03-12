using System;
using System.Threading.Tasks;
using Abitech.NextApi.Server.Entity;

namespace Abitech.NextApi.Server.Tests.EntityService.DAL
{
    public class TestUnitOfWork : INextApiUnitOfWork
    {
        private readonly TestDbContext _context;

        public TestUnitOfWork(TestDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task Commit()
        {
            await _context.SaveChangesAsync();
        }
    }
}
