using System;
using Abitech.NextApi.Server.EfCore.Model.Base;

namespace Abitech.NextApi.Server.EfCore.Model
{
    /// <summary>
    /// Column changes log item
    /// </summary>
    public class ColumnChangesLog: IEntity<int>
    {
        // composite key TableName - RowGuid - ColumnName
        /// <summary>
        /// Name of table
        /// </summary>
        public string TableName { get; set; }
        /// <summary>
        /// Guid of row
        /// </summary>
        public Guid RowGuid { get; set; }
        /// <summary>
        /// Name of column
        /// </summary>
        public string ColumnName { get; set; }
        /// <summary>
        /// Last changed on time
        /// </summary>
        public DateTimeOffset LastChangedOn { get; set; }
        // TODO: implement in db context (when entity implements IColumnLoggedEntity write or update row in ColumnChangesLog)
        /// <inheritdoc />
        public int Id { get; set; }
    }
}
