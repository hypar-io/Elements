using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Elements.GeoJSON
{
    /// <summary>
    /// Convert geojson geometry.
    /// </summary>
    public class GeometryConverter : JsonConverter<object>
    {
        /// <summary>
        /// Read geometry.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="typeToConvert"></param>
        /// <param name="options"></param>
        public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using (var doc = JsonDocument.ParseValue(ref reader))
            {
                var root = doc.RootElement;

                var typeName = root.GetProperty("type").GetString();
                switch (typeName)
                {
                    case "Point":
                        return root.Deserialize<Point>();
                    case "Line":
                        return root.Deserialize<Line>();
                    case "MultiPoint":
                        return root.Deserialize<MultiPoint>();
                    case "LineString":
                        return root.Deserialize<LineString>();
                    case "MultiLineString":
                        return root.Deserialize<MultiLineString>();
                    case "Polygon":
                        return root.Deserialize<Polygon>();
                    case "MultiPolygon":
                        return root.Deserialize<MultiPolygon>();
                    case "GeometryCollection":
                        return root.Deserialize<GeometryCollection>();
                    default:
                        throw new Exception($"The type found in the GeoJSON, {typeName}, could not be resolved.");
                }
            }
        }

        /// <summary>
        /// Write geometry.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="options"></param>
        public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, options);
        }
    }
}