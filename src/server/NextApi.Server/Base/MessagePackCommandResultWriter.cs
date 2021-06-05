using System.Threading.Tasks;
using MessagePack;
using Microsoft.AspNetCore.Http;
using NextApi.Common;
using NextApi.Server.Service;

namespace NextApi.Server.Base
{
    /// <summary>
    /// Writes NextApi response to HttpResponse as MessagePack encoded byte array.
    /// </summary>
    internal class MessagePackCommandResultWriter : ICommandResultWriter
    {
        /// <inheritdoc/>
        public async Task Write(HttpResponse httpResponse, INextApiResponse commandResult)
        {
            await httpResponse.SendByteArray(MessagePackSerializer.Serialize(commandResult));
        }
    }
}
