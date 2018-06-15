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
        /// Construct a polyline from a collection of vertices.
        /// </summary>
        /// <param name="vertices">A CCW wound set of vertices.</param>
        public Polyline(IEnumerable<Vector3> vertices)
        {
            _vertices.AddRange(vertices);
            this._bbox = BBox.FromPoints(vertices);
        }

        /// <summary>
        /// Reverse the direction of a polyline.
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
        /// Get a collection a lines representing each segment of the polyline.
        /// </summary>
        /// <returns>A collection of Lines.</returns>
        public IEnumerable<Line> Segments()
        {
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
                
                yield return new Line(a, b);
            }
        }

        public Vector3[] ToArray()
        {
            return this._vertices.ToArray();
        }

        /// <summary>
        /// Compute the normal of the polyline using the first 3 vertices to define a plane.
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

        public Polyline Offset(double offset)
        {
            var pts = new Vector3[this._vertices.Count];
            Vector3 prevN = null;
            for(var i=0; i < this._vertices.Count; i++)
            {
                var a = i;
                var b = a + 1 > this._vertices.Count-1 ? 0 : a + 1;
                var c = b + 1 > this._vertices.Count-1 ? 0 : b + 1;

                var v1 = (this._vertices[a]-this._vertices[b]).Normalized();
                var v2 = (this._vertices[c]-this._vertices[b]).Normalized();
                var n = v1.Cross(v2);
                if(prevN == null)
                {
                    prevN = n;
                }

                // Naive flipping logic. We find a change in concavity/convexity
                // by comparing the cross product of the vectors to the
                // previous corner. If the vectors point in different directions
                // then we offset in the opposite direction.
                var dir = prevN.Dot(n) < 0 ? -1 : 1;

                var theta = v1.AngleTo(v2);
                var halfAngle = (Math.PI - theta)/2;
                var d = offset/Math.Cos(halfAngle);

                pts[i] = this._vertices[b] + v1.Average(v2) * d * dir;
            }
            return new Polyline(pts);
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