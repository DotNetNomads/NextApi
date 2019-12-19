using System.Security.Claims;

namespace Abitech.NextApi.Common.Abstractions
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
        int? SubjectId { get; }
    }
}
