using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Collections;

namespace Hypar.Geometry
{
    /// <summary>
    /// A planar polygon in 3D.
    /// </summary>
    public class Polyline : IEnumerable<Vector3>
    {
        /// <summary>
        /// The bounds of the polyline.
        /// </summary>
        public BBox BoundingBox
        {
            get{return new BBox(this.Vertices);}
        }

        /// <summary>
        /// The area enclosed by the polyline.
        /// </summary>
        /// <value></value>
        public double Area
        {
            get{return Area3D(this._vertices.Count, this.Vertices.ToArray().Concat(new[]{this._vertices[0]}).ToArray(), this.Normal);}
        }

        /// <summary>
        /// The normal of the polyline using the first 3 vertices to define a plane.
        /// </summary>
        /// <returns></returns>
        public Vector3 Normal
        {
            get
            {
                var a = (this._vertices[2] - this._vertices[1]).Normalized();
                var b = (this._vertices[0] - this._vertices[1]).Normalized();
                return a.Cross(b);
            }
        }

        private List<Vector3> _vertices = new List<Vector3>();

        /// <summary>
        /// The vertices of the polyline.
        /// </summary>
        /// <returns></returns>
        [JsonProperty("vertices")]
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
        }

        /// <summary>
        /// Reverse the direction of a polyline.
        /// </summary>
        /// <returns>Returns a new polyline with opposite winding.</returns>
        public Polyline Reversed()
        {
            var verts = new List<Vector3>(_vertices);
            verts.Reverse();
            return new Polyline(verts);
        }

        /// <summary>
        /// Get a string representation of this polyline.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Join(",", this.Vertices);
        }
        
        /// <summary>
        /// Get a collection a lines representing each segment of this polyline.
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

        /// <summary>
        /// Convert this polyline to an array of vectors.
        /// </summary>
        /// <returns></returns>
        public Vector3[] ToArray()
        {
            return this._vertices.ToArray();
        }

        /// <summary>
        /// Get segment i of this polyline.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Offset this polyline by the specified amount.
        /// </summary>
        /// <param name="offset">The amount to offset.</param>
        /// <returns>A new polyline offset by offset.</returns>
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

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerator<Vector3> GetEnumerator()
        {
            return ((IEnumerable<Vector3>)_vertices).GetEnumerator();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<Vector3>)_vertices).GetEnumerator();
        }

        /// <summary>
        /// Compute the area of a 3D planar polygon.
        /// </summary>
        /// <param name="n">The number of vertices of the polygon.</param>
        /// <param name="V">An array of n+1 points in a 2D plane with V[n]=V[0]</param>
        /// <param name="N">The normal vector of the Polygon's plane.</param>
        /// <returns></returns>
        private double Area3D(int n, Vector3[] V, Vector3 N )
        {
            double area = 0;
            double an, ax, ay, az; // abs value of normal and its coords
            int  coord;           // coord to ignore: 1=x, 2=y, 3=z
            int  i, j, k;         // loop indices

            if (n < 3) return 0;  // a degenerate polygon

            // select largest abs coordinate to ignore for projection
            ax = (N.X>0 ? N.X : -N.X);    // abs x-coord
            ay = (N.Y>0 ? N.Y : -N.Y);    // abs y-coord
            az = (N.Z>0 ? N.Z : -N.Z);    // abs z-coord

            coord = 3;                    // ignore z-coord
            if (ax > ay) {
                if (ax > az) coord = 1;   // ignore x-coord
            }
            else if (ay > az) coord = 2;  // ignore y-coord

            // compute area of the 2D projection
            switch (coord) {
            case 1:
                for (i=1, j=2, k=0; i<n; i++, j++, k++)
                    area += (V[i].Y * (V[j].Z - V[k].Z));
                break;
            case 2:
                for (i=1, j=2, k=0; i<n; i++, j++, k++)
                    area += (V[i].Z * (V[j].X - V[k].X));
                break;
            case 3:
                for (i=1, j=2, k=0; i<n; i++, j++, k++)
                    area += (V[i].X * (V[j].Y - V[k].Y));
                break;
            }
            switch (coord) {    // wrap-around term
            case 1:
                area += (V[n].Y * (V[1].Z - V[n-1].Z));
                break;
            case 2:
                area += (V[n].Z * (V[1].X - V[n-1].X));
                break;
            case 3:
                area += (V[n].X * (V[1].Y - V[n-1].Y));
                break;
            }

            // scale to get area before projection
            an = Math.Sqrt( ax*ax + ay*ay + az*az); // length of normal vector
            switch (coord) {
                case 1:
                    area *= (an / (2 * N.X));
                    break;
                case 2:
                    area *= (an / (2 * N.Y));
                    break;
                case 3:
                    area *= (an / (2 * N.Z));
                    break;
            }
            return area;
        }
    }
}