using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Hypar.Geometry
{
    /// <summary>
    /// A face bounded by a set of edges.
    /// </summary>
    public class Face
    {
        private List<Line> _edges = new List<Line>();
        
        private List<Vector3> _vertices = new List<Vector3>();

        /// <summary>
        /// The vertices which form the Face.
        /// </summary>
        public IList<Vector3> Vertices
        {
            get {return this._vertices;}
        }

        /// <summary>
        /// The edges of the Face.
        /// </summary>
        public IList<Line> Edges
        {
            get{return this._edges;}
        }

        /// <summary>
        /// Construct a Face.
        /// </summary>
        /// <param name="edges">A collection of Lines which bound the Face.</param>
        public Face(IList<Line> edges)
        {
            this._edges.AddRange(edges);
            this._vertices = this._edges.Select(l=>l.End).ToList();
        }
    }
}