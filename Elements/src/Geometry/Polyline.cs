using Elements.Geometry.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using ClipperLib;

namespace Elements.Geometry
{
    /// <summary>
    /// A coplanar continuous set of lines.
    /// </summary>
    /// <example>
    /// [!code-csharp[Main](../../Elements/test/PolylineTests.cs?name=example)]
    /// </example>
    public partial class Polyline : ICurve, IEquatable<Polyline>
    {
        /// <summary>
        /// Calculate the length of the polygon.
        /// </summary>
        public override double Length()
        {
            var length = 0.0;
            for (var i = 0; i < this.Vertices.Count - 1; i++)
            {
                length += this.Vertices[i].DistanceTo(this.Vertices[i + 1]);
            }
            return length;
        }

        /// <summary>
        /// The start of the polyline.
        /// </summary>
        [JsonIgnore]
        public Vector3 Start
        {
            get { return this.Vertices[0]; }
        }

        /// <summary>
        /// The end of the polyline.
        /// </summary>
        [JsonIgnore]
        public Vector3 End
        {
            get { return this.Vertices[this.Vertices.Count - 1]; }
        }

        /// <summary>
        /// Reverse the direction of a polyline.
        /// </summary>
        /// <returns>Returns a new polyline with opposite winding.</returns>
        public Polyline Reversed()
        {
            var revVerts = new List<Vector3>(this.Vertices);
            revVerts.Reverse();
            return new Polyline(revVerts);
        }

        /// <summary>
        /// Get a string representation of this polyline.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Join<Vector3>(",", this.Vertices);
        }

        /// <summary>
        /// Get a collection a lines representing each segment of this polyline.
        /// </summary>
        /// <returns>A collection of Lines.</returns>
        public virtual Line[] Segments()
        {
            return SegmentsInternal(this.Vertices);
        }

        /// <summary>
        /// Get a point on the polygon at parameter u.
        /// </summary>
        /// <param name="u">A value between 0.0 and 1.0.</param>
        /// <returns>Returns a Vector3 indicating a point along the Polygon length from its start vertex.</returns>
        public override Vector3 PointAt(double u)
        {
            var segmentIndex = 0;
            var p = PointAtInternal(u, out segmentIndex);
            return p;
        }

        /// <summary>
        /// Get the Transform at the specified parameter along the Polygon.
        /// </summary>
        /// <param name="u">The parameter on the Polygon between 0.0 and 1.0.</param>
        /// <returns>A Transform with its Z axis aligned trangent to the Polygon.</returns>
        public override Transform TransformAt(double u)
        {
            if (u < 0.0 || u > 1.0)
            {
                throw new ArgumentOutOfRangeException($"The provided value for u ({u}) must be between 0.0 and 1.0.");
            }

            var segmentIndex = 0;
            var o = PointAtInternal(u, out segmentIndex);
            var y = Normal();
            Vector3 x = Vector3.XAxis; // Vector3: Convert to XAxis

            // Check if the provided parameter is equal
            // to one of the vertices.
            Vector3 a = new Vector3();
            var isEqualToVertex = false;
            foreach (var v in this.Vertices)
            {
                if (v.Equals(o))
                {
                    isEqualToVertex = true;
                    a = v;
                }
            }

            if (isEqualToVertex)
            {
                var idx = this.Vertices.IndexOf(a);

                if (idx == 0 || idx == this.Vertices.Count - 1)
                {
                    return CreateOthogonalTransform(idx, a, y);
                }
                else
                {
                    return CreateMiterTransform(idx, a, y);
                }
            }
            else
            {
                var d = this.Length() * u;
                var totalLength = 0.0;
                var segments = Segments();
                for (var i = 0; i < segments.Length; i++)
                {
                    var s = segments[i];
                    var currLength = s.Length();
                    if (totalLength <= d && totalLength + currLength >= d)
                    {
                        o = s.PointAt((d - totalLength) / currLength);
                        x = s.Direction().Cross(y);
                        break;
                    }
                    totalLength += currLength;
                }
            }
            return new Transform(o, x, y, x.Cross(y));
        }

        /// <summary>
        /// Construct a transformed copy of this Polyline.
        /// </summary>
        /// <param name="transform">The transform to apply.</param>
        public Polyline TransformedPolyline(Transform transform)
        {
            var transformed = new Vector3[this.Vertices.Count];
            for (var i = 0; i < transformed.Length; i++)
            {
                transformed[i] = transform.OfPoint(this.Vertices[i]);
            }
            var p = new Polyline(transformed);
            return p;
        }

        /// <summary>
        /// Construct a transformed copy of this Curve.
        /// </summary>
        /// <param name="transform">The transform to apply.</param>
        public override Curve Transformed(Transform transform)
        {
            return TransformedPolyline(transform);
        }

        /// <summary>
        /// Get the transforms used to transform a Profile extruded along this Polyline.
        /// </summary>
        /// <param name="startSetback"></param>
        /// <param name="endSetback"></param>
        public override Transform[] Frames(double startSetback, double endSetback)
        {
            return FramesInternal(startSetback, endSetback, false);
        }

