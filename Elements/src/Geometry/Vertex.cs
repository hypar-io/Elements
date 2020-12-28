using System.Collections.Generic;
using Newtonsoft.Json;

namespace Elements.Geometry
{
    public partial class Vertex
    {
        // The associated triangles are not defined on the 
        // auto-generated vertex class because doing so causes a circular reference
        // during serialization.

        /// <summary>The triangles associated with this vertex.</summary>
        [Newtonsoft.Json.JsonProperty("Triangles", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required]
        [JsonIgnore]
        public IList<Triangle> Triangles { get; set; } = new List<Triangle>();

        /// <summary>
        /// Create a vertex.
        /// </summary>
        /// <param name="position">The position of the vertex.</param>
        /// <param name="normal">The vertex's normal.</param>
        /// <param name="color">The vertex's color.</param>
        public Vertex(Vector3 position, Vector3? normal = null, Color color = default(Color))
        {
            this.Position = position;
            this.Normal = normal ?? Vector3.Origin;
            this.Color = color;
        }
    }
}