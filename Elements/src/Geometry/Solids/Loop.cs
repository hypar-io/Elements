using System.Collections.Generic;
using System.Linq;

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
        public Face Face { get; set; }

        /// <summary>
        /// A collection of HalfEdges which comprise the Loop.
        /// </summary>
        public List<HalfEdge> Edges { get; }

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
            foreach (var e in edges)
            {
                this.Edges.Add(e);
                e.Loop = this;
            }
        }

        /// <summary>
        /// Convert this loop to a polygon.
        /// </summary>
        public Polygon ToPolygon(Transform transform = null)
        {
            return new Polygon(
                Edges.Select(e => transform != null ? transform.OfPoint(e.Vertex.Point) : e.Vertex.Point).ToList()
            );
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

        /// <summary>
        /// Insert the provided half edge after the target half edge.
        /// </summary>
        /// <param name="target">The half after which the new edge will be inserted.</param>
        /// <param name="newEdge">The half edge to be inserted.</param>
        public void InsertEdgeAfter(HalfEdge target, HalfEdge newEdge)
        {
            var idx = this.Edges.IndexOf(target);
            this.Edges.Insert(idx + 1, newEdge);
            newEdge.Loop = this;
        }

        /// <summary>
        /// Insert the provided half edge before the target half edge.
        /// </summary>
        /// <param name="target">The half before which the new edge will be inserted.</param>
        /// <param name="newEdge">The half edge to be inserted.</param>
        public void InsertEdgeBefore(HalfEdge target, HalfEdge newEdge)
        {
            var idx = this.Edges.IndexOf(target);
            this.Edges.Insert(idx, newEdge);
            newEdge.Loop = this;
        }
    }
}