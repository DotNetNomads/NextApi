using System.Linq;
using System.Security.Claims;

namespace NextApi.Server.Security
{
    /// <summary>
    /// useful extensions for security
    /// </summary>
    public static class NextApiSecurityExtensions
    {
        /// <summary>
        /// Gets the subject identifier.
        /// </summary>
        /// <param name="claimsPrincipal"></param>
        /// <returns>subject id or null</returns>
        public static string GetSubjectId(this ClaimsPrincipal claimsPrincipal)
        {
            var claim = claimsPrincipal?.Claims?.FirstOrDefault(c => c.Type == "sub");
            return claim?.Value;
        }
    }
}
