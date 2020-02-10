using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Abitech.NextApi.Common.Serialization
{
    /// <summary>
    /// Useful serialization utils
    /// </summary>
    public static class SerializationUtils
    {
        /// <summary>
        /// Get default JSON serializer settings
        /// </summary>
        /// <returns></returns>
        public static JsonSerializerSettings GetJsonConfig() =>
            new JsonSerializerSettings
            {
                Converters = new JsonConverter[] {new StringEnumConverter(), new UnionInterfaceConverter()},
                NullValueHandling = NullValueHandling.Ignore,
                TypeNameHandling = TypeNameHandling.None
            };
    }
}
