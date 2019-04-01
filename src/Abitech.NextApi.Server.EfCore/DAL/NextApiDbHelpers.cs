using System;
using System.Linq;
using System.Threading.Tasks;
using Abitech.NextApi.Server.EfCore.Model;
using Abitech.NextApi.Server.EfCore.Model.Base;
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
        public static void RecordAuditInfo(this DbContext context, int? userId, EntityEntry entityEntry)
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
                        entity.CreatedById = userId;
                        entity.Created = DateTimeOffset.Now;
                        break;
                }
            }
        }

        /// <summary>
        /// Records logs with changes per column
        /// </summary>
        /// <param name="context"></param>
        /// <param name="entityEntry"></param>
        public static async Task RecordColumnChangesInfo(this DbContext context, EntityEntry entityEntry)
        {
            if (entityEntry.Entity is IColumnLoggedEntity
                && entityEntry.State == EntityState.Modified
                && entityEntry.Entity is IRowGuidEnabled entity)
            {
                var columnChangesDbSet = context.Set<ColumnChangesLog>();
                var rowGuid = entity.RowGuid;
                var mapping = context.Model.FindEntityType(
                    entityEntry.Entity.GetType()).Relational();
                var tableName = mapping.TableName;
                foreach (var propertyEntry in entityEntry.Properties.Where(p => p.IsModified))
                {
                    var columnName = propertyEntry.Metadata.Name;

                    var columnChangesRecord = await columnChangesDbSet.FirstOrDefaultAsync(e =>
                        e.RowGuid == rowGuid && e.TableName == tableName && e.ColumnName == columnName);
                    if (columnChangesRecord == null)
                    {
                        columnChangesRecord = new ColumnChangesLog()
                        {
                            RowGuid = rowGuid,
                            TableName = tableName,
                            ColumnName = columnName,
                            LastChangedOn = DateTimeOffset.Now
                        };
                        await columnChangesDbSet.AddAsync(columnChangesRecord);
                    }
                    else
                    {
                        columnChangesRecord.LastChangedOn = DateTimeOffset.Now;
                    }
                }
            }
        }
    }
}
