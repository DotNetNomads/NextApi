using NextApi.Common;
using NextApi.Common.Serialization;

namespace NextApi.Server.Base
{
    /// <summary>
    /// Factory class for creating <see cref="ICommandResultWriter"/> for <see cref="INextApiResponse"/> and <see cref="SerializationType"/>.
    /// </summary>
    internal static class CommandResultWriters
    {
        public static ICommandResultWriter Create(INextApiResponse response, SerializationType serializationType)
        {
            if (response is NextApiFileResponse)
            {
                return new FileCommandResultWriter();
            }

            return serializationType switch
            {
                SerializationType.MessagePack => new MessagePackCommandResultWriter(),

                _ => new JsonCommandResultWriter()
            };
        }
    }
}
