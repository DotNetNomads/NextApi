using System.Threading.Tasks;
using Abitech.NextApi.Common.Abstractions;
using Abitech.NextApi.Server.EfCore.DAL;
using Abitech.NextApi.UploadQueue.Common.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Abitech.NextApi.Server.UploadQueue.DAL
{
    /// <summary>
    /// DbContext with UploadQueue service support
    /// </summary>
    public abstract class UploadQueueDbContext : NextApiDbContext, IUploadQueueDbContext
    {
        /// <inheritdoc />
        public DbSet<ColumnChangesLog> ColumnChangesLogs { get; set; }

        /// <inheritdoc />
        public bool ColumnChangesLogEnabled { get; set; } = true;


        /// <inheritdoc />
        protected override async Task HandleTrackedEntity(EntityEntry entityEntry)
        {
            await base.HandleTrackedEntity(entityEntry);
            await this.RecordColumnChangesInfo(entityEntry);
        }

        /// <inheritdoc />
        protected UploadQueueDbContext(DbContextOptions options,
            INextApiUserAccessor nextApiUserAccessor) : base(options, nextApiUserAccessor)
        {
        }

        /// <inheritdoc />
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<ColumnChangesLog>(config =>
        {
                config.HasIndex("RowGuid", "TableName", "ColumnName")
                    .IsUnique();
                config.Property(p => p.RowGuid).IsRequired();
                config.Property(p => p.TableName).IsRequired();
                config.Property(p => p.ColumnName).IsRequired();
                config.Property(p => p.LastChangedOn);
            });
        }
    }
}
