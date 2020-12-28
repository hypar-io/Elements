#pragma warning disable CS1591

using System;
using Elements.Geometry;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Elements.Serialization.JSON
{
    /// <summary>
    /// Color converter.
    /// </summary>
    public class ColorConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Color);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var obj = JArray.Load(reader);
            return Color.FromArray(obj.ToObject<double[]>());
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var c = (Color)value;
            serializer.Serialize(writer, c.ToArray());
        }
    }
}
