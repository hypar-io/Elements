namespace Elements.Geometry.Solids
{
    /// <summary>
    /// A Solid Edge.
    /// </summary>
    public class Edge
    {
        /// <summary>
        /// The Id of the Edge.
        /// </summary>
        public long Id{get;}

        /// <summary>
        /// The Left edge.
        /// </summary>
        public HalfEdge Left {get; internal set;}

        /// <summary>
        /// The Right edge.
        /// </summary>
        public HalfEdge Right {get; internal set;}

        /// <summary>
        /// Construct an Edge
        /// </summary>
        /// <param name="id"></param>
        /// <param name="from">The start Vertex of the Edge.</param>
        /// <param name="to">The end Vertex of the Edge.</param>
        public Edge(long id, Vertex from, Vertex to)
        {
            this.Id = id;
            this.Left = new HalfEdge(this, from);
            this.Right = new HalfEdge(this, to);
        }
        
        /// <summary>
        /// Get the string representation of the Edge.
        /// </summary>
        public override string ToString()
        {
            return $"Id: {this.Id}, From: {this.Left.Vertex}, To: {this.Right.Vertex}";
        }

        internal Edge(long id)
        {
            this.Id = id;
        }

        internal void Reverse()
        {   
            // Reverse the half edges.
            Left.Vertex = Right.Vertex;
            Right.Vertex = Left.Vertex;
            var left = Left;
            var right = Right;

            //Flip the edge
            Left = right;
            Right = left;
        }
    }
}