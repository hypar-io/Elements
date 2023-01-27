using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Elements.GeoJSON
{
    /// <summary>
    /// Convert a geojson position object to an array of coordinates.
    /// </summary>
    public class PositionConverter : JsonConverter<Position>
    {
        /// <summary>
        /// Read a position.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="typeToConvert"></param>
        /// <param name="options"></param>
        public override Position Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using (var doc = JsonDocument.ParseValue(ref reader))
            {
                var root = doc.RootElement;
                var values = root.Deserialize<double[]>();
                return new Position(values[1], values[0]);
            }
        }

        /// <summary>
        /// Write a position.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="options"></param>
        public override void Write(Utf8JsonWriter writer, Position value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            writer.WriteNumberValue(value.Longitude);
            writer.WriteNumberValue(value.Latitude);
            writer.WriteEndArray();
        }
    }
}