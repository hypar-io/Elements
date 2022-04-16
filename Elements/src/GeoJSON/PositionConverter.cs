using System;
using Newtonsoft.Json;

namespace Elements.GeoJSON
{
    public class PositionConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            if (objectType == typeof(Position))
            {
                return true;
            }
            return false;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var lon = reader.ReadAsDouble();
            var lat = reader.ReadAsDouble();
            reader.Read();
            return new Position(lat.Value, lon.Value);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var p = (Position)value;
            writer.WriteStartArray();
            writer.WriteValue(p.Longitude);
            writer.WriteValue(p.Latitude);
            writer.WriteEndArray();
        }
    }
}