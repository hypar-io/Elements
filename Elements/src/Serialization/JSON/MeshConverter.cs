#pragma warning disable CS1591

using System;
using System.Collections.Generic;
using System.Linq;
using Elements.Geometry;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Elements.Serialization.JSON
{
    /// <summary>
    /// Triangle converter.
    /// </summary>
    public class MeshConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Mesh);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var obj = JObject.Load(reader);
            var mesh = new Mesh();
            var positions = obj["Positions"].ToObject<List<double>>();
            var colors = obj["Colors"].ToObject<List<double>>();
            var normals = obj["Normals"].ToObject<List<double>>();
            var uvs = obj["UVs"].ToObject<List<double>>();

            for (var i = 0; i < positions.Count / 3; i++)
            {
                var pi = i * 3;
                var ui = i * 2;
                var ci = i * 4;
                var v = mesh.AddVertex(new Vector3(positions[pi],
                                               positions[pi + 1],
                                               positions[pi + 2]), new UV(uvs[ui],
                                                                          uvs[ui + 1]), new Vector3(normals[pi],
                                                                                                    normals[pi + 1],
                                                                                                    normals[pi + 2]), new Color(colors[ci],
                                                                                                                                colors[ci + 1],
                                                                                                                                colors[ci + 2],
                                                                                                                                colors[ci + 3]));
            }

            var triangles = obj["Triangles"].ToObject<List<int>>();
            for (var i = 0; i < triangles.Count; i += 3)
            {
                var a = mesh.Vertices[triangles[i]];
                var b = mesh.Vertices[triangles[i + 1]];
                var c = mesh.Vertices[triangles[i + 2]];
                var tri = new Triangle(a, b, c);
                mesh.AddTriangle(tri);
            }

            return mesh;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var m = (Mesh)value;

            writer.WriteStartObject();

            writer.WritePropertyName("Positions");
            serializer.Serialize(writer, m.Vertices.SelectMany(v => new[] { v.Position.X, v.Position.Y, v.Position.Z }));

            writer.WritePropertyName("Normals");
            serializer.Serialize(writer, m.Vertices.SelectMany(v => new[] { v.Normal.X, v.Normal.Y, v.Normal.Z }));

            writer.WritePropertyName("UVs");
            serializer.Serialize(writer, m.Vertices.SelectMany(v => new[] { v.UV.U, v.UV.V }));

            writer.WritePropertyName("Colors");
            serializer.Serialize(writer, m.Vertices.SelectMany(v => new[] { v.Color.Red, v.Color.Green, v.Color.Blue, v.Color.Alpha }));

            writer.WritePropertyName("Triangles");
            serializer.Serialize(writer, m.Triangles.SelectMany(t => new[] { t.Vertices[0].Index, t.Vertices[1].Index, t.Vertices[2].Index }));

            writer.WriteEndObject();
        }
    }
}
