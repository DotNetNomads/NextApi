using System.Security.Claims;

namespace Abitech.NextApi.Server.Security
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

    /// <inheritdoc />
    public class NextApiUserAccessor : INextApiUserAccessor
    {
        /// <inheritdoc />
        public ClaimsPrincipal User { get; set; }

        /// <inheritdoc />
        public int? SubjectId => User?.GetSubjectId();
    }
}
