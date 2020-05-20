using System.Security.Claims;
using NextApi.Common.Abstractions.Security;

namespace NextApi.Server.EfCore.Tests.Base
{
    public class TestNextApiUserAccessor: INextApiUserAccessor
    {
        /// <inheritdoc />
        public ClaimsPrincipal User { get; set; }

        /// <inheritdoc />
        public string SubjectId => "1";
    }
}
