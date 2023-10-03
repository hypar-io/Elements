using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using ClipperLib;
using Elements.Search;

namespace Elements.Geometry
{
    /// <summary>
    /// A continuous set of lines.
    /// Parameterization of the curve is 0->n-1 where n is the number of vertices.
    /// </summary>
    /// <example>
    /// [!code-csharp[Main](../../Elements/test/PolylineTests.cs?name=example)]
    /// </example>
    public class Polyline : IndexedPolycurve
    {
        /// <summary>
        /// The domain of the curve.
        /// </summary>
        [JsonIgnore]
        public override Domain1d Domain => new Domain1d(0, Vertices.Count - 1);

        /// <summary>
        /// Construct a polyline.
        /// </summary>
        /// <param name="vertices">A collection of vertex locations.</param>
        [JsonConstructor]
        public Polyline(IList<Vector3> @vertices) : base(vertices) { }

        /// <summary>
        /// Construct a polyline.
        /// </summary>
        /// <param name="vertices">A collection of vertex locations.</param>
        /// <param name="disableValidation">Should self intersection testing be disabled?</param>
        public Polyline(IList<Vector3> @vertices, bool disableValidation = false) : base(vertices, disableValidation: disableValidation) { }

        /// <summary>
        /// Construct a polyline from points. This is a convenience constructor
        /// that can be used like this: `new Polyline((0,0,0), (10,0,0), (10,10,0))`
        /// </summary>
        /// <param name="vertices">The vertices of the polyline.</param>
        public Polyline(params Vector3[] vertices) : base(vertices) { }

        /// <summary>
        /// Construct a polyline from points. This is a convenience constructor
        /// that can be used like this: `new Polyline((0,0,0), (10,0,0), (10,10,0))`
        /// </summary>
        /// <param name="disableValidation">Should self intersection testing be disabled?</param>
        /// <param name="vertices">The vertices of the polyline.</param>
        public Polyline(bool disableValidation, params Vector3[] vertices) : base(vertices, disableValidation: disableValidation) { }

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
        /// Get a collection a line segments which connect the vertices of this polyline.
        /// </summary>
        /// <returns>A collection of Lines.</returns>
        public virtual Line[] Segments()
        {
            return SegmentsInternal(this.Vertices);
        }

        /// <summary>
        /// Get the transform at the specified parameter along the polyline.
        /// </summary>
        /// <param name="u">The parameter on the polygon between 0.0 and length.</param>
        /// <returns>A transform with its Z axis aligned trangent to the polyline.</returns>
        public override Transform TransformAt(double u)
        {
            if (!Domain.Includes(u, true))
            {
                throw new Exception($"The parameter {u} is not on the trimmed portion of the basis curve. The parameter must be between {Domain.Min} and {Domain.Max}.");
            }

            var o = PointAtInternal(u, out var segmentIndex);

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

                if (!(this is Polygon) && (idx == 0 || idx == this.Vertices.Count - 1))
                {
                    return CreateOrthogonalTransform(idx, a, normals[idx]);
                }
                else
                {
                    return CreateMiterTransform(idx, a, normals[idx]);
                }
            }

            var nextIndex = (segmentIndex + 1) % this.Vertices.Count;
            var segment = new Line(Vertices[segmentIndex], Vertices[nextIndex]);
            var parameterOnSegment = u - segmentIndex;
            var previousNormal = normals[segmentIndex];
            var nextNormal = normals[nextIndex];
            var normal = ((nextNormal - previousNormal) * parameterOnSegment + previousNormal).Unitized();
            var x = segment.Direction().Cross(normal);

            return new Transform(o, x, normal, x.Cross(normal));
        }

        /// <summary>
        /// Construct a transformed copy of this Polyline.
        /// </summary>
        /// <param name="transform">The transform to apply.</param>
        public Polyline TransformedPolyline(Transform transform)
        {
            if (transform == null)
            {
                return this;
            }

            var transformed = new Vector3[this.Vertices.Count];
            for (var i = 0; i < transformed.Length; i++)
            {
                transformed[i] = transform.OfPoint(this.Vertices[i]);
            }
            var p = new Polyline(transformed);
            return p;
        }

