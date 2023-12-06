#pragma warning disable CS1591

using Elements.Geometry.Solids;
using System;
using System.Collections.Generic;
using Elements.Geometry;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace Elements.Serialization.JSON
{
    /// <summary>
    /// The SolidConverter is used to deserialize a Solid.
    /// Solids have a self-referencing structure which does not serialize
    /// effectively using the default serialization logic. The SolidConverter
    /// serializes and deserializes starting at the Solid's Faces, using
    /// Vertex and Edge ids to reconstruct and link the Edges and Vertices as necessary.
    /// </summary>
    internal class SolidConverter : JsonConverter<Solid>
    {
        private List<Type> _solidTypes;

        public SolidConverter()
        {
            LoadSolidTypes();
        }

        private void LoadSolidTypes()
        {
            try
            {
                _solidTypes = Assembly.GetExecutingAssembly().GetTypes().Where(t => typeof(Solid).IsAssignableFrom(t)).ToList();
            }
            catch (ReflectionTypeLoadException ex)
            {
                foreach (var x in ex.LoaderExceptions)
                {
                    Console.WriteLine(x.Message);
                }
            }
        }

        public override Solid Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using (var obj = JsonDocument.ParseValue(ref reader))
            {
                // TODO: REMOVE OLD CODE BEFORE SYSTEM.TEXT.JSON UPDATES
                //     throw new Exception($"The object with type name, {typeName}, could not be deserialzed.");
                // }

                // var solid = (Solid)Activator.CreateInstance(foundType, new object[] { });

                // foreach (JObject vobj in (JArray)obj.GetValue("vertices"))
                // {
                //     var id = (uint)vobj.GetValue("id");
                //     var x = (double)vobj.GetValue("x");
                //     var y = (double)vobj.GetValue("y");
                //     var z = (double)vobj.GetValue("z");
                //     solid.AddVertex(id, new Vector3(x, y, z));
                // }

                // foreach (JObject face in (JArray)obj.GetValue("faces"))
                // {
                //     var id = (uint)face.GetValue("id");

                //     var outer = new Loop();

                //     foreach (JObject heobj in (JArray)face.GetValue("outer"))

                var root = obj.RootElement;
                var typeName = root.GetProperty("type").GetString();
                var foundType = _solidTypes.FirstOrDefault(t => t.FullName.ToLower() == typeName);
                if (foundType == null)
                {
                    throw new Exception($"The object with type name, {typeName}, could not be deserialzed.");
                }

                var solid = (Solid)Activator.CreateInstance(foundType, new object[] { });

                foreach (var vobj in root.GetProperty("vertices").EnumerateArray())
                {
                    var id = vobj.GetProperty("id").GetInt64();
                    var x = vobj.GetProperty("x").GetDouble();
                    var y = vobj.GetProperty("y").GetDouble();
                    var z = vobj.GetProperty("z").GetDouble();
                    solid.AddVertex(id, new Vector3(x, y, z));
                }

                foreach (var face in root.GetProperty("faces").EnumerateArray())
                {
                    var id = face.GetProperty("id").GetInt64();

                    var outer = new Loop();

                    foreach (var heobj in face.GetProperty("outer").EnumerateArray())
                    {
                        ReadHalfEdge(heobj, outer, solid);
                    }

                    var inners = new List<Loop>();
                    if (face.TryGetProperty("inner", out var innerobjs))
                    {
                        foreach (var innerobj in innerobjs.EnumerateArray())
                        {
                            var inner = new Loop();
                            inners.Add(inner);
                            foreach (var heobj in innerobj.EnumerateArray())
                            {
                                ReadHalfEdge(heobj, inner, solid);
                            }
                        }
                    }
                    solid.AddFace(id, outer, inners.ToArray());
                }

                return solid;
            }
        }

        private void ReadHalfEdge(JsonElement heobj, Loop loop, Solid solid)
        {
            // var vobj = heobj.GetProperty("vertex");
            var vid = heobj.GetProperty("vertex_id").GetInt64();
            var v = solid.Vertices[vid];

            var he = new HalfEdge(v, loop);
            loop.AddEdgeToEnd(he);

            var eid = (uint)heobj.GetProperty("edge_id").GetInt64();
            if (!solid.Edges.TryGetValue(eid, out Edge edge))
            {
                edge = solid.AddEdge(eid);
            }

            he.Edge = edge;

            var side = heobj.GetProperty("edge_side").GetString();
            if (side == "left")
            {
                edge.Left = he;
            }
            else
            {
                edge.Right = he;
            }
        }

        public override void Write(Utf8JsonWriter writer, Solid value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("type");
            writer.WriteStringValue(value.GetType().FullName.ToLower());

            writer.WritePropertyName("vertices");
            writer.WriteStartArray();
            foreach (var v in value.Vertices.Values)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("id");
                writer.WriteNumberValue(v.Id);
                writer.WritePropertyName("x");
                writer.WriteNumberValue(v.Point.X);
                writer.WritePropertyName("y");
                writer.WriteNumberValue(v.Point.Y);
                writer.WritePropertyName("z");
                writer.WriteNumberValue(v.Point.Z);
                writer.WriteEndObject();
            }
            writer.WriteEndArray();

            writer.WritePropertyName("faces");
            writer.WriteStartArray();

            foreach (var f in value.Faces.Values)
            {
                writer.WriteStartObject();

                writer.WritePropertyName("id");
                writer.WriteNumberValue(f.Id);

                writer.WritePropertyName("outer");
                writer.WriteStartArray();
                foreach (var he in f.Outer.Edges)
                {
                    WriteHalfEdge(he, writer);
                }
                writer.WriteEndArray();

                if (f.Inner != null)
                {
                    writer.WritePropertyName("inner");
                    writer.WriteStartArray();

                    foreach (var loop in f.Inner)
                    {
                        writer.WriteStartArray();
                        foreach (var he in loop.Edges)
                        {
                            WriteHalfEdge(he, writer);
                        }
                        writer.WriteEndArray();
                    }

                    writer.WriteEndArray();
                }

                writer.WriteEndObject();
            }
            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        private void WriteHalfEdge(HalfEdge he, Utf8JsonWriter writer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("vertex_id");
            writer.WriteNumberValue(he.Vertex.Id);
            writer.WritePropertyName("edge_id");
            writer.WriteNumberValue(he.Edge.Id);
            writer.WritePropertyName("edge_side");
            writer.WriteStringValue(he.Edge.Left == he ? "left" : "right");
            writer.WriteEndObject();
        }
    }
}