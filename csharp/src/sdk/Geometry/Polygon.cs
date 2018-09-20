using ClipperLib;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Hypar.Geometry
{
    /// <summary>
    /// A closed planar polygon.
    /// </summary>
    public class Polygon : ICurve
    {
        private List<Vector3> _vertices = new List<Vector3>();

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
        /// The centroid of the Polygon.
        /// </summary>
        /// <returns>
        /// Vector3
        /// </returns>
        [JsonIgnore]
        public Vector3 Centroid
        {
            get
            {
                var x = 0.0;
                var y = 0.0;
                var factor = 0.0;
                for (var i = 0; i < this._vertices.Count; i++)
                {
                    factor =
                        (_vertices[i].X * _vertices[(i + 1) % _vertices.Count].Y) -
                        (_vertices[(i + 1) % _vertices.Count].X * _vertices[i].Y);
                    x += (_vertices[i].X + _vertices[(i + 1) % _vertices.Count].X) * factor;
                    y += (_vertices[i].Y + _vertices[(i + 1) % _vertices.Count].Y) * factor;
                }
                var divisor = this.Area * 6;
                x /= divisor;
                y /= divisor;
                return new Vector3(System.Math.Abs(x), System.Math.Abs(y));
            }
        }

        /// <summary>
        /// Construct a Polygon from a collection of vertices.
        /// </summary>
        /// <param name="vertices"></param>
        public Polygon(IEnumerable<Vector3> vertices)
        {
            this._vertices.AddRange(vertices);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerator<Vector3> GetEnumerator()
        {
            return this._vertices.GetEnumerator();
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
            for (var i = 0; i < this._vertices.Count - 1; i++)
            {
                var a = this._vertices[i];
                var b = this._vertices[i + 1];
                var currLength = a.DistanceTo(b);
                var currVec = (b - a).Normalized();
                if (totalLength <= d && totalLength + currLength >= d)
                {
                    return a + currVec * ((d - totalLength) / currLength);
                }
                totalLength += currLength;
            }

            return this.End;
        }

        /// <summary>
        /// Returns true if every vertex of the supplied Polygon falls within the perimeter of this Polygon.
        /// </summary>
        /// <param name="polygon">The Polygon to compare.</param>
        /// <returns></returns>
        public bool Contains(Polygon polygon)
        {
            var thisPath = PolygonExtensions.ToClipperPath(this);
            var polyPath = PolygonExtensions.ToClipperPath(polygon);
            foreach (IntPoint vertex in polyPath)
            {
                if (Clipper.PointInPolygon(vertex, thisPath) <= 0)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Returns true if every point of the supplied Polygon falls within or on the perimeter of this Polygon.
        /// </summary>
        /// <param name="polygon">The Polygon to compare.</param>
        /// <returns></returns>
        public bool Covers(Polygon polygon)
        {
            var thisPath = PolygonExtensions.ToClipperPath(this);
            var polyPath = PolygonExtensions.ToClipperPath(polygon);
            foreach (IntPoint vertex in polyPath)
            {
                if (Clipper.PointInPolygon(vertex, thisPath) == 0)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Returns true if this Polygon and the supplied Polygon do not touch or intersect.
        /// </summary>
        /// <param name="polygon">The Polygon to compare.</param>
        /// <returns></returns>
        public bool Disjoint(Polygon polygon)
        {
            var thisPath = PolygonExtensions.ToClipperPath(this);
            var polyPath = PolygonExtensions.ToClipperPath(polygon);
            foreach (IntPoint vertex in thisPath)
            {
                if (Clipper.PointInPolygon(vertex, polyPath) != 0)
                {
                    return false;
                }
            }
            foreach (IntPoint vertex in polyPath)
            {
                if (Clipper.PointInPolygon(vertex, thisPath) != 0)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Returns true if this Polygon and the supplied Polygon share areas within their perimeters.
        /// </summary>
        /// <param name="polygon">The Polygon to compare.</param>
        /// <returns></returns>
        public bool Intersects(Polygon polygon)
        {
            var thisPath = PolygonExtensions.ToClipperPath(this);
            var polyPath = PolygonExtensions.ToClipperPath(polygon);
            foreach (IntPoint vertex in thisPath)
            {
                if (Clipper.PointInPolygon(vertex, polyPath) == 1)
                {
                    return true;
                }
            }
            foreach (IntPoint vertex in polyPath)
            {
                if (Clipper.PointInPolygon(vertex, thisPath) == 1)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns true if this Polygon and the supplied Polygon share at least one perimeter point without interesecting.
        /// </summary>
        /// <param name="polygon">The Polygon to compare.</param>
        /// <returns></returns>
        public bool Touches(Polygon polygon)
        {
            var thisPath = PolygonExtensions.ToClipperPath(this);
            var polyPath = PolygonExtensions.ToClipperPath(polygon);
            bool touches = false;
            foreach (IntPoint vertex in polyPath)
            {
                switch (Clipper.PointInPolygon(vertex, thisPath))
                {
                    case -1:
                        touches = true;
                        break;
                    case 1: return false;
                }
            }
            return touches;
        }

        /// <summary>
        /// Returns a list of Polygons representing the difference between this Polygon and the supplied Polygon.
        /// </summary>
        /// <param name="polygon">The intersecting Polygon.</param>
        /// <returns></returns>
        public IEnumerable<Polygon> Difference(Polygon polygon)
        {
            var thisPath = PolygonExtensions.ToClipperPath(this);
            var polyPath = PolygonExtensions.ToClipperPath(polygon);
            Clipper clipper = new Clipper();
            clipper.AddPath(thisPath, PolyType.ptSubject, true);
            clipper.AddPath(polyPath, PolyType.ptClip, true);
            var solution = new List<List<IntPoint>>();
            clipper.Execute(ClipType.ctDifference, solution);
            var polygons = new List<Polygon>();
            foreach (List<IntPoint> path in solution)
            {
                polygons.Add(PolygonExtensions.ToPolygon(path));
            }
            return polygons;
        }

        /// <summary>
        /// Returns a list of Polygons representing the intersections between this Polygon and the supplied Polygon.
        /// </summary>
        /// <param name="polygon">The intersecting Polygon.</param>
        /// <returns></returns>
        public IEnumerable<Polygon> Intersection(Polygon polygon)
        {
            var thisPath = PolygonExtensions.ToClipperPath(this);
            var polyPath = PolygonExtensions.ToClipperPath(polygon);
            Clipper clipper = new Clipper();
            clipper.AddPath(thisPath, PolyType.ptSubject, true);
            clipper.AddPath(polyPath, PolyType.ptClip, true);
            var solution = new List<List<IntPoint>>();
            clipper.Execute(ClipType.ctIntersection, solution);
            var polygons = new List<Polygon>();
            foreach (List<IntPoint> path in solution)
            {
                polygons.Add(PolygonExtensions.ToPolygon(path));
            }
            return polygons;
        }

        /// <summary>
        /// Returns Polygons representing the attempted Union between this Polygon and the supplied Polygon.
        /// </summary>
        /// <param name="polygon">The Polygon to be combined with this Polygon.</param>
        /// <returns></returns>
        public IEnumerable<Polygon> Union(Polygon polygon)
        {
            var thisPath = PolygonExtensions.ToClipperPath(this);
            var polyPath = PolygonExtensions.ToClipperPath(polygon);
            Clipper clipper = new Clipper();
            clipper.AddPath(thisPath, PolyType.ptSubject, true);
            clipper.AddPath(polyPath, PolyType.ptClip, true);
            var solution = new List<List<IntPoint>>();
            clipper.Execute(ClipType.ctUnion, solution);
            var polygons = new List<Polygon>();
            foreach (List<IntPoint> path in solution)
            {
                polygons.Add(PolygonExtensions.ToPolygon(path));
            }
            return polygons;
        }

        /// <summary>
        /// Returns Polygons representing the symmetric difference between this Polygon and the supplied Polygon.
        /// </summary>
        /// <param name="polygon">The intersecting polygon.</param>
        /// <returns></returns>
        public IEnumerable<Polygon> XOR(Polygon polygon)
        {
            var thisPath = PolygonExtensions.ToClipperPath(this);
            var polyPath = PolygonExtensions.ToClipperPath(polygon);
            Clipper clipper = new Clipper();
            clipper.AddPath(thisPath, PolyType.ptSubject, true);
            clipper.AddPath(polyPath, PolyType.ptClip, true);
            var solution = new List<List<IntPoint>>();
            clipper.Execute(ClipType.ctUnion, solution);
            var polygons = new List<Polygon>();
            foreach (List<IntPoint> path in solution)
            {
                polygons.Add(PolygonExtensions.ToPolygon(path));
            }
            return polygons;
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
            var verts = new List<Vector3>(_vertices);
            verts.Reverse();
            return new Polygon(verts);
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

        /// <summary>
        /// Construct a Polygon from a clipper path 
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        internal static Polygon ToPolygon(this List<IntPoint> p)
        {
            var scale = 1024.0;
            return new Polygon(p.Select(v=>new Vector3(v.X/scale, v.Y/scale)));
        } 
    }
}