using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using NextApi.Server.Base;

namespace NextApi.Server.Request
{
    /// <summary>
    /// Provides all information about current NextApi request
    /// </summary>
    public interface INextApiRequest
    {
        /// <summary>
        /// Contains information about NextApiHub clients, etc.
        /// <remarks>Only SignalR</remarks>
        /// </summary>
        IHubContext<NextApiHub> HubContext { get; set; }

        /// <summary>
        /// Contains information about current service call (user, etc.)
        /// <remarks>Only SignalR</remarks>
        /// </summary>
        HubCallerContext ClientContext { get; set; }
        /// <summary>
        /// Accessor to form data from client
        /// <remarks>Only HTTP</remarks>
        /// </summary>
        IFormFileCollection FilesFromClient { get; set; }
    }

    /// <inheritdoc />
    public class NextApiRequest : INextApiRequest
    {
        /// <inheritdoc />
        public IHubContext<NextApiHub> HubContext { get; set; }

        /// <inheritdoc />
        public HubCallerContext ClientContext { get; set; }

        /// <inheritdoc />
        public IFormFileCollection FilesFromClient { get; set; }
    }
}
