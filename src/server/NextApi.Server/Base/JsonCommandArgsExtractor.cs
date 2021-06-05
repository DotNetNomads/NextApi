using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using NextApi.Common;
using NextApi.Common.Serialization;

namespace NextApi.Server.Base
{
    /// <summary>
    /// Extracts NextApi command arguments using Json serializer.
    /// </summary>
    internal class JsonCommandArgsExtractor : ICommandArgsExtractor
    {
        private static readonly Task<INextApiArgument[]> CachedNull = Task.FromResult(null as INextApiArgument[]);

        /// <inheritdoc/>
        public Task<INextApiArgument[]> Extract(IFormCollection form)
        {
            var argsString = form["Args"].FirstOrDefault();

            if (string.IsNullOrEmpty(argsString))
            {
                return CachedNull;
            }

            return Task.FromResult(JsonConvert
                .DeserializeObject<IEnumerable<NextApiJsonArgument>>(argsString, SerializationUtils.GetJsonConfig())
                .Cast<INextApiArgument>()
                .ToArray());
        }
    }
}
