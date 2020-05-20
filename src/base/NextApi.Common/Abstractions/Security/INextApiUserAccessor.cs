using System.Security.Claims;

namespace NextApi.Common.Abstractions.Security
{
    /// <summary>
    /// Provides current user info
    /// </summary>
    public interface INextApiUserAccessor
    {
        /// <summary>
        /// Current user
        /// </summary>
        ClaimsPrincipal User { get; set; }

        /// <summary>
        /// Current users subject id
        /// </summary>
        string SubjectId { get; }
    }
}
