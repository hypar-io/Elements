#pragma warning disable CS1591

using Elements.Geometry.Solids;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Elements.Geometry;
using System.Linq;
using System.Reflection;

namespace Elements.Serialization.JSON
{
    /// <summary>
    /// The SolidConverter is used to serialize and deserialize a Solid.
    /// Solids have a self-referencing structure which does not serialize
    /// effectively using the default serialization logic. The SolidConverter
    /// serializes and deserializes starting at the Solid's Faces, using
    /// Vertex and Edge ids to reconstruct and link the Edges and Vertices as necessary.
    /// </summary>
    internal class SolidConverter : JsonConverter
    {
        private List<Type> _solidTypes;

        public SolidConverter(IDictionary<Guid, Material> materials)
        {
            LoadSolidTypes();
        }

        private void LoadSolidTypes()
        {
            try
            {
                _solidTypes = Assembly.GetExecutingAssembly().GetTypes().Where(t => typeof(Solid).IsAssignableFrom(t)).ToList();
            }
            catch (System.Reflection.ReflectionTypeLoadException ex)
            {
                foreach (var x in ex.LoaderExceptions)
                {
                    Console.WriteLine(x.Message);
                }
            }
        }

        public SolidConverter()
        {
            LoadSolidTypes();
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(Solid).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var obj = JObject.Load(reader);
            var typeName = (string)obj.GetValue("type");
            var foundType = _solidTypes.FirstOrDefault(t => t.FullName.ToLower() == typeName);
            if (foundType == null)
            {
                throw new Exception($"The object with type name, {typeName}, could not be deserialzed.");
            }

            var solid = (Solid)Activator.CreateInstance(foundType, new object[] { });

            foreach (JObject vobj in (JArray)obj.GetValue("vertices"))
            {
                var id = (uint)vobj.GetValue("id");
                var x = (double)vobj.GetValue("x");
                var y = (double)vobj.GetValue("y");
                var z = (double)vobj.GetValue("z");
                solid.AddVertex(id, new Vector3(x, y, z));
            }

            foreach (JObject face in (JArray)obj.GetValue("faces"))
            {
                var id = (uint)face.GetValue("id");

                var outer = new Loop();

                foreach (JObject heobj in (JArray)face.GetValue("outer"))
                {
                    ReadHalfEdge(heobj, outer, solid);
                }

                JToken innerobjs;
                var inners = new List<Loop>();
                if (face.TryGetValue("inner", out innerobjs))
                {
                    foreach (JArray innerobj in innerobjs)
                    {
                        var inner = new Loop();
                        inners.Add(inner);
                        foreach (JObject heobj in innerobj)
                        {
                            ReadHalfEdge(heobj, inner, solid);
                        }
                    }
                }
                solid.AddFace(id, outer, inners.ToArray());
            }

            return solid;
        }

        private void ReadHalfEdge(JObject heobj, Loop loop, Solid solid)
        {
            var vobj = (JObject)heobj.GetValue("vertex");
            var vid = (long)heobj.GetValue("vertex_id");
            var v = solid.Vertices[vid];

            var he = new HalfEdge(v, loop);
            loop.AddEdgeToEnd(he);

            var eid = (uint)heobj.GetValue("edge_id");
            Edge edge;
            if (!solid.Edges.TryGetValue(eid, out edge))
            {
                edge = solid.AddEdge(eid);
            }

            he.Edge = edge;

            var side = (string)heobj.GetValue("edge_side");
            if (side == "left")
            {
                edge.Left = he;
            }
            else
            {
                edge.Right = he;
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var solid = (Solid)value;

            writer.WriteStartObject();
            writer.WritePropertyName("type");
            writer.WriteValue(value.GetType().FullName.ToLower());

            writer.WritePropertyName("vertices");
            writer.WriteStartArray();
            foreach (var v in solid.Vertices.Values)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("id");
                writer.WriteValue(v.Id);
                writer.WritePropertyName("x");
                writer.WriteValue(v.Point.X);
                writer.WritePropertyName("y");
                writer.WriteValue(v.Point.Y);
                writer.WritePropertyName("z");
                writer.WriteValue(v.Point.Z);
                writer.WriteEndObject();
            }
            writer.WriteEndArray();

            writer.WritePropertyName("faces");
            writer.WriteStartArray();

            foreach (var f in solid.Faces.Values)
            {
                writer.WriteStartObject();

                writer.WritePropertyName("id");
                writer.WriteValue(f.Id);

                writer.WritePropertyName("outer");
                writer.WriteStartArray();
                foreach (var he in f.Outer.Edges)
                {
                    WriteHalfEdge(he, writer, value);
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
                            WriteHalfEdge(he, writer, value);
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

        private void WriteHalfEdge(HalfEdge he, JsonWriter writer, object value)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("vertex_id");
            writer.WriteValue(he.Vertex.Id);
            writer.WritePropertyName("edge_id");
            writer.WriteValue(he.Edge.Id);
            writer.WritePropertyName("edge_side");
            writer.WriteValue(he.Edge.Left == he ? "left" : "right");
            writer.WriteEndObject();
        }
    }
}