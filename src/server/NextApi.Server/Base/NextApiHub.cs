using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using NextApi.Common;
using NextApi.Common.Abstractions.Security;
using NextApi.Server.Request;
using Hub = Microsoft.AspNetCore.SignalR.Hub;

namespace NextApi.Server.Base
{
    /// <inheritdoc />
    /// <summary>
    /// Main Hub. Entry point for all NextApi calls via SignalR
    /// </summary>
    public class NextApiHub : Hub
    {
        private readonly NextApiHandler _handler;
        private readonly INextApiUserAccessor _nextApiUserAccessor;
        private readonly INextApiRequest _nextApiRequest;
        private readonly IHubContext<NextApiHub> _hubContext;

        /// <summary>
        /// Initialize Hub
        /// </summary>
        public NextApiHub(NextApiHandler handler, INextApiRequest nextApiRequest,
            INextApiUserAccessor nextApiUserAccessor, IHubContext<NextApiHub> hubContext)
        {
            _handler = handler;
            _nextApiRequest = nextApiRequest;
            _nextApiUserAccessor = nextApiUserAccessor;
            _hubContext = hubContext;
        }

        /// <summary>
        /// Entry point of service calls
        /// </summary>
        /// <param name="command">Information about service call</param>
        /// <returns>Response from called service</returns>
        public async Task<dynamic> ExecuteCommand(NextApiCommand command)
        {
            // set current nextapi user
            _nextApiUserAccessor.User = Context.User;
            // set current request info
            _nextApiRequest.ClientContext = Context;
            _nextApiRequest.HubContext = _hubContext;
            var result = await _handler.ExecuteCommand(command);
            if (!(result is NextApiFileResponse))
            {
                return result;
            }

            var error = new NextApiError(NextApiErrorCode.OperationIsNotSupported.ToString(),
                new Dictionary<string, object>()
                {
                    {"message", "File operations is not supported over SignalR, use HTTP"}
                });
            return new NextApiResponse(data: null, error: error, success: false);

        }

        /// <summary>
        /// Returns list of supported permissions
        /// </summary>
        /// <returns></returns>
        public string[] GetSupportedPermissions()
        {
            return _handler.GetSupportedPermissions();
        }
    }
}
