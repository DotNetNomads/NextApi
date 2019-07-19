using System;

namespace Abitech.NextApi.Model.DTO
{
    /// <summary>
    /// Basic interface for RowGuid DTO
    /// </summary>
    public interface IRowGuidEnabledDto
    {
        /// <summary>
        /// Guid for entity row
        /// </summary>
        Guid RowGuid { get; set; }
    }
}
