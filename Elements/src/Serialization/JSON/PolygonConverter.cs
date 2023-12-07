using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Elements.Geometry;

namespace Elements.Serialization.JSON
{
    internal class PolygonConverter : JsonConverter<Polygon>
    {
        public override Polygon Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using (var doc = JsonDocument.ParseValue(ref reader))
            {
                var root = doc.RootElement;
                var vertices = new List<Vector3>();

                foreach (var vObj in root.GetProperty("Vertices").EnumerateArray())
                {
                    var x = vObj.GetProperty("X").GetDouble();
                    var y = vObj.GetProperty("Y").GetDouble();
                    var z = vObj.GetProperty("Z").GetDouble();
                    vertices.Add(new Vector3(x, y, z));
                }

                return new Polygon(vertices);
            }
        }

        public override void Write(Utf8JsonWriter writer, Polygon value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WritePropertyName("Vertices");
            writer.WriteStartArray();
            foreach (var vertex in value.Vertices)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("X");
                writer.WriteNumberValue(vertex.X);
                writer.WritePropertyName("Y");
                writer.WriteNumberValue(vertex.Y);
                writer.WritePropertyName("Z");
                writer.WriteNumberValue(vertex.Z);
                writer.WriteEndObject();
            }
            writer.WriteEndArray();

            writer.WriteEndObject();
        }
    }
}
