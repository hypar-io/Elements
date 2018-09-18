using ClipperLib;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Hypar.Geometry
{
    /// <summary>
    /// A closed planar polygon.
    /// </summary>
    public class Polygon : ICurve
    {
        private IList<Vector3> _vertices;

        /// <summary>
        /// The area enclosed by the polygon.
        /// </summary>
        /// <value></value>
        [JsonProperty("area")]
        public double Area
        {
            get {
                var area = 0.0;
                for(var i = 0; i<= _vertices.Count-1; i++)
                {
                    var j = (i+1) % _vertices.Count;
                    area += _vertices[i].X * _vertices[j].Y;
                    area -= _vertices[i].Y * _vertices[j].X;
                }
                return area/2.0;
            }
        }

        /// <summary>
        /// The vertices of the polygon.
        /// </summary>
        [JsonProperty("vertices")]
        public IList<Vector3> Vertices
        {
            get{return this._vertices;}
        }

        /// <summary>
        /// The length of the polygon.
        /// </summary>
        [JsonProperty("length")]
        public double Length
        {
            get{return this.Segments().Sum(s=>s.Length);}
        }

        /// <summary>
        /// The start of the polygon.
        /// </summary>
        [JsonIgnore]
        public Vector3 Start
        {
            get{return this._vertices[0];}
        }

        /// <summary>
        /// The end of the polygon.
        /// </summary>
        [JsonIgnore]
        public Vector3 End
        {
            get{return this._vertices[this._vertices.Count - 1];}
        }

        /// <summary>
        /// Construct a Polygon from a collection of points.
        /// </summary>
        /// <param name="vertices">A collection of vertices.</param>
        public Polygon(IList<Vector3> vertices)
        {
            this._vertices = vertices;
        }

        /// <summary>
        /// Offset this polyline by the specified amount.
        /// </summary>
        /// <param name="offset">The amount to offset.</param>
        /// <returns>A new polyline offset by offset.</returns>
        public IList<Polygon> Offset(double offset)
        {
            var scale = 1024.0;
            var path = this.ToClipperPath();

            var solution = new List<List<IntPoint>>();
            var co = new ClipperOffset();
            co.AddPath(path, JoinType.jtMiter, EndType.etClosedPolygon);
            co.Execute(ref solution, offset * scale);  // important, scale also used here

            var result = new List<Polygon>();
            foreach (var loop in solution)
            {
                result.Add(loop.ToPolygon());
            }
            return result;
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
        /// Get a point on the polygon at parameter u.
        /// </summary>
        /// <param name="u">A value between 0.0 and 1.0.</param>
        /// <returns></returns>
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
        /// Tessellate the polygon.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Vector3> Tessellate()
        {
            return this._vertices;
        }

        /// <summary>
        /// Reverse the direction of a polygon.
        /// </summary>
        /// <returns>Returns a new polgon with opposite winding.</returns>
        public Polygon Reversed()
        {
            return new Polygon(this._vertices.Reverse().ToArray());
        }

        /// <summary>
        /// Is this polygon equal to the provided polygon?
        /// </summary>
        /// <param name="obj"></param>
        public override bool Equals(object obj)
        {
            var p = obj as Polygon;
            if(p == null)
            {
                return false;
            }
            if(this.Vertices.Count != p.Vertices.Count)
            {
                return false;
            }

            for(var i=0; i<this.Vertices.Count; i++)
            {
                if(!this.Vertices[i].Equals(p.Vertices[i]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Get the hash code for the polygon.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return this.Vertices.GetHashCode();
        }
    }

    /// <summary>
    /// Polygon extension methods.
    /// </summary>
    internal static class PolygonExtensions
    {
        /// <summary>
        /// Construct a clipper path from a Polygon.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        internal static List<IntPoint> ToClipperPath(this Polygon p)
        {
            var scale = 1024.0;
            var path = new List<IntPoint>();
            foreach(var v in p.Vertices)
            {
                path.Add(new IntPoint(v.X * scale, v.Y * scale));
            }
            return path;
        }

        internal static Polygon ToPolygon(this List<IntPoint> p)
        {
            var scale = 1024.0;
            return new Polygon(p.Select(v=>new Vector3(v.X/scale, v.Y/scale)).ToArray());
        }
    }
}