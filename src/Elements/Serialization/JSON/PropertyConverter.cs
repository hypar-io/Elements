#pragma warning disable CS1591

using Elements.Interfaces;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Elements.Serialization.JSON
{
    public class PropertyConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(NumericProperty) || objectType == typeof(StringProperty);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var obj = JObject.Load(reader);
            var unitType = (string)obj.GetValue("unit_type");
            switch(unitType)
            {
                case "none":
                case "text":
                    return obj.ToObject<StringProperty>(serializer);
                case "area":
                case "force":
                case "mass":
                case "volume":
                    return obj.ToObject<NumericProperty>(serializer);
            }

            throw new Exception($"The supplied unit type, {unitType}, is not supported for deserialization.");
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var formatting = writer.Formatting;
            writer.Formatting = Formatting.None;
            writer.WriteStartObject();
            
            if(value is IPropertySingleValue<double>)
            {
                var p = (IPropertySingleValue<double>)value;
                writer.WritePropertyName("type");
                writer.WriteValue(p.UnitType);
                writer.WritePropertyName("value");
                writer.WriteValue(p.Value);
                writer.WriteEndObject();
            }
            else if(value is IPropertySingleValue<string>)
            {
                var p = (IPropertySingleValue<string>)value;
                writer.WritePropertyName("type");
                writer.WriteValue(p.UnitType);
                writer.WritePropertyName("value");
                writer.WriteValue(p.Value);
                writer.WriteEndObject();
            }
            var v = (IProperty)value;
            writer.Formatting = formatting;
        }
    }
}