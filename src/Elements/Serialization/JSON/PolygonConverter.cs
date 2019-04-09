#pragma warning disable CS1591

using System;
using Elements.Geometry;
using Newtonsoft.Json;

namespace Elements.Serialization.JSON
{
    public class PolygonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Polygon);
        }

        public override bool CanRead
        {
            get{return false;}
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var p = (Polygon)value;

            var formatting = writer.Formatting;
            writer.Formatting = Formatting.None;
            writer.WriteStartObject();
            // writer.WriteWhitespace("\n");
            writer.WritePropertyName("vertices");
            writer.WriteRawValue(JsonConvert.SerializeObject(p.Vertices));
            // writer.WriteWhitespace("\n");
            writer.WriteEndObject();
            writer.Formatting = formatting;
        }
    }
}