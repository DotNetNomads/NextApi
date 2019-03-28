using Abitech.NextApi.Server.EfCore.DAL;
using Abitech.NextApi.Server.EfCore.Tests.Entity;
using Microsoft.EntityFrameworkCore;

namespace Abitech.NextApi.Server.EfCore.Tests.Base
{
    public class TestDbContext : NextApiDbContext
    {
        public TestDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<TestEntity> TestEntities { get; set; }
        public DbSet<TestEntityKeyPredicate> TestEntityKeyPredicates { get; set; }
    }
}
