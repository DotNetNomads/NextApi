using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NextApi.Common;

namespace NextApi.Server.Base
{
    /// <summary>
    /// Interface is a contract for extracing NextApi command arguments from <see cref="IFormCollection"/>.
    /// </summary>
    internal interface ICommandArgsExtractor
    {
        /// <summary>
        /// Extracts NextApi command arguments from form.
        /// </summary>
        /// <param name="form"> Http forms collection. </param>
        /// <returns> Array of NextApi command arguments. </returns>
        Task<INextApiArgument[]> Extract(IFormCollection form);
    }
}
