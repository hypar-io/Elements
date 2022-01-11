using Elements.Geometry.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using ClipperLib;

namespace Elements.Geometry
{
    /// <summary>
    /// A continuous set of lines.
    /// </summary>
    /// <example>
    /// [!code-csharp[Main](../../Elements/test/PolylineTests.cs?name=example)]
    /// </example>
    public partial class Polyline : ICurve, IEquatable<Polyline>
    {

        /// <summary>
        /// Construct a polyline from points. This is a convenience constructor
        /// that can be used like this: `new Polyline((0,0,0), (10,0,0), (10,10,0))`
        /// </summary>
        /// <param name="vertices">The vertices of the polyline.</param>
        public Polyline(params Vector3[] vertices) : this(new List<Vector3>(vertices))
        {

        }

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

            var normals = this.NormalsAtVertices();

            if (isEqualToVertex)
            {
                var idx = this.Vertices.IndexOf(a);

                if (idx == 0 || idx == this.Vertices.Count - 1)
                {
                    return CreateOrthogonalTransform(idx, a, normals[idx]);
                }
                else
                {
                    return CreateMiterTransform(idx, a, normals[idx]);
                }
            }

            var d = this.Length() * u;
            var totalLength = 0.0;
            var segments = Segments();
            var normal = new Vector3();
            for (var i = 0; i < segments.Length; i++)
            {
                var s = segments[i];
                var currLength = s.Length();
                if (totalLength <= d && totalLength + currLength >= d)
                {
                    var parameterOnSegment = (d - totalLength) / currLength;
                    o = s.PointAt(parameterOnSegment);
                    var previousNormal = normals[i];
                    var nextNormal = normals[(i + 1) % this.Vertices.Count];
                    normal = ((nextNormal - previousNormal) * parameterOnSegment + previousNormal).Unitized();
                    x = s.Direction().Cross(normal);
                    break;
                }
                totalLength += currLength;
            }
            return new Transform(o, x, normal, x.Cross(normal));
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
        /// Transform a specified segment of this polyline in place.
        /// </summary>
        /// <param name="t">The transform. If it is not within the polygon plane, then an exception will be thrown.</param>
        /// <param name="i">The segment to transform. If it does not exist, then no work will be done.</param>
        /// <param name="isClosed">If set to true, the segment between the start end end point will be considered a valid target.</param>
        /// <param name="isPlanar">If set to true, an exception will be thrown if the resultant shape is no longer planar.</param>
        public void TransformSegment(Transform t, int i, bool isClosed = false, bool isPlanar = false)
        {
            var v = this.Vertices;

            if (i < 0 || i > v.Count)
            {
                // Segment index is out of range, do no work.
                return;
            }

            var candidates = new List<Vector3>(this.Vertices);

            var endIndex = (i + 1) % v.Count;

            candidates[i] = t.OfPoint(v[i]);
            candidates[endIndex] = t.OfPoint(v[endIndex]);

            // All motion for a triangle results in a planar shape, skip this case.
            var enforcePlanar = v.Count != 3 && isPlanar;

            if (enforcePlanar && !candidates.AreCoplanar())
            {
                throw new Exception("Segment transformation must be within the polygon's plane.");
            }

            this.Vertices = candidates;
        }

