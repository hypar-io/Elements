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
            try
            {
                // New color format Ex: [x,x,x,x]
                var arr = JArray.Load(reader);
                return Color.FromArray(arr.ToObject<double[]>());
            }
            catch
            {

                // Old color format Ex: Red:x, Green:x, Blue:x, Alpha:x
                var obj = JObject.Load(reader);
                return obj.ToObject<Color>();
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var c = (Color)value;
            serializer.Serialize(writer, c.ToArray());
        }
    }
}
