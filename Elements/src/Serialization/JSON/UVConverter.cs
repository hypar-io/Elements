#pragma warning disable CS1591

using System;
using Elements.Geometry;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Elements.Serialization.JSON
{
    /// <summary>
    /// UV converter.
    /// </summary>
    public class UVConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(UV);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var token = JToken.Load(reader);
            if (token is JArray)
            {
                // New UV format Ex: [x,x]
                var arr = JArray.Load(reader);
                return UV.FromArray(arr.ToObject<double[]>());
            }
            else if (token is JObject)
            {
                // Old UV format Ex: U:x, V:x
                var obj = JObject.Load(reader);
                return obj.ToObject<UV>();
            }

            throw new Exception("The token representing a UV was not an object or an array. Check the JSON to ensure that UVs are encoded as U: V: or [U,V].");
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var uv = (UV)value;
            serializer.Serialize(writer, uv.ToArray());
        }
    }
}
