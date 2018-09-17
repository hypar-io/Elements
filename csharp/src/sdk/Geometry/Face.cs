using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Hypar.Geometry
{
    /// <summary>
    /// A face bounded by a set of edges.
    /// </summary>
    public class Face : IEnumerable<Line>
    {
        private List<Line> _lines = new List<Line>();
        
        private List<Vector3> _vertices = new List<Vector3>();

        /// <summary>
        /// The vertices which form the face.
        /// </summary>
        /// <value></value>
        public IEnumerable<Vector3> Vertices
        {
            get {return this._vertices;}
        }

        /// <summary>
        /// Construct a face.
        /// </summary>
        /// <param name="edges"></param>
        public Face(IEnumerable<Line> edges)
        {
            this._lines.AddRange(edges);
            this._vertices = this._lines.Select(l=>l.End).ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerator<Line> GetEnumerator()
        {
            return this._lines.GetEnumerator();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this._lines.GetEnumerator();
        }
    }
}