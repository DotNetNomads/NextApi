using System;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;

namespace Abitech.NextApi.Server.Security
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
        public static int? GetSubjectId(this ClaimsPrincipal claimsPrincipal)
        {
            var claim = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == "sub");
            if (claim == null) return null;
            return int.Parse(claim.Value);
        }
    }
}
