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

    public class DisabledNextApiPermissionProvider : INextApiPermissionProvider
    {
        public async Task<bool> HasPermission(string userName, object permission)
        {
            return true;
        }
    }
}
