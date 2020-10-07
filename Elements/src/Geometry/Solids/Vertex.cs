using System.Collections.Generic;

namespace Elements.Geometry.Solids
{
    /// <summary>
    /// A Solid Vertex.
    /// </summary>
    public partial class Vertex
    {
        /// <summary>
        /// The triangles which contain this vertex.
        /// </summary>
        public List<Triangle> Triangles { get; } = new List<Triangle>();

        /// <summary>
        /// The Id of the Vertex.
        /// </summary>
        public long Id { get; }

        /// <summary>
        /// The HalfEdge which has this Vertex as its start.
        /// </summary>
        public HalfEdge HalfEdge { get; set; }

        /// <summary>
        /// The location of the Vertex.
        /// </summary>
        public Vector3 Point { get; }

        /// <summary>
        /// Construct a Vertex.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="point">The location of the Vertex.</param>
        internal Vertex(long id, Vector3 point)
        {
            this.Id = id;
            this.Point = point;
        }

        /// <summary>
        /// Get the string representation of the Vertex.
        /// </summary>
        public override string ToString()
        {
            return $"Id: {this.Id}";
        }

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