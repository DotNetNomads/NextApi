using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Abitech.NextApi.Model.UploadQueue
{
    /// <summary>
    /// Model to send to an implementation of UploadQueueService
    /// </summary>
    public class UploadQueueDto
    {
        /// <summary>
        /// UploadQueue item identifier
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();
        /// <summary>
        /// RowGuid of the created, updated or deleted entity.
        /// NOTE! Entity has to implement IRowGuidEnabled interface or have a Guid property
        /// </summary>
        public Guid EntityRowGuid { get; set; } = default;
        /// <summary>
        /// Model name
        /// </summary>
        public string EntityName { get; set; }
        /// <summary>
        /// Modified column name
        /// </summary>
        public string ColumnName { get; set; }
        /// <summary>
        /// Operation type
        /// </summary>
        public OperationType OperationType { get; set; }
        /// <summary>
        /// New value of the specified column (property)
        /// </summary>
        public object NewValue { get; set; }
        /// <summary>
        /// Time when the operation occured
        /// </summary>
        public DateTimeOffset OccuredAt { get; set; }
        /// <summary>
        /// Extra objects
        /// </summary>
        public object[] Extras { get; set; }
    }

    /// <summary>
    /// Operation type enum
    /// </summary>
    public enum OperationType
    {
#pragma warning disable 1591
        None,
        Create,
        Update,
        Delete
#pragma warning restore 1591
    }
}