        /// <summary>
        /// Get the bounding box for this curve.
        /// </summary>
        public override BBox3 Bounds()
        {
            return new BBox3(this.Vertices);
        }

        /// <summary>
        /// Compute the Plane defined by the first three non-collinear vertices of the Polygon.
        /// </summary>
        /// <returns>A Plane.</returns>
        public Plane Plane()
        {
            var xform = Vertices.ToTransform();
            return xform.OfPlane(new Plane(Vector3.Origin, Vector3.ZAxis));
        }

        /// <summary>
        /// The normal of this polyline, according to Newell's Method.
        /// </summary>
        /// <returns>The unitized sum of the cross products of each pair of edges.</returns>
        public virtual Vector3 Normal()
        {
            // This is a slight variation on Newell that does not
            // close the loop, as we do for polygons. For polylines, 
            // closing the loop often results in self-intersection
            // which returns a zero length vector.
            var normal = new Vector3();
            for (int i = 0; i < Vertices.Count - 1; i++)
            {
                var p0 = Vertices[i];
                var p1 = Vertices[i + 1];
                normal.X += (p0.Y - p1.Y) * (p0.Z + p1.Z);
                normal.Y += (p0.Z - p1.Z) * (p0.X + p1.X);
                normal.Z += (p0.X - p1.X) * (p0.Y + p1.Y);
            }
            return normal.Unitized();
        }

        internal Transform[] FramesInternal(double startSetback, double endSetback, bool closed = false)
        {
            // Create an array of transforms with the same
            // number of items as the vertices.
            var result = new Transform[this.Vertices.Count];

            // Cache the normal so we don't have to recalculate
            // using Newell for every frame.
            var up = Normal();
            for (var i = 0; i < result.Length; i++)
            {
                var a = this.Vertices[i];
                if (closed)
                {
                    result[i] = CreateMiterTransform(i, a, up);
                }
                else
                {
                    result[i] = CreateOthogonalTransform(i, a, up);
                }
            }
            return result;
        }

        /// <summary>
        /// A list of vertices describing the arc for rendering.
        /// </summary>
        internal override IList<Vector3> RenderVertices()
        {
            return this.Vertices;
        }

        /// <summary>
        /// Check for coincident vertices in the supplied vertex collection.
        /// </summary>
        /// <param name="vertices"></param>
        protected void CheckCoincidenceAndThrow(IList<Vector3> vertices)
        {
            for (var i = 0; i < vertices.Count; i++)
            {
                for (var j = 0; j < vertices.Count; j++)
                {
                    if (i == j)
                    {
                        continue;
                    }
                    if (vertices[i].IsAlmostEqualTo(vertices[j]))
                    {
                        throw new ArgumentException($"The polyline could not be created. Two vertices were almost equal: {i} {vertices[i]} {j} {vertices[j]}.");
                    }
                }
            }
        }

        /// <summary>
        /// Check if any of the polygon segments have zero length.
        /// </summary>
        internal static void CheckSegmentLengthAndThrow(IList<Line> segments)
        {
            foreach (var s in segments)
            {
                if (s.Length() == 0)
                {
                    throw new ArgumentException("A segment fo the polyline has zero length.");
                }
            }
        }

        /// <summary>
        /// Check for self-intersection in the supplied line segment collection.
        /// </summary>
        /// <param name="t">The transform representing the plane of the polygon.</param>
        /// <param name="segments"></param>
        internal static void CheckSelfIntersectionAndThrow(Transform t, IList<Line> segments)
        {
            var segmentsTrans = new List<Line>();

            foreach (var l in segments)
            {
                segmentsTrans.Add(l.TransformedLine(t));
            };

            for (var i = 0; i < segmentsTrans.Count; i++)
            {
                for (var j = 0; j < segmentsTrans.Count; j++)
                {
                    if (i == j)
                    {
                        // Don't check against itself.
                        continue;
                    }

                    if (segmentsTrans[i].Intersects2D(segmentsTrans[j]))
                    {
                        throw new ArgumentException($"The polyline could not be created. Segments {i} and {j} intersect.");
                    }
                }
            }
        }

        internal static Line[] SegmentsInternal(IList<Vector3> vertices)
        {
            var result = new Line[vertices.Count - 1];
            for (var i = 0; i < vertices.Count - 1; i++)
            {
                var a = vertices[i];
                var b = vertices[i + 1];
                result[i] = new Line(a, b);
            }
            return result;
        }

        private Transform CreateMiterTransform(int i, Vector3 a, Vector3 up)
        {
            var b = i == 0 ? this.Vertices[this.Vertices.Count - 1] : this.Vertices[i - 1];
            var c = i == this.Vertices.Count - 1 ? this.Vertices[0] : this.Vertices[i + 1];
            var l1 = (a - b).Unitized();
            var l2 = (c - a).Unitized();
            var x1 = l1.Cross(up);
            var x2 = l2.Cross(up);
            var x = x1.Average(x2);
            return new Transform(this.Vertices[i], x, x.Cross(up));
        }

