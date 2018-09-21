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
        /// Construct a Polygon from a collection of vertices.
        /// </summary>
        /// <param name="vertices">A collection of vertices.</param>
        public Polygon(IList<Vector3> vertices)
        {
            for(var i=0; i<vertices.Count; i++)
            {
                for(var j=0; j<vertices.Count; j++)
                {
                    if(i == j)
                    {
                        continue;
                    }
                    if(vertices[i].IsAlmostEqualTo(vertices[j]))
                    {
                        throw new Exception($"The polygon could not be constructed. Two vertices were almost equal: a {vertices[i]} b {vertices[j]}.");
                    }
                }
            }
            this._vertices = vertices;
        }

        /// <summary>
        /// Get a point on the polygon at parameter u.
        /// </summary>
        /// <param name="u">A value between 0.0 and 1.0.</param>
        /// <returns>Returns a Vector3 indicating a point along the Polygon length from its start vertex.</returns>
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
        /// Tests if the supplied Polygon is within the perimeter of this Polygon.
        /// </summary>
        /// <param name="polygon">The Polygon to compare.</param>
        /// <returns>
        /// Returns true if every vertex of the supplied Polygon falls within the perimeter of this Polygon.
        /// </returns>
        public bool Contains(Polygon polygon)
        {
            var thisPath = this.ToClipperPath();
            var polyPath = polygon.ToClipperPath();
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
        /// Tests if the supplied Polygon is within or coincident with the perimeter of this Polygon.
        /// </summary>
        /// <param name="polygon">The Polygon to compare.</param>
        /// <returns>
        /// Returns true if every vertex of the supplied Polygon falls within or on the perimeter of this Polygon.
        /// </returns>
        public bool Covers(Polygon polygon)
        {
            var thisPath = this.ToClipperPath();
            var polyPath = polygon.ToClipperPath();
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
        /// Tests if this Polygon and the supplied Polygon are coincident in any way.
        /// </summary>
        /// <param name="polygon">The Polygon to compare.</param>
        /// <returns>
        /// Returns true if this Polygon and the supplied Polygon do not intersect or touch.
        /// </returns>
        public bool Disjoint(Polygon polygon)
        {
            var thisPath = this.ToClipperPath();
            var polyPath = polygon.ToClipperPath();
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
        /// Tests if this Polygon and the supplied Polygon share areas within their perimeters.
        /// </summary>
        /// <param name="polygon">The Polygon to compare.</param>
        /// <returns>
        /// Returns true if any vertex of either Polygon is within the perimeter of the other.
        /// </returns>
        public bool Intersects(Polygon polygon)
        {
            var thisPath = this.ToClipperPath();
            var polyPath = polygon.ToClipperPath();
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
        /// Tests if this Polygon and the supplied Polygon share at least one perimeter point without interesecting.
        /// </summary>
        /// <param name="polygon">The Polygon to compare.</param>
        /// <returns>
        /// Returns true if this Polygon and the supplied polygon share at least one perimeter point.
        /// </returns>
        public bool Touches(Polygon polygon)
        {
            var thisPath = this.ToClipperPath();
            var polyPath = polygon.ToClipperPath();
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
        /// Constructs the geometric difference between this Polygon and the supplied Polygon.
        /// </summary>
        /// <param name="polygon">The intersecting Polygon.</param>
        /// <returns>
        /// Returns a list of Polygons representing the subtraction of the supplied Polygon from this Polygon.
        /// Returns null if the area of this Polygon is entirely subtracted.
        /// Returns a list containing a representation of the perimeter of this Polygon if the two Polygons do not intersect.
        /// </returns>
        public IEnumerable<Polygon> Difference(Polygon polygon)
        {
            var thisPath = this.ToClipperPath();
            var polyPath = polygon.ToClipperPath();
            Clipper clipper = new Clipper();
            clipper.AddPath(thisPath, PolyType.ptSubject, true);
            clipper.AddPath(polyPath, PolyType.ptClip, true);
            var solution = new List<List<IntPoint>>();
            clipper.Execute(ClipType.ctDifference, solution);
            if (solution.Count == 0)
            {
                return null;
            }
            var polygons = new List<Polygon>();
            foreach (List<IntPoint> path in solution)
            {
                polygons.Add(PolygonExtensions.ToPolygon(path));
            }
            return polygons;
        }

        /// <summary>
        /// Constructs the Polygon intersections between this Polygon and the supplied Polygon.
        /// </summary>
        /// <param name="polygon">The intersecting Polygon.</param>
        /// <returns>
        /// Returns a list of Polygons representing the interesction of this Polygon with the supplied Polygon.
        /// Returns null if the two Polygons do not intersect.
        /// </returns>
        public IEnumerable<Polygon> Intersection(Polygon polygon)
        {
            var thisPath = this.ToClipperPath();
            var polyPath = polygon.ToClipperPath();
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
        /// Constructs the geometric union between this Polygon and the supplied Polygon.
        /// </summary>
        /// <param name="polygon">The Polygon to be combined with this Polygon.</param>
        /// <returns>
        /// Returns a single Polygon from a successful union.
        /// Returns null if a union cannot be performed on the two Polygons.
        /// </returns>
        public Polygon Union(Polygon polygon)
        {
            var thisPath = this.ToClipperPath();
            var polyPath = polygon.ToClipperPath();
            Clipper clipper = new Clipper();
            clipper.AddPath(thisPath, PolyType.ptSubject, true);
            clipper.AddPath(polyPath, PolyType.ptClip, true);
            var solution = new List<List<IntPoint>>();
            clipper.Execute(ClipType.ctUnion, solution);
            if (solution.Count > 1)
            {
                return null;
            }
            return solution[0].ToPolygon();
        }

        /// <summary>
        /// Returns Polygons representing the symmetric difference between this Polygon and the supplied Polygon.
        /// </summary>
        /// <param name="polygon">The intersecting polygon.</param>
        /// <returns>
        /// Returns a list of Polygons representing the symmetric difference of this Polygon and the supplied Polygon.
        /// Returns a representation of this Polygon and the supplied Polygon if the Polygons do not intersect.
        /// </returns>
        public IEnumerable<Polygon> XOR(Polygon polygon)
        {
            var thisPath = this.ToClipperPath();
            var polyPath = polygon.ToClipperPath();
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

        /// <summary>
        /// Project the specified vector onto the plane.
        /// </summary>
        /// <param name="p"></param>
        public Polygon Project(Plane p)
        {
            return new Polygon(this.Vertices.Select(v=>v.Project(p)).ToList());
        }

        /// <summary>
        /// Transform the polygon by the specified transform.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public Polygon Transform(Transform t)
        {
            return new Polygon(this.Vertices.Select(v=>t.OfPoint(v)).ToList());
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

            return new Polygon(p.Select(v=>new Vector3(v.X/scale, v.Y/scale)).ToArray());
        }
    }
}