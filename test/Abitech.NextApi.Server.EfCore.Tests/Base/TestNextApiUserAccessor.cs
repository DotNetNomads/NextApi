using System.Security.Claims;
using Abitech.NextApi.Common.Abstractions;
using Abitech.NextApi.Server.Security;

namespace Abitech.NextApi.Server.EfCore.Tests.Base
{
    public class TestNextApiUserAccessor: INextApiUserAccessor
    {
        /// <inheritdoc />
        public ClaimsPrincipal User { get; set; }

        /// <inheritdoc />
        public int? SubjectId => 1;
    }
}