        /// <inheritdoc/>
        public override Curve Transformed(Transform transform)
        {
            if (transform == null)
            {
                return this;
            }

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
            if (Vector3Extensions.AreCollinearByDistance(this.Vertices))
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
                    if (Math.Abs(previousDirection.Dot(nextDirection)) < 1 - Vector3.EPSILON)
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
                // Otherwise keep using the normal from the previous bend.
                if (i > 0 && i < result.Length - 1)
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
                if (i >= 1 && previousNormal.Dot(normal) < 0)
                {
                    normal *= -1;
                }
                result[i] = normal.Unitized();
                previousNormal = result[i];
            }
            return result;
        }

        /// <inheritdoc/>
        public override Transform[] Frames(double startSetbackDistance = 0.0,
                                           double endSetbackDistance = 0.0,
                                           double additionalRotation = 0.0)
        {
            var normals = this.NormalsAtVertices();

            // Create an array of transforms with the same number of items as the vertices.
            var result = new Transform[this.Vertices.Count];
            var l = Length();
            for (var i = 0; i < result.Length; i++)
            {
                Vector3 a;
                if (i == 0)
                {
                    a = PointAt(ParameterAtDistanceFromParameter(startSetbackDistance, this.Domain.Min));
                }
                else if (i == Vertices.Count - 1)
                {
                    a = PointAt(ParameterAtDistanceFromParameter(l - endSetbackDistance, this.Domain.Min));
                }
                else
                {
                    a = this.Vertices[i];
                }
                result[i] = CreateOrthogonalTransform(i, a, normals[i]);
                if (additionalRotation != 0.0)
                {
                    result[i].RotateAboutPoint(result[i].Origin, result[i].ZAxis, additionalRotation);
                }
            }
            return result;
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

        internal Line[] SegmentsInternal(IList<Vector3> vertices)
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

        private Transform CreateOrthogonalTransform(int i, Vector3 origin, Vector3 up)
        {
            Vector3 prev, next, tangent;

            if (i == 0)
            {
                next = this.Vertices[i + 1];
                tangent = (next - origin).Unitized();
            }
            else if (i == this.Vertices.Count - 1)
            {
                prev = this.Vertices[i - 1];
                tangent = (origin - prev).Unitized();
            }
            else
            {
                prev = this.Vertices[i - 1];
                next = this.Vertices[i + 1];
                var v1 = (origin - prev).Unitized();
                var v2 = (next - origin).Unitized();
                tangent = v1.Average(v2).Unitized();
                // if a segment doubles back on itself, this will be zero — just
                // pick one tangent if this happens.
                if (tangent.IsZero())
                {
                    tangent = v1;
                }
            }
            tangent = tangent.Negate();
            return new Transform(origin, up.Cross(tangent), tangent);
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
            var joinInfo = GetOffsetPoints(isCycle, segments, offset, flip);

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
        /// A naïve control-point-only 2D open offset. This algorithm does not
        /// do any self-intersection checking.
        /// </summary>
        /// <param name="offset">The offset distance.</param>
        /// <returns>A new polyline with the same number of control points.</returns>
        public Polyline OffsetOpen(double offset)
        {
            var newVertices = new List<Vector3>();
            var segments = Segments().Select(s => s.Offset(offset, false)).ToList();
            if (segments.Count == 1)
            {
                return new Polyline(segments[0].Start, segments[0].End);
            }
            for (int i = 0; i < segments.Count - 1; i++)
            {
                var currSegment = segments[i];
                var nextSegment = segments[i + 1];
                if (i == 0)
                {
                    newVertices.Add(currSegment.Start);
                }

                if (currSegment.Direction().Dot(nextSegment.Direction()) > 1 - Vector3.EPSILON)
                {
                    newVertices.Add(currSegment.End);
                }
                else
                {
                    if (currSegment.Intersects(nextSegment, out Vector3 intersection, true, true))
                    {
                        newVertices.Add(intersection);
                    }
                    else
                    {
                        newVertices.Add(currSegment.End);
                    }
                }

                if (i == segments.Count - 2)
                {
                    newVertices.Add(nextSegment.End);
                }
            }
            var polyline = new Polyline(newVertices);
            if (polyline.Vertices.Count < 2)
            {
                throw new Exception("The offset of the polyline resulted in invalid geometry, such as a single point.");
            }
            return polyline;
        }

        /// <summary>
        /// Offset this polyline by the specified amount. The resulting polygon will have acute angles.
        /// </summary>
        /// <remarks>This blunts sharp corners to keep widths close to the target.</remarks>
        /// <param name="offset">The amount to offset.</param>
        public Polygon[] OffsetWithAcuteAngle(double offset)
        {
            var polygons = new List<Polygon>();
            if (this.Vertices.Count <= 1)
            {
                return polygons.ToArray();
            }

            var isCycle = this.Vertices.Count > 2 && this.Vertices[0].DistanceTo(this.Vertices.Last()) <= offset / 2;
            var segments = this.Segments();

            // Step through each point, collecting info on what its join will look like.
            var joinInfo = GetOffsetPoints(isCycle, segments, offset, false, true);

            polygons.Add(new Polygon(joinInfo.SelectMany(p => p).ToList()));

            return polygons.ToArray();
        }

        /// <summary>
        /// Return offset points for polyline
        /// </summary>
        /// <param name="isCycle">The polyline has the shape of a circle.</param>
        /// <param name="segments">List of polyline segments.</param>
        /// <param name="offset">The amount to offset.</param>
        /// <param name="flip">Offset on the opposite of the default side. The default is to draw on the +X side of a polyline that goes up the +Y axis.</param>
        /// <param name="isSkipOriginVertices">Option to specify whether to skip the original polyline points.</param>
        private List<Vector3[]> GetOffsetPoints(bool isCycle, Line[] segments, double offset, bool flip, bool isSkipOriginVertices = false)
        {
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
                    if (intersects && !isSkipOriginVertices)
                    {
                        isAcuteExteriorAngle = nextOffsetSegment.Direction().Dot((offsetIntersection - vertex).Unitized()) < Math.Cos(Math.PI * -3 / 4);
                    }

                    // Tight joins should get a cap, to maintain minimum width.
                    if (!intersects ||
                        isAcuteExteriorAngle &&
                        (previousOffsetSegment.End.DistanceTo(offsetIntersection) > offset ||
                        nextOffsetSegment.Start.DistanceTo(offsetIntersection) > offset))
                    {
                        var offsetPoint1 = previousOffsetSegment.End + (isSkipOriginVertices ? new Vector3() : previousOffsetSegment.Direction() * offset);
                        var offsetPoint2 = nextOffsetSegment.Start + (isSkipOriginVertices ? new Vector3() : nextOffsetSegment.Direction() * (offset * -1));
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

                if (!isSkipOriginVertices)
                {
                    joinPoints.Add(vertex);
                }
                joinInfo.Add(joinPoints.ToArray());
            }

            return joinInfo;
        }

        /// <summary>
        /// Project this polyline onto the plane.
        /// </summary>
        /// <param name="plane">The plane of the returned polyline.</param>
        public Polyline Project(Plane plane)
        {
            var projected = new Vector3[this.Vertices.Count];
            for (var i = 0; i < projected.Length; i++)
            {
                projected[i] = this.Vertices[i].Project(plane);
            }
            return new Polyline(projected);
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
        /// <param name="closed">Is the polyline closed?</param>
        protected void Split(IList<Vector3> points, bool closed = false)
        {
            for (var i = 0; i < this.Vertices.Count; i++)
            {
                var a = this.Vertices[i];
                var b = closed && i == this.Vertices.Count - 1 ? this.Vertices[0] : this.Vertices[i + 1];
                var edge = (a, b);

                // An edge may have multiple split points. 
                // We store these in a list and sort it along the
                // direction of the edge, before inserting the points
                // into the vertex list and incrementing i by the correct
                // amount to move forward to the next edge.
                var newEdgeVertices = new List<Vector3>();
                var comparer = new DirectionComparer((edge.a - edge.b).Unitized());

                for (var j = points.Count - 1; j >= 0; j--)
                {
                    var point = points[j];

                    if (point.IsAlmostEqualTo(edge.a) || point.IsAlmostEqualTo(edge.b))
                    {
                        // The split point is coincident with a vertex.
                        continue;
                    }

                    if (point.DistanceTo(edge, out var newPoint).ApproximatelyEquals(0.0))
                    {
                        if (!newEdgeVertices.Contains(newPoint))
                        {
                            newEdgeVertices.Add(newPoint);
                        }
                    }
                }
                newEdgeVertices.Sort(comparer);
                ((List<Vector3>)this.Vertices).InsertRange(i + 1, newEdgeVertices);
                i += newEdgeVertices.Count;
            }
            return;
        }

        /// <summary>
        /// Find the parameter corresponding to a point along a polyline.
        /// </summary>
        /// <param name="point">A point.</param>
        /// <returns>A parameter for the point along a polyline if the point is on the polyline. Otherwise, -1.</returns>
        public double GetParameterAt(Vector3 point)
        {
            if (point.IsAlmostEqualTo(Start))
            {
                return Domain.Min;
            }

            if (point.IsAlmostEqualTo(End))
            {
                return Domain.Max;
            }

            var i = 0;
            foreach (var e in Edges())
            {
                if (Line.PointOnLine(point, e.from, e.to, true))
                {
                    var l = e.from.DistanceTo(e.to);
                    var t = point.DistanceTo(e.from) / l; // normalized length along this segment
                    return i + t;
                }
                i++;
            }

            return -1;
        }

        /// <summary>
        /// Check if polyline intersects with line
        /// </summary>
        /// <param name="line">Line to check</param>
        /// <param name="intersections">Intersections between polyline and line</param>
        /// <param name="infinite">Threat the line as infinite?</param>
        /// <param name="includeEnds">If the end of line lies exactly on the vertex of polyline, count it as an intersection? </param>
        /// <returns>True if line intersects with polyline, false if they do not intersect</returns>
        public bool Intersects(Line line, out List<Vector3> intersections, bool infinite = false, bool includeEnds = false)
        {
            var segments = Segments();

            intersections = new List<Vector3>();

            foreach (var segment in segments)
            {
                if (segment.Intersects(line, out var point, infinite: infinite, includeEnds: includeEnds))
                {
                    if (segment.PointOnLine(point, includeEnds) && intersections.All(x => !x.IsAlmostEqualTo(point)))
                    {
                        intersections.Add(point);
                    }
                }
            }

            return intersections.Any();
        }

        /// <summary>
        /// Checks if polyline intersects with polygon
        /// </summary>
        /// <param name="polygon">Polygon to check</param>
        /// <param name="sharedSegments">List of shared subsegments</param>
        /// <returns>Result of check if polyline and polygon intersects</returns>
        public bool Intersects(Polygon polygon, out List<Polyline> sharedSegments)
        {
            sharedSegments = new List<Polyline>();

            var intersections = polygon.Segments()
                .SelectMany(x =>
                {
                    Intersects(x, out var result, includeEnds: true);
                    return result;
                })
                .UniqueWithinTolerance()
                .OrderBy(GetParameterAt)
                .ToList();

            if (intersections.Count == 0)
            {
                if (polygon.Contains(Start) && polygon.Contains(End))
                {
                    sharedSegments.Add(this);
                }
                return sharedSegments.Any();
            }

            if (polygon.Contains(Start))
            {
                var intersection = intersections.First();
                var startSegment = GetSubsegment(Start, intersection);
                sharedSegments.Add(startSegment);
                intersections.Remove(intersection);
            }

            for (var i = 0; i < intersections.Count - 1; i++)
            {
                var subsegment = GetSubsegment(intersections[i], intersections[i + 1]);
                if (polygon.Contains(subsegment.Mid(), out var containment) && containment == Containment.Inside)
                {
                    sharedSegments.Add(subsegment);
                }
            }

            if (polygon.Contains(End))
            {
                var intersection = intersections.Last();
                var endSegment = GetSubsegment(intersection, End);
                sharedSegments.Add(endSegment);
            }

            return sharedSegments.Any();
        }

        /// <summary>
        /// Get new polyline between two points
        /// </summary>
        /// <param name="start">Start point</param>
        /// <param name="end">End point</param>
        /// <returns>New polyline or null if any of points is not on polyline</returns>
        public Polyline GetSubsegment(Vector3 start, Vector3 end)
        {
            if (start.IsAlmostEqualTo(end))
            {
                return null;
            }

            var startParameter = GetParameterAt(start);
            var endParameter = GetParameterAt(end);

            if (startParameter < 0 || endParameter < 0)
            {
                return null;
            }

            List<Vector3> filteredVertices;

            if (startParameter > endParameter)
            {
                filteredVertices = Vertices
                    .Where(x =>
                    {
                        var parameter = GetParameterAt(x);
                        return parameter < startParameter && parameter > endParameter;
                    })
                    .Reverse()
                    .ToList();
            }
            else
            {
                filteredVertices = Vertices
                    .Where(x =>
                    {
                        var parameter = GetParameterAt(x);
                        return parameter > startParameter && parameter < endParameter;
                    })
                    .ToList();
            }

            filteredVertices.Insert(0, start);
            filteredVertices.Add(end);

            return new Polyline(filteredVertices);
        }

        /// <summary>
        /// Make the polyline correspond to the supported angles by moving the vertices slightly.
        /// The result polyline will have only allowed angles, but vertices positions can be changed.
        /// The first vertex is never moved.
        /// </summary>
        /// <param name="supportedAngles">List of supported angles that the returned polyline can have. Supported angles must be between 0 and 90.</param>
        /// <param name="referenceVector">Vector to align first segment of polyline with.</param>
        /// <param name="pathType">
        /// The path type.
        /// For each 3 consecutive points A, B, C to make angle ABC be one of allowed angles:
        /// - NormalizationType.Start: move B vertex
        /// - NormalizationType.Middle: move both B and C vertices in approximately equivalent proportions
        /// - NormalizationType.End : move C vertex
        /// </param>
        /// <param name="furthestDistancePointsMoved">The furthest distance that any point moved.</param>
        /// <returns>The result polyline that has only allowed angles.</returns>
        public Polyline ForceAngleCompliance(IEnumerable<double> supportedAngles,
            Vector3 referenceVector, out double furthestDistancePointsMoved, NormalizationType pathType = NormalizationType.Start)
        {
            if (supportedAngles.Any(a => a > 90 || a < 0))
            {
                throw new ArgumentException("Supported angles must be between 0 and 90");
            }

            List<Vector3> normalized = Vertices.ToList();

            for (int i = 0; i < normalized.Count - 1; i++)
            {
                NormalizationType localType = pathType;
                if (i == 0)
                {
                    localType = NormalizationType.End;
                }
                else if (i == normalized.Count - 2)
                {
                    localType = NormalizationType.Start;
                }

                Vector3 incomingDirection = referenceVector;
                if (i > 0)
                {
                    incomingDirection = (normalized[i] - normalized[i - 1]).Unitized();
                    if (i > 1)
                    {
                        var before = (normalized[i - 1] - normalized[i - 2]).Unitized();
                        referenceVector = before.Cross(incomingDirection);
                    }
                }

                var direction = (normalized[i + 1] - normalized[i]).Unitized();
                if (direction.Dot(incomingDirection).Equals(0) ||
                    direction.ProjectOnto(incomingDirection).Length().ApproximatelyEquals(0))
                {
                    // When path drastically changes direction - (1, 2, 2) -> (1, 0, 2) -> (0, 0, 0) for example,
                    // angle will be 90 degrees regardless if third point is (0, 0, 0), (0, 0, 1) or (0, 0, 1.5).
                    // These points still need to be aligned to avoid bad angles further in the path.
                    // Reference vector is used in this case as cross product of 3 previous points.
                    if (i < normalized.Count - 2)
                    {
                        incomingDirection = referenceVector.Dot(direction) < 0 ? referenceVector.Negate() : referenceVector;
                        localType = NormalizationType.End;
                    }
                    else
                    {
                        continue;
                    }
                }

                double incomingAngle = direction.AngleTo(incomingDirection);
                if (incomingAngle.ApproximatelyEquals(180, 0.1) || incomingAngle.ApproximatelyEquals(0, 0.1) ||
                    supportedAngles.Any(a => incomingAngle.ApproximatelyEquals(a, 0.1) ||
                                            (incomingAngle - 90).ApproximatelyEquals(a, 0.1)))
                {
                    continue;
                }

                double angleDelta = 180;
                double bestFitAngle = -1;
                foreach (var angle in supportedAngles)
                {
                    var delta = Math.Abs(angle - incomingAngle);
                    if (delta < angleDelta)
                    {
                        bestFitAngle = angle;
                        angleDelta = delta;
                    }
                }

                double directionalDistance = AngleAlignedDistance(
                    normalized[i], normalized[i + 1], incomingDirection, bestFitAngle, out var cornerPoint);
                var directionalVector = normalized[i] - cornerPoint;

                if (bestFitAngle.ApproximatelyEquals(0))
                {
                    switch (localType)
                    {
                        case NormalizationType.Start:
                        case NormalizationType.Middle:
                            normalized[i] = cornerPoint;
                            break;
                        default:
                            normalized[i + 1] = cornerPoint;
                            break;
                    }
                    continue;
                }
                switch (localType)
                {
                    case NormalizationType.Start:
                        {
                            normalized[i] = cornerPoint + directionalVector.Unitized() * directionalDistance;
                        }
                        break;
                    case NormalizationType.Middle:
                        {
                            var delta = (directionalDistance - directionalVector.Length()) / 2;
                            normalized[i] = cornerPoint + directionalVector.Unitized() * (directionalDistance - delta);

                            var point = DisplacementAlignedPoint(
                                normalized[i], normalized[i + 1], normalized[i + 2], directionalVector.Unitized(), delta);
                            if (point.HasValue)
                            {
                                normalized[i + 1] = point.Value;
                            }
                        }
                        break;
                    default:
                    case NormalizationType.End:
                        {
                            var delta = directionalDistance - directionalVector.Length();
                            var point = DisplacementAlignedPoint(
                                normalized[i], normalized[i + 1], normalized[i + 2], directionalVector.Unitized(), delta);
                            if (point.HasValue)
                            {
                                normalized[i + 1] = point.Value;
                            }
                        }
                        break;
                }
            }

            furthestDistancePointsMoved = normalized.Zip(Vertices, (a, b) => a.DistanceTo(b)).Max();
            return new Polyline(normalized);
        }

        /// <summary>
        /// Make the polyline correspond to the supported angles by moving the vertices slightly.
        /// The result polyline will have only allowed angles, but vertices positions can be changed.
        /// </summary>
        /// <param name="supportedAngles">List of supported angles that the returned polyline can have. Supported angles must be between 0 and 90.</param>
        /// <param name="referenceVector">Vector to align first segment of polyline with.</param>
        /// <param name="pathType">
        /// The path type.
        /// For each 3 consecutive points A, B, C to make angle ABC be one of allowed angles:
        /// - NormalizationType.Start: move B vertex
        /// - NormalizationType.Middle: move both B and C vertices in approximately equivalent proportions
        /// - NormalizationType.End : move C vertex
        /// </param>
        /// <returns>The result polyline that has only allowed angles.</returns>
        public Polyline ForceAngleCompliance(IEnumerable<double> supportedAngles,
            Vector3 referenceVector, NormalizationType pathType = NormalizationType.Start)
        {
            return ForceAngleCompliance(supportedAngles, referenceVector, out _, pathType);
        }

        /// <summary>
        /// Calculate distance from corner point, so point X = cornerPoint + incoming * D has needed angle (cornerPoint -> X -> end)
        /// </summary>
        private double AngleAlignedDistance(Vector3 start, Vector3 end, Vector3 incoming, double angle, out Vector3 cornerPoint)
        {
            var dot = (end - start).Dot(incoming);
            cornerPoint = start + incoming * dot;
            double directionalDistance = 0;

            if (!angle.ApproximatelyEquals(90, 0.1))
            {
                var perdendicularDistance = (end - cornerPoint).Length();
                directionalDistance = perdendicularDistance / Math.Tan(Units.DegreesToRadians(angle));
            }

            return directionalDistance;
        }

        /// <summary>
        /// Calculate a point X on infinite B->C line, that intersects with A->(B+d) line, where d is displacement.
        /// </summary>
        private Vector3? DisplacementAlignedPoint(Vector3 a, Vector3 b, Vector3 c,
            Vector3 displacementDirection, double displacementDistance)
        {
            var roughEndPoint = b - displacementDirection * displacementDistance;
            Plane plane = new Plane(a, roughEndPoint, c);
            var bcProjected = new Line(b, c).Projected(plane);
            Line displacementLine = new Line(a, roughEndPoint);
            if (displacementLine.Intersects(bcProjected, out var position, infinite: true))
            {
                return position;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Fillet all corners on this polygon.
        /// </summary>
        /// <param name="radius">The fillet radius.</param>
        /// <returns>A contour containing trimmed edge segments and fillets.</returns>
        public IndexedPolycurve Fillet(double radius)
        {
            var curves = new List<BoundedCurve>();
            var segments = this.Segments();

            for (var i = 0; i < segments.Length - 1; i++)
            {
                var a = segments[i];
                var b = segments[i + 1];
                var arc = b.Fillet(a, radius);
                if (i == 0)
                {
                    curves.Add(new Line(Start, arc.Start));
                }
                else
                {
                    curves.Add(new Line(curves[curves.Count - 1].End, arc.Start));
                }
                curves.Add(arc);
            }
            curves.Add(new Line(curves[curves.Count - 1].End, End));

            return new IndexedPolycurve(curves);
        }
    }
}
