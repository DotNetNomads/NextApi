using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NextApi.Common;
using NextApi.Server.Service;

namespace NextApi.Server.Base
{
    /// <summary>
    /// Writes <see cref="NextApiFileResponse"/> to <see cref="HttpResponse"/>.
    /// </summary>
    internal class FileCommandResultWriter : ICommandResultWriter
    {
        /// <inheritdoc/>
        /// <exception cref="ArgumentException"> Thrown when <paramref name="commandResult"/> is not <see cref="NextApiFileResponse"/></exception>
        public async Task Write(HttpResponse httpResponse, INextApiResponse commandResult)
        {
            if (commandResult is NextApiFileResponse fileResponse)
            {
                await httpResponse.SendNextApiFileResponse(fileResponse);
            }

            throw new ArgumentException($"Result must be of type {typeof(NextApiFileResponse)}", nameof(commandResult));
        }
    }
}
