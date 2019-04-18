using Abitech.NextApi.Server.EfCore.DAL;
using Abitech.NextApi.Server.Security;
using Abitech.NextApi.Server.Tests.EntityService.Model;
using Microsoft.EntityFrameworkCore;

namespace Abitech.NextApi.Server.Tests.EntityService.DAL
{
    public class TestDbContext : ColumnChangesEnabledNextApiDbContext
    {
        public DbSet<TestCity> Cities { get; set; }
        public DbSet<TestRole> Roles { get; set; }
        public DbSet<TestUser> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<TestUser>(e =>
            {
                e.HasOne(u => u.Role)
                    .WithMany()
                    .HasForeignKey(u => u.RoleId);
                e.HasOne(u => u.City)
                    .WithMany()
                    .HasForeignKey(u => u.CityId);
            });
            base.OnModelCreating(builder);
        }

        public TestDbContext(DbContextOptions options, INextApiUserAccessor nextApiUserAccessor) : base(options, nextApiUserAccessor)
        {
        }
    }
}
