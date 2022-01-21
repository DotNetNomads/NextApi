using System;

namespace NextApi.Common.Entity
{
    /// <summary>
    /// Interface represents entity to be logged with string user id property type
    /// </summary>
    public interface ILoggedSoftDeletableEntity<TUserIdType> : ISoftDeletableEntity
    {
        /// <summary>
        /// Contains id of user removed this entity last time
        /// </summary>
        TUserIdType RemovedById { get; set; }

        /// <summary>
        /// Contains time when entity last time removed
        /// </summary>
        DateTimeOffset? Removed { get; set; }
    }

    /// <summary>
    /// Interface represents entity to be logged with string user id property type
    /// </summary>
    public interface ILoggedSoftDeletableEntity : ILoggedSoftDeletableEntity<string>
    {
    }
}
