using Abitech.NextApi.Common.Abstractions;
using Abitech.NextApi.Server.EfCore.DAL;
using Abitech.NextApi.Server.Security;
using Abitech.NextApi.Server.Tests.EntityService.Model;
using Abitech.NextApi.Server.UploadQueue.DAL;
using Microsoft.EntityFrameworkCore;

namespace Abitech.NextApi.Server.Tests.EntityService.DAL
{
    public class TestDbContext : UploadQueueDbContext
    {
        public DbSet<TestCity> Cities { get; set; }
        public DbSet<TestRole> Roles { get; set; }
        public DbSet<TestUser> Users { get; set; }
        public DbSet<TestTreeItem> TestTreeItems { get; set; }

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
            builder.Entity<TestTreeItem>(e =>
            {
                e.HasOne(t => t.Parent)
                    .WithMany(tp => tp.Children)
                    .HasForeignKey(t => t.ParentId)
                    .IsRequired(false);
            });
        }

        public TestDbContext(DbContextOptions options, INextApiUserAccessor nextApiUserAccessor) : base(options,
            nextApiUserAccessor)
        {
        }
    }
}
