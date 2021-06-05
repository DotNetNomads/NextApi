using System.Security.Claims;
using System.Threading.Tasks;
using NextApi.Common.Abstractions.Security;

namespace NextApi.Server.Security
{
    /// <summary>
    /// Stub for disabled permission provider
    /// </summary>
    public class DisabledNextApiPermissionProvider : INextApiPermissionProvider
    {
        private static readonly Task<bool> CachedTrue = Task.FromResult(true);

        /// <inheritdoc />
        public Task<bool> HasPermission(ClaimsPrincipal userInfo, object permission)
        {
            return CachedTrue;
        }

        /// <inheritdoc />
        public string[] SupportedPermissions { get; } = { "" };
    }
}
