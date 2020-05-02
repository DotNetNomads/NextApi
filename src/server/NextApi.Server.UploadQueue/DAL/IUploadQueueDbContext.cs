using Abitech.NextApi.UploadQueue.Common.Entity;
using Microsoft.EntityFrameworkCore;
using NextApi.Server.EfCore.DAL;

namespace NextApi.Server.UploadQueue.DAL
{
    /// <summary>
    /// Context with UploadQueue service support
    /// </summary>
    public interface IUploadQueueDbContext: INextApiDbContext
    {
        /// <summary>
        /// Accessor to ColumnChangesLogs
        /// </summary>
        DbSet<ColumnChangesLog> ColumnChangesLogs { get; set; }

        /// <summary>
        /// Indicates that column changes logger enabled
        /// </summary>
        bool ColumnChangesLogEnabled { get; set; }
    }
}
