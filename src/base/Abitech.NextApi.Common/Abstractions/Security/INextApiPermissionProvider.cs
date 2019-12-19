using System.Security.Claims;
using System.Threading.Tasks;
namespace Abitech.NextApi.Common.Abstractions
{
    /// <summary>
    /// Provides information about users permissions
    /// </summary>
    public interface INextApiPermissionProvider
    {
        /// <summary>
        /// Checks that user has permission
        /// </summary>
        /// <param name="userInfo">User info</param>
        /// <param name="permission">Permission</param>
        /// <returns>Returns <c>true</c> if user has requested permission</returns>
        Task<bool> HasPermission(ClaimsPrincipal userInfo, object permission);

        /// <summary>
        /// List of supported permissions
        /// </summary>
        string[] SupportedPermissions { get; }
    }
}
