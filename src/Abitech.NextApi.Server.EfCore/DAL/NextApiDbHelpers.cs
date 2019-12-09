using System;
using System.Linq;
using System.Threading.Tasks;
using Abitech.NextApi.Server.EfCore.Model;
using Abitech.NextApi.Server.EfCore.Model.Base;
using Abitech.NextApi.Server.EfCore.Service;
using Abitech.NextApi.Server.Entity;
using Abitech.NextApi.Server.Entity.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Abitech.NextApi.Server.EfCore.DAL
{
    /// <summary>
    /// Functional helpers for NextApi DbContext
    /// </summary>
    public static class NextApiDbHelpers
    {
        /// <summary>
        /// Records audit info about user who created the entity
        /// </summary>
        /// <param name="context"></param>
        /// <param name="userId"></param>
        /// <param name="entityEntry"></param>
        public static void RecordAuditInfo(this NextApiDbContext context, int? userId, EntityEntry entityEntry)
        {
            if (entityEntry.Entity is ILoggedEntity entity)
            {
                switch (entityEntry.State)
                {
                    case EntityState.Modified:
                        entity.UpdatedById = userId;
                        entity.Updated = DateTimeOffset.Now;
                        break;
                    case EntityState.Added:
                        if (!entity.CreatedById.HasValue)
                            entity.CreatedById = userId;
                        if (!entity.Created.HasValue)
                            entity.Created = DateTimeOffset.Now;
                        break;
                }
            }
        }

        /// <summary>
        /// Creates RowGuid in the entity if it does not exist
        /// </summary>
        /// <param name="context"></param>
        /// <param name="entityEntry"></param>
        public static void CheckRowGuid(this NextApiDbContext context, EntityEntry entityEntry)
        {
            if (entityEntry.Entity is IRowGuidEnabled rowGuidEnabled)
            {
                switch (entityEntry.State)
                {
                    case EntityState.Added:
                        if (rowGuidEnabled.RowGuid == null)
                            rowGuidEnabled.RowGuid = Guid.NewGuid();
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Records logs with changes per column
        /// </summary>
        /// <param name="context"></param>
        /// <param name="entityEntry"></param>
        public static async Task RecordColumnChangesInfo(this ColumnChangesEnabledNextApiDbContext context,
            EntityEntry entityEntry)
        {
            if (context.ColumnChangesLogEnabled &&
                entityEntry.Entity is IColumnLoggedEntity &&
                entityEntry.State == EntityState.Modified &&
                entityEntry.Entity is IRowGuidEnabled entity)
            {
                var rowGuid = entity.RowGuid;
                var mapping = context.Model.FindEntityType(
                    entityEntry.Entity.GetType());
                var tableName = mapping.GetTableName();
                foreach (var propertyEntry in entityEntry.Properties.Where(p =>
                    p.IsModified && p.Metadata.Name != "RowGuid"))
                {
                    var columnName = propertyEntry.Metadata.Name;
                    await context.ColumnChangesLogs.SetLastColumnChange(tableName, columnName, rowGuid,
                        DateTimeOffset.Now);
                }
            }
        }

        public static async Task SetLastColumnChange(this DbSet<ColumnChangesLog> dbSet, string tableName,
            string columnName, Guid rowGuid, DateTimeOffset time)
        {
            var columnChangesRecord = await dbSet.FirstOrDefaultAsync(e =>
                e.RowGuid == rowGuid &&
                e.TableName == tableName &&
                e.ColumnName == columnName);
            if (columnChangesRecord == null)
            {
                columnChangesRecord = new ColumnChangesLog()
                {
                    RowGuid = rowGuid, TableName = tableName, ColumnName = columnName, LastChangedOn = time
                };
                await dbSet.AddAsync(columnChangesRecord);
            }
            else
            {
                columnChangesRecord.LastChangedOn = time;
            }
        }
    }
}
