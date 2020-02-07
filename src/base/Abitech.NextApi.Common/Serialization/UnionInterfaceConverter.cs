using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using MessagePack;
using Newtonsoft.Json.Linq;

namespace Abitech.NextApi.Common.Serialization
{
    /// <summary>
    /// Converts interfaces with Union correctly (in situation when interface had inheritors)
    /// </summary>
    public class UnionInterfaceConverter : JsonConverter
    {
        // each thread has own cache!
        [ThreadStatic] private static List<UnionTypeItem> _unionTypes;

        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            var convertedObject = JObject.FromObject(value);
            var typeKey = _unionTypes.First(t => t.SubType == value.GetType()).SubTypeKey;
            convertedObject.Add("__typeKey", typeKey);
            convertedObject.WriteTo(writer);
        }

        /// <inheritdoc />
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var jsonObject = JObject.Load(reader);
            if (!jsonObject.ContainsKey("__typeKey") || jsonObject["__typeKey"].Type != JTokenType.Integer)
                return jsonObject.ToObject(objectType, serializer);
            var typeKey = jsonObject.Value<int>("__typeKey");
            var correctType = _unionTypes.FirstOrDefault(t => t.BaseType == objectType && t.SubTypeKey == typeKey)
                .SubType;
            return jsonObject.ToObject(correctType ?? objectType);
        }

        /// <inheritdoc />
        public override bool CanConvert(Type objectType)
        {
            // we skip, not user-defined types
            if (objectType.Assembly.GetName().Name == "mscorlib" || objectType.FullName == null ||
                objectType.FullName.StartsWith("System.") || typeof(INextApiArgument).IsAssignableFrom(objectType))
                return false;
            // we found type in cache, then true
            if (_unionTypes != null && (_unionTypes.Any(t => t.BaseType == objectType) ||
                                        _unionTypes.Any(t => t.SubType == objectType)))
                return true;

            var attributes = new Dictionary<Type, IList<Attribute>>();
            var attributeType = typeof(UnionAttribute);

            // collecting union attributes from current type
            {
                var unionAttrs = objectType.GetCustomAttributes(attributeType).ToList();
                if (unionAttrs.Any()) attributes.Add(objectType, unionAttrs);
            }
            // collecting union attributes from base type
            {
                var unionAttrs = objectType.BaseType?.GetCustomAttributes(attributeType).ToList();
                if (unionAttrs != null && unionAttrs.Any() && !attributes.ContainsKey(objectType.BaseType))
                    attributes.Add(objectType.BaseType, unionAttrs.ToList());
            }
            // collection union attributes from interfaces
            foreach (var interfaceType in objectType.GetInterfaces()
                .Where(i => i.FullName != null && i.Assembly.GetName().Name != "mscorlib" &&
                            !i.FullName.StartsWith("System.")))
            {
                var interfaceAttributes = interfaceType.GetCustomAttributes(attributeType).ToList();
                if (interfaceAttributes.Any() && !attributes.ContainsKey(interfaceType))
                    attributes.Add(interfaceType, interfaceAttributes);
            }

            // there are no union attributes, we can't work with this type
            if (!attributes.Any())
                return false;

            CacheAttributes(attributes);
            return true;
        }

        private static void CacheAttributes(Dictionary<Type, IList<Attribute>> attributes)
        {
            _unionTypes ??= new List<UnionTypeItem>();
            foreach (var keyValuePair in attributes)
            {
                var parentType = keyValuePair.Key;
                // this type already in cache
                if (_unionTypes.Any(t => t.BaseType == parentType))
                    continue;
                var attributeData = keyValuePair.Value;
                foreach (var unionAttribute in attributeData.Cast<UnionAttribute>())
                    _unionTypes.Add(new UnionTypeItem
                    {
                        BaseType = parentType, SubType = unionAttribute.SubType, SubTypeKey = unionAttribute.Key
                    });
            }
        }
    }

    /// <summary>
    /// Union type description instance
    /// </summary>
    public struct UnionTypeItem
    {
        /// <summary>
        /// Base type
        /// </summary>
        public Type BaseType { get; set; }

        /// <summary>
        /// Base type of sub type
        /// </summary>
        public Type SubType { get; set; }

        /// <summary>
        /// Key for sub type
        /// </summary>
        public int SubTypeKey { get; set; }
    }
}
