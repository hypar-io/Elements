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
            var token = JToken.Load(reader);
            if (token is JArray)
            {
                // New color format Ex: [x,x,x,x]
                var arr = (JArray)token;
                return Color.FromArray(arr.ToObject<double[]>());
            }
            else if (token is JObject)
            {
                // Old color format Ex: Red:x, Green:x, Blue:x, Alpha:x
                var obj = (JObject)token;
                return obj.ToObject<Color>();
            }
            throw new Exception("The token representing a Color was not an object or an array. Check the JSON to ensure that colors are encoded as R: G: B: A: or [R,G,B,A].");
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var c = (Color)value;
            serializer.Serialize(writer, c.ToArray());
        }
    }
}
