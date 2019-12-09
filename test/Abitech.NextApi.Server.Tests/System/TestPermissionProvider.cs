using System.Security.Claims;
using System.Threading.Tasks;
using Abitech.NextApi.Server.Security;
using NotImplementedException = System.NotImplementedException;

namespace Abitech.NextApi.Server.Tests.System
{
    public class TestPermissionProvider : INextApiPermissionProvider
    {
#pragma warning disable 1998
        public async Task<bool> HasPermission(ClaimsPrincipal userInfo, object permission)
#pragma warning restore 1998
        {
            return true;
        }

        public string[] SupportedPermissions { get; } = {"permission1", "permission2"};
    }
}
