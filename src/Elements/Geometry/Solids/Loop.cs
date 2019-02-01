using System.Collections.Generic;

namespace Elements.Geometry.Solids
{
    /// <summary>
    /// A Loop of HalfEdges which bound a Face.
    /// </summary>
    public class Loop
    {
        /// <summary>
        /// The Face to which this Loop corresponds.
        /// </summary>
        public Face Face{get; set;}

        /// <summary>
        /// A collection of HalfEdges which comprise the Loop.
        /// </summary>
        public List<HalfEdge> Edges {get;}

        /// <summary>
        /// Construct a Loop.
        /// </summary>
        public Loop()
        {
            this.Edges = new List<HalfEdge>();
        }

        /// <summary>
        /// Construct a Loop from an array of HalfEdges.
        /// </summary>
        /// <param name="edges"></param>
        public Loop(HalfEdge[] edges)
        {
            this.Edges = new List<HalfEdge>();
            foreach(var e in edges)
            {
                this.Edges.Add(e);
                e.Loop = this;
            }
        }

        /// <summary>
        /// Add a HalfEdge ot the start of the Loop.
        /// </summary>
        /// <param name="he"></param>
        public void AddEdgeToStart(HalfEdge he)
        {
            this.Edges.Insert(0, he);
            he.Loop = this;
        }

        /// <summary>
        /// Add a HalfEdge to the end of the Loop.
        /// </summary>
        /// <param name="he"></param>
        public void AddEdgeToEnd(HalfEdge he)
        {
            this.Edges.Add(he);
            he.Loop = this;
        }
    }
}