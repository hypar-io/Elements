using System.Collections.Generic;
using System;
using System.Linq;
using System.Collections;

namespace Hypar.Geometry
{
    /// <summary>
    /// A planar polyon in 3D.
    /// </summary>
    public class Polyline : IEnumerable<Vector3>
    {
        private BBox _bbox;

        public BBox BoundingBox => _bbox;

        private List<Vector3> _vertices = new List<Vector3>();

        /// <summary>
        /// The vertices of the polygon.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Vector3> Vertices
        {
            get{return _vertices;}
        }

        /// <summary>
        /// Construct a Polygon3 from a collection of vertices.
        /// </summary>
        /// <param name="vertices">A CCW wound set of vertices.</param>
        public Polyline(IEnumerable<Vector3> vertices)
        {
            _vertices.AddRange(vertices);
            this._bbox = BBox.FromPoints(vertices);
        }

        /// <summary>
        /// Reverse the direction of a Polygon3.
        /// </summary>
        /// <returns>Returns a new Polygon3 with opposite winding.</returns>
        public Polyline Reversed()
        {
            var verts = new List<Vector3>(_vertices);
            verts.Reverse();
            return new Polyline(verts);
        }

        public override string ToString()
        {
            return string.Join(",", this.Vertices);
        }
        
        /// <summary>
        /// Explode a Polygon3 into a collection of Lines.
        /// </summary>
        /// <returns>A collection of Lines.</returns>
        public IEnumerable<Line> Explode()
        {
            var lines = new List<Line>();
            for(var i=0; i<_vertices.Count; i++)
            {
                var a = _vertices[i];
                Vector3 b;
                if(i == _vertices.Count-1)
                {
                    b = _vertices[0];
                }
                else
                {
                    b = _vertices[i+1];
                }
                
                lines.Add(new Line(a, b));
            }
            return lines;
        }

        public Vector3[] ToArray()
        {
            return this._vertices.ToArray();
        }

        /// <summary>
        /// Compute the normal of the Polygon using the first 3 vertices to define a plane.
        /// </summary>
        /// <returns></returns>
        public Vector3 Normal()
        {
            var a = (this._vertices[2] - this._vertices[1]).Normalized();
            var b = (this._vertices[0] - this._vertices[1]).Normalized();

            return a.Cross(b);
        }

        public Line Segment(int i)
        {
            if(this._vertices.Count <= i)
            {
                throw new Exception($"The specified index is greater than the number of segments.");
            }

            var a = this._vertices[i];
            var b = this._vertices[i+1];
            return new Line(a,b);
        }


        public IEnumerator<Vector3> GetEnumerator()
        {
            return ((IEnumerable<Vector3>)_vertices).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<Vector3>)_vertices).GetEnumerator();
        }
    }
}