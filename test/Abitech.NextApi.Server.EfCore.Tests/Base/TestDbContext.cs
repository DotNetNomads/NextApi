using Abitech.NextApi.Server.EfCore.DAL;
using Abitech.NextApi.Server.EfCore.Service;
using Abitech.NextApi.Server.EfCore.Tests.Entity;
using Microsoft.EntityFrameworkCore;

namespace Abitech.NextApi.Server.EfCore.Tests.Base
{
    public class TestDbContext : ColumnChangesEnabledNextApiDbContext
    {
        public TestDbContext(DbContextOptions options, INextApiUserInfoProvider userInfoProvider) : base(options,
            userInfoProvider)
        {
        }

        public DbSet<TestEntity> TestEntities { get; set; }
        public DbSet<TestEntityKeyPredicate> TestEntityKeyPredicates { get; set; }
        public DbSet<TestSoftDeletableEntity> TestSoftDeletableEntities { get; set; }
        public DbSet<TestAuditEntity> TestAuditEntities { get; set; }
        public DbSet<TestColumnChangesEnabledEntity> TestColumnChangesEnabledEntities { get; set; }
    }
}
