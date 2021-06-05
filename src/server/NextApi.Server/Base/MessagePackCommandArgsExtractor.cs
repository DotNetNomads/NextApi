using System.IO;
using System.Threading.Tasks;
using MessagePack;
using Microsoft.AspNetCore.Http;
using NextApi.Common;

namespace NextApi.Server.Base
{
    /// <summary>
    /// Extracts NextApi command arguments using MessagePack serializer.
    /// </summary>
    internal class MessagePackCommandArgsExtractor : ICommandArgsExtractor
    {
        /// <inheritdoc/>
        public async Task<INextApiArgument[]> Extract(IFormCollection form)
        {
            var argsFile = form.Files["Args"];

            using var memoryStream = new MemoryStream();
            await argsFile.CopyToAsync(memoryStream);

            return MessagePackSerializer.Typeless.Deserialize(memoryStream.ToArray()) as INextApiArgument[];
        }
    }
}
