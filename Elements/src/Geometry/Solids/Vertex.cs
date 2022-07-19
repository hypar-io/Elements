namespace Elements.Geometry.Solids
{
    /// <summary>
    /// A Solid Vertex.
    /// </summary>
    public partial class Vertex
    {
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
        public Vertex(long id, Vector3 point)
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
    }
}