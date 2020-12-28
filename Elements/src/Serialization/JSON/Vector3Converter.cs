#pragma warning disable CS1591

using System;
using Elements.Geometry;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Elements.Serialization
{
    /// <summary>
    /// Vector3 converter.
    /// </summary>
    public class Vector3Converter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Vector3);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var obj = JArray.Load(reader);
            return Vector3.FromArray(obj.ToObject<double[]>());
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var v = (Vector3)value;
            serializer.Serialize(writer, v.ToArray());
        }
    }
}