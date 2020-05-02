using System;
using System.Linq;
using System.Threading.Tasks;
using Abitech.NextApi.UploadQueue.Common.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace NextApi.Server.UploadQueue.DAL
{
    /// <summary>
    /// UploadQueueDbContext specific extensions
    /// </summary>
    public static class UploadQueueDbHelpers
    {
        /// <summary>
        /// Records logs with changes per column
        /// </summary>
        /// <param name="context"></param>
        /// <param name="entityEntry"></param>
        public static async Task RecordColumnChangesInfo(this UploadQueueDbContext context,
            EntityEntry entityEntry)
        {
            if (context.ColumnChangesLogEnabled &&
                entityEntry.Entity is IUploadQueueEntity entity &&
                entityEntry.State == EntityState.Modified)
            {
                var id = entity.Id;
                var mapping = context.Model.FindEntityType(
                    entityEntry.Entity.GetType());
                var tableName = mapping.GetTableName();
                foreach (var propertyEntry in entityEntry.Properties.Where(p =>
                    p.IsModified))
                {
                    var columnName = propertyEntry.Metadata.Name;
                    await context.ColumnChangesLogs.SetLastColumnChange(tableName, columnName, id,
                        DateTimeOffset.Now);
                }
            }
        }

        /// <summary>
        /// Set last change time for specific column
        /// </summary>
        /// <param name="dbSet"></param>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        /// <param name="id"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public static async Task SetLastColumnChange(this DbSet<ColumnChangesLog> dbSet, string tableName,
            string columnName, Guid id, DateTimeOffset time)
        {
            var columnChangesRecord = await dbSet.FirstOrDefaultAsync(e =>
                e.RowGuid == id &&
                e.TableName == tableName &&
                e.ColumnName == columnName);
            if (columnChangesRecord == null)
            {
                columnChangesRecord = new ColumnChangesLog()
                {
                    RowGuid = id, TableName = tableName, ColumnName = columnName, LastChangedOn = time
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
