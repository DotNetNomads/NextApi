using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NextApi.Common;
using NextApi.Server.Service;

namespace NextApi.Server.Base
{
    /// <summary>
    /// Writes <see cref="INextApiResponse"/> to <see cref="HttpResponse"/> as json.
    /// </summary>
    internal class JsonCommandResultWriter : ICommandResultWriter
    {
        /// <inheritdoc/>
        public async Task Write(HttpResponse httpResponse, INextApiResponse commandResult)
        {
            await httpResponse.SendJson(commandResult);
        }
    }
}
