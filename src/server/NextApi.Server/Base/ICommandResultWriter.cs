using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NextApi.Common;

namespace NextApi.Server.Base
{
    /// <summary>
    /// Interface is contract for writing NextApi responses to <see cref="HttpResponse"/>.
    /// </summary>
    internal interface ICommandResultWriter
    {
        /// <summary>
        /// Writes NextApi response to HttpResponse.
        /// </summary>
        /// <param name="httpResponse"> Http response </param>
        /// <param name="commandResult"> NextApi response to write </param>
        /// <returns></returns>
        Task Write(HttpResponse httpResponse, INextApiResponse commandResult);
    }
}
