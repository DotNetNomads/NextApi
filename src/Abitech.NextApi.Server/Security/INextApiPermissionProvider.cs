using System.Threading.Tasks;

namespace Abitech.NextApi.Server.Security
{
    /// <summary>
    /// Provides information about users permissions
    /// </summary>
    public interface INextApiPermissionProvider
    {
        /// <summary>
        /// Checks that user has permission
        /// </summary>
        /// <param name="userName">UserName</param>
        /// <param name="permission">Permission</param>
        /// <returns>Returns <c>true</c> if user has requested permission</returns>
        Task<bool> HasPermission(string userName, object permission);
    }

    /// <summary>
    /// Stub for disabled permission provider
    /// </summary>
    public class DisabledNextApiPermissionProvider : INextApiPermissionProvider
    {
        /// <inheritdoc />
#pragma warning disable 1998
        public async Task<bool> HasPermission(string userName, object permission)
#pragma warning restore 1998
        {
            return true;
        }
    }
}