        /// <summary>
        /// Get the normal of each vertex on the polyline.
        /// </summary>
        /// <returns>A collection of unit vectors, each corresponding to a single vertex.</returns>
        protected virtual Vector3[] NormalsAtVertices()
        {
            // Vertex normals will be the cross product of the previous edge and the next edge.
            var nextDirection = (this.Vertices[1] - this.Vertices[0]).Unitized();

            // At the first point, use either the next non-collinear edge or a cardinal direction to choose a normal.
            var previousDirection = new Vector3();
            if (Vector3Extensions.AreCollinear(this.Vertices))
            {
                // If the polyline is collinear, use whichever cardinal direction isn't collinear with it.
                if (Math.Abs(nextDirection.Dot(Vector3.YAxis)) < 1 - Vector3.EPSILON)
                {
                    previousDirection = Vector3.YAxis;
                }
                else
                {
                    previousDirection = Vector3.XAxis;
                }
            }
            else
            {
                // Find the next non-collinear edge and use that to hint the normal of the first vertex.
                for (var i = 2; i < this.Vertices.Count; i++)
                {
                    previousDirection = (this.Vertices[i] - this.Vertices[1]).Unitized();
                    if (Math.Abs(previousDirection.Dot(nextDirection)) > 1 - Vector3.EPSILON)
                    {
                        break;
                    }
                }
            }

            // Create an array of transforms with the same number of items as the vertices.
            var result = new Vector3[this.Vertices.Count];
            var previousNormal = new Vector3();
            for (var i = 0; i < result.Length; i++)
            {
                // If this vertex has a bend, use the normal computed from the previous and next edges.
                // Otherwise keep using the normal frnom the previous bend.
                if (i < result.Length - 1)
                {
                    var direction = (this.Vertices[i + 1] - this.Vertices[i]).Unitized();
                    if (Math.Abs(nextDirection.Dot(direction)) < 1 - Vector3.EPSILON)
                    {
                        previousDirection = nextDirection;
                        nextDirection = direction;
                    }
                }
                var normal = nextDirection.Cross(previousDirection);

                // Flip the normal if it's pointing away from the previous point's normal.
                if (i > 1 && previousNormal.Dot(normal) < 0)
                {
                    normal *= -1;
                }
                result[i] = normal.Unitized();
                previousNormal = normal;
            }
            return result;
        }

