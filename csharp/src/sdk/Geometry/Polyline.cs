using ClipperLib;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Collections;

namespace Hypar.Geometry
{
    /// <summary>
    /// A coplanar continuous set of lines.
    /// </summary>
    public class Polyline: ICurve
    {
        private List<Vector3> _vertices = new List<Vector3>();

        /// <summary>
        /// The vertices of the polyline.
        /// </summary>
        /// <value></value>
        [JsonProperty("vertices")]
        public IList<Vector3> Vertices
        {
            get{return this._vertices;}
        }

        /// <summary>
        /// The length of the polyline.
        /// </summary>
        public double Length
        {
            get{return this.Segments().Sum(s=>s.Length);}
        }

        /// <summary>
        /// The start of the polyline.
        /// </summary>
        public Vector3 Start
        {
            get{return this._vertices[0];}
        }

        /// <summary>
        /// The end of the polyline.
        /// </summary>
        public Vector3 End
        {
            get{return this._vertices[this._vertices.Count - 1];}
        }

        /// <summary>
        /// Construct a polyline from a collection of vertices.
        /// </summary>
        /// <param name="vertices">A CCW wound set of vertices.</param>
        public Polyline(IList<Vector3> vertices)
        {
            this._vertices.AddRange(vertices);
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
            return string.Join(",", this._vertices);
        }

        /// <summary>
        /// Get a collection a lines representing each segment of this polyline.
        /// </summary>
        /// <returns>A collection of Lines.</returns>
        public IEnumerable<Line> Segments()
        {
            for (var i = 0; i < _vertices.Count-1; i++)
            {
                var a = _vertices[i];
                var b = _vertices[i+1];
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
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the specified index is greater than the number of available segments.</exception>
        public Line Segment(int i)
        {
            if (this._vertices.Count <= i)
            {
                throw new ArgumentOutOfRangeException($"The specified index is greater than the number of segments.");
            }

            var a = this._vertices[i];
            var b = this._vertices[i + 1];
            return new Line(a, b);
        }

        /// <summary>
        /// Get the point at parameter u along the polyline.
        /// </summary>
        /// <param name="u">A value between 0.0 and 1.0.</param>
        public Vector3 PointAt(double u)
        {
            var d = this.Length * u;
            var totalLength = 0.0;
            for(var i=0; i<this._vertices.Count-1; i++)
            {
                var a = this._vertices[i];
                var b = this._vertices[i+1];
                var currLength = a.DistanceTo(b);
                var currVec = (b - a).Normalized();
                if(totalLength <= d && totalLength + currLength >= d)
                {
                    return a + currVec * ((d-totalLength)/currLength);
                }
                totalLength += currLength;
            }

            return this.End;
        }

        /// <summary>
        /// Tessellate the polyline.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Vector3> Tessellate()
        {
            return this._vertices;
        }
    }
}