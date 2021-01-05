#pragma warning disable CS1591

using System;
using Elements.Geometry;
using Newtonsoft.Json;

namespace Elements.Serialization.JSON
{
    /// <summary>
    /// UV converter.
    /// </summary>
    public class VertexConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Vertex);
        }

        public override bool CanRead => false;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            // Triangle vertices are written to JSON as indices as their
            // vertex references are already written in the Mesh.Vertices collection.
            // The MeshConverter handles re-hydrating vertices in Mesh.Vertices
            // then setting the references on triangles appropriately using the index.
            throw new NotSupportedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            // Write only the index.
            var v = (Vertex)value;
            serializer.Serialize(writer, v.Index);
        }
    }
}
