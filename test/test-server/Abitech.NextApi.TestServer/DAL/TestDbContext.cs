using Abitech.NextApi.Common.Abstractions;
using Abitech.NextApi.Server.UploadQueue.DAL;
using Abitech.NextApi.TestServer.Model;
using Microsoft.EntityFrameworkCore;

namespace Abitech.NextApi.TestServer.DAL
{
    public interface ITestDbContext : IUploadQueueDbContext
    {
        DbSet<TestCity> Cities { get; set; }
        DbSet<TestRole> Roles { get; set; }
        DbSet<TestUser> Users { get; set; }
        DbSet<TestTreeItem> TestTreeItems { get; set; }
    }

    public class TestDbContext : UploadQueueDbContext, ITestDbContext
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
            builder.Entity<TestCity>().Property(t => t.Id).HasColumnType("binary(16)");
        }

        public TestDbContext(DbContextOptions options, INextApiUserAccessor nextApiUserAccessor) : base(options,
            nextApiUserAccessor)
        {
        }
    }
}
