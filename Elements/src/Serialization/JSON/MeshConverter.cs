#pragma warning disable CS1591

using System;
using System.Collections.Generic;
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
            mesh.Vertices = obj["Vertices"].ToObject<List<Vertex>>(serializer);
            var triangles = (JArray)obj["Triangles"];
            foreach (JObject t in triangles)
            {
                var n = t["Normal"].ToObject<Vector3>(serializer);
                var v = (JArray)t["Vertices"];
                var a = mesh.Vertices[v[0].ToObject<int>(serializer)];
                var b = mesh.Vertices[v[1].ToObject<int>(serializer)];
                var c = mesh.Vertices[v[2].ToObject<int>(serializer)];
                var tri = new Triangle(new[] { a, b, c }, n);
                mesh.AddTriangle(a, b, c);
            }
            return mesh;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var m = (Mesh)value;
            writer.WriteStartObject();
            writer.WritePropertyName("Vertices");
            serializer.Serialize(writer, m.Vertices);
            writer.WritePropertyName("Triangles");
            var triangleSerializer = new JsonSerializer();
            triangleSerializer.Converters.Add(new VertexConverter());
            triangleSerializer.Converters.Add(new Vector3Converter());
            triangleSerializer.Serialize(writer, m.Triangles);
            writer.WriteEndObject();
        }
    }
}
