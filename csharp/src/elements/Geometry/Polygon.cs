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
        private const double scale = 1024.0;
        private const double areaTolerance = 0.00001;

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
        /// Tests if the supplied Vector3 point is within this Polygon without coincidence with an edge when compared on a shared plane.
        /// </summary>
        /// <param name="point">The Vector3 point to compare to this Polygon.</param>
        /// <returns>
        /// Returns true if the supplied Vector3 point is within this Polygon when compared on a shared plane. Returns false if the Vector3 point is outside this Polygon or if the supplied Vector3 point is null.
        /// </returns>
        public bool Contains(Vector3 point)
        {
            if (point == null)
            {
                return false;
            }
            var thisPath = this.ToClipperPath();
            var intPoint = new IntPoint(point.X * scale, point.Y * scale);
            if (Clipper.PointInPolygon(intPoint, thisPath) != 1)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Tests if all the supplied Vector3 points are within this Polygon without coincidence with an edge when compared on a shared plane.
        /// </summary>
        /// <param name="points">The list of Vector3 points to compare to this Polygon.</param>
        /// <returns>
        /// Returns true if all the supplied Vector3 points are within this Polygon when compared on a shared plane. Returns false if any of the Vector3 points are outside this Polygon or if the supplied Vector3 point list is null.
        /// </returns>
        public bool Contains(IList<Vector3> points)
        {
            if (points == null)
            {
                return false;
            }
            foreach (Vector3 point in points)
            {
                if (!this.Contains(point))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Tests if the supplied Polygon is within this Polygon without coincident edges when compared on a shared plane.
        /// </summary>
        /// <param name="polygon">The Polygon to compare to this Polygon.</param>
        /// <returns>
        /// Returns true if every vertex of the supplied Polygon is within this Polygon when compared on a shared plane. Returns false if the supplied Polygon is not entirely within this Polygon, or if the supplied Polygon is null.
        /// </returns>
        public bool Contains(Polygon polygon)
        {
            if (polygon == null)
            {
                return false;
            }
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
        /// Tests if all the supplied Polygons are within this Polygon without coincident edges when compared on a shared plane.
        /// </summary>
        /// <param name="polygons">The list of Polygons to compare to this Polygon.</param>
        /// <returns>
        /// Returns true if every vertex of the supplied Polygons are within this Polygon when compared on a shared plane. Returns false if any of the supplied Polygons is not entirely within this Polygon, or if the supplied list of Polygons is null.
        /// </returns>
        public bool Contains(IList<Polygon> polygons)
        {
            if (polygons == null)
            {
                return false;
            }
            foreach (Polygon polygon in polygons)
            {
                if (!this.Contains(polygon))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Tests if the supplied Vector3 point is within this Polygon or coincident with an edge when compared on a shared plane.
        /// </summary>
        /// <param name="point">The Vector3 point to compare to this Polygon.</param>
        /// <returns>
        /// Returns true if the supplied Vector3 point is within this Polygon or coincident with an edge when compared on a shared plane. Returns false if the supplied point is outside this Polygon, or if the supplied Vector3 point is null.
        /// </returns>
        public bool Covers(Vector3 point)
        {
            if (point == null)
            {
                return false;
            }
            var thisPath = this.ToClipperPath();
            var intPoint = new IntPoint(point.X * scale, point.Y * scale);
            if (Clipper.PointInPolygon(intPoint, thisPath) == 0)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Tests if all the supplied Vector3 points are within this Polygon or coincident with an edge when compared on a shared plane.
        /// </summary>
        /// <param name="points">The list of Vector3 points to compare to this Polygon.</param>
        /// <returns>
        /// Returns true if all the supplied Vector3 points are within this Polygon or coincident with an edge when compared on a shared plane. Returns false if all the supplied point are outside this Polygon, or if the supplied list Vector3 points is null.
        /// </returns>
        public bool Covers(IList<Vector3> points)
        {
            if (points == null)
            {
                return false;
            }
            foreach (Vector3 point in points)
            {
                if (!this.Covers(point))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Tests if the supplied Polygon is within this Polygon with or without edge coincident vertices when compared on a shared plane.
        /// </summary>
        /// <param name="polygon">The Polygon to compare to this Polygon.</param>
        /// <returns>
        /// Returns true if every vertex of the supplied Polygon is within this Polygon or coincident with an edge when compared on a shared plane. Returns false if any vertex of the supplied Polygon is outside this Polygon, or if the supplied Polygon is null.
        /// </returns>
        public bool Covers(Polygon polygon)
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
            if (solution.Count != 1)
            {
                return false;
            }
            var testPolygon = solution.First().ToPolygon();
            if (Math.Abs(polygon.Area - testPolygon.Area) > areaTolerance)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Tests if all the supplied Polygons are within this Polygon with or without edge coincident vertices when compared on a shared plane.
        /// </summary>
        /// <param name="polygons">The Polygon to compare to this Polygon.</param>
        /// <returns>
        /// Returns true if every vertex of the supplied Polygons is within this Polygon or coincident with an edge when compared on a shared plane. Returns false if any vertex of the supplied Polygons is outside this Polygon, or if the supplied list of Polygons is null.
        /// </returns>
        public bool Covers(IList<Polygon> polygons)
        {
            if (polygons == null)
            {
                return false;
            }
            foreach (Polygon polygon in polygons)
            {
                if(!this.Covers(polygon))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Tests if the supplied Vector3 point is outside this Polygon when compared on a shared plane.
        /// </summary>
        /// <param name="point">The Vector3 point to compare to this Polygon.</param>
        /// <returns>
        /// Returns true if the supplied Vector3 point is outside this Polygon when compared on a shared plane or if the supplied Vector3 point is null.
        /// </returns>
        public bool Disjoint(Vector3 point)
        {
            if (point == null)
            {
                return true;
            }
            var thisPath = this.ToClipperPath();
            var intPoint = new IntPoint(point.X * scale, point.Y * scale);
            if (Clipper.PointInPolygon(intPoint, thisPath) != 0)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Tests if all the supplied Vector3 points are outside this Polygon when compared on a shared plane.
        /// </summary>
        /// <param name="points">The list of Vector3 points to compare to this Polygon.</param>
        /// <returns>
        /// Returns true if all the supplied Vector3 points are outside this Polygon when compared on a shared plane or if the supplied Vector3 point list is null.
        /// </returns>
        public bool Disjoint(IList<Vector3> points)
        {
            if (points == null)
            {
                return true;
            }
            foreach (Vector3 point in points)
            {
                if (!this.Disjoint(point))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Tests if the supplied Polygon and this Polygon are coincident in any way when compared on a shared plane.
        /// </summary>
        /// <param name="polygon">The Polygon to compare to this Polygon.</param>
        /// <returns>
        /// Returns true if the supplied Polygon do not intersect or touch this Polygon when compared on a shared plane or if the supplied Polygon is null.
        /// </returns>
        public bool Disjoint(Polygon polygon)
        {
            if (polygon == null)
            {
                return true;
            }
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
        /// Tests if all the supplied Polygons are coincident in any way when compared on a shared plane.
        /// </summary>
        /// <param name="polygons">The list of Polygons to compare to this Polygon.</param>
        /// <returns>
        /// Returns true if none of the supplied Polygons do not intersect or touch this Polygon when compared on a shared plane or if the supplied list of Polygons is null.
        /// </returns>
        public bool Disjoint(IList<Polygon> polygons)
        {
            if (polygons == null)
            {
                return true;
            }
            foreach (Polygon polygon in polygons)
            {
                if (!this.Disjoint(polygon))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Tests if the supplied Polygon shares one or more areas with this Polygon when compared on a shared plane.
        /// </summary>
        /// <param name="polygon">The Polygon to compare with this Polygon.</param>
        /// <returns>
        /// Returns true if the supplied Polygon shares one or more areas with this Polygon when compared on a shared plane. Returns false if the supplied Polygon does not share an area with this Polygon or if the supplied Polygon is null.
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
        /// Tests if any of the supplied Polygons share one or more areas with this Polygon when compared on a shared plane.
        /// </summary>
        /// <param name="polygons">The Polygon to compare with this Polygon.</param>
        /// <returns>
        /// Returns true if any of the supplied Polygons share one or more areas with this Polygon when compared on a shared plane or if the list of supplied Polygons is null. Returns false if the none of the supplied Polygons share an area with this Polygon or if the supplied list of Polygons is null.
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
        /// Tests if the supplied Vector3 point is coincident with an edge of this Polygon when compared on a shared plane.
        /// </summary>
        /// <param name="point">The Vector3 point to compare to this Polygon.</param>
        /// <returns>
        /// Returns true if the supplied Vector3 point coincides with an edge of this Polygon when compared on a shared plane. Returns false if the supplied Vector3 point is not coincident with an edge of this Polygon, or if the supplied Vector3 point is null.
        /// </returns>
        public bool Touches(Vector3 point)
        {
            if (point == null)
            {
                return false;
            }
            var thisPath = this.ToClipperPath();
            var intPoint = new IntPoint(point.X * scale, point.Y * scale);
            if (Clipper.PointInPolygon(intPoint, thisPath) != -1)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Tests if the all supplied Vector3 point are coincident with an edge of this Polygon when compared on a shared plane.
        /// </summary>
        /// <param name="points">The list of Vector3 points to compare to this Polygon.</param>
        /// <returns>
        /// Returns true if all the supplied Vector3 points coincide with an edge of this Polygon when compared on a shared plane. Returns false if at least one of the supplied Vector3 points are not coincident with an edge of this Polygon, or if the supplied list of Vector3 points is null.
        /// </returns>
        public bool Touches(IList<Vector3> points)
        {
            if (points == null)
            {
                return false;
            }
            foreach (Vector3 point in points)
            {
                if (!this.Touches(point))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Tests if at least one point of an edge of the supplied Polygon is shared with an edge of this Polygon without the Polygons interesecting when compared on a shared plane.
        /// </summary>
        /// <param name="polygon">The Polygon to compare to this Polygon.</param>
        /// <returns>
        /// Returns true if the supplied Polygon shares at least one edge point with this Polygon without the Polygons intersecting when compared on a shared plane. Returns false if the Polygons intersect, are disjoint, or if the supplied Polygon is null.
        /// </returns>
        public bool Touches(Polygon polygon)
        {
            if (polygon == null || this.Intersects(polygon))
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
        /// Tests if all the supplied Polygons share at least one point of an edge with an edge of this Polygon without the Polygons interesecting when compared on a shared plane.
        /// </summary>
        /// <param name="polygons">The list of Polygons to compare to this Polygon.</param>
        /// <returns>
        /// Returns true if all the supplied Polygon share at least one edge point with this Polygon without the Polygons intersecting when compared on a shared plane. Returns false if any of the Polygons intersect, is disjoint, or if the supplied list of Polygons is null.
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
                if (!this.Touches(polygon))
                {
                    return false;
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
        private const double scale = 1024.0;

        /// <summary>
        /// Construct a clipper path from a Polygon.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        internal static List<IntPoint> ToClipperPath(this Polygon p)
        {
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
            return new Polygon(p.Select(v=>new Vector3(v.X/scale, v.Y/scale)).ToArray());
        }
    }
}