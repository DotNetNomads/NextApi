using System.Security.Claims;
using NextApi.Common.Abstractions.Security;

namespace NextApi.Server.Security
{
    /// <inheritdoc />
    public class NextApiUserAccessor : INextApiUserAccessor
    {
        /// <inheritdoc />
        public ClaimsPrincipal User { get; set; }

        /// <inheritdoc />
        public int? SubjectId => User?.GetSubjectId();
    }
}
