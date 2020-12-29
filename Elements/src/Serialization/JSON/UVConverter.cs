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

            try
            {
                // New UV format Ex: [x,x]
                var arr = JArray.Load(reader);
                return UV.FromArray(arr.ToObject<double[]>());
            }
            catch
            {
                // Old UV format Ex: U:x, V:x
                var obj = JObject.Load(reader);
                return obj.ToObject<UV>();
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var uv = (UV)value;
            serializer.Serialize(writer, uv.ToArray());
        }
    }
}
