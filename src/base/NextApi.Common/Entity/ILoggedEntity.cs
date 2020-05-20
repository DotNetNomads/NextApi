using System;

namespace NextApi.Common.Entity
{
    /// <summary>
    /// Interface represents entity to be logged
    /// <remarks>WARNING! If you are going to use generic arguments other than string,
    /// you will have to override NextApiDbContext.RecordAuditInfo</remarks>
    /// </summary>
    public interface ILoggedEntity<TUserIdType>
    {

        /// <summary>
        /// Contains id of user created this entity
        /// </summary>
        TUserIdType CreatedById { get; set; }
        /// <summary>
        /// Contains creation time
        /// </summary>
        DateTimeOffset? Created { get; set; }
        /// <summary>
        /// Contains id of user updated this entity last time
        /// </summary>
        TUserIdType UpdatedById { get; set; }
        /// <summary>
        /// Contains time when entity last time updated
        /// </summary>
        DateTimeOffset? Updated { get; set; }
    }

    /// <summary>
    /// Interface represents entity to be logged with string user id property type
    /// </summary>
    public interface ILoggedEntity : ILoggedEntity<string>
    {
    }
}
