using ClipperLib;
using Elements.Search;
using Elements.Spatial;
using LibTessDotNet.Double;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Elements.Geometry
{
    /// <summary>
    /// A closed planar polygon.
    /// Parameterization of the curve is 0->length.
    /// </summary>
    /// <example>
    /// [!code-csharp[Main](../../Elements/test/PolygonTests.cs?name=example)]
    /// </example>
    public partial class Polygon : Polyline
    {
        /// <summary>
        /// A plane created once during the polygon's construction.
        /// This will not be updated when a polygon's vertices are changed.
        /// </summary>
        internal Plane _plane;

        /// <summary>
        /// Should the curve be considered closed for rendering?
        /// </summary>
        public override bool IsClosedForRendering => true;

        /// <summary>
        /// Construct a polygon.
        /// </summary>
        /// <param name="vertices">A collection of vertex locations.</param>
        [JsonConstructor]
        public Polygon(IList<Vector3> @vertices) : base(vertices)
        {
            _plane = Plane();
        }

        /// <summary>
        /// Construct a polygon.
        /// </summary>
        /// <param name="vertices">A collection of vertex locations.</param>
        /// <param name="disableValidation">Should self-intersection testing be disabled?</param>
        public Polygon(IList<Vector3> @vertices, bool disableValidation = false) : base(vertices, disableValidation)
        {
            _plane = Plane();
        }

        /// <summary>
        /// Validate that this Polygon's vertices are coplanar, clean up any
        /// duplicate vertices, and fix any overlapping edges.
        /// </summary>
        protected override void ValidateVertices()
        {
            if (!Vertices.AreCoplanar())
            {
                throw new ArgumentException("The polygon could not be created. The provided vertices are not coplanar.");
            }

            this.Vertices = Vector3.RemoveSequentialDuplicates(this.Vertices, true);
            DeleteVerticesForOverlappingEdges();
            if (this.Vertices.Count < 3)
            {
                throw new ArgumentException("The polygon could not be created. At least 3 vertices are required.");
            }

            CheckSegmentLengthAndThrow(Edges());
            var t = Vertices.ToTransform();
            CheckSelfIntersectionAndThrow(t, Edges());
        }

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
        /// Construct a polygon from points. This is a convenience constructor
        /// that can be used like this: `new Polygon((0,0,0), (10,0,0), (10,10,0))`
        /// </summary>
        /// <param name="vertices">The vertices of the polygon.</param>
        public Polygon(params Vector3[] vertices) : this(new List<Vector3>(vertices)) { }

        /// <summary>
        /// Construct a polygon from points.
        /// </summary>
        /// <param name="disableValidation">Should self-intersection testing be disabled?</param>
        /// <param name="vertices">The vertices of the polygon.</param>
        public Polygon(bool disableValidation, params Vector3[] vertices) : this(new List<Vector3>(vertices), disableValidation) { }

        /// <summary>
        /// Construct a transformed copy of this Polygon.
        /// </summary>
        /// <param name="transform">The transform to apply.</param>
        public Polygon TransformedPolygon(Transform transform)
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
            var p = new Polygon(transformed);
            return p;
        }

        /// <summary>
        /// Construct a transformed copy of this Curve.
        /// </summary>
        /// <param name="transform">The transform to apply.</param>
        public override Curve Transformed(Transform transform)
        {
            if (transform == null)
            {
                return this;
            }

            return TransformedPolygon(transform);
        }

        /// <summary>
        /// Construct a transform in the plane of this polygon, with its Z axis along the polygon's normal.
        /// </summary>
        /// <returns></returns>
        public Transform ToTransform()
        {
            return new Transform(Vertices[0], Vertices[1] - Vertices[0], this.Normal());
        }

        /// <summary>
        /// Tests if the supplied Vector3 is within this Polygon in 3D without coincidence with an edge or vertex when compared on a shared plane.
        /// </summary>
        /// <param name="point">The point to compare to this polygon.</param>
        /// <returns>
        /// Returns true if the supplied point is within this polygon when compared on a shared plane.
        /// </returns>
        public bool Contains(Vector3 point)
        {
            Contains(point, out Containment containment);
            return containment == Containment.Inside;
        }

        /// <summary>
        /// Tests if the supplied point is within this polygon in 3D, using a 2D method.
        /// </summary>
        /// <param name="point">The point to compare to this polygon.</param>
        /// <param name="containment">Whether the point is inside, outside, at an edge, or at a vertex.</param>
        /// <returns>
        /// Returns true if the supplied point is within this polygon when compared on a shared plane.
        /// </returns>
        public bool Contains(Vector3 point, out Containment containment)
        {
            return Contains3D(point, out containment);
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

                if (!this.Intersects(plane, out List<Vector3> intersections, true))
                {
                    return null;
                }

                var split = this;
                if (intersections.Count > 0)
                {
                    split = new Polygon(this.Vertices);
                    split.Split(intersections);
                }
                else
                {
                    // A polygon with one intersection will not be trimmed;
                    return null;
                }

                var newVertices = new List<Vector3>();
                for (var i = 0; i <= split.Vertices.Count - 1; i++)
                {
                    var v1 = split.Vertices[i];
                    var v2 = i == split.Vertices.Count - 1 ? split.Vertices[0] : split.Vertices[i + 1];

                    var d1 = v1.DistanceTo(plane);
                    var d2 = v2.DistanceTo(plane);
                    if (d1.ApproximatelyEquals(0, precision))
                    {
                        d1 = 0.0;
                    }
                    if (d2.ApproximatelyEquals(0, precision))
                    {
                        d2 = 0.0;
                    }

                    if (d1 == 0.0 && d2 == 0.0)
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

                    if (d1 < 0 && d2 == 0.0)
                    {
                        // The first point is outside and
                        // the second point is on the plane.
                        continue;
                    }

                    if (d1 > 0 && d2 > 0)
                    {
                        // Both points are on the inside of
                        // the plane.
                        newVertices.Add(v1);
                        continue;
                    }

                    if (d1 > 0 && d2 == 0.0)
                    {
                        // The first point is inside and
                        // the second point is on the plane.
                        newVertices.Add(v1);

                        // Insert what will become a duplicate
                        // vertex.
                        newVertices.Add(v2);
                        continue;
                    }

                    if (d1 == 0.0 && d2 > 0)
                    {
                        // The first point is on the plane,
                        // and the second is inside.
                        newVertices.Add(v1);
                        continue;
                    }

                    if (d1 < 0 && d2 > 0)
                    {
                        // The first point is inside,
                        // and the second point is outside.
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
                }

                var graph = new HalfEdgeGraph2d
                {
                    EdgesPerVertex = new List<List<(int from, int to, int? tag)>>()
                };

                if (newVertices.Count == 0)
                {
                    return null;
                }

                graph.Vertices = newVertices;

                // Initialize the graph.
                foreach (var v in newVertices)
                {
                    graph.EdgesPerVertex.Add(new List<(int from, int to, int? tag)>());
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
                    graph.EdgesPerVertex[a].Add((a, b, null));
                }

                for (var i = 0; i < intersections.Count - 1; i++)
                {
                    // Because we'll have duplicate vertices where an
                    // intersection is on the plane, we need to choose
                    // which one to use. This follows the rule of finding
                    // the one whose index is closer to the first index used.
                    var a = ClosestIndexOf(newVertices, intersections[i], i);
                    var b = ClosestIndexOf(newVertices, intersections[i + 1], a);

                    if (!Contains(newVertices[a].Average(newVertices[b]), out _))
                    {
                        continue;
                    }
                    graph.EdgesPerVertex[a].Add((a, b, null));
                }

                if (graph.EdgesPerVertex[newVertices.Count - 1].Count == 0)
                {
                    // Close the graph
                    var a = newVertices.Count - 1;
                    var b = 0;
                    graph.EdgesPerVertex[a].Add((a, b, null));
                }
                return graph.Polygonize();

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);
            }
            return null;
        }

        /// <summary>
        /// Compute the plane of the Polygon.
        /// </summary>
        /// <returns>A Plane.</returns>
        public override Plane Plane()
        {
            return new Plane(this.Vertices[0], this.Normal());
        }

        /// <summary>
        /// Intersect this polygon with the provided polygon in 2d.
        /// </summary>
        /// <param name="polygon">The target polygon.</param>
        /// <param name="results">The points resulting from the intersection of
        /// the two polygons.</param>
        /// <param name="includeEnds">Should intersection with segment ends be included?</param>
        /// <returns>True if this polygon intersects the provided polygon,
        /// otherwise false.</returns>
        internal bool Intersects2d(Polygon polygon,
                                   out List<(Vector3 result, int aSegmentIndices, int bSegmentIndices)> results,
                                   bool includeEnds = false)
        {
            var aSegs = this.Segments();
            var bSegs = polygon.Segments();

            results = new List<(Vector3, int, int)>();

            for (var i = 0; i < aSegs.Length; i++)
            {
                var a = aSegs[i];
                for (var j = 0; j < bSegs.Length; j++)
                {
                    var b = bSegs[j];
                    if (a.Intersects(b, out Vector3 result, includeEnds: includeEnds))
                    {
                        results.Add((result, i, j));
                    }
                }
            }
            return results.Count > 0;
        }

        /// <summary>
        /// Intersect this polygon with the provided polygon in 3d.
        /// Unlike other methods that do 2d intersection, this method is able to
        /// calculate intersections in 3d by doing planar intersections and
        /// 3D containment checks. If you are looking for 2d intersection, use the
        /// Polygon.Intersects(Plane plane, ...) method.
        /// </summary>
        /// <param name="polygon">The target polygon.</param>
        /// <param name="intersections">The points resulting from the intersection
        /// of the two polygons.</param>
        /// <param name="sort">Should the resulting intersections be sorted along
        /// the plane?</param>
        /// <returns>True if this polygon intersect the provided polygon, otherwise false.
        /// The result collection may have duplicate vertices where intersection
        /// with a vertex occurs as there is one intersection associated with each
        /// edge attached to the vertex.</returns>
        internal bool Intersects3d(Polygon polygon, out List<Vector3> intersections, bool sort = true)
        {
            var p = this._plane;
            intersections = new List<Vector3>();
            var targetP = polygon._plane;

            if (p.IsCoplanar(targetP))
            {
                return false;
            }

            var d = this.Normal().Cross(targetP.Normal).Unitized();

            // Intersect the polygon against this polygon's plane.
            // Keep the points that lie within the polygon.
            if (polygon.Intersects(p, out List<Vector3> results, sort: false))
            {
                foreach (var r in results)
                {
                    if (this.Contains(r, out _))
                    {
                        if (!intersections.Contains(r))
                        {
                            intersections.Add(r);
                        }
                    }
                }
            }

            // Intersect this polygon against the target polygon's plane.
            // Keep the points within the target polygon.
            if (this.Intersects(targetP, out List<Vector3> results2, sort: false))
            {
                foreach (var r in results2)
                {
                    if (polygon.Contains(r, out _))
                    {
                        if (!intersections.Contains(r))
                        {
                            intersections.Add(r);
                        }
                    }
                }
            }

            if (sort)
            {
                intersections.Sort(new DirectionComparer(d));
            }

            return intersections.Count > 0;
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
        /// <param name="sort">Should the intersection results be sorted
        /// along the plane?</param>
        /// <returns>True if the plane intersects the polygon,
        /// otherwise false.</returns>
        public bool Intersects(Plane plane, out List<Vector3> results, bool distinct = true, bool sort = true)
        {
            results = new List<Vector3>();
            var d = this.Normal().Cross(plane.Normal).Unitized();

            foreach (var (from, to) in this.Edges())
            {
                if (Line.Intersects(plane, from, to, out Vector3 result))
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

            if (sort)
            {
                // Order the intersections along the direction.
                results.Sort(new DirectionComparer(d));
            }

            return results.Count > 0;
        }

        // Projects non-flat containment request into XY plane and returns the answer for this projection
        internal bool Contains3D(Vector3 location, out Containment containment)
        {
            // Test that the test point is in the same plane
            // as the polygon.
            var transformTo3D = Vertices.ToTransform();
            if (!location.DistanceTo(transformTo3D.XY()).ApproximatelyEquals(0))
            {
                containment = Containment.Outside;
                return false;
            }

            var is3D = Vertices.Any(vertex => vertex.Z != 0);
            if (!is3D)
            {
                return Contains(Edges(), location, out containment);
            }

            var transformToGround = new Transform(transformTo3D);
            transformToGround.Invert();
            var groundSegments = Edges(transformToGround);
            var groundLocation = transformToGround.OfPoint(location);
            return Contains(groundSegments, groundLocation, out containment);
        }

        internal bool Contains3D(Polygon polygon)
        {
            return polygon.Vertices.All(v => this.Contains(v, out _));
        }

        // Adapted from https://stackoverflow.com/questions/46144205/point-in-polygon-using-winding-number/46144206
        internal static bool Contains(IEnumerable<(Vector3 from, Vector3 to)> edges, Vector3 location, out Containment containment)
        {
            int windingNumber = 0;

            foreach (var edge in edges)
            {
                // check for coincidence with edge vertices
                var toStart = location - edge.from;
                if (toStart.IsZero())
                {
                    containment = Containment.CoincidesAtVertex;
                    return true;
                }
                var toEnd = location - edge.to;
                if (toEnd.IsZero())
                {
                    containment = Containment.CoincidesAtVertex;
                    return true;
                }
                //along segment - check if perpendicular distance to segment is below tolerance and that point is between ends
                var a = toStart.Length();
                var b = toStart.Dot((edge.to - edge.from).Unitized());
                if (a * a - b * b < Vector3.EPSILON * Vector3.EPSILON && toStart.Dot(toEnd) < 0)
                {
                    containment = Containment.CoincidesAtEdge;
                    return true;
                }

                if (AscendingRelativeTo(location, edge) &&
                    LocationInRange(location, Orientation.Ascending, edge))
                {
                    windingNumber += Wind(location, edge, Position.Left);
                }
                if (!AscendingRelativeTo(location, edge) &&
                    LocationInRange(location, Orientation.Descending, edge))
                {
                    windingNumber -= Wind(location, edge, Position.Right);
                }
            }

            var result = windingNumber != 0;
            containment = result ? Containment.Inside : Containment.Outside;
            return result;
        }

        #region WindingNumberCalcs
        private static int Wind(Vector3 location, (Vector3 from, Vector3 to) edge, Position position)
        {
            return RelativePositionOnEdge(location, edge) != position ? 0 : 1;
        }

        private static Position RelativePositionOnEdge(Vector3 location, (Vector3 from, Vector3 to) edge)
        {
            double positionCalculation =
                (edge.to.Y - edge.from.Y) * (location.X - edge.from.X) -
                (location.Y - edge.from.Y) * (edge.to.X - edge.from.X);

            if (positionCalculation > 0)
            {
                return Position.Left;
            }

            if (positionCalculation < 0)
            {
                return Position.Right;
            }

            return Position.Center;
        }

        private static bool AscendingRelativeTo(Vector3 location, (Vector3 from, Vector3 to) edge)
        {
            return edge.from.X <= location.X;
        }

        private static bool LocationInRange(Vector3 location, Orientation orientation, (Vector3 from, Vector3 to) edge)
        {
            if (orientation == Orientation.Ascending)
            {
                return edge.to.X > location.X;
            }

            return edge.to.X <= location.X;
        }

        private enum Position
        {
            Left,
            Right,
            Center
        }

        private enum Orientation
        {
            Ascending,
            Descending
        }
        #endregion


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
        /// Tests if the supplied Vector3 is within this Polygon or coincident with an edge when compared on the XY plane.
        /// </summary>
        /// <param name="vector">The Vector3 to compare to this Polygon.</param>
        /// <returns>
        /// Returns true if the supplied Vector3 is within this Polygon or coincident with an edge when compared in the XY shared plane. Returns false if the supplied Vector3 is outside this Polygon, or if the supplied Vector3 is null.
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
        /// Tests if the supplied polygon is within this Polygon or coincident with an edge.
        /// </summary>
        /// <param name="polygon">The polygon we want to know is inside this polygon.</param>
        /// <param name="containment">The containment status.</param>
        /// <returns>Returns false if any part of the polygon is entirely outside of this polygon.</returns>
        public bool Contains3D(Polygon polygon, out Containment containment)
        {
            containment = Containment.Inside;
            foreach (var v in polygon.Vertices)
            {
                Contains3D(v, out var foundContainment);
                if (foundContainment == Containment.Outside)
                {
                    return false;
                }
                if (foundContainment > containment)
                {
                    containment = foundContainment;
                }
            }
            return true;
        }

        /// <summary>
        /// Tests if the supplied Polygon is within this Polygon with or without edge coincident vertices when compared on a shared plane.
        /// </summary>
        /// <param name="polygon">The Polygon to compare to this Polygon.</param>
        /// <returns>
        /// Returns true if every edge of the provided polygon is on or within this polygon when compared on a shared plane. Returns false if any edge of the supplied Polygon is outside this Polygon, or if the supplied Polygon is null.
        /// </returns>
        public bool Covers(Polygon polygon)
        {
            if (polygon == null)
            {
                return false;
            }

            // If an edge crosses without being fully overlapping, the polygon is only partially covered.
            foreach (var edge1 in Edges())
            {
                foreach (var edge2 in polygon.Edges())
                {
                    var direction1 = Line.Direction(edge1.from, edge1.to);
                    var direction2 = Line.Direction(edge2.from, edge2.to);
                    if (Line.Intersects2d(edge1.from, edge1.to, edge2.from, edge2.to) &&
                        !direction1.IsParallelTo(direction2) &&
                        !direction1.IsParallelTo(direction2.Negate()))
                    {
                        return false;
                    }
                }
            }

            var allInside = true;
            foreach (var vertex in polygon.Vertices)
            {
                Contains(Edges(), vertex, out Containment containment);
                if (containment == Containment.Outside)
                {
                    return false;
                }
                if (containment != Containment.Inside)
                {
                    allInside = false;
                }
            }

            // If all vertices of the polygon are inside this polygon then there is full coverage since no edges cross.
            if (allInside)
            {
                return true;
            }

            // If some edges are partially shared (!allInside) then we must still make sure that none of this.Vertices are inside the given polygon.
            // The above two checks aren't sufficient in cases like two almost identical polygons, but with an extra vertex on an edge of this polygon that's pulled into the other polygon.
            foreach (var vertex in Vertices)
            {
                Contains(polygon.Edges(), vertex, out Containment containment);
                if (containment == Containment.Inside)
                {
                    return false;
                }
            }

            return true;
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
        /// Split this polygon with one or more open polylines.
        /// </summary>
        /// <param name="polylines">The polylines with which to split.</param>
        public List<Polygon> Split(params Polyline[] polylines)
        {
            return Split(polylines.ToList());
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
        /// <param name="points">The points at which to split the polygon.</param>
        /// <returns>The index of the new vertex.</returns>
        public override void Split(IList<Vector3> points)
        {
            Split(points, true);
        }

        /// <summary>
        /// Trim the polygon with a collection of polygons that intersect it
        /// in 3d. Portions of the intersected polygon on the "outside"
        /// (normal-facing side) of the trimming polygons will remain.
        /// Portions inside the trimming polygons will be discarded.
        /// </summary>
        /// <param name="polygons">The trimming polygons.</param>
        /// <param name="inOut">A classification indicating which trim area should be returned.</param>
        /// <returns>A collection of polygons resulting from the trim or null if no trim occurred.</returns>
        public List<Polygon> TrimmedTo(IList<Polygon> polygons, LocalClassification inOut = LocalClassification.Outside)
        {
            var polys = this.IntersectAndClassify(polygons,
                                                  polygons,
                                                  out _,
                                                  out _);
            return polys.Where(p =>
            {
                switch (inOut)
                {
                    case LocalClassification.Outside:
                        return p.Item2 == SetClassification.AOutsideB;
                    case LocalClassification.Inside:
                        return p.Item2 == SetClassification.AInsideB;
                    default:
                        return false;
                }
            }).Select(p => p.Item1).ToList();
        }

        /// <summary>
        /// Trim the polygon with a collection of polygons that intersect it
        /// in 3d. Portions of the intersected polygon on the "outside"
        /// (normal-facing side) of the trimming polygons will remain.
        /// Portions inside the trimming polygons will be discarded.
        /// </summary>
        /// <param name="polygons">The trimming polygons.</param>
        /// <param name="intersections">A collection of intersection locations.</param>
        /// <param name="trimEdges">A collection of vertex pairs representing all edges in the timming graph.</param>
        /// <param name="inOut">A classification indicating which trim area should be returned.</param>
        /// <returns>A collection of polygons resulting from the trim or null if no trim occurred.</returns>
        public List<Polygon> TrimmedTo(IList<Polygon> polygons,
                                       out List<Vector3> intersections,
                                       out List<(Vector3 from, Vector3 to, int? index)> trimEdges,
                                       LocalClassification inOut = LocalClassification.Outside)
        {
            var polys = this.IntersectAndClassify(polygons,
                                                  polygons,
                                                  out intersections,
                                                  out trimEdges);
            return polys.Where(p =>
            {
                switch (inOut)
                {
                    case LocalClassification.Outside:
                        return p.Item2 == SetClassification.AOutsideB;
                    case LocalClassification.Inside:
                        return p.Item2 == SetClassification.AInsideB;
                    default:
                        return false;
                }
            }).Select(p => p.Item1).ToList();
        }

        internal List<Polygon> IntersectOneToMany(IList<Polygon> polygons,
                                                 out List<Vector3> intersections,
                                                 out List<(Vector3 from, Vector3 to, int? index)> trimEdges)
        {
            var localPlane = _plane;
            var graphVertices = new List<Vector3>();
            var edges = new List<List<(int from, int to, int? index)>>();

            var splitPoly = new Polygon(this.Vertices);
            var results = new List<Vector3>[polygons.Count];
            var planes = new List<Plane>();

            foreach (var polygon in polygons)
            {
                planes.Add(polygon._plane);
            }

            for (var i = 0; i < polygons.Count; i++)
            {
                var polygon = polygons[i];

                // Add a results collection for each polygon.
                // This may or may not have results in it after processing.
                results[i] = new List<Vector3>();

                if (localPlane.IsCoplanar(planes[i]))
                {
                    if (this.Intersects2d(polygon, out List<(Vector3 result, int aSegumentIndices, int bSegmentIndices)> planarIntersectionResults, false))
                    {
                        // Split the polygon so that we have extra vertices,
                        // but don't add results. We don't want to insert edges
                        // because boolean operations on the coplanar faces will
                        // happen later.
                        var result = planarIntersectionResults.Select(r => r.result).ToList();
                        splitPoly.Split(result);
                    }
                }
                else
                {
                    // We intersect but don't sort, because the three-plane check
                    // below may result in new intersections which will need to
                    // be sorted as well.
                    if (this.Intersects3d(polygon, out List<Vector3> result, false))
                    {
                        splitPoly.Split(result);
                        results[i].AddRange(result);
                    }

                    // Intersect this plane with all other planes.
                    // This handles the case where two trim polygons intersect
                    // or meet at a T.
                    for (var j = 0; j < polygons.Count; j++)
                    {
                        if (i == j)
                        {
                            continue;
                        }
                        var inner = polygons[j];

                        // Do a three plane intersection amongst all the planes.
                        if (localPlane.Intersects(planes[i], planes[j], out Vector3 xsect))
                        {
                            // Test containment in the current splitting polygon.
                            if (polygon.Contains(xsect)
                                && inner.Contains(xsect)
                                && this.Contains(xsect))
                            {
                                if (!results[i].Contains(xsect))
                                {
                                    results[i].Add(xsect);
                                }
                            }
                        }
                    }
                }
            }

            // Sort all the intersection results across their planes
            // so the lacing works across all polys.
            for (var i = 0; i < polygons.Count; i++)
            {
                var polygon = polygons[i];
                var d = localPlane.Normal.Cross(planes[i].Normal).Unitized();
                if (results[i].Count > 0)
                {
                    results[i].Sort(new DirectionComparer(d));
                }
            }

            for (var i = 0; i < splitPoly.Vertices.Count; i++)
            {
                var a = i;
                var b = i == splitPoly.Vertices.Count - 1 ? 0 : i + 1;
                graphVertices.Add(splitPoly.Vertices[i]);
                edges.Add(new List<(int from, int to, int? index)>());
                edges[i].Add((a, b, -1));
            }

            for (var i = 0; i < results.Count(); i++)
            {
                var result = results[i];
                for (var j = 0; j < result.Count - 1; j += 1)
                {
                    // Don't create zero-length edges.
                    if (result[j].IsAlmostEqualTo(result[j + 1]))
                    {
                        continue;
                    }

                    // We look for the intersection result in the graph vertices
                    // because this collection will now contain the splitpoly
                    // vertices AND the non split vertices which are contained
                    // in the polygon itself.
                    var a = graphVertices.IndexOf(result[j]);
                    if (a == -1)
                    {
                        // An intersection that does not happen on the original poly
                        graphVertices.Add(result[j]);
                        a = graphVertices.Count - 1;
                        edges.Add(new List<(int from, int to, int? index)>());
                    }
                    var b = graphVertices.IndexOf(result[j + 1]);
                    if (b == -1)
                    {
                        graphVertices.Add(result[j + 1]);
                        b = graphVertices.Count - 1;
                        edges.Add(new List<(int from, int to, int? index)>());
                    }
                    edges[a].Add((a, b, i));
                    edges[b].Add((b, a, i));
                }
            }

            var heg = new HalfEdgeGraph2d()
            {
                Vertices = graphVertices,
                EdgesPerVertex = edges
            };

            trimEdges = new List<(Vector3 from, Vector3 to, int? index)>();
            foreach (var edgeSet in edges)
            {
                foreach (var (from, to, index) in edgeSet)
                {
                    var a = graphVertices[from];
                    var b = graphVertices[to];
                    if (!a.IsAlmostEqualTo(b))
                    {
                        // Don't return duplicate edges
                        // TODO: The half edge graph should not require these
                        // duplicate edges. Can we remove them there?
                        if (!trimEdges.Contains((a, b, index)) && !trimEdges.Contains((b, a, index)))
                        {
                            trimEdges.Add((a, b, index));
                        }
                    }
                }
            }

            var polys = heg.Polygonize(null, localPlane.Normal);

            // TODO: Can we skip this culling step? This is required because
            // we make two-way edged graphs which generate polygons on top of
            // each other but facing in opposite directions. We really just need
            // a single set of polygons which are facing in the same direction.
            for (var i = polys.Count - 1; i >= 0; i--)
            {
                for (var j = polys.Count - 1; j >= 0; j--)
                {
                    if (i == j)
                    {
                        continue;
                    }

                    if (!polys[i].Normal().IsAlmostEqualTo(localPlane.Normal))
                    {
                        polys.Remove(polys[i]);
                        break;
                    }
                }
            }

            intersections = results.SelectMany(t => t).ToList();

            return polys;
        }

        /// <summary>
        /// Intersect a polygon against a set of trim polygons and identify the
        /// resulting polygons as "inside" or "outside" of the set of trimming
        /// polygons. Containment is done using edge direction comparison for
        /// polygons which intersect with their trims, or ray testing in the
        /// case of polygons which do not intersect with their trims.
        /// </summary>
        /// <param name="trimPolygons">A collection of polygons to trim against.</param>
        /// <param name="rayTestPolygons">A collection of polygons to ray trace against.</param>
        /// <param name="outsideClassification">The outside classification.</param>
        /// <param name="insideClassification">The inside classification.</param>
        /// <param name="intersections">A collection of intersection locations.</param>
        /// <param name="trimEdges">A collection of trim edge data.</param>
        /// <returns>A collection of polygons and their local classification.</returns>
        internal List<(Polygon, SetClassification, CoplanarSetClassification)> IntersectAndClassify(IList<Polygon> trimPolygons,
                                                                                                    IList<Polygon> rayTestPolygons,
                                                                                                    out List<Vector3> intersections,
                                                                                                    out List<(Vector3 from, Vector3 to, int? parentPolygonIndex)> trimEdges,
                                                                                                    SetClassification outsideClassification = SetClassification.AOutsideB,
                                                                                                    SetClassification insideClassification = SetClassification.AInsideB)
        {
            var classifications = new List<(Polygon, SetClassification, CoplanarSetClassification)>();

            if (trimPolygons.Count == 0)
            {
                // Quick out if no trimming polygons are supplied.
                var singleClassification = ClassifyByRayTest(this, rayTestPolygons, outsideClassification, insideClassification);
                classifications.Add((this, singleClassification, CoplanarSetClassification.None));
                intersections = null;
                trimEdges = null;
                return classifications;
            }

            var splitFaces = this.IntersectOneToMany(trimPolygons, out intersections, out trimEdges);

            if (splitFaces.Count == 1)
            {
                // If there's only one face, we ray cast to test
                // for inclusion. This happens in a couple of scenarios:
                // 1. A polygon is completely inside the trim polygons and wasn't trimmed.
                // 2. A polygon is completely outside the trim polygons and wasn't trimmed.
                var singleClassification = ClassifyByRayTest(this, trimPolygons, outsideClassification, insideClassification);
                classifications.Add((splitFaces[0], singleClassification, CoplanarSetClassification.None));
            }
            else
            {
                // TODO: This doesn't work right with disjoint polygons. It will
                // return a -1 parent polygon index for the "outer" polygon.
                // var compareEdges = trimEdges;
                var compareEdges = trimEdges.Where(e => e.parentPolygonIndex != -1).ToList();

                var n = _plane.Normal;

                foreach (var splitFace in splitFaces)
                {
                    var splitFaceEdges = splitFace.Edges();
                    var inside = 0;
                    var outside = 0;

                    // Every edge needs to be checked against its trimming
                    // poly to ensure that a face is "inside" or "outside" all
                    // of the polys that trim the polygon.
                    foreach (var (from, to) in splitFaceEdges)
                    {
                        // Find the matching trim edge.
                        // TODO: Find a way to organize this data so that we
                        // don't have to do a comparison.
                        var compareEdge = compareEdges.FirstOrDefault(e => (e.from == from && e.to == to) || (e.from == to && e.to == from));

                        // In the case where an edge is an edge of the original
                        // polygon, and isn't trimmed by another polygon, there
                        // will be no matching edge.
                        if (compareEdge == default((Vector3, Vector3, int?)))
                        {
                            continue;
                        }

                        var trimPolyIndex = compareEdge.parentPolygonIndex.Value;

                        // During intersection of one to many, this polygon's
                        // edges are added and given the index -1.
                        var trimPoly = trimPolyIndex == -1 ? this : trimPolygons[trimPolyIndex];
                        var bn = trimPoly._plane.Normal;
                        var d = (from - to).Unitized();
                        var dot = bn.Dot(n.Cross(d));
                        if (dot <= 0.0)
                        {
                            outside++;
                        }
                        else
                        {
                            inside++;
                        }
                    }

                    if (outside > 0 && inside == 0)
                    {
                        classifications.Add((splitFace, outsideClassification, CoplanarSetClassification.None));
                    }
                    else if (inside > 0 && outside == 0)
                    {
                        classifications.Add((splitFace, insideClassification, CoplanarSetClassification.None));
                    }
                    else if (inside == 0 && outside == 0)
                    {
                        // This will happen with disjoint polygons.
                        classifications.Add((this, outsideClassification, CoplanarSetClassification.None));
                    }
                }
            }

            return classifications;
        }

        private static SetClassification ClassifyByRayTest(Polygon p,
                                                           IList<Polygon> trimPolygons,
                                                           SetClassification outsideClassification,
                                                           SetClassification insideClassification)
        {
            var intersectionCount = 0;
            var ray = new Ray(p.Vertices[0], p._plane.Normal);
            foreach (var trimPoly in trimPolygons)
            {
                if (ray.Intersects(trimPoly, out _, out _))
                {
                    intersectionCount++;
                }
            }
            return intersectionCount % 2 == 0 ? outsideClassification : insideClassification;
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
        /// Constructs the geometric union of a set of polygons, using default
        /// tolerance.
        /// </summary>
        /// <param name="polygons">The polygons to union</param>
        /// <returns>Returns a list of Polygons representing the union of all polygons.</returns>
        public static IList<Polygon> UnionAll(params Polygon[] polygons)
        {
            return BooleanTwoSets(polygons, new List<Polygon>(), BooleanMode.Union, VoidTreatment.IgnoreInternalVoids, Vector3.EPSILON);
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
                var result = PolygonExtensions.ToPolygon(path, tolerance);
                if (result != null)
                {
                    polygons.Add(result);
                }
            }
            return polygons;
        }

        /// <summary>
        /// Constructs the geometric difference between this Polygon and one or
        /// more supplied Polygons, using the default tolerance.
        /// </summary>
        /// <param name="polygons">The intersecting Polygons.</param>
        /// <returns>
        /// Returns a list of Polygons representing the subtraction of the supplied Polygons from this Polygon.
        /// Returns null if the area of this Polygon is entirely subtracted.
        /// Returns a list containing a representation of the perimeter of this Polygon if the Polygons do not intersect.
        /// </returns>
        public IList<Polygon> Difference(params Polygon[] polygons)
        {
            return Difference(polygons, Vector3.EPSILON);
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
                var result = PolygonExtensions.ToPolygon(path, tolerance);
                if (result != null)
                {
                    polygons.Add(result);
                }
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
                var result = PolygonExtensions.ToPolygon(path, tolerance);
                if (result != null)
                {
                    polygons.Add(result);
                }
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
        /// Constructs the geometric union between this Polygon and one or more
        /// supplied polygons, using the default tolerance.
        /// </summary>
        /// <param name="polygons">The Polygons to be combined with this Polygon.</param>
        /// <returns>
        /// Returns a single Polygon from a successful union.
        /// Returns null if a union cannot be performed on the complete list of Polygons.
        /// </returns>
        public Polygon Union(params Polygon[] polygons)
        {
            return Union(polygons, Vector3.EPSILON);
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
            if (ignoreWinding && other.Normal().Dot(this.Normal()) < 0)
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
        /// Find the rectangle along axis containing a set of points,
        /// calculated without regard for Z coordinate,
        /// located at the height of points minimum Z coordinate.
        /// </summary>
        /// <param name="points">The points to contain within the rectangle.</param>
        /// <param name="axis">The axis along which the rectangle is built. Must be a non-zero vector.</param>
        /// <param name="minSideSize">The minimum size of a side of a polygon when all points lie on the same line and polygon cannot be constructed. Must be greater than 0.</param>
        /// <returns></returns>
        public static Polygon FromAlignedBoundingBox2d(IEnumerable<Vector3> points, Vector3 axis, double minSideSize = 0.1)
        {
            if (minSideSize < Vector3.EPSILON)
            {
                throw new ArgumentOutOfRangeException(nameof(minSideSize), "Must be greater than 0.");
            }

            if (axis.IsZero())
            {
                throw new ArgumentException("Axis must be a non-zero vector.", nameof(axis));
            }
            var transform = new Transform(Vector3.Origin, axis, Vector3.ZAxis);
            var box = new Box(points, transform);
            var xOffset = 0.0;
            var length = box.Bounds.Max.X - box.Bounds.Min.X;
            var yOffset = 0.0;
            var width = box.Bounds.Max.Y - box.Bounds.Min.Y;
            if (length.ApproximatelyEquals(0))
            {
                length = minSideSize / 2;
                xOffset = minSideSize / 2;
            }
            if (width.ApproximatelyEquals(0))
            {
                width = minSideSize / 2;
                yOffset = minSideSize / 2;
            }
            var boundary = Rectangle(new Vector3(box.Bounds.Min.X - xOffset, box.Bounds.Min.Y - yOffset),
                                     new Vector3(box.Bounds.Min.X + length, box.Bounds.Min.Y + width))
                          .TransformedPolygon(transform.Moved(new Vector3(0, 0, box.Bounds.Min.Z)));
            return boundary;
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
        /// Project this polygon onto the plane.
        /// </summary>
        /// <param name="plane">The plane of the returned polygon.</param>
        public new Polygon Project(Plane plane)
        {
            var projected = new Vector3[this.Vertices.Count];
            for (var i = 0; i < projected.Length; i++)
            {
                projected[i] = this.Vertices[i].Project(plane);
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
        /// Trim vertices from a polygon that lie near a given curve.
        /// </summary>
        /// <param name="curve">The curve used to trim the polygon</param>
        /// <param name="tolerance">Optional tolerance value.</param>
        /// <param name="removed">The vertices that were removed.</param>
        public Polygon RemoveVerticesNearCurve(Curve curve, out List<Vector3> removed, double tolerance = Vector3.EPSILON)
        {
            var newVertices = new List<Vector3>(this.Vertices.Count);
            removed = new List<Vector3>(this.Vertices.Count);
            foreach (var v in Vertices)
            {
                switch (curve)
                {
                    case Polygon polygon:
                        var d = v.DistanceTo(polygon, out _);
                        var covers = polygon.Contains(v);
                        if (d > tolerance && !covers)
                        {
                            newVertices.Add(v);
                        }
                        else
                        {
                            removed.Add(v);
                        }
                        break;
                    case Polyline polyline:
                        if (v.DistanceTo(polyline, out _) > tolerance)
                        {
                            newVertices.Add(v);
                        }
                        else
                        {
                            removed.Add(v);
                        }
                        break;
                    case Line line:
                        if (v.DistanceTo(line, out _) > tolerance)
                        {
                            newVertices.Add(v);
                        }
                        else
                        {
                            removed.Add(v);
                        }
                        break;
                    default:
                        throw new ArgumentException("Unknown curve type for removing vertices.  Only Polygon, Polyline, and Line are supported.");
                }
            }
            return new Polygon(newVertices);
        }

        /// <summary>
        /// Remove collinear points from this Polygon.
        /// </summary>
        /// <returns>New Polygon without collinear points.</returns>
        public Polygon CollinearPointsRemoved(double tolerance = Vector3.EPSILON)
        {
            int count = this.Vertices.Count;
            var unique = new List<Vector3>(count);

            if (!Vector3.AreCollinearByDistance(Vertices[count - 1], Vertices[0], Vertices[1], tolerance))
                unique.Add(Vertices[0]);

            for (int i = 1; i < count - 1; i++)
            {
                if (!Vector3.AreCollinearByDistance(Vertices[i - 1], Vertices[i], Vertices[i + 1], tolerance))
                    unique.Add(Vertices[i]);
            }

            if (!Vector3.AreCollinearByDistance(Vertices[count - 2], Vertices[count - 1], Vertices[0], tolerance))
            {
                unique.Add(Vertices[count - 1]);
            }
            if (unique.Count < 3)
            {
                return this;
            }
            return new Polygon(unique);
        }

        /// <summary>
        /// Get the transforms used to transform a Profile extruded along this Polyline.
        /// </summary>
        /// <param name="startSetback"></param>
        /// <param name="endSetback"></param>
        /// <param name="additionalRotation"></param>
        public override Transform[] Frames(double startSetback = 0.0,
                                           double endSetback = 0.0,
                                           double additionalRotation = 0.0)
        {
            // Create an array of transforms with the same
            // number of items as the vertices.
            var result = new Transform[this.Vertices.Count];

            // Cache the normal so we don't have to recalculate
            // using Newell for every frame.
            var up = this.Normal();
            for (var i = 0; i < result.Length; i++)
            {
                var a = this.Vertices[i];
                result[i] = CreateMiterTransform(i, a, up);
                if (additionalRotation != 0.0)
                {
                    result[i].RotateAboutPoint(result[i].Origin, result[i].ZAxis, additionalRotation);
                }
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
            var normal = Normal();
            var pgon = this;
            Transform transform = null;
            if (Math.Abs(normal.Dot(Vector3.ZAxis)) < 1 - Vector3.EPSILON)
            {
                transform = new Transform(Vertices[0], normal);
                var inverse = transform.Inverted();
                pgon = TransformedPolygon(inverse);
            }
            double area = 0;
            var (x, y) = (0.0, 0.0);
            var vertices = pgon.Vertices;
            for (int i = 0; i < vertices.Count; i++)
            {
                int j = (i + 1) % vertices.Count;
                double crossProduct = vertices[i].X * vertices[j].Y - vertices[j].X * vertices[i].Y;

                area += crossProduct;
                x += (vertices[i].X + vertices[j].X) * crossProduct;
                y += (vertices[i].Y + vertices[j].Y) * crossProduct;
            }

            area *= 0.5;
            x /= (6 * area);
            y /= (6 * area);
            return transform == null ? new Vector3(x, y, vertices[0].Z) : transform.OfPoint(new Vector3(x, y, 0));
        }

        /// <summary>
        /// Calculate the center of the polygon as the average of vertices.
        /// </summary>
        /// <returns></returns>
        public Vector3 Center()
        {
            var center = Vector3.Origin;
            foreach (var v in this.Vertices)
            {
                center += v;
            }
            return center / this.Vertices.Count;
        }

        /// <summary>
        /// Calculate the polygon's signed area in 3D.
        /// </summary>
        public double Area()
        {
            var vertices = this.Vertices;
            var normal = Normal();
            if (!(normal.IsAlmostEqualTo(Vector3.ZAxis) ||
                  normal.Negate().IsAlmostEqualTo(Vector3.ZAxis)
                 ))
            {
                var t = new Transform(Vector3.Origin, normal).Inverted();
                var transformedPolygon = this.TransformedPolygon(t);
                vertices = transformedPolygon.Vertices;
            }
            var area = 0.0;
            for (var i = 0; i <= vertices.Count - 1; i++)
            {
                var j = (i + 1) % vertices.Count;
                area += vertices[i].X * vertices[j].Y;
                area -= vertices[i].Y * vertices[j].X;
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
            var curves = new List<BoundedCurve>();
            var segments = this.Segments();
            var arcs = new List<Arc>();

            for (var i = 0; i < segments.Length; i++)
            {
                var a = segments[i];
                var b = segments[i == segments.Length - 1 ? 0 : i + 1];
                var arc = a.Fillet(b, 0.5);
                arcs.Add(arc);
            }

            for (var i = 0; i < arcs.Count; i++)
            {
                var a = arcs[i];
                var b = arcs[i == arcs.Count - 1 ? 0 : i + 1];
                curves.Add(new Line(a.Start, b.End));
                curves.Add(b);
            }

            return new Contour(curves);
        }

        /// <summary>
        /// Get a point on the polygon at parameter u.
        /// </summary>
        /// <param name="u">A value between 0.0 and length.</param>
        /// <param name="segmentIndex">The index of the segment containing parameter u.</param>
        /// <returns>Returns a Vector3 indicating a point along the Polygon length from its start vertex.</returns>
        protected override Vector3 PointAtInternal(double u, out int segmentIndex)
        {
            if (!Domain.Includes(u, true))
            {
                throw new Exception($"The parameter {u} is not on the trimmed portion of the basis curve. The parameter must be between {Domain.Min} and {Domain.Max}.");
            }

            var totalLength = 0.0;
            for (var i = 0; i < this.Vertices.Count; i++)
            {
                var a = this.Vertices[i];
                var b = i == this.Vertices.Count - 1 ? this.Vertices[0] : this.Vertices[i + 1];
                var currLength = a.DistanceTo(b);
                var currVec = (b - a).Unitized();
                if (totalLength <= u && totalLength + currLength >= u)
                {
                    segmentIndex = i;
                    return a + currVec * (u - totalLength);
                }
                totalLength += currLength;
            }
            segmentIndex = this.Vertices.Count - 1;
            return this.End;
        }

        // TODO: Investigate converting Polyline to IEnumerable<(Vector3, Vector3)>
        internal override IEnumerable<(Vector3 from, Vector3 to)> Edges(Transform transform = null)
        {
            for (var i = 0; i < this.Vertices.Count; i++)
            {
                var from = this.Vertices[i];
                var to = i == this.Vertices.Count - 1 ? this.Vertices[0] : this.Vertices[i + 1];
                yield return transform != null ? (transform.OfPoint(from), transform.OfPoint(to)) : (from, to);
            }
        }

        /// <summary>
        /// The normal of this polygon, according to Newell's Method.
        /// </summary>
        /// <returns>The unitized sum of the cross products of each pair of edges.</returns>
        public Vector3 Normal()
        {
            return this.Vertices.NormalFromPlanarWoundPoints();
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
        /// Deletes Vertices that are out on overloping Edges
        /// D__________C
        ///  |         |
        ///  |         |
        /// E|_________|B_____A
        /// Vertex A will be deleted
        /// </summary>
        private void DeleteVerticesForOverlappingEdges()
        {
            if (Vertices.Count < 4)
            {
                return;
            }

            for (var i = 0; i < Vertices.Count; i++)
            {
                var a = Vertices[i];
                var b = Vertices[(i + 1) % Vertices.Count];
                var c = Vertices[(i + 2) % Vertices.Count];
                bool invalid = (a - b).Unitized().Dot((b - c).Unitized()) < (Vector3.EPSILON - 1);
                if (invalid)
                {
                    Vertices.Remove(b);
                    i--;

                    if (a.IsAlmostEqualTo(c))
                    {
                        Vertices.Remove(c);
                    }
                }
            }
        }

        /// <summary>
        /// A Polygon can't have self intersections, but points can still lay on other lines.
        /// This leads to hidden voids embedded in the perimeter.
        /// This function checks if any points are on another line of the loop and splits into distinct loops if found.
        /// </summary>
        /// <returns>List of simple polygons</returns>
        internal List<Polygon> SplitInternalLoops()
        {
            List<List<Vector3>> polygonPresets = new List<List<Vector3>>();

            //Store accumulated vertices and lines between them.
            List<Vector3> loopVertices = new List<Vector3>();
            List<Line> openLoop = new List<Line>();

            //Check if a point lay on active open loop lines.
            foreach (var v in Vertices)
            {
                bool intersectionFound = false;
                for (int i = 0; i < openLoop.Count; i++)
                {
                    if (openLoop[i].PointOnLine(v) && v.DistanceTo(openLoop[i]) < Vector3.EPSILON)
                    {
                        //Remove points and lines from intersection points to this.
                        var vertices = loopVertices.Skip(i + 1).ToList();
                        loopVertices.RemoveRange(i + 1, vertices.Count);
                        openLoop.RemoveRange(i + 1, vertices.Count - 1);
                        //Cut intersected line and add this point to open loop.
                        loopVertices.Add(v);
                        openLoop[i] = new Line(openLoop[i].Start, v);

                        //Loop can possibly be just two points connected forth and back.
                        //Filter it early.
                        vertices.Add(v);
                        if (vertices.Count > 2)
                        {
                            polygonPresets.Add(vertices);
                        }
                        intersectionFound = true;
                        break;
                    }
                }

                //Then check if line (this plus last points) intersects with any accumulated points (going backward)
                if (!intersectionFound)
                {
                    Line segment = loopVertices.Any() ? new Line(loopVertices.Last(), v) : null;
                    for (int i = loopVertices.Count - 1; i >= 0; i--)
                    {
                        //Last point is already part of the line.
                        if (i == loopVertices.Count)
                        {
                            continue;
                        }

                        if (segment.PointOnLine(loopVertices[i]) && loopVertices[i].DistanceTo(segment) < Vector3.EPSILON)
                        {
                            var vertices = loopVertices.Skip(i).ToList();
                            segment = new Line(loopVertices[i], segment.End);

                            loopVertices.RemoveRange(i + 1, vertices.Count - 1);
                            openLoop.RemoveRange(i, vertices.Count - 1);

                            if (vertices.Count > 2)
                            {
                                polygonPresets.Add(vertices);
                            }
                        }
                    }

                    //If no intersection found just add point and line to open loop.
                    loopVertices.Add(v);
                    if (segment != null)
                    {
                        openLoop.Add(segment);
                    }
                }
            }

            //Leftover points form last loop if it has enough points.
            if (loopVertices.Count > 2)
            {
                polygonPresets.Add(loopVertices);
            }

            List<Polygon> polygons = new List<Polygon>();
            foreach (var preset in polygonPresets)
            {
                try
                {
                    //Polygon constructor cleanup removes any excess vertices and segments.
                    //This can lead to, again, having too little vertices for valid polygon.
                    var loop = new Polygon(preset);
                    polygons.Add(loop);
                }
                catch
                {
                    //Just ignore polygons that failed check due to having less than 3 points.
                }
            }
            return polygons;
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
                if (cleanedVertices.Count < 3)
                {
                    return null;
                }
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

        internal static ContourVertex[] ToContourVertexArray(this Polyline poly)
        {
            var contour = new ContourVertex[poly.Vertices.Count];
            for (var i = 0; i < poly.Vertices.Count; i++)
            {
                var vert = poly.Vertices[i];
                var cv = new ContourVertex();
                cv.Position = new Vec3 { X = vert.X, Y = vert.Y, Z = vert.Z };
                contour[i] = cv;
            }
            return contour;
        }
    }
}
