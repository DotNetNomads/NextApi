using System;

namespace Abitech.NextApi.Server.Entity.Model
{
    /// <summary>
    /// Main interface for implement RowGuid based synchronization.
    /// </summary>
    public interface IRowGuidEnabled
    {
        /// <summary>
        /// Guid for entity row
        /// </summary>
        Guid RowGuid { get; set; }
    }
}
