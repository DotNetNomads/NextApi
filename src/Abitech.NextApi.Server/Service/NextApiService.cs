using System;
using Abitech.NextApi.Server.Base;
using Microsoft.AspNetCore.SignalR;

namespace Abitech.NextApi.Server.Service
{
    /// <summary>
    /// Abstract realization of NextApi service
    /// </summary>
    public abstract class NextApiService
    {
        /// <summary>
        /// Contains information about NextApiHub clients, etc.
        /// </summary>
        public IHubContext<NextApiHub> HubContext { get; set; }

        /// <summary>
        /// Contains information about current service call (user, etc.)
        /// </summary>
        public HubCallerContext ClientContext { get; set; }
    }
}
