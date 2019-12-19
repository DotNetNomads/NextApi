using System.Security.Claims;
using Abitech.NextApi.Common.Abstractions;

namespace Abitech.NextApi.Server.Security
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
