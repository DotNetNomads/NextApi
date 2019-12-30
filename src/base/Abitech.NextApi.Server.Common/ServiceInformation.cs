using System;
using System.Collections.Generic;

namespace Abitech.NextApi.Server.Common
{
    /// <summary>
    /// Information about NextApiService implementation
    /// </summary>
    public class ServiceInformation
    {
        /// <summary>
        /// System type for service
        /// </summary>
        public Type ServiceType { get; set; }

        /// <summary>
        /// Indicates that service requires authorized user
        /// </summary>
        public bool RequiresAuthorization { get; set; } = true;

        /// <summary>
        /// Indicates that service allows execution without checking permissions
        /// (by default, if method doesn't have defined permissions)
        /// </summary>
        public bool AllowByDefault { get; set; } = true;

        /// <summary>
        /// Information about per-method permissions
        /// </summary>
        public Dictionary<string, object> MethodsPermissionInfo { get; } = new Dictionary<string, object>();
    }
}
