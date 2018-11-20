#pragma warning disable CS1591

using Hypar.Geometry;
using System;
using Newtonsoft.Json;

namespace Hypar.Elements.Serialization
{
    public class ColorConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Color);
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
            var c = (Color)value;
            writer.WriteStartObject();
            writer.WritePropertyName("red");
            writer.WriteValue(c.Red);
            writer.WritePropertyName("green");
            writer.WriteValue(c.Green);
            writer.WritePropertyName("blue");
            writer.WriteValue(c.Blue);
            writer.WritePropertyName("alpha");
            writer.WriteValue(c.Alpha);
            writer.WriteEndObject();
            writer.Formatting = formatting;
        }
    }
}