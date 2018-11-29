using ClipperLib;
using Hypar.Elements.Serialization;
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
    // [JsonConverter(typeof(PolygonConverter))]
    public partial class Polygon : Polyline
    {
        /// <summary>
        /// The area enclosed by the polygon.
        /// </summary>
        /// <value></value>
        [JsonIgnore]
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
        /// The centroid of the Polygon.
        /// </summary>
        /// <returns>
        /// Retruns a Vector3 representation of the Polygon centroid.
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
        /// <param name="vertices">A collection of vertices.</param>
        /// <exception cref="System.ArgumentException">Thrown when coincident vertices are provided.</exception>
        public Polygon(IList<Vector3> vertices) : base(vertices){}

        /// <summary>
        /// Tests if the supplied 2D point is within the perimeter of this Polygon.
        /// </summary>
        /// <param name="point">The 2D Vector3 point to compare.</param>
        /// <returns>
        /// Returns true if the supplied Vector3 point is inside this Polygon.
        /// </returns>
        public bool Contains(Vector3 point)
        {
            var scale = 1024.0;
            var thisPath = this.ToClipperPath();
            var intPoint = new IntPoint(point.X * scale, point.Y * scale);
            if (Clipper.PointInPolygon(intPoint, thisPath) != 1)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Tests if all the supplied 2D Vector3 points fall within the perimeter of this Polygon.
        /// </summary>
        /// <param name="points">The collection of 2D Vector3 points to compare.</param>
        /// <returns>
        /// Returns true if the supplied Vector3 point is inside this Polygon.
        /// </returns>
        public bool Contains(IList<Vector3> points)
        {
            var scale = 1024.0;
            var thisPath = this.ToClipperPath();
            foreach (Vector3 point in points)
            {
                var intPoint = new IntPoint(point.X * scale, point.Y * scale);
                if (Clipper.PointInPolygon(intPoint, thisPath) != 1)
                {
                    return false;
                }
            }
            return true;
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
                if (Clipper.PointInPolygon(vertex, thisPath) != 1)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Tests if all Polygons in the supplied collection are within the perimeter of this Polygon.
        /// </summary>
        /// <param name="polygons">The collection of Polygons to compare.</param>
        /// <returns>
        /// Returns true if every supplied Polygon falls within the perimeter of this Polygon.
        /// </returns>
        public bool Contains(IList<Polygon> polygons)
        {
            var thisPath = this.ToClipperPath();
            foreach (Polygon polygon in polygons)
            {
                var polyPath = polygon.ToClipperPath();
                foreach (IntPoint vertex in polyPath)
                {
                    if (Clipper.PointInPolygon(vertex, thisPath) != 1)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Tests if the supplied 2D Vector3 point is within or touches the perimeter of this Polygon.
        /// </summary>
        /// <param name="point">The 2D Vector3 point to compare.</param>
        /// <returns>
        /// Returns true if the supplied 2D Vector3 point falls within or touches the perimeter of this Polygon.
        /// </returns>
        public bool Covers(Vector3 point)
        {
            var scale = 1024.0;
            var thisPath = this.ToClipperPath();
            var intPoint = new IntPoint(point.X * scale, point.Y * scale);
            if (Clipper.PointInPolygon(intPoint, thisPath) == 0)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Tests if all the 2D Vector3 points in the supplied list fall within or on the perimeter of this Polygon.
        /// </summary>
        /// <param name="points">The list of 2D Vector3 points to compare.</param>
        /// <returns>
        /// Returns true if any of the supplied 2D Vector3 points fall within or on the perimeter of this Polygon.
        /// </returns>
        public bool Covers(IList<Vector3> points)
        {
            var scale = 1024.0;
            var thisPath = this.ToClipperPath();
            foreach (Vector3 point in points)
            {
                var intPoint = new IntPoint(point.X * scale, point.Y * scale);
                if (Clipper.PointInPolygon(intPoint, thisPath) == 0)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Tests if this Polygon covers the supplied Polygon. This Polygon covers the supplied Polygon if all points of the supplied Polygon fall within or on the perimeter of this Polygon.
        /// </summary>
        /// <param name="polygon">The Polygon to test for coverage by this Polygon.</param>
        /// <returns>
        /// Returns true if the supplied Polygon is contained by or coincides with the perimeter of the supplied Polygon.
        /// </returns>
        public bool Covers(Polygon polygon)
        {
            var tolerance = 0.00001;
            var clipper = new Clipper();
            var solution = new List<List<IntPoint>>();
            clipper.AddPath(this.ToClipperPath(), PolyType.ptSubject, true);
            clipper.AddPath(polygon.ToClipperPath(), PolyType.ptClip, true);
            clipper.Execute(ClipType.ctIntersection, solution);
            if (solution.Count != 1)
            {
                return false;
            }
            var testPolygon = solution.First().ToPolygon();
            if (Math.Abs(polygon.Area - testPolygon.Area) > tolerance)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Tests if this Polygon covers all of the supplied Polygons. This Polygon covers the supplied Polygons if all points of the supplied Polygons fall within or on the perimeter of this Polygon.
        /// </summary>
        /// <param name="polygons">The list of Polygons to compare.</param>
        /// <returns>
        /// Returns true if any of the supplied Polygons intersects with this Polygon.
        /// </returns>
        public bool Covers(IList<Polygon> polygons)
        {
            var tolerance = 0.00001;
            var clipper = new Clipper();
            var solution = new List<List<IntPoint>>();
            foreach (Polygon polygon in polygons)
            {
                clipper.AddPath(this.ToClipperPath(), PolyType.ptSubject, true);
                clipper.AddPath(polygon.ToClipperPath(), PolyType.ptClip, true);
                clipper.Execute(ClipType.ctIntersection, solution);
                if (solution.Count == 0)
                {
                    return false;
                }
                var testPolygon = solution.First().ToPolygon();
                if (Math.Abs(polygon.Area - testPolygon.Area) > tolerance)
                {
                    return false;
                }
                solution.Clear();
                clipper.Clear();
            }
            return true;
        }

        /// <summary>
        /// Tests if the supplied 2D Vector3 point is outside the perimeter of this Polygon.
        /// </summary>
        /// <param name="point">The 2D Vector3 point to compare.</param>
        /// <returns>
        /// Returns true if the supplied 2D Vector3 point falls outside the perimeter of this Polygon.
        /// </returns>
        public bool Disjoint(Vector3 point)
        {
            var scale = 1024.0;
            var thisPath = this.ToClipperPath();
            var intPoint = new IntPoint(point.X * scale, point.Y * scale);
            if (Clipper.PointInPolygon(intPoint, thisPath) != 0)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Tests if all the supplied 2D Vector3 points are outside the perimeter of this Polygon.
        /// </summary>
        /// <param name="points">The collection of 2D Vector3 points to compare.</param>
        /// <returns>
        /// Returns true if any of the supplied 2D Vector3 points fall outside the perimeter of this Polygon.
        /// </returns>
        public bool Disjoint(IList<Vector3> points)
        {
            var scale = 1024.0;
            var thisPath = this.ToClipperPath();
            foreach (Vector3 point in points)
            {
                var intPoint = new IntPoint(point.X * scale, point.Y * scale);
                if (Clipper.PointInPolygon(intPoint, thisPath) != 0)
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
        /// Tests if any of the supplied Polygons are coincident with this Polygon.
        /// </summary>
        /// <param name="polygons">The collection of Polygons to compare.</param>
        /// <returns>
        /// Returns true if any of the supplied Polygons do not intersect or touch.
        /// </returns>
        public bool Disjoint(IList<Polygon> polygons)
        {
            var thisPath = this.ToClipperPath();
            foreach (Polygon polygon in polygons)
            {
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
            }
            return true;
        }

        /// <summary>
        /// Tests if the supplied Polygon shares areas with this Polygon.
        /// </summary>
        /// <param name="polygon">The Polygon to compare with this Polygon.</param>
        /// <returns>
        /// Returns true if the two Polygons intersect at least once.
        /// </returns>
        public bool Intersects(Polygon polygon)
        {
            if (polygon == null)
            {
                return false;
            }
            var clipper = new Clipper();
            var solution = new List<List<IntPoint>>();
            clipper.AddPath(this.ToClipperPath(), PolyType.ptSubject, true);
            clipper.AddPath(polygon.ToClipperPath(), PolyType.ptClip, true);
            clipper.Execute(ClipType.ctIntersection, solution);
            return solution.Count != 0;
        }

        /// <summary>
        /// Tests if any of the supplied Polygons share areas with this Polygon.
        /// </summary>
        /// <param name="polygons">The list of Polygons to compare.</param>
        /// <returns>
        /// Returns true if any of the supplied Polygons intersects with this Polygon.
        /// </returns>
        public bool Intersects(IList<Polygon> polygons)
        {
            if (polygons == null)
            {
                return false;
            }
            var clipper = new Clipper();
            var solution = new List<List<IntPoint>>();
            clipper.AddPath(this.ToClipperPath(), PolyType.ptSubject, true);
            foreach (Polygon polygon in polygons)
            {
                clipper.AddPath(polygon.ToClipperPath(), PolyType.ptClip, true);
            }
            clipper.Execute(ClipType.ctIntersection, solution);
            if (solution.Count != 0)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Tests if the supplied 2D Vector3 point is on the perimeter of this Polygon.
        /// </summary>
        /// <param name="point">The 2D Vector3 point to compare.</param>
        /// <returns>
        /// Returns true if the supplied 2D Vector3 point coincides with the perimeter of this Polygon.
        /// </returns>
        public bool Touches(Vector3 point)
        {
            var scale = 1024.0;
            var thisPath = this.ToClipperPath();
            var intPoint = new IntPoint(point.X * scale, point.Y * scale);
            if (Clipper.PointInPolygon(intPoint, thisPath) != -1)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Tests if all the 2D Vector3 points in the supplied collection are on the perimeter of this Polygon.
        /// </summary>
        /// <param name="points">The 2D Vector3 point to compare.</param>
        /// <returns>
        /// Returns true if all the supplied 2D Vector3 points coincide with the perimeter of this Polygon.
        /// </returns>
        public bool Touches(IList<Vector3> points)
        {
            var scale = 1024.0;
            var thisPath = this.ToClipperPath();
            foreach (Vector3 point in points)
            {
                var intPoint = new IntPoint(point.X * scale, point.Y * scale);
                if (Clipper.PointInPolygon(intPoint, thisPath) != -1)
                {
                    return false;
                }
            }
            return true;
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
            if (this.Intersects(polygon))
            {
                return false;
            }
            var thisPath = this.ToClipperPath();
            var polyPath = polygon.ToClipperPath();
            foreach (IntPoint vertex in thisPath)
            {
                if (Clipper.PointInPolygon(vertex, polyPath) == -1)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Tests if the all the Polygons in the supplied collection share at least one perimeter point without interesecting.
        /// </summary>
        /// <param name="polygons">The Polygons to compare.</param>
        /// <returns>
        /// Returns true if this Polygon and any of the supplied Polygons share at least one perimeter point and do not intersect.
        /// </returns>
        public bool Touches(IList<Polygon> polygons)
        {
            if (polygons == null)
            {
                return false;
            }
            var thisPath = this.ToClipperPath();
            foreach (Polygon polygon in polygons)
            {
                if (this.Intersects(polygon))
                {
                    return false;
                }
                var polyPath = polygon.ToClipperPath();
                foreach (IntPoint vertex in thisPath)
                {
                    if (Clipper.PointInPolygon(vertex, polyPath) == -1)
                    {
                        return true;
                    }
                }
            }
            return false;
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
        public IList<Polygon> Difference(Polygon polygon)
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
        /// Constructs the geometric difference between this Polygon and the supplied Polygons.
        /// </summary>
        /// <param name="difPolys">The list of intersecting Polygons.</param>
        /// <returns>
        /// Returns a list of Polygons representing the subtraction of the supplied Polygons from this Polygon.
        /// Returns null if the area of this Polygon is entirely subtracted.
        /// Returns a list containing a representation of the perimeter of this Polygon if the two Polygons do not intersect.
        /// </returns>
        public IList<Polygon> Difference(IList<Polygon> difPolys)
        {
            var thisPath = this.ToClipperPath();
            var polyPaths = new List<List<IntPoint>>();
            foreach (Polygon polygon in difPolys)
            {
                polyPaths.Add(polygon.ToClipperPath());
            }
            Clipper clipper = new Clipper();
            clipper.AddPath(thisPath, PolyType.ptSubject, true);
            clipper.AddPaths(polyPaths, PolyType.ptClip, true);
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
        /// Returns a list of Polygons representing the intersection of this Polygon with the supplied Polygon.
        /// Returns null if the two Polygons do not intersect.
        /// </returns>
        public IList<Polygon> Intersection(Polygon polygon)
        {
            var thisPath = this.ToClipperPath();
            var polyPath = polygon.ToClipperPath();
            Clipper clipper = new Clipper();
            clipper.AddPath(thisPath, PolyType.ptSubject, true);
            clipper.AddPath(polyPath, PolyType.ptClip, true);
            var solution = new List<List<IntPoint>>();
            clipper.Execute(ClipType.ctIntersection, solution);
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
            return solution.First().ToPolygon();
        }

        /// <summary>
        /// Constructs the geometric union between this Polygon and the supplied Polygon.
        /// </summary>
        /// <param name="polygons">The Polygons to be combined with this Polygon.</param>
        /// <returns>
        /// Returns a single Polygon from a successful union.
        /// Returns null if a union cannot be performed on the complete list of Polygons.
        /// </returns>
        public Polygon Union(IList<Polygon> polygons)
        {
            var thisPath = this.ToClipperPath();
            var polyPaths = new List<List<IntPoint>>();
            foreach (Polygon polygon in polygons)
            {
                polyPaths.Add(polygon.ToClipperPath());
            }
            Clipper clipper = new Clipper();
            clipper.AddPath(thisPath, PolyType.ptSubject, true);
            clipper.AddPaths(polyPaths, PolyType.ptClip, true);
            var solution = new List<List<IntPoint>>();
            clipper.Execute(ClipType.ctUnion, solution);
            if (solution.Count > 1)
            {
                return null;
            }
            return solution.First().Distinct().ToList().ToPolygon();
        }

        /// <summary>
        /// Returns Polygons representing the symmetric difference between this Polygon and the supplied Polygon.
        /// </summary>
        /// <param name="polygon">The intersecting polygon.</param>
        /// <returns>
        /// Returns a list of Polygons representing the symmetric difference of this Polygon and the supplied Polygon.
        /// Returns a representation of this Polygon and the supplied Polygon if the Polygons do not intersect.
        /// </returns>
        public IList<Polygon> XOR(Polygon polygon)
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
        public override Line[] Segments()
        {
            var lines = new Line[_vertices.Count];
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
                lines[i] = new Line(a, b);
            }
            return lines;
        }

        /// <summary>
        /// Reverse the direction of a polygon.
        /// </summary>
        /// <returns>Returns a new polgon with opposite winding.</returns>
        public new Polygon Reversed()
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
        /// Project this Polygon onto a Plane along a vector.
        /// </summary>
        /// <param name="direction">The projection vector.</param>
        /// <param name="p">The Plane onto which to project the Polygon.</param>
        /// <returns>A Polygon projected onto the Plane.</returns>
        public Polygon ProjectAlong(Vector3 direction, Plane p)
        {
            return new Polygon(this.Vertices.Select(v=>v.ProjectAlong(direction, p)).ToList());
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

        /// <summary>
        /// Create a Profile from this Polygon.
        /// </summary>
        /// <returns>A new Profile.</returns>
        public static implicit operator Profile(Polygon p)
        {
            return new Profile(p);
        }

        /// <summary>
        /// Get the transforms used to transform a Profile extruded along this Polyline.
        /// </summary>
        /// <param name="startSetback"></param>
        /// <param name="endSetback"></param>
        /// <returns></returns>
        public override Transform[] Frames(double startSetback, double endSetback)
        {
            return FramesInternal(startSetback, endSetback, true);
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