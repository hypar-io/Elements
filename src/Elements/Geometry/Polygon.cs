using ClipperLib;
using Elements.Geometry.Interfaces;
using Elements.Serialization;
using LibTessDotNet.Double;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Elements.Geometry
{
    /// <summary>
    /// A closed planar polygon.
    /// </summary>
    public partial class Polygon : Polyline
    {
        private const double scale = 1024.0;

        /// <summary>
        /// The area enclosed by the polygon.
        /// </summary>
        [JsonIgnore]
        public double Area
        {
            get {
                var area = 0.0;
                for(var i = 0; i<= _vertices.Length-1; i++)
                {
                    var j = (i+1) % _vertices.Length;
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
                for (var i = 0; i < this._vertices.Length; i++)
                {
                    factor =
                        (_vertices[i].X * _vertices[(i + 1) % _vertices.Length].Y) -
                        (_vertices[(i + 1) % _vertices.Length].X * _vertices[i].Y);
                    x += (_vertices[i].X + _vertices[(i + 1) % _vertices.Length].X) * factor;
                    y += (_vertices[i].Y + _vertices[(i + 1) % _vertices.Length].Y) * factor;
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
        public Polygon(Vector3[] vertices) : base(vertices){}

        /// <summary>
        /// Tests if the supplied Vector3 is within this Polygon without coincidence with an edge when compared on a shared plane.
        /// </summary>
        /// <param name="vector">The Vector3 to compare to this Polygon.</param>
        /// <returns>
        /// Returns true if the supplied Vector3 is within this Polygon when compared on a shared plane. Returns false if the Vector3 is outside this Polygon or if the supplied Vector3 is null.
        /// </returns>
        public bool Contains(Vector3 vector)
        {
            if (vector == null)
            {
                return false;
            }
            var thisPath = this.ToClipperPath();
            var intPoint = new IntPoint(vector.X * scale, vector.Y * scale);
            if (Clipper.PointInPolygon(intPoint, thisPath) != 1)
            {
                return false;
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
        /// Tests if the supplied Vector3 is within this Polygon or coincident with an edge when compared on a shared plane.
        /// </summary>
        /// <param name="vector">The Vector3 to compare to this Polygon.</param>
        /// <returns>
        /// Returns true if the supplied Vector3 is within this Polygon or coincident with an edge when compared on a shared plane. Returns false if the supplied Vector3 is outside this Polygon, or if the supplied Vector3 is null.
        /// </returns>
        public bool Covers(Vector3 vector)
        {
            if (vector == null)
            {
                return false;
            }
            var thisPath = this.ToClipperPath();
            var intPoint = new IntPoint(vector.X * scale, vector.Y * scale);
            if (Clipper.PointInPolygon(intPoint, thisPath) == 0)
            {
                return false;
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
            return solution.First().ToPolygon().Area == polygon.ToClipperPath().ToPolygon().Area;
        }

        /// <summary>
        /// Tests if the supplied Vector3 is outside this Polygon when compared on a shared plane.
        /// </summary>
        /// <param name="vector">The Vector3 to compare to this Polygon.</param>
        /// <returns>
        /// Returns true if the supplied Vector3 is outside this Polygon when compared on a shared plane or if the supplied Vector3 is null.
        /// </returns>
        public bool Disjoint(Vector3 vector)
        {
            if (vector == null)
            {
                return true;
            }
            var thisPath = this.ToClipperPath();
            var intPoint = new IntPoint(vector.X * scale, vector.Y * scale);
            if (Clipper.PointInPolygon(intPoint, thisPath) != 0)
            {
                return false;
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
        /// Tests if the supplied Vector3 is coincident with an edge of this Polygon when compared on a shared plane.
        /// </summary>
        /// <param name="vector">The Vector3 to compare to this Polygon.</param>
        /// <returns>
        /// Returns true if the supplied Vector3 coincides with an edge of this Polygon when compared on a shared plane. Returns false if the supplied Vector3 is not coincident with an edge of this Polygon, or if the supplied Vector3 is null.
        /// </returns>
        public bool Touches(Vector3 vector)
        {
            if (vector == null)
            {
                return false;
            }
            var thisPath = this.ToClipperPath();
            var intPoint = new IntPoint(vector.X * scale, vector.Y * scale);
            if (Clipper.PointInPolygon(intPoint, thisPath) != -1)
            {
                return false;
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
                polygons.Add(PolygonExtensions.ToPolygon(path.Distinct().ToList()));
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
        /// Constructs the geometric union between this Polygon and the supplied list of Polygons.
        /// </summary>
        /// <param name="polygons">The list of Polygons to be combined with this Polygon.</param>
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
        /// Offset this polygon by the specified amount.
        /// </summary>
        /// <param name="offset">The amount to offset.</param>
        /// <returns>A new Polygon offset by offset.</returns>
        public Polygon[] Offset(double offset)
        {
            var path = this.ToClipperPath();

            var solution = new List<List<IntPoint>>();
            var co = new ClipperOffset();
            co.AddPath(path, JoinType.jtMiter, EndType.etClosedPolygon);
            co.Execute(ref solution, offset * scale);  // important, scale also used here

            var result = new Polygon[solution.Count];
            for(var i=0; i<result.Length; i++)
            {
                result[i] = solution[i].ToPolygon();
            }
            return result;
        }

        /// <summary>
        /// Get a collection a lines representing each segment of this polyline.
        /// </summary>
        /// <returns>A collection of Lines.</returns>
        public override Line[] Segments()
        {
            var lines = new Line[_vertices.Length];
            for(var i=0; i<_vertices.Length; i++)
            {
                var a = _vertices[i];
                Vector3 b;
                if(i == _vertices.Length-1)
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
        /// <returns>Returns a new Polygon whose vertices are reversed.</returns>
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
            if(this.Vertices.Length != p.Vertices.Length)
            {
                return false;
            }

            for(var i=0; i<this.Vertices.Length; i++)
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
            var projected = new Vector3[this.Vertices.Length];
            for(var i=0; i<projected.Length; i++)
            {
                projected[i] = this.Vertices[i].Project(p);
            }
            return new Polygon(projected);
        }

        /// <summary>
        /// Project this Polygon onto a Plane along a vector.
        /// </summary>
        /// <param name="direction">The projection vector.</param>
        /// <param name="p">The Plane onto which to project the Polygon.</param>
        /// <returns>A Polygon projected onto the Plane.</returns>
        public Polygon ProjectAlong(Vector3 direction, Plane p)
        {
            var projected = new Vector3[this.Vertices.Length];
            for(var i=0; i<this.Vertices.Length; i++)
            {
                projected[i] = this.Vertices[i].ProjectAlong(direction, p);
            }
            return new Polygon(projected);
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

        /// <summary>
        /// Compute the Plane defined by the first three vertices of the Polygon.
        /// </summary>
        /// <returns>A Plane.</returns>
        public Plane Plane()
        {
            return new Plane(this._vertices[0], this._vertices[1], this._vertices[2]);
        }

        /// <summary>
        /// The string representation of the Polygon.
        /// </summary>
        /// <returns>A string containing the string representations of this Polygon's vertices.</returns>
        public override string ToString()
        {
            return string.Join(", ", this._vertices.Select(v=>v.ToString()));
        }

        /// <summary>
        /// Calculate the length of the polygon.
        /// </summary>
        public override double Length()
        {
            var length = 0.0;
            for(var i=0; i<this._vertices.Length; i++)
            {
                var next = i == this._vertices.Length - 1 ? 0 : i+1;
                length += this._vertices[i].DistanceTo(this._vertices[next]);
            }
            return length;
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
            var converted = new Vector3[p.Count];
            for(var i=0; i<converted.Length; i++)
            {
                var v = p[i];
                converted[i] = new Vector3(v.X/scale, v.Y/scale);
            }
            return new Polygon(converted);
        }

        public static Polygon[] Reversed(this Polygon[] polygons)
        {
            return polygons.Select(p=>p.Reversed()).ToArray();
        }
    }
}