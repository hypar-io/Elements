#pragma warning disable CS1591

using Elements.Geometry;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Elements.Serialization.JSON
{
    public class Vector3Converter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Vector3);
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
            var formatting = writer.Formatting;
            writer.Formatting = Formatting.None;
            var v = (Vector3)value;
            writer.WriteStartObject();
            writer.WritePropertyName("x");
            writer.WriteValue(v.X);
            writer.WritePropertyName("y");
            writer.WriteValue(v.Y);
            writer.WritePropertyName("z");
            writer.WriteValue(v.Z);
            writer.WriteEndObject();
            writer.Formatting = formatting;
        }
    }

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