        private Transform CreateOthogonalTransform(int i, Vector3 a, Vector3 up)
        {
            Vector3 b, x, c;

            if (i == 0)
            {
                b = this.Vertices[i + 1];
                var z = (a - b).Unitized();
                return new Transform(a, up.Cross(z), z);
            }
            else if (i == this.Vertices.Count - 1)
            {
                b = this.Vertices[i - 1];
                var z = (b - a).Unitized();
                return new Transform(a, up.Cross(z), z);
            }
            else
            {
                b = this.Vertices[i - 1];
                c = this.Vertices[i + 1];
                var v1 = (b - a).Unitized();
                var v2 = (c - a).Unitized();
                x = v1.Average(v2).Negate();
                return new Transform(this.Vertices[i], x, x.Cross(up));
            }
        }

        /// <summary>
        /// Get a point on the polygon at parameter u.
        /// </summary>
        /// <param name="u">A value between 0.0 and 1.0.</param>
        /// <param name="segmentIndex">The index of the segment containing parameter u.</param>
        /// <returns>Returns a Vector3 indicating a point along the Polygon length from its start vertex.</returns>
        protected virtual Vector3 PointAtInternal(double u, out int segmentIndex)
        {
            if (u < 0.0 || u > 1.0)
            {
                throw new Exception($"The value of u ({u}) must be between 0.0 and 1.0.");
            }

            var d = this.Length() * u;
            var totalLength = 0.0;
            for (var i = 0; i < this.Vertices.Count - 1; i++)
            {
                var a = this.Vertices[i];
                var b = this.Vertices[i + 1];
                var currLength = a.DistanceTo(b);
                var currVec = (b - a);
                if (totalLength <= d && totalLength + currLength >= d)
                {
                    segmentIndex = i;
                    return a + currVec * ((d - totalLength) / currLength);
                }
                totalLength += currLength;
            }
            segmentIndex = this.Vertices.Count - 1;
            return this.End;
        }

        /// <summary>
        /// Offset this polyline by the specified amount.
        /// </summary>
        /// <param name="offset">The amount to offset.</param>
        /// <param name="endType">The closure type to use on the offset polygon.</param>
        /// <param name="tolerance">An optional tolerance.</param>
        /// <returns>A new closed Polygon offset in all directions by offset from the polyline.</returns>
        public virtual Polygon[] Offset(double offset, EndType endType, double tolerance = Vector3.EPSILON)
        {
            var clipperScale = 1.0 / tolerance;
            var path = this.ToClipperPath(tolerance);

            var solution = new List<List<IntPoint>>();
            var co = new ClipperOffset();
            ClipperLib.EndType clEndType;
            switch (endType)
            {
                case EndType.Butt:
                    clEndType = ClipperLib.EndType.etOpenButt;
                    break;
                case EndType.ClosedPolygon:
                    clEndType = ClipperLib.EndType.etClosedPolygon;
                    break;
                case EndType.Square:
                default:
                    clEndType = ClipperLib.EndType.etOpenSquare;
                    break;
            }
            co.AddPath(path, JoinType.jtMiter, clEndType);
            co.Execute(ref solution, offset * clipperScale);  // important, scale also used here

            var result = new Polygon[solution.Count];
            for (var i = 0; i < result.Length; i++)
            {
                result[i] = solution[i].ToPolygon(tolerance);
            }
            return result;
        }

        /// <summary>
        /// Does this polyline equal the provided polyline?
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(Polyline other)
        {
            if (this.Vertices.Count != other.Vertices.Count)
            {
                return false;
            }
            for (var i = 0; i < Vertices.Count; i++)
            {
                if (!this.Vertices[i].Equals(other.Vertices[i]))
                {
                    return false;
                }
            }
            return true;
        }
    }

    /// <summary>
    /// Polyline extension methods.
    /// </summary>
    internal static class PolylineExtensions
    {
        /// <summary>
        /// Construct a clipper path from a Polygon.
        /// </summary>
        /// <param name="p"></param>
        /// <param name="tolerance">An optional tolerance. If converting back to a Polyline, be sure to use the same tolerance.</param>
        /// <returns></returns>
        internal static List<IntPoint> ToClipperPath(this Polyline p, double tolerance = Vector3.EPSILON)
        {
            var clipperScale = Math.Round(1.0 / tolerance);
            var path = new List<IntPoint>();
            foreach (var v in p.Vertices)
            {
                path.Add(new IntPoint(Math.Round(v.X * clipperScale), Math.Round(v.Y * clipperScale)));
            }
            return path;
        }
    }

    /// <summary>
    /// Offset end types
    /// </summary>
    public enum EndType
    {
        /// <summary>
        /// Open ends are extended by the offset distance and squared off 
        /// </summary>
        Square,
        /// <summary>
        /// Ends are squared off with no extension
        /// </summary>
        Butt,
        /// <summary>
        /// If open, ends are joined and treated as a closed polygon
        /// </summary>
        ClosedPolygon,
    }
}
