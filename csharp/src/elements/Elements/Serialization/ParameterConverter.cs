#pragma warning disable CS1591

using System;
using Newtonsoft.Json;

namespace Hypar.Elements.Serialization
{
    public class ParameterConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Parameter);
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
            var v = (Parameter)value;
            writer.WriteStartObject();
            writer.WritePropertyName("type");
            writer.WriteValue(v.Type);
            writer.WritePropertyName("value");
            writer.WriteValue(v.Value);
            writer.WriteEndObject();
            writer.Formatting = formatting;
        }
    }
}