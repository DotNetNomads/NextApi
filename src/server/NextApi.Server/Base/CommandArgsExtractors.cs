using NextApi.Common.Serialization;

namespace NextApi.Server.Base
{
    /// <summary>
    /// Factory class for creating <see cref="ICommandArgsExtractor"/> for specific <see cref="SerializationType"/>.
    /// </summary>
    internal static class CommandArgsExtractors
    {
        public static ICommandArgsExtractor Create(SerializationType serializationType)
        {
            return serializationType switch
            {
                SerializationType.MessagePack => new MessagePackCommandArgsExtractor(),
                _ => new JsonCommandArgsExtractor()
            };
        }
    }
}
