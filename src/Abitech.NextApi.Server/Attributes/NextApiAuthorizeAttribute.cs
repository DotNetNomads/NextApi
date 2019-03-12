using System;

namespace Abitech.NextApi.Server.Attributes
{
    /// <inheritdoc />
    /// <summary>
    /// Attribute allows method only for user with specific permission
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class NextApiAuthorizeAttribute : Attribute
    {
        /// <summary>
        /// Permission
        /// </summary>
        ///
        public readonly object Permission;

        /// <inheritdoc />
        ///  <summary>
        ///  </summary>
        ///  <param name="permission"></param>
        public NextApiAuthorizeAttribute(object permission)
        {
            Permission = permission;
        }
    }
}
