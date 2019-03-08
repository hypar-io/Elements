namespace Elements.Geometry.Solids
{
    /// <summary>
    /// One half of the directional components of an Edge.
    /// </summary>
    public class HalfEdge
    {
        /// <summary>
        /// The Edge of which this is one half.
        /// </summary>
        public Edge Edge{get; internal set;}

        /// <summary>
        /// The Vertex at the start of the edge.
        /// </summary>
        public Vertex Vertex{get; set;}

        /// <summary>
        /// The Loop to which this HalfEdge belongs.
        /// </summary>
        public Loop Loop{get; internal set;}

        internal HalfEdge(Vertex v)
        {
            this.Vertex = v;
            v.HalfEdge = this;
        }

        internal HalfEdge(Vertex v, Loop l)
        {
            this.Vertex = v;
            v.HalfEdge = this;
            this.Loop = l;
        }

        internal HalfEdge(Edge edge, Loop loop)
        {
            this.Edge = edge;
            this.Loop = loop;
        }

        /// <summary>
        /// Construct a HalfEdge.
        /// </summary>
        /// <param name="edge">The Edge associated with this HalfEdge.</param>
        /// <param name="vertex">The Vertex at the start of the edge.</param>
        public HalfEdge(Edge edge, Vertex vertex)
        {
            this.Edge = edge;
            this.Vertex = vertex;
            this.Vertex.HalfEdge = this;
        }

        /// <summary>
        /// Get the string representation of this half edge.
        /// </summary>
        public override string ToString()
        {
            return this.Vertex.ToString();
        }
    }
}