        /// <summary>
        /// Get the transforms used to transform a Profile extruded along this Polyline.
        /// </summary>
        /// <param name="startSetback"></param>
        /// <param name="endSetback"></param>
        public override Transform[] Frames(double startSetback, double endSetback)
        {
            var normals = this.NormalsAtVertices();

            // Create an array of transforms with the same number of items as the vertices.
            var result = new Transform[this.Vertices.Count];
            for (var i = 0; i < result.Length; i++)
            {
                var a = this.Vertices[i];
                result[i] = CreateOrthogonalTransform(i, a, normals[i]);
            }
            return result;
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
        public virtual Plane Plane()
        {
            var xform = Vertices.ToTransform();
            return xform.OfPlane(new Plane(Vector3.Origin, Vector3.ZAxis));
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

        /// <summary>
        /// Generates a transform that expresses the plane of a miter join at a point on the curve.
        /// </summary>
        protected Transform CreateMiterTransform(int i, Vector3 a, Vector3 up)
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

        private Transform CreateOrthogonalTransform(int i, Vector3 a, Vector3 up)
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
        /// Offset this polyline by the specified amount, only on one side.
        /// </summary>
        /// <remarks>This blunts sharp corners to keep widths close to the target.</remarks>
        /// <param name="offset">The amount to offset.</param>
        /// <param name="flip">Offset on the opposite of the default side. The default is to draw on the +X side of a polyline that goes up the +Y axis.</param>
        /// <returns>An array of polygons that are extruded from each segment of the polyline.</returns>
        public Polygon[] OffsetOnSide(double offset, bool flip)
        {
            var polygons = new List<Polygon>();
            if (this.Vertices.Count <= 1)
            {
                return polygons.ToArray();
            }

            var isCycle = this.Vertices.Count > 2 && this.Vertices[0].DistanceTo(this.Vertices.Last()) <= offset / 2;
            var segments = this.Segments();

            // Step through each point, collecting info on what its join will look like.
            var joinInfo = new List<Vector3[]>();
            for (var vertexIndex = 0; vertexIndex < this.Vertices.Count; vertexIndex++)
            {
                var vertex = this.Vertices[vertexIndex];

                // Don't draw both the first and last point if treating as a cycle.
                if (isCycle && vertexIndex == this.Vertices.Count - 1)
                {
                    continue;
                }

                Line previousSegment = null;
                Line previousOffsetSegment = null;
                if (vertexIndex - 1 >= 0 || isCycle)
                {
                    if (vertexIndex == 0)
                    {
                        previousSegment = new Line(this.Vertices[this.Vertices.Count - 2], vertex);
                    }
                    else
                    {
                        previousSegment = segments[vertexIndex - 1];
                    }
                    previousOffsetSegment = previousSegment.Offset(offset, flip);
                }

                Line nextSegment = null;
                Line nextOffsetSegment = null;
                if (vertexIndex + 1 < this.Vertices.Count || isCycle)
                {
                    if (vertexIndex + 1 == this.Vertices.Count)
                    {
                        nextSegment = new Line(vertex, this.Vertices[0]);
                    }
                    else
                    {
                        nextSegment = segments[vertexIndex];
                    }
                    nextOffsetSegment = nextSegment.Offset(offset, flip);
                }

                var joinPoints = new List<Vector3>();
                if (previousOffsetSegment != null && nextOffsetSegment != null)
                {
                    // Find where the virtual edges would naturally intersect.
                    var intersects = previousOffsetSegment.Intersects(nextOffsetSegment, out Vector3 offsetIntersection, true);

                    // When the end of one of the thickened segments overlaps with the other one, the intersection point may be very far away.
                    // Address this by either picking an intersection point on one of the thickened segments or adding a cap (which happens when offsetIntersection is null)
                    if (intersects)
                    {
                        // Identify if the offset interection lands beyond the previous point or beyond the next point in the polyline.
                        if (previousOffsetSegment.Start.DistanceTo(previousOffsetSegment.End) < previousOffsetSegment.End.DistanceTo(offsetIntersection) &&
                            previousOffsetSegment.Start.DistanceTo(offsetIntersection) < previousOffsetSegment.End.DistanceTo(offsetIntersection))
                        {
                            // The edge at the end of the outgoing segment.
                            var endOfNextSegment = new Line(nextOffsetSegment.End, nextSegment.End);
                            if (previousOffsetSegment.Intersects(endOfNextSegment, out Vector3 endOfNextSegmentIntersection))
                            {
                                // If the virtual offset point of the next line segment is inside the previous segment, the incoming virtual edge should be inside the outgoing thickened segment.
                                offsetIntersection = endOfNextSegmentIntersection;
                            }
                            else if (previousSegment.Intersects(endOfNextSegment, out _))
                            {
                                // If the next point is entirely inside the previous segment, add a cap since any accute angle would be too narrow.
                                intersects = false;
                            }
                            else
                            {
                                previousOffsetSegment.Start.DistanceTo(nextOffsetSegment, out offsetIntersection);
                            }
                        }
                        else if (nextOffsetSegment.Start.DistanceTo(nextOffsetSegment.End) < nextOffsetSegment.Start.DistanceTo(offsetIntersection) &&
                            nextOffsetSegment.Start.DistanceTo(offsetIntersection) > nextOffsetSegment.End.DistanceTo(offsetIntersection))
                        {
                            // The edge at the end of the incoming segment.
                            var endOfPreviousSegment = new Line(previousOffsetSegment.Start, previousSegment.Start);
                            if (nextOffsetSegment.Intersects(endOfPreviousSegment, out Vector3 endOfPreviousSegmentIntersection))
                            {
                                // If the virtual offset point of the previous line segment is inside the next segment, the outgoing virtual edge should be inside the incoming thickened segment.
                                offsetIntersection = endOfPreviousSegmentIntersection;
                            }
                            else if (nextSegment.Intersects(endOfPreviousSegment, out _))
                            {
                                // If the previous point is entirely inside the next segment, add a cap since any accute angle would be too narrow.
                                intersects = false;
                            }
                            else
                            {
                                nextOffsetSegment.End.DistanceTo(previousOffsetSegment, out offsetIntersection);
                            }
                        }
                    }

                    var isAcuteExteriorAngle = false;
                    if (intersects)
                    {
                        isAcuteExteriorAngle = nextOffsetSegment.Direction().Dot((offsetIntersection - vertex).Unitized()) < Math.Cos(Math.PI * -3 / 4);
                    }

                    // Tight joins should get a cap, to maintain minimum width.
                    if (!intersects ||
                        isAcuteExteriorAngle &&
                        (previousOffsetSegment.End.DistanceTo(offsetIntersection) > offset ||
                        nextOffsetSegment.Start.DistanceTo(offsetIntersection) > offset))
                    {
                        var offsetPoint1 = previousOffsetSegment.Direction() * offset + previousOffsetSegment.End;
                        var offsetPoint2 = nextOffsetSegment.Direction() * (offset * -1) + nextOffsetSegment.Start;
                        joinPoints.Add(offsetPoint1);
                        if (offsetPoint1.DistanceTo(offsetPoint2) != 0)
                        {
                            joinPoints.Add(offsetPoint2);
                        }
                    }
                    else
                    {
                        joinPoints.Add(offsetIntersection);
                    }
                }
                else if (previousOffsetSegment != null)
                {
                    joinPoints.Add(previousOffsetSegment.End);
                }
                else if (nextOffsetSegment != null)
                {
                    joinPoints.Add(nextOffsetSegment.Start);
                }
                else
                {
                    continue;
                }

                joinPoints.Add(vertex);
                joinInfo.Add(joinPoints.ToArray());
            }

            // Create a polygon for each point's join, connecting back to the end of the previous point's join.
            for (var joinIndex = 0; joinIndex < joinInfo.Count; joinIndex++)
            {
                if (joinIndex == 0 && !isCycle)
                {
                    continue;
                }

                var joinPoints = joinInfo[joinIndex];
                var previousJoinPoints = joinIndex > 0 ? joinInfo[joinIndex - 1] : joinInfo.Last();
                var vertices = new List<Vector3>();
                vertices.Add(previousJoinPoints.Last());
                vertices.Add(previousJoinPoints[previousJoinPoints.Length - 2]);
                vertices.AddRange(joinPoints);
                var polygon = new Polygon(vertices);
                if (polygon.IsClockWise())
                {
                    polygons.Add(polygon.Reversed());
                }
                else
                {
                    polygons.Add(polygon);
                }
            }

            return polygons.ToArray();
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

        /// <summary>
        /// Identify any shared segments between two polylines.
        /// </summary>
        /// <param name="a">The first polyline to compare.</param>
        /// <param name="b">The second polyline to compare.</param>
        /// <param name="isClosed">Flag as closed to include segment between first and last vertex.</param>
        /// <returns>Returns a list of tuples of indices for the segments that match in each polyline.</returns>
        public static List<(int indexOnA, int indexOnB)> SharedSegments(Polyline a, Polyline b, bool isClosed = false)
        {
            var result = new List<(int, int)>();

            // Abbreviate lists to compare
            var va = a.Vertices;
            var vb = b.Vertices;

            for (var i = 0; i < va.Count; i++)
            {
                var ia = va[i];
                var ib = va[(i + 1) % va.Count];

                var iterations = isClosed ? vb.Count : vb.Count - 1;

                for (var j = 0; j < iterations; j++)
                {
                    var ja = vb[j];

                    if (ia.IsAlmostEqualTo(ja))
                    {
                        // Current vertices match, compare next vertices
                        var jNext = (j + 1) % vb.Count;
                        var jPrev = j == 0 ? vb.Count - 1 : j - 1;

                        var jb = vb[jNext];
                        var jc = vb[jPrev];

                        if (ib.IsAlmostEqualTo(jb))
                        {
                            // Match is current segment a and current segment b
                            result.Add((i, j));
                        }

                        if (ib.IsAlmostEqualTo(jc))
                        {
                            // Match is current segment a and previous segment b
                            result.Add((i, jPrev));
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Insert a point into the polyline if it lies along one
        /// of the polyline's segments.
        /// </summary>
        /// <param name="points">The points at which to split the polyline.</param>
        /// <returns>The index of the new vertex.</returns>
        public virtual void Split(IList<Vector3> points)
        {
            Split(points);
        }

        /// <summary>
        /// Insert a point into the polyline if it lies along one
        /// of the polyline's segments.
        /// </summary>
        /// <param name="points">The points at which to split the polyline.</param>
        /// <param name="closed">Flag as closed to include a segment between the first and last vertex.</param>
        /// <returns>The index of the new vertex.</returns>
        protected void Split(IList<Vector3> points, bool closed = false)
        {
            for (var i = 0; i < this.Vertices.Count; i++)
            {
                var a = this.Vertices[i];
                var b = closed && i == this.Vertices.Count - 1 ? this.Vertices[0] : this.Vertices[i + 1];

                for (var j = points.Count - 1; j >= 0; j--)
                {
                    var point = points[j];

                    if (point.IsAlmostEqualTo(a) || point.IsAlmostEqualTo(b))
                    {
                        // The split point is coincident with a vertex.
                        continue;
                    }

                    if (point.DistanceTo(new Line(a, b)).ApproximatelyEquals(0.0))
                    {
                        if (i > this.Vertices.Count - 1)
                        {
                            this.Vertices.Add(point);
                        }
                        else
                        {
                            this.Vertices.Insert(i + 1, point);
                        }

                        break;
                    }
                }
            }
            return;
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

        /// <summary>
        /// Convert a line to a polyline
        /// </summary>
        /// <param name="l">The line to convert.</param>
        public static Polyline ToPolyline(this Line l) => new Polyline(new[] { l.Start, l.End });

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
