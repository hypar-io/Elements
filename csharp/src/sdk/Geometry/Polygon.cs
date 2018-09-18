using ClipperLib;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Hypar.SDK.Tests")]
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
        /// Construct a Polygon from a collection of points.
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
        /// Offset this polyline by the specified amount.
        /// </summary>
        /// <param name="offset">The amount to offset.</param>
        /// <returns>A new polyline offset by offset.</returns>
        public IEnumerable<Polygon> Offset(double offset)
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
        internal enum PolygonOps
        {
            Difference,
            Intersection,
            Union,
            XOR
        }

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

        /// <summary>
        /// Returns true if the interior of the first polygon is coincident with every interior and boundary point of the second polygon.
        /// </summary>
        /// <param name="p1">The polygon boundary.</param>
        /// <param name="p2">The polygon tested for containment within the boundary.</param>
        /// <param name="cvr">If true, polygon is contained if boundary points are coincident.</param>
        /// <returns></returns>
        internal static bool Contains(Polygon p1, Polygon p2, bool cvr = false)
        {
            var p1Path = ToClipperPath(p1);
            var p2Path = ToClipperPath(p2);
            foreach (IntPoint point in p2Path)
            {
                if (cvr ? Clipper.PointInPolygon(point, p1Path) == 0 : Clipper.PointInPolygon(point, p1Path) <= 0)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Returns true if the two polygons have no interior or boundary points coincident.
        /// </summary>
        /// <param name="p1">The first polygon to compare.</param>
        /// <param name="p2">The second polygon to compare.</param>
        /// <returns></returns>
        internal static bool Disjoint(Polygon p1, Polygon p2)
        {
            var p1Path = ToClipperPath(p1);
            var p2Path = ToClipperPath(p2);
            foreach (IntPoint point in p1Path)
            {
                if (Clipper.PointInPolygon(point, p2Path) != 0)
                {
                    return false;
                }
            }
            foreach (IntPoint point in p2Path)
            {
                if (Clipper.PointInPolygon(point, p1Path) != 0)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Returns true if the interior of two polygons intersect.
        /// </summary>
        /// <param name="p1">The first polygon to compare.</param>
        /// <param name="p2">The second polygon to compare.</param>
        /// <returns></returns>
        internal static bool Intersect(Polygon p1, Polygon p2)
        {
            var p1Path = ToClipperPath(p1);
            var p2Path = ToClipperPath(p2);
            foreach (IntPoint point in p1Path)
            {
                if (Clipper.PointInPolygon(point, p2Path) == 1)
                {
                    return true;
                }
            }
            foreach (IntPoint point in p2Path)
            {
                if (Clipper.PointInPolygon(point, p1Path) == 1)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns true if two polygons share at least one perimeter point without their interiors intersecting.
        /// </summary>
        /// <param name="p1">The first polygon for the comparison.</param>
        /// <param name="p2">The second polygon for the comparison.</param>
        /// <returns></returns>
        internal static bool Touches(Polygon p1, Polygon p2)
        {
            var p1Path = ToClipperPath(p1);
            var p2Path = ToClipperPath(p2);
            bool touches = false;
            foreach (IntPoint point in p2Path)
            {
                switch (Clipper.PointInPolygon(point, p1Path))
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
        /// Performs difference,intersection, union, or XOR operations on two polygons.
        /// </summary>
        /// <param name="p1">The first intersecting polygon.</param>
        /// <param name="p2">The second intersecting polygon.</param>
        /// <param name="op">The polygon operation.</param>
        /// <returns></returns>
        internal static IEnumerable<Polygon> Ops(Polygon p1, Polygon p2, PolygonOps op)
        {
            var p1Path = ToClipperPath(p1);
            var p2Path = ToClipperPath(p2);
            Clipper clipper = new Clipper();
            clipper.AddPath(p1Path, PolyType.ptSubject, true);
            clipper.AddPath(p2Path, PolyType.ptClip, true);
            var solution = new List<List<IntPoint>>();
            switch (op)
            {
                case PolygonOps.Difference:
                    clipper.Execute(ClipType.ctDifference, solution);
                    break;
                case PolygonOps.Intersection:
                    clipper.Execute(ClipType.ctIntersection, solution);
                    break;
                case PolygonOps.Union:
                    clipper.Execute(ClipType.ctUnion, solution);
                    break;
                case PolygonOps.XOR:
                    clipper.Execute(ClipType.ctUnion, solution);
                    break;
            }
            var polygons = new List<Polygon>();
            foreach (List<IntPoint> path in solution)
            {
                polygons.Add(ToPolygon(path));
            }
            return polygons;
        }
    }
}