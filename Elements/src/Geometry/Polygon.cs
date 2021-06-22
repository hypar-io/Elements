using ClipperLib;
using Elements.Geometry.Interfaces;
using Elements.Spatial;
using LibTessDotNet.Double;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Elements.Geometry
{
    /// <summary>
    /// A closed planar polygon.
    /// </summary>
    /// <example>
    /// [!code-csharp[Main](../../Elements/test/PolygonTests.cs?name=example)]
    /// </example>
    public partial class Polygon : Polyline
    {
        /// <summary>
        /// Implicitly convert a polygon to a profile.
        /// </summary>
        /// <param name="p">The polygon to convert.</param>
        public static implicit operator Profile(Polygon p) => new Profile(p);

        // Though this conversion may seem redundant to the Curve => ModelCurve converter, it is needed to
        // make this the default implicit conversion from a polygon to an element (rather than the
        // polygon => profile conversion above.)
        /// <summary>
        /// Implicitly convert a Polygon to a ModelCurve Element.
        /// </summary>
        /// <param name="c">The curve to convert.</param>
        public static implicit operator Element(Polygon c) => new ModelCurve(c);

        /// <summary>
        /// Construct a transformed copy of this Polygon.
        /// </summary>
        /// <param name="transform">The transform to apply.</param>
        public Polygon TransformedPolygon(Transform transform)
        {
            var transformed = new Vector3[this.Vertices.Count];
            for (var i = 0; i < transformed.Length; i++)
            {
                transformed[i] = transform.OfPoint(this.Vertices[i]);
            }
            var p = new Polygon(transformed);
            return p;
        }

        /// <summary>
        /// Construct a transformed copy of this Curve.
        /// </summary>
        /// <param name="transform">The transform to apply.</param>
        public override Curve Transformed(Transform transform)
        {
            return TransformedPolygon(transform);
        }

        /// <summary>
        /// Construct a transform in the plane of this polygon, with its Z axis along the polygon's normal.
        /// </summary>
        /// <returns></returns>
        public Transform ToTransform()
        {
            var normal = Normal();
            return new Transform(Vertices[0], Vertices[1] - Vertices[0], normal);
        }

        /// <summary>
        /// Tests if the supplied Vector3 is within this Polygon without coincidence with an edge when compared on a shared plane.
        /// </summary>
        /// <param name="vector">The Vector3 to compare to this Polygon.</param>
        /// <returns>
        /// Returns true if the supplied Vector3 is within this Polygon when compared on a shared plane. Returns false if the Vector3 is outside this Polygon or if the supplied Vector3 is null.
        /// </returns>
        public bool Contains(Vector3 vector)
        {
            Contains(vector, out Containment containment);
            return containment == Containment.Inside;
        }

        /// <summary>
        /// Tests if the supplied Vector3 is within this Polygon, using a 2D method.
        /// </summary>
        /// <param name="vector">The position to test.</param>
        /// <param name="containment">Whether the point is inside, outside, at an edge, or at a vertex.</param>
        /// <returns>Returns true if the supplied Vector3 is within this polygon.</returns>
        public bool Contains(Vector3 vector, out Containment containment)
        {
            return Contains3D(Segments(), vector, out containment);
        }

        /// <summary>
        /// Trim the polygon with a plane.
        /// Everything on the "back" side of the plane will be trimmed.
        /// </summary>
        /// <param name="plane">The trimming plane.</param>
        /// <param name="flip">Should the plane be flipped?</param>
        /// <returns>A collection of new polygons, trimmed by the plane, or null if no
        /// trimming occurred.</returns>
        public List<Polygon> Trimmed(Plane plane, bool flip = false)
        {
            const double precision = 1e-05;
            try
            {
                if (flip)
                {
                    plane = new Plane(plane.Origin, plane.Normal.Negate());
                }

                if (!this.Intersects(plane, out List<Vector3> intersections, false))
                {
                    return null;
                }

                var newVertices = new List<Vector3>();
                for (var i = 0; i <= this.Vertices.Count - 1; i++)
                {
                    var v1 = this.Vertices[i];
                    var v2 = i == this.Vertices.Count - 1 ? this.Vertices[0] : this.Vertices[i + 1];

                    var d1 = v1.DistanceTo(plane);
                    var d2 = v2.DistanceTo(plane);

                    if (d1.ApproximatelyEquals(0, precision) && d2.ApproximatelyEquals(0, precision))
                    {
                        // The segment is in the plane.
                        newVertices.Add(v1);
                        continue;
                    }

                    if (d1 < 0 && d2 < 0)
                    {
                        // Both points are on the outside of
                        // the plane.
                        continue;
                    }

                    if (d1 > 0 && d2 > 0)
                    {
                        // Both points are on the inside of
                        // the plane.
                        newVertices.Add(v1);
                        continue;
                    }

                    if (d1 > 0 && d2.ApproximatelyEquals(0, precision))
                    {
                        // The first point is inside and 
                        // the second point is on the plane.
                        newVertices.Add(v1);

                        // Insert what will become a duplicate 
                        // vertex.
                        newVertices.Add(v2);
                        continue;
                    }

                    if (d1.ApproximatelyEquals(0, precision) && d2 > 0)
                    {
                        // The first point is on the plane,
                        // and the second is inside.
                        newVertices.Add(v1);
                        continue;
                    }

                    var l = new Line(v1, v2);
                    if (l.Intersects(plane, out Vector3 result))
                    {
                        // Figure out what side the intersection is on.
                        if (d1 < 0)
                        {
                            newVertices.Add(result);
                        }
                        else
                        {
                            newVertices.Add(v1);
                            newVertices.Add(result);
                        }
                    }
                }

                var graph = new HalfEdgeGraph2d();
                graph.EdgesPerVertex = new List<List<(int from, int to)>>();

                if (newVertices.Count > 0)
                {
                    graph.Vertices = newVertices;

                    // Initialize the graph.
                    foreach (var v in newVertices)
                    {
                        graph.EdgesPerVertex.Add(new List<(int from, int to)>());
                    }

                    for (var i = 0; i < newVertices.Count - 1; i++)
                    {
                        var a = i;
                        var b = i + 1 > newVertices.Count - 1 ? 0 : i + 1;
                        if (intersections.Contains(newVertices[a]) && intersections.Contains(newVertices[b]))
                        {
                            continue;
                        }

                        // Only add one edge around the outside of the shape.
                        graph.EdgesPerVertex[a].Add((a, b));
                    }

                    for (var i = 0; i < intersections.Count - 1; i += 2)
                    {
                        // Because we'll have duplicate vertices where an 
                        // intersection is on the plane, we need to choose
                        // which one to use. This follows the rule of finding 
                        // the one whose index is closer to the first index used.
                        var a = ClosestIndexOf(newVertices, intersections[i], i);
                        var b = ClosestIndexOf(newVertices, intersections[i + 1], a);

                        graph.EdgesPerVertex[a].Add((a, b));
                    }

                    if (graph.EdgesPerVertex[newVertices.Count - 1].Count == 0)
                    {
                        // Close the graph
                        var a = newVertices.Count - 1;
                        var b = 0;
                        graph.EdgesPerVertex[a].Add((a, b));
                    }
                    return graph.Polygonize();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
            return null;
        }

        /// <summary>
        /// Intersect this polygon with the provided polygon in 3d.
        /// </summary>
        /// <param name="polygon">The target polygon.</param>
        /// <param name="result">The points resulting from the intersection
        /// of the two polygons.</param>
        /// <returns>A collection of point sorted along the polygon's plane.</returns>
        internal bool Intersects3d(Polygon polygon, out List<Vector3> result)
        {
            var p = this.Plane();
            result = new List<Vector3>();
            var targetP = polygon.Plane();
            var d = this.Plane().Normal.Cross(targetP.Normal).Unitized();

            // Intersect the polygon against this polygon's plane.
            // Keep the points that lie within the polygon.
            if (polygon.Intersects(p, out List<Vector3> results))
            {
                foreach (var r in results)
                {
                    if (this.Contains3D(r))
                    {
                        result.Add(r);
                    }
                }
            }

            // Intersect this polygon against the target polyon's plane.
            // Keep the points within the target polygon.
            if (this.Intersects(targetP, out List<Vector3> results2))
            {
                foreach (var r in results2)
                {
                    if (polygon.Contains3D(r))
                    {
                        result.Add(r);
                    }
                }
            }

            if (result.Count > 0)
            {
                result.Sort(new DotComparer(d));
                return true;
            }

            return false;
        }

        private int ClosestIndexOf(List<Vector3> vertices, Vector3 target, int targetIndex)
        {
            var first = vertices.IndexOf(target);
            var last = vertices.LastIndexOf(target);
            if (first == last)
            {
                return first;
            }
            var d1 = Math.Abs(targetIndex - first);
            var d2 = Math.Abs(targetIndex - last);
            if (d1 < d2)
            {
                return first;
            }
            return last;
        }

        /// <summary>
        /// Does this polygon intersect the provided plane?
        /// </summary>
        /// <param name="plane">The intersection plane.</param>
        /// <param name="results">A collection of intersection results 
        /// sorted along the plane.</param>
        /// <param name="distinct">Should the intersection results that
        /// are returned be distinct?</param>
        /// <returns>True if the plane intersects the polygon, 
        /// otherwise false.</returns>
        public bool Intersects(Plane plane, out List<Vector3> results, bool distinct = true)
        {
            results = new List<Vector3>();
            var d = this.Plane().Normal.Cross(plane.Normal).Unitized();

            foreach (var s in this.Segments())
            {
                if (s.Intersects(plane, out Vector3 result))
                {
                    if (distinct)
                    {
                        if (!results.Contains(result))
                        {
                            results.Add(result);
                        }
                    }
                    else
                    {
                        results.Add(result);
                    }
                }
            }
            if (results.Count > 0)
            {
                // Order the intersections along the direction.
                results.Sort(new DotComparer(d));
                return true;
            }
            return false;
        }

        // Projects non-flat containment request into XY plane and returns the answer for this projection
        internal static bool Contains3D(IEnumerable<Line> segments, Vector3 location, out Containment containment)
        {
            var vertices = segments.Select(segment => segment.Start).ToList();
            var is3D = vertices.Any(vertex => vertex.Z != 0);
            if (!is3D)
            {
                return Contains(segments, location, out containment);
            }
            var transformTo3D = vertices.ToTransform();
            var transformToGround = new Transform(transformTo3D);
            transformToGround.Invert();
            var groundSegments = segments.Select(segment => segment.TransformedLine(transformToGround));
            var groundLocation = transformToGround.OfPoint(location);
            return Contains(groundSegments, groundLocation, out containment);
        }

        /// <summary>
        /// Does this polygon contain the specified point.
        /// https://en.wikipedia.org/wiki/Point_in_polygon
        /// </summary>
        /// <param name="point">The point to test.</param>
        /// <returns>True if the point is contained in the polygon, otherwise false.</returns>
        internal bool Contains3D(Vector3 point)
        {
            var p = this.Plane();
            var t = new Transform(point, p.Normal);

            // Intersect a randomly directed ray in the plane
            // of the polygon and intersect with the polygon edges.
            var ray = new Ray(point, t.XAxis);
            var intersects = 0;
            var xsects = new List<Vector3>();
            foreach (var s in this.Segments())
            {
                var result = default(Vector3);
                if (s.Start.IsAlmostEqualTo(point) ||
                    s.End.IsAlmostEqualTo(point) ||
                    ray.Intersects(s, out result))
                {
                    // Look for duplicates like when a ray
                    // hits a vertex. In this case, you
                    // only want to register one intersection.
                    if (!xsects.Contains(result))
                    {
                        xsects.Add(result);
                        intersects++;
                    }
                }
            }
            return intersects % 2 != 0;
        }

        // Adapted from https://stackoverflow.com/questions/46144205/point-in-polygon-using-winding-number/46144206
        internal static bool Contains(IEnumerable<Line> segments, Vector3 location, out Containment containment)
        {
            int windingNumber = 0;

            foreach (var edge in segments)
            {
                // check for coincidence with edge vertices
                var toStart = location - edge.Start;
                if (toStart.IsZero())
                {
                    containment = Containment.CoincidesAtVertex;
                    return true;
                }
                var toEnd = location - edge.End;
                if (toEnd.IsZero())
                {
                    containment = Containment.CoincidesAtVertex;
                    return true;
                }
                //along segment - check if perpendicular distance to segment is below tolerance and that point is between ends
                var a = toStart.Length();
                var b = toStart.Dot((edge.End - edge.Start).Unitized());
                if (a * a - b * b < Vector3.EPSILON * Vector3.EPSILON && toStart.Dot(toEnd) < 0)
                {
                    containment = Containment.CoincidesAtEdge;
                    return true;
                }


                if (edge.AscendingRelativeTo(location) &&
                    edge.LocationInRange(location, Line.Orientation.Ascending))
                {
                    windingNumber += Wind(location, edge, Line.Position.Left);
                }
                if (!edge.AscendingRelativeTo(location) &&
                    edge.LocationInRange(location, Line.Orientation.Descending))
                {
                    windingNumber -= Wind(location, edge, Line.Position.Right);
                }
            }

            var result = windingNumber != 0;
            containment = result ? Containment.Inside : Containment.Outside;
            return result;
        }

        private static int Wind(Vector3 location, Line edge, Line.Position position)
        {
            return edge.RelativePositionOf(location) != position ? 0 : 1;
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
        /// Calculates whether this polygon is configured clockwise.
        /// This method only works for 2D polygons. For 3D polygons, you
        /// will need to transform your polygon into the XY plane, then
        /// run this method on that polygon.
        /// </summary>
        /// <returns>True if this polygon is oriented clockwise.</returns>
        public bool IsClockWise()
        {
            // https://en.wikipedia.org/wiki/Shoelace_formula
            var sum = 0.0;
            for (int i = 0; i < this.Vertices.Count; i++)
            {
                var point = this.Vertices[i];
                var nextPoint = this.Vertices[(i + 1) % this.Vertices.Count];
                sum += (nextPoint.X - point.X) * (nextPoint.Y + point.Y);
            }
            return sum > 0;
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
            var clipperScale = 1 / Vector3.EPSILON;
            var thisPath = this.ToClipperPath();
            var intPoint = new IntPoint(vector.X * clipperScale, vector.Y * clipperScale);
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
            if (this.IsClockWise() != polygon.IsClockWise())
            {
                polygon = polygon.Reversed();
            }
            var clipper = new Clipper();
            var solution = new List<List<IntPoint>>();
            clipper.AddPath(this.ToClipperPath(), PolyType.ptSubject, true);
            clipper.AddPath(polygon.ToClipperPath(), PolyType.ptClip, true);
            clipper.Execute(ClipType.ctUnion, solution);
            if (solution.Count != 1)
            {
                return false;
            }
            return Math.Abs(solution.First().ToPolygon().Area() - this.ToClipperPath().ToPolygon().Area()) <= 0.0001;
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
            var clipperScale = 1.0 / Vector3.EPSILON;
            var thisPath = this.ToClipperPath();
            var intPoint = new IntPoint(vector.X * clipperScale, vector.Y * clipperScale);
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
        /// <param name="tolerance">An optional tolerance.</param>
        /// <returns>
        /// Returns true if the supplied Vector3 coincides with an edge of this Polygon when compared on a shared plane. Returns false if the supplied Vector3 is not coincident with an edge of this Polygon, or if the supplied Vector3 is null.
        /// </returns>
        public bool Touches(Vector3 vector, double tolerance = Vector3.EPSILON)
        {
            var clipperScale = 1.0 / tolerance;
            var thisPath = this.ToClipperPath(tolerance);
            var intPoint = new IntPoint(vector.X * clipperScale, vector.Y * clipperScale);
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
        /// <param name="tolerance">An optional tolerance.</param>
        /// <returns>
        /// Returns true if the supplied Polygon shares at least one edge point with this Polygon without the Polygons intersecting when compared on a shared plane. Returns false if the Polygons intersect, are disjoint, or if the supplied Polygon is null.
        /// </returns>
        public bool Touches(Polygon polygon, double tolerance = Vector3.EPSILON)
        {
            if (polygon == null || this.Intersects(polygon))
            {
                return false;
            }
            var thisPath = this.ToClipperPath(tolerance);
            var polyPath = polygon.ToClipperPath(tolerance);
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
        /// Split this polygon with an open polyline.
        /// </summary>
        /// <param name="pl">The polyline with which to split.</param>
        public List<Polygon> Split(Polyline pl)
        {
            var plXform = this.ToTransform();
            var inverse = new Transform(plXform);
            inverse.Invert();
            var thisInXY = this.TransformedPolygon(inverse);
            // Construct a half-edge graph from the polygon and the polyline
            var graph = Elements.Spatial.HalfEdgeGraph2d.Construct(thisInXY, pl.TransformedPolyline(inverse));
            // Find closed regions in that graph
            return graph.Polygonize().Select(p => p.TransformedPolygon(plXform)).ToList();
        }

        /// <summary>
        /// Split this polygon with a collection of open polylines.
        /// </summary>
        /// <param name="polylines">The polylines with which to split.</param>
        public List<Polygon> Split(IEnumerable<Polyline> polylines)
        {
            var plXform = this.ToTransform();
            var inverse = new Transform(plXform);
            inverse.Invert();
            var thisInXY = this.TransformedPolygon(inverse);
            // Construct a half-edge graph from the polygon and the polylines
            var graph = Elements.Spatial.HalfEdgeGraph2d.Construct(new[] { thisInXY }, polylines.Select(p => p.TransformedPolyline(inverse)));
            // Find closed regions in that graph
            return graph.Polygonize().Select(p => p.TransformedPolygon(plXform)).ToList();
        }

        /// <summary>
        /// Insert a point into the polygon if it lies along one
        /// of the polyline's segments.
        /// </summary>
        /// <param name="point">The point at which to split the polygon.</param>
        /// <returns>The index of the new vertex.</returns>
        public override int Split(Vector3 point)
        {
            return Split(point, true);
        }

        /// <summary>
        /// Trim the polygon with a collection of polygons.
        /// </summary>
        /// <param name="polygons">A collection of trimming polygons.</param>
        /// <returns>A new polygon trimmed by the specified polygons.</returns>
        public Line[] TrimmedTo(IList<Polygon> polygons)
        {
            var finalVertices = new List<Vector3>();
            var splitPoly = new Polygon(this.Vertices);
            var extraSegments = new List<Line>();
            foreach (var p in polygons)
            {
                if (this.Intersects3d(p, out List<Vector3> result))
                {
                    foreach (var split in result)
                    {
                        splitPoly.Split(split);
                    }

                    for (var j = 0; j < result.Count - 1; j += 2)
                    {
                        var l = new Line(result[j], result[j + 1]);
                        extraSegments.Add(l);
                    }
                }
            }


            return splitPoly.Segments().Concat(extraSegments).ToArray();
        }

        /// <summary>
        /// Constructs the geometric difference between two sets of polygons.
        /// </summary>
        /// <param name="firstSet">First set of polygons</param>
        /// <param name="secondSet">Second set of polygons</param>
        /// <param name="tolerance">An optional tolerance.</param>
        /// <returns>Returns a list of Polygons representing the subtraction of the second set of polygons from the first set.</returns>
        public static IList<Polygon> Difference(IList<Polygon> firstSet, IList<Polygon> secondSet, double tolerance = Vector3.EPSILON)
        {
            return BooleanTwoSets(firstSet, secondSet, BooleanMode.Difference, VoidTreatment.PreserveInternalVoids, tolerance);
        }

        /// <summary>
        /// Constructs the geometric union of two sets of polygons.
        /// </summary>
        /// <param name="firstSet">First set of polygons</param>
        /// <param name="secondSet">Second set of polygons</param>
        /// <param name="tolerance">An optional tolerance.</param>
        /// <returns>Returns a list of Polygons representing the union of both sets of polygons.</returns>
        [Obsolete("Please use UnionAll, which takes a single list of polygons.")]
        public static IList<Polygon> Union(IList<Polygon> firstSet, IList<Polygon> secondSet, double tolerance = Vector3.EPSILON)
        {
            return BooleanTwoSets(firstSet, secondSet, BooleanMode.Union, VoidTreatment.IgnoreInternalVoids, tolerance);
        }

        /// <summary>
        /// Constructs the geometric union of a set of polygons.
        /// </summary>
        /// <param name="polygons">The polygons to union</param>
        /// <param name="tolerance">An optional tolerance.</param>
        /// <returns>Returns a list of Polygons representing the union of all polygons.</returns>
        public static IList<Polygon> UnionAll(IList<Polygon> polygons, double tolerance = Vector3.EPSILON)
        {
            return BooleanTwoSets(polygons, new List<Polygon>(), BooleanMode.Union, VoidTreatment.IgnoreInternalVoids, tolerance);
        }

        /// <summary>
        /// Returns Polygons representing the symmetric difference between two sets of polygons.
        /// </summary>
        /// <param name="firstSet">First set of polygons</param>
        /// <param name="secondSet">Second set of polygons</param>
        /// <param name="tolerance">An optional tolerance.</param>
        /// <returns>
        /// Returns a list of Polygons representing the symmetric difference of these two sets of polygons.
        /// Returns a representation of all polygons if they do not intersect.
        /// </returns>
        public static IList<Polygon> XOR(IList<Polygon> firstSet, IList<Polygon> secondSet, double tolerance = Vector3.EPSILON)
        {
            return BooleanTwoSets(firstSet, secondSet, BooleanMode.XOr, VoidTreatment.PreserveInternalVoids, tolerance);
        }

        /// <summary>
        /// Constructs the Polygon intersections between two sets of polygons.
        /// </summary>
        /// <param name="firstSet">First set of polygons</param>
        /// <param name="secondSet">Second set of polygons</param>
        /// <param name="tolerance">An optional tolerance.</param>
        /// <returns>
        /// Returns a list of Polygons representing the intersection of the first set of Polygons with the second set.
        /// Returns null if the Polygons do not intersect.
        /// </returns>
        public static IList<Polygon> Intersection(IList<Polygon> firstSet, IList<Polygon> secondSet, double tolerance = Vector3.EPSILON)
        {
            return BooleanTwoSets(firstSet, secondSet, BooleanMode.Intersection, VoidTreatment.PreserveInternalVoids, tolerance);
        }


        /// <summary>
        /// Apply a boolean operation (Union, Difference, Intersection, or XOr) to two lists of Polygons.
        /// </summary>
        /// <param name="subjectPolygons">Polygons to clip</param>
        /// <param name="clippingPolygons">Polygons with which to clip</param>
        /// <param name="booleanMode">The operation to apply: Union, Difference, Intersection, or XOr</param>
        /// <param name="voidTreatment">Optional setting for how to process the polygons in each set: either treat polygons inside others as voids, or treat them all as solid (thereby ignoring internal polygons).</param>
        /// <param name="tolerance">Optional override of the tolerance for determining if two polygons are identical.</param>
        private static IList<Polygon> BooleanTwoSets(IList<Polygon> subjectPolygons, IList<Polygon> clippingPolygons, BooleanMode booleanMode, VoidTreatment voidTreatment = VoidTreatment.PreserveInternalVoids, double tolerance = Vector3.EPSILON)
        {
            var subjectPaths = subjectPolygons.Select(s => s.ToClipperPath(tolerance)).ToList();
            var clipPaths = clippingPolygons.Select(s => s.ToClipperPath(tolerance)).ToList();
            Clipper clipper = new Clipper();
            clipper.AddPaths(subjectPaths, PolyType.ptSubject, true);
            clipper.AddPaths(clipPaths, PolyType.ptClip, true);
            var solution = new List<List<IntPoint>>();
            var executionMode = ClipType.ctDifference;
            var polyFillType = PolyFillType.pftEvenOdd;
            if (voidTreatment == VoidTreatment.IgnoreInternalVoids)
            {
                polyFillType = PolyFillType.pftNonZero;
            }
            switch (booleanMode)
            {
                case BooleanMode.Difference:
                    executionMode = ClipType.ctDifference;
                    break;
                case BooleanMode.Union:
                    executionMode = ClipType.ctUnion;
                    break;
                case BooleanMode.Intersection:
                    executionMode = ClipType.ctIntersection;
                    break;
                case BooleanMode.XOr:
                    executionMode = ClipType.ctXor;
                    break;
            }
            clipper.Execute(executionMode, solution, polyFillType);
            if (solution.Count == 0)
            {
                return null;
            }
            var polygons = new List<Polygon>();
            foreach (List<IntPoint> path in solution)
            {
                polygons.Add(PolygonExtensions.ToPolygon(path, tolerance));
            }
            return polygons;
        }

        /// <summary>
        /// Constructs the geometric difference between this Polygon and the supplied Polygon.
        /// </summary>
        /// <param name="polygon">The intersecting Polygon.</param>
        /// <param name="tolerance">An optional tolerance value.</param>
        /// <returns>
        /// Returns a list of Polygons representing the subtraction of the supplied Polygon from this Polygon.
        /// Returns null if the area of this Polygon is entirely subtracted.
        /// Returns a list containing a representation of the perimeter of this Polygon if the two Polygons do not intersect.
        /// </returns>
        public IList<Polygon> Difference(Polygon polygon, double tolerance = Vector3.EPSILON)
        {
            var thisPath = this.ToClipperPath(tolerance);
            var polyPath = polygon.ToClipperPath(tolerance);
            Clipper clipper = new Clipper();
            clipper.AddPath(thisPath, PolyType.ptSubject, true);
            clipper.AddPath(polyPath, PolyType.ptClip, true);
            var solution = new List<List<IntPoint>>();
            clipper.Execute(ClipType.ctDifference, solution, PolyFillType.pftNonZero);
            if (solution.Count == 0)
            {
                return null;
            }
            var polygons = new List<Polygon>();
            foreach (List<IntPoint> path in solution)
            {
                polygons.Add(PolygonExtensions.ToPolygon(path, tolerance));
            }
            return polygons;
        }

        /// <summary>
        /// Constructs the geometric difference between this Polygon and the supplied Polygons.
        /// </summary>
        /// <param name="difPolys">The list of intersecting Polygons.</param>
        /// <param name="tolerance">An optional tolerance value.</param>
        /// <returns>
        /// Returns a list of Polygons representing the subtraction of the supplied Polygons from this Polygon.
        /// Returns null if the area of this Polygon is entirely subtracted.
        /// Returns a list containing a representation of the perimeter of this Polygon if the two Polygons do not intersect.
        /// </returns>
        public IList<Polygon> Difference(IList<Polygon> difPolys, double tolerance = Vector3.EPSILON)
        {
            var thisPath = this.ToClipperPath(tolerance);
            var polyPaths = new List<List<IntPoint>>();
            foreach (Polygon polygon in difPolys)
            {
                polyPaths.Add(polygon.ToClipperPath(tolerance));
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
                try
                {
                    polygons.Add(PolygonExtensions.ToPolygon(path.Distinct().ToList(), tolerance));
                }
                catch
                {
                    // swallow invalid polygons
                }
            }
            return polygons;
        }

        /// <summary>
        /// Constructs the Polygon intersections between this Polygon and the supplied Polygon.
        /// </summary>
        /// <param name="polygon">The intersecting Polygon.</param>
        /// <param name="tolerance">An optional tolerance.</param>
        /// <returns>
        /// Returns a list of Polygons representing the intersection of this Polygon with the supplied Polygon.
        /// Returns null if the two Polygons do not intersect.
        /// </returns>
        public IList<Polygon> Intersection(Polygon polygon, double tolerance = Vector3.EPSILON)
        {
            var thisPath = this.ToClipperPath(tolerance);
            var polyPath = polygon.ToClipperPath(tolerance);
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
                polygons.Add(PolygonExtensions.ToPolygon(path, tolerance));
            }
            return polygons;
        }

        /// <summary>
        /// Constructs the geometric union between this Polygon and the supplied Polygon.
        /// </summary>
        /// <param name="polygon">The Polygon to be combined with this Polygon.</param>
        /// <param name="tolerance">An optional tolerance.</param>
        /// <returns>
        /// Returns a single Polygon from a successful union.
        /// Returns null if a union cannot be performed on the two Polygons.
        /// </returns>
        public Polygon Union(Polygon polygon, double tolerance = Vector3.EPSILON)
        {
            var thisPath = this.ToClipperPath(tolerance);
            var polyPath = polygon.ToClipperPath(tolerance);
            Clipper clipper = new Clipper();
            clipper.AddPath(thisPath, PolyType.ptSubject, true);
            clipper.AddPath(polyPath, PolyType.ptClip, true);
            var solution = new List<List<IntPoint>>();
            clipper.Execute(ClipType.ctUnion, solution);
            if (solution.Count > 1)
            {
                return null;
            }
            return solution.First().ToPolygon(tolerance);
        }

        /// <summary>
        /// Constructs the geometric union between this Polygon and the supplied list of Polygons.
        /// </summary>
        /// <param name="polygons">The list of Polygons to be combined with this Polygon.</param>
        /// <param name="tolerance">An optional tolerance.</param>
        /// <returns>
        /// Returns a single Polygon from a successful union.
        /// Returns null if a union cannot be performed on the complete list of Polygons.
        /// </returns>
        public Polygon Union(IList<Polygon> polygons, double tolerance = Vector3.EPSILON)
        {
            var thisPath = this.ToClipperPath(tolerance);
            var polyPaths = new List<List<IntPoint>>();
            foreach (Polygon polygon in polygons)
            {
                polyPaths.Add(polygon.ToClipperPath(tolerance));
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
            return solution.First().Distinct().ToList().ToPolygon(tolerance);
        }

        /// <summary>
        /// Returns Polygons representing the symmetric difference between this Polygon and the supplied Polygon.
        /// </summary>
        /// <param name="polygon">The intersecting polygon.</param>
        /// <param name="tolerance">An optional tolerance.</param>
        /// <returns>
        /// Returns a list of Polygons representing the symmetric difference of this Polygon and the supplied Polygon.
        /// Returns a representation of this Polygon and the supplied Polygon if the Polygons do not intersect.
        /// </returns>
        public IList<Polygon> XOR(Polygon polygon, double tolerance = Vector3.EPSILON)
        {
            var thisPath = this.ToClipperPath(tolerance);
            var polyPath = polygon.ToClipperPath(tolerance);
            Clipper clipper = new Clipper();
            clipper.AddPath(thisPath, PolyType.ptSubject, true);
            clipper.AddPath(polyPath, PolyType.ptClip, true);
            var solution = new List<List<IntPoint>>();
            clipper.Execute(ClipType.ctUnion, solution);
            var polygons = new List<Polygon>();
            foreach (List<IntPoint> path in solution)
            {
                polygons.Add(PolygonExtensions.ToPolygon(path, tolerance));
            }
            return polygons;
        }

        /// <summary>
        /// Offset this polygon by the specified amount.
        /// </summary>
        /// <param name="offset">The amount to offset.</param>
        /// <param name="endType">The type of closure used for the offset polygon.</param>
        /// <param name="tolerance">An optional tolerance.</param>
        /// <returns>A new Polygon offset by offset.</returns>
        ///
        public override Polygon[] Offset(double offset, EndType endType = EndType.ClosedPolygon, double tolerance = Vector3.EPSILON)
        {
            return base.Offset(offset, endType, tolerance);
        }


        /// <summary>
        /// Get a collection a lines representing each segment of this polyline.
        /// </summary>
        /// <returns>A collection of Lines.</returns>
        public override Line[] Segments()
        {
            return SegmentsInternal(this.Vertices);
        }

        internal new static Line[] SegmentsInternal(IList<Vector3> vertices)
        {
            var lines = new Line[vertices.Count];
            for (var i = 0; i < vertices.Count; i++)
            {
                var a = vertices[i];
                var b = i == vertices.Count - 1 ? vertices[0] : vertices[i + 1];
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
            return new Polygon(this.Vertices.Reverse().ToArray());
        }

        /// <summary>
        /// Is this polygon equal to the provided polygon?
        /// </summary>
        /// <param name="obj"></param>
        public override bool Equals(object obj)
        {
            var p = obj as Polygon;
            if (p == null)
            {
                return false;
            }
            if (this.Vertices.Count != p.Vertices.Count)
            {
                return false;
            }

            for (var i = 0; i < this.Vertices.Count; i++)
            {
                if (!this.Vertices[i].Equals(p.Vertices[i]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Test if this polygon has the same vertex count and shape as another, within tolerance.
        /// </summary>
        /// <param name="other">The other polygon.</param>
        /// <param name="tolerance">The optional tolerance value to use. If not supplied, the global tolerance will be used.</param>
        /// <param name="ignoreWinding">If true, polygons with opposite winding will be considered as equal.</param>
        /// <returns></returns>
        public bool IsAlmostEqualTo(Polygon other, double tolerance = Vector3.EPSILON, bool ignoreWinding = false)
        {
            var otherVertices = other.Vertices;
            if (otherVertices.Count != Vertices.Count) return false;
            if (ignoreWinding && other.Normal().Dot(Normal()) < 0)
            {
                //ensure winding is consistent
                otherVertices = other.Vertices.Reverse().ToList();
            }

            var firstVertex = Vertices[0];
            var distance = double.MaxValue;
            //find index of closest vertex to this 0 vertex
            int indexOffset = -1;
            for (int i = 0; i < otherVertices.Count; i++)
            {
                Vector3 otherVertex = otherVertices[i];
                var thisDistance = firstVertex.DistanceTo(otherVertex);
                if (thisDistance < distance)
                {
                    distance = thisDistance;
                    indexOffset = i;
                }
            }

            // rounding errors could occur in X and Y, so the max distance tolerance is the linear tolerance * sqrt(2).
            var distanceTolerance = Math.Sqrt(2) * tolerance;

            if (distance > distanceTolerance) return false;

            for (int i = 0; i < Vertices.Count; i++)
            {
                var thisVertex = Vertices[i];
                var otherVertex = otherVertices[(i + indexOffset) % otherVertices.Count];
                if (thisVertex.DistanceTo(otherVertex) > distanceTolerance)
                {
                    return false;
                }
            }

            return true;

        }

        /// <summary>
        /// Find the minimum-area rotated rectangle containing a set of points,
        /// calculated without regard for Z coordinate.
        /// </summary>
        /// <param name="points">The points to contain within the rectangle</param>
        /// <returns>A rectangular polygon that contains all input points</returns>
        public static Polygon FromAlignedBoundingBox2d(IEnumerable<Vector3> points)
        {
            var hull = ConvexHull.FromPoints(points);
            var minBoxArea = double.MaxValue;
            BBox3 minBox = new BBox3();
            Transform minBoxXform = new Transform();
            foreach (var edge in hull.Segments())
            {
                var edgeVector = edge.End - edge.Start;
                var xform = new Transform(Vector3.Origin, edgeVector, Vector3.ZAxis, 0);
                var invertedXform = new Transform(xform);
                invertedXform.Invert();
                var transformedPolygon = hull.TransformedPolygon(invertedXform);
                var bbox = new BBox3(transformedPolygon.Vertices);
                var bboxArea = (bbox.Max.X - bbox.Min.X) * (bbox.Max.Y - bbox.Min.Y);
                if (bboxArea < minBoxArea)
                {
                    minBoxArea = bboxArea;
                    minBox = bbox;
                    minBoxXform = xform;
                }
            }
            var xy = new Plane(Vector3.Origin, Vector3.ZAxis);
            var boxRect = Polygon.Rectangle(minBox.Min.Project(xy), minBox.Max.Project(xy));
            return boxRect.TransformedPolygon(minBoxXform);
        }

        /// <summary>
        /// Find a point that is guaranteed to be internal to the polygon.
        /// </summary>
        public Vector3 PointInternal()
        {
            var centroid = Centroid();
            if (Contains(centroid))
            {
                return centroid;
            }
            int currentIndex = 0;
            while (true)
            {
                if (currentIndex == Vertices.Count)
                {
                    return centroid;
                }
                // find midpoint of the diagonal between two non-adjacent vertices.
                // At any convex corner, this will be inside the boundary
                // (unless it passes all the way through to the other side — but
                // this can't be true for all corners). Inspired by
                // http://apodeline.free.fr/FAQ/CGAFAQ/CGAFAQ-3.html 3.6
                var a = Vertices[currentIndex];
                var b = Vertices[(currentIndex + 2) % Vertices.Count];
                var candidate = (a + b) * 0.5;
                if (Contains(candidate))
                {
                    return candidate;
                }
                currentIndex++;
            }
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
            var projected = new Vector3[this.Vertices.Count];
            for (var i = 0; i < projected.Length; i++)
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
            var projected = new Vector3[this.Vertices.Count];
            for (var i = 0; i < this.Vertices.Count; i++)
            {
                projected[i] = this.Vertices[i].ProjectAlong(direction, p);
            }
            return new Polygon(projected);
        }

        /// <summary>
        /// Remove collinear points from this Polygon.
        /// </summary>
        /// <returns>New Polygon without collinear points.</returns>
        public Polygon CollinearPointsRemoved()
        {
            int count = this.Vertices.Count;
            var unique = new List<Vector3>(count);

            if (!Vector3.AreCollinear(Vertices[count - 1], Vertices[0], Vertices[1]))
                unique.Add(Vertices[0]);

            for (int i = 1; i < count - 1; i++)
            {
                if (!Vector3.AreCollinear(Vertices[i - 1], Vertices[i], Vertices[i + 1]))
                    unique.Add(Vertices[i]);
            }

            if (!Vector3.AreCollinear(Vertices[count - 2], Vertices[count - 1], Vertices[0]))
                unique.Add(Vertices[count - 1]);

            return new Polygon(unique);
        }

        /// <summary>
        /// Get the transforms used to transform a Profile extruded along this Polyline.
        /// </summary>
        /// <param name="startSetback"></param>
        /// <param name="endSetback"></param>
        public override Transform[] Frames(double startSetback, double endSetback)
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
                result[i] = CreateMiterTransform(i, a, up);
            }
            return result;
        }

        /// <summary>
        /// The string representation of the Polygon.
        /// </summary>
        /// <returns>A string containing the string representations of this Polygon's vertices.</returns>
        public override string ToString()
        {
            return string.Join(", ", this.Vertices.Select(v => v.ToString()));
        }

        /// <summary>
        /// Calculate the length of the polygon.
        /// </summary>
        public override double Length()
        {
            var length = 0.0;
            for (var i = 0; i < this.Vertices.Count; i++)
            {
                var next = i == this.Vertices.Count - 1 ? 0 : i + 1;
                length += this.Vertices[i].DistanceTo(this.Vertices[next]);
            }
            return length;
        }

        /// <summary>
        /// Calculate the centroid of the polygon.
        /// </summary>
        public Vector3 Centroid()
        {
            var x = 0.0;
            var y = 0.0;
            var z = 0.0;
            foreach (var pnt in Vertices)
            {
                x += pnt.X;
                y += pnt.Y;
                z += pnt.Z;
            }
            return new Vector3(x / Vertices.Count, y / Vertices.Count, z / Vertices.Count);
        }

        /// <summary>
        /// Calculate the polygon's area.
        /// </summary>
        public double Area()
        {
            var area = 0.0;
            for (var i = 0; i <= this.Vertices.Count - 1; i++)
            {
                var j = (i + 1) % this.Vertices.Count;
                area += this.Vertices[i].X * this.Vertices[j].Y;
                area -= this.Vertices[i].Y * this.Vertices[j].X;
            }
            return area / 2.0;
        }

        /// <summary>
        /// Transform this polygon in place.
        /// </summary>
        /// <param name="t">The transform.</param>
        public void Transform(Transform t)
        {
            for (var i = 0; i < this.Vertices.Count; i++)
            {
                this.Vertices[i] = t.OfPoint(this.Vertices[i]);
            }
        }

        /// <summary>
        /// Transform a specified segment of this polygon in place.
        /// </summary>
        /// <param name="t">The transform. If it is not within the polygon plane, then an exception will be thrown.</param>
        /// <param name="i">The segment to transform. If it does not exist, then no work will be done.</param>
        public void TransformSegment(Transform t, int i)
        {
            this.TransformSegment(t, i, true, true);
        }

        /// <summary>
        /// Fillet all corners on this polygon.
        /// </summary>
        /// <param name="radius">The fillet radius.</param>
        /// <returns>A contour containing trimmed edge segments and fillets.</returns>
        public Contour Fillet(double radius)
        {
            var curves = new List<Curve>();
            Vector3 contourStart = new Vector3();
            Vector3 contourEnd = new Vector3();

            var segs = this.Segments();
            for (var i = 0; i < segs.Length; i++)
            {
                var a = segs[i];
                var b = i == segs.Length - 1 ? segs[0] : segs[i + 1];
                var fillet = a.Fillet(b, radius);

                var right = a.Direction().Cross(Vector3.ZAxis);
                var dot = b.Direction().Dot(right);
                var convex = dot <= 0.0;
                if (i > 0)
                {
                    var l = new Line(contourEnd, convex ? fillet.Start : fillet.End);
                    curves.Add(l);
                }
                else
                {
                    contourStart = convex ? fillet.Start : fillet.End;
                }
                contourEnd = convex ? fillet.End : fillet.Start;
                curves.Add(fillet);
            }
            curves.Add(new Line(contourEnd, contourStart));
            return new Contour(curves);
        }

        /// <summary>
        /// Get a point on the polygon at parameter u.
        /// </summary>
        /// <param name="u">A value between 0.0 and 1.0.</param>
        /// <param name="segmentIndex">The index of the segment containing parameter u.</param>
        /// <returns>Returns a Vector3 indicating a point along the Polygon length from its start vertex.</returns>
        protected override Vector3 PointAtInternal(double u, out int segmentIndex)
        {
            if (u < 0.0 || u > 1.0)
            {
                throw new Exception($"The value of u ({u}) must be between 0.0 and 1.0.");
            }

            var d = this.Length() * u;
            var totalLength = 0.0;
            for (var i = 0; i < this.Vertices.Count; i++)
            {
                var a = this.Vertices[i];
                var b = i == this.Vertices.Count - 1 ? this.Vertices[0] : this.Vertices[i + 1];
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
        /// The normal of this polygon, according to Newell's Method.
        /// </summary>
        /// <returns>The unitized sum of the cross products of each pair of edges.</returns>
        public Vector3 Normal()
        {
            var normal = new Vector3();
            for (int i = 0; i < Vertices.Count; i++)
            {
                var p0 = Vertices[i];
                var p1 = Vertices[(i + 1) % Vertices.Count];
                normal.X += (p0.Y - p1.Y) * (p0.Z + p1.Z);
                normal.Y += (p0.Z - p1.Z) * (p0.X + p1.X);
                normal.Z += (p0.X - p1.X) * (p0.Y + p1.Y);
            }
            return normal.Unitized();
        }
        /// <summary>
        /// Get the normal of each vertex on the polygon.
        /// </summary>
        /// <remarks>All normals will be the same since polygons are coplanar by definition.</remarks>
        /// <returns>A collection of unit vectors, each corresponding to a single vertex.</returns>
        protected override Vector3[] NormalsAtVertices()
        {
            // Create an array of transforms with the same number of items as the vertices.
            var result = new Vector3[this.Vertices.Count];

            // Since polygons must be coplanar, all vertex normals can match the polygon's normal.
            var normal = this.Normal();
            for (int i = 0; i < Vertices.Count; i++)
            {
                result[i] = normal;
            }
            return result;
        }

        /// <summary>
        /// A list of vertices describing the arc for rendering.
        /// </summary>
        internal override IList<Vector3> RenderVertices()
        {
            var verts = new List<Vector3>(this.Vertices);
            verts.Add(this.Start);
            return verts;
        }
    }

    /// <summary>
    /// Mode to apply a boolean operation
    /// </summary>
    public enum BooleanMode
    {
        /// <summary>
        /// A and not B
        /// </summary>
        Difference,
        /// <summary>
        /// A or B
        /// </summary>
        Union,
        /// <summary>
        /// A and B
        /// </summary>
        Intersection,
        /// <summary>
        /// Exclusive or — either A or B but not both.
        /// </summary>
        XOr
    }


    /// <summary>
    /// Controls the handling of internal regions in a polygon boolean operation.
    /// </summary>
    public enum VoidTreatment
    {
        /// <summary>
        /// Use an Even/Odd fill pattern to decide whether internal polygons are solid or void.
        /// This corresponds to Clipper's "EvenOdd" PolyFillType.
        /// </summary>
        PreserveInternalVoids = 0,
        /// <summary>
        /// Treat all contained or overlapping polygons as solid.
        /// This corresponds to Clipper's "Positive" PolyFillType.
        /// </summary>
        IgnoreInternalVoids = 1
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
        /// <param name="tolerance">Optional tolerance value. If converting back to a polygon after the operation, be sure to use the same tolerance value.</param>
        /// <returns></returns>
        internal static List<IntPoint> ToClipperPath(this Polygon p, double tolerance = Vector3.EPSILON)
        {
            var scale = Math.Round(1.0 / tolerance);
            var path = new List<IntPoint>();
            foreach (var v in p.Vertices)
            {
                path.Add(new IntPoint(Math.Round(v.X * scale), Math.Round(v.Y * scale)));
            }
            return path;
        }

        /// <summary>
        /// Construct a Polygon from a clipper path
        /// </summary>
        /// <param name="p"></param>
        /// <param name="tolerance">Optional tolerance value. Be sure to use the same tolerance value as you used when converting to Clipper path.</param>
        /// <returns></returns>
        internal static Polygon ToPolygon(this List<IntPoint> p, double tolerance = Vector3.EPSILON)
        {
            var scale = Math.Round(1.0 / tolerance);
            var converted = new Vector3[p.Count];
            for (var i = 0; i < converted.Length; i++)
            {
                var v = p[i];
                converted[i] = new Vector3(v.X / scale, v.Y / scale);
            }
            try
            {
                return new Polygon(converted);
            }
            catch
            {
                // Often, the polygons coming back from clipper will have self-intersections, in the form of lines that go out and back.
                // here we make a last-ditch attempt to fix this and construct a new polygon.
                var cleanedVertices = Vector3.AttemptPostClipperCleanup(converted);
                try
                {
                    return new Polygon(cleanedVertices);
                }
                catch
                {
                    throw new Exception("Unable to clean up bad polygon resulting from a polygon boolean operation.");
                }
            }
        }

        public static IList<Polygon> Reversed(this IList<Polygon> polygons)
        {
            return polygons.Select(p => p.Reversed()).ToArray();
        }

        internal static ContourVertex[] ToContourVertexArray(this Polygon poly)
        {
            var contour = new List<ContourVertex>();
            foreach (var vert in poly.Vertices)
            {
                var cv = new ContourVertex();
                cv.Position = new Vec3 { X = vert.X, Y = vert.Y, Z = vert.Z };
                contour.Add(cv);
            }
            return contour.ToArray();
        }
    }
}
