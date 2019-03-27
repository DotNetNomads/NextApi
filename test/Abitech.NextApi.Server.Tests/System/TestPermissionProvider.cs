using System.Security.Claims;
using System.Threading.Tasks;
using Abitech.NextApi.Server.Security;
using NotImplementedException = System.NotImplementedException;

namespace Abitech.NextApi.Server.Tests.System
{
    public class TestPermissionProvider : INextApiPermissionProvider
    {
        public async Task<bool> HasPermission(ClaimsPrincipal userInfo, object permission)
        {
            return true;
        }

        public string[] SupportedPermissions { get; } = {"permission1", "permission2"};
    }
}
