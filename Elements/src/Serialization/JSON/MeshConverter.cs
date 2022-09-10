#pragma warning disable CS1591

using System;
using System.Collections.Generic;
using System.Linq;
using Elements.Geometry;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace Elements.Serialization.JSON
{
    /// <summary>
    /// Triangle converter.
    /// </summary>
    public class MeshConverter : JsonConverter<Mesh>
    {
        // There are currently two version of the Mesh schema used on our platform.  
        // One used in explore and one used in the Elements Library.
        // Explore - https://hypar.io/Schemas/Mesh.json - used in input_schema
        // Elements - https://raw.githubusercontent.com/hypar-io/Elements/master/Schemas/Geometry/Mesh.json
        // TODO These schemas should converge to one. They both should change to match the serialized format of the 
        // data that is being transmitted on the platform.  Neither schema matches the desired serialization format currently.
        // The desired schema format is the one this converter reads/writes, it is very compact.
        public override Mesh Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            using (var doc = JsonDocument.ParseValue(ref reader))
            {
                var root = doc.RootElement;

                var mesh = new Mesh();

                if (root.TryGetProperty("vertices", out var verticesElement))
                {
                    //input_schema Mesh path
                    var allVertices = new List<Vertex>(verticesElement.GetArrayLength());
                    foreach (var v in verticesElement.EnumerateArray())
                    {
                        ;
                        var i = v.GetProperty("index").GetInt32();
                        var p = v.GetProperty("position");

                        allVertices.Insert(i, new Vertex(new Vector3(
                                                                p.GetProperty("X").GetDouble(),
                                                                p.GetProperty("Y").GetDouble(),
                                                                p.GetProperty("Z").GetDouble()
                                                                    )
                                                        ));
                    }
                    foreach (var v in allVertices)
                    {
                        mesh.AddVertex(v);
                    }

                    foreach (var t in root.GetProperty("triangles").EnumerateArray())
                    {
                        var indices = t.GetProperty("vertexIndices");
                        mesh.AddTriangle(allVertices[indices[0].GetInt32()],
                                          allVertices[indices[1].GetInt32()],
                                          allVertices[indices[2].GetInt32()]);
                    }
                }
                else
                {
                    //Elements Mesh path
                    var positions = root.GetProperty("Positions").Deserialize<List<double>>();
                    var colors = root.GetProperty("Colors").Deserialize<List<double>>();
                    var normals = root.GetProperty("Normals").Deserialize<List<double>>();
                    var uvs = root.GetProperty("UVs").Deserialize<List<double>>();

                    for (var i = 0; i < positions.Count / 3; i++)
                    {
                        var pi = i * 3;
                        var ui = i * 2;
                        var ci = i * 4;
                        mesh.AddVertex(new Vector3(positions[pi],
                                                       positions[pi + 1],
                                                       positions[pi + 2]), new UV(uvs[ui],
                                                                                  uvs[ui + 1]), new Vector3(normals[pi],
                                                                                                            normals[pi + 1],
                                                                                                            normals[pi + 2]), new Color(colors[ci],
                                                                                                                                        colors[ci + 1],
                                                                                                                                        colors[ci + 2],
                                                                                                                                        colors[ci + 3]));
                    }

                    var triangles = root.GetProperty("Triangles").Deserialize<List<int>>();
                    for (var i = 0; i < triangles.Count; i += 3)
                    {
                        var a = mesh.Vertices[triangles[i]];
                        var b = mesh.Vertices[triangles[i + 1]];
                        var c = mesh.Vertices[triangles[i + 2]];
                        var tri = new Triangle(a, b, c);
                        mesh.AddTriangle(tri);
                    }
                }

                return mesh;
            }
        }

        public override void Write(Utf8JsonWriter writer, Mesh value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WritePropertyName("Positions");
            JsonSerializer.Serialize(writer, value.Vertices.SelectMany(v => new[] { v.Position.X, v.Position.Y, v.Position.Z }));

            writer.WritePropertyName("Normals");
            JsonSerializer.Serialize(writer, value.Vertices.SelectMany(v => new[] { v.Normal.X, v.Normal.Y, v.Normal.Z }));

            writer.WritePropertyName("UVs");
            JsonSerializer.Serialize(writer, value.Vertices.SelectMany(v => new[] { v.UV.U, v.UV.V }));

            writer.WritePropertyName("Colors");
            JsonSerializer.Serialize(writer, value.Vertices.SelectMany(v => new[] { v.Color.Red, v.Color.Green, v.Color.Blue, v.Color.Alpha }));

            writer.WritePropertyName("Triangles");
            JsonSerializer.Serialize(writer, value.Triangles.SelectMany(t => new[] { t.Vertices[0].Index, t.Vertices[1].Index, t.Vertices[2].Index }));

            writer.WriteEndObject();
        }
    }
}
