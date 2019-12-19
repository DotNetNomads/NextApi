using Abitech.NextApi.Server.EfCore.Tests.Entity;
using Abitech.NextApi.Server.Security;
using Abitech.NextApi.Server.UploadQueue.DAL;
using Microsoft.EntityFrameworkCore;

namespace Abitech.NextApi.Server.EfCore.Tests.Base
{
    public class TestDbContext : UploadQueueDbContext
    {

        public DbSet<TestEntity> TestEntities { get; set; }
        public DbSet<TestEntityKeyPredicate> TestEntityKeyPredicates { get; set; }
        public DbSet<TestSoftDeletableEntity> TestSoftDeletableEntities { get; set; }
        public DbSet<TestAuditEntity> TestAuditEntities { get; set; }
        public DbSet<TestColumnChangesEnabledEntity> TestColumnChangesEnabledEntities { get; set; }

        public TestDbContext(DbContextOptions options, INextApiUserAccessor nextApiUserAccessor) : base(options, nextApiUserAccessor)
        {
        }
    }
}
