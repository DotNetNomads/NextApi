using System;

namespace Abitech.NextApi.Server.EfCore.Model.Base
{
    /// <summary>
    /// Base interface for entity
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public interface IEntity<TKey>
    {
        /// <summary>
        /// Db identifier
        /// </summary>
        TKey Id { get; set; }
    }

    /// <summary>
    /// Base interface for entity with row guid
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public interface IGuidEntity<TKey> : IEntity<TKey>, IRowGuidEnabled
    {
    }

    public interface IRowGuidEnabled
    {
        /// <summary>
        /// Guid for entity row
        /// </summary>
        Guid RowGuid { get; set; }
    }
}
