

using System.Security.Claims;
using System.Threading.Tasks;
using Abitech.NextApi.Common.Abstractions;

namespace Abitech.NextApi.Server.Security
{
    /// <summary>
    /// Stub for disabled permission provider
    /// </summary>
    public class DisabledNextApiPermissionProvider : INextApiPermissionProvider
    {
        /// <inheritdoc />
#pragma warning disable 1998
        public async Task<bool> HasPermission(ClaimsPrincipal userInfo, object permission)
#pragma warning restore 1998
        {
            return true;
        }

        /// <inheritdoc />
        public string[] SupportedPermissions { get; } = {""};
    }
}
