#pragma warning disable CS1591

using System;
using Elements.Geometry;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Elements.Serialization.JSON
{
    /// <summary>
    /// Vector3 converter.
    /// </summary>
    public class Vector3Converter : JsonConverter
    {
        private JsonSerializer _serializer = new JsonSerializer();

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Vector3);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            try
            {
                // New vector format Ex: [1,2,3]
                var arr = JArray.Load(reader);
                return Vector3.FromArray(arr.ToObject<double[]>());
            }
            catch
            {
                // Old vector format Ex: X:1, Y:2, Z:3
                var obj = JObject.Load(reader);
                return obj.ToObject<Vector3>();
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var v = (Vector3)value;
            serializer.Serialize(writer, v.ToArray());
        }
    }
}