using System;
using System.Collections.Generic;
using System.Linq;

namespace Elements.Geometry
{
    /// <summary>
    /// A linear curve between two points.
    /// </summary>
    /// <example>
    /// [!code-csharp[Main](../../Elements/test/LineTests.cs?name=example)]
    /// </example>
    public partial class Line : Curve, IEquatable<Line>
    {
        /// <summary>
        /// Calculate the length of the line.
        /// </summary>
        public override double Length()
        {
            return this.Start.DistanceTo(this.End);
        }

        /// <summary>
        /// Create a line of one unit length along the X axis.
        /// </summary>
        public Line()
        {
            this.Start = Vector3.Origin;
            this.End = new Vector3(1, 0, 0);
        }

        /// <summary>
        /// Construct a line of length from a start along direction.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="direction"></param>
        /// <param name="length"></param>
        public Line(Vector3 start, Vector3 direction, double length)
        {
            this.Start = start;
            this.End = start + direction.Unitized() * length;
        }

        /// <summary>
        /// Get a transform whose XY plane is perpendicular to the curve, and whose
        /// positive Z axis points along the curve.
        /// </summary>
        /// <param name="u">The parameter along the Line, between 0.0 and 1.0, at which to calculate the Transform.</param>
        /// <returns>A transform.</returns>
        public override Transform TransformAt(double u)
        {
            return new Transform(PointAt(u), (this.Start - this.End).Unitized());
        }

        /// <summary>
        /// Get a point along the line at parameter u.
        /// </summary>
        /// <param name="u"></param>
        /// <returns>A point on the curve at parameter u.</returns>
        public override Vector3 PointAt(double u)
        {
            if (u.ApproximatelyEquals(0.0))
            {
                return this.Start;
            }

            if (u.ApproximatelyEquals(1.0))
            {
                return this.End;
            }

            if (u > 1.0 || u < 0.0)
            {
                throw new Exception("The parameter t must be between 0.0 and 1.0.");
            }
            var offset = this.Length() * u;
            return this.Start + offset * this.Direction();
        }

        /// <summary>
        /// Construct a transformed copy of this Curve.
        /// </summary>
        /// <param name="transform">The transform to apply.</param>
        public override Curve Transformed(Transform transform)
        {
            return TransformedLine(transform);
        }

        /// <summary>
        /// Construct a transformed copy of this Line.
        /// </summary>
        /// <param name="transform">The transform to apply.</param>
        public Line TransformedLine(Transform transform)
        {
            return new Line(transform.OfPoint(this.Start), transform.OfPoint(this.End));
        }

        /// <summary>
        /// Get a new line that is the reverse of the original line.
        /// </summary>
        public Line Reversed()
        {
            return new Line(End, Start);
        }

        /// <summary>
        /// Thicken a line by the specified amount.
        /// </summary>
        /// <param name="amount">The amount to thicken the line.</param>
        public Polygon Thicken(double amount)
        {
            if (Start.Z != End.Z)
            {
                throw new Exception("The line could not be thickened. Only lines with their start and end at the same elevation can be thickened.");
            }

            var offsetN = this.Direction().Cross(Vector3.ZAxis);
            var a = this.Start + (offsetN * (amount / 2));
            var b = this.End + (offsetN * (amount / 2));
            var c = this.End - (offsetN * (amount / 2));
            var d = this.Start - (offsetN * (amount / 2));
            return new Polygon(new[] { a, b, c, d });
        }

        /// <summary>
        /// Is this line equal to the provided line?
        /// </summary>
        /// <param name="other">The target line.</param>
        /// <returns>True if the start and end points of the lines are equal, otherwise false.</returns>
        public bool Equals(Line other)
        {
            if (other == null)
            {
                return false;
            }
            return this.Start.Equals(other.Start) && this.End.Equals(other.End);
        }

        /// <summary>
        /// Get the hash code for the line.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return new[] { this.Start, this.End }.GetHashCode();
        }

        /// <summary>
        /// Intersect this line with the specified plane
        /// </summary>
        /// <param name="p">The plane.</param>
        /// <param name="result">The location of intersection.</param>
        /// <param name="infinite">If true, line will be treated as infinite. (False by default)</param>
        /// <returns>True if the line intersects the plane, false if no intersection occurs.</returns>
        public bool Intersects(Plane p, out Vector3 result, bool infinite = false)
        {
            var rayIntersects = new Ray(Start, Direction()).Intersects(p, out Vector3 location, out double t);
            if (rayIntersects)
            {
                var l = Length();
                if (infinite || t.ApproximatelyEquals(l) || t < l)
                {
                    result = location;
                    return true;
                }
            }
            else if (infinite)
            {
                var rayIntersectsBackwards = new Ray(End, Direction().Negate()).Intersects(p, out Vector3 location2, out double t2);
                if (rayIntersectsBackwards)
                {
                    result = location2;
                    return true;
                }
            }
            result = default(Vector3);
            return false;
        }

        /// <summary>
        /// Does this line intersect the provided line in 2D?
        /// </summary>
        /// <param name="l"></param>
        /// <returns>Return true if the lines intersect,
        /// false if the lines have coincident vertices or do not intersect.</returns>
        public bool Intersects2D(Line l)
        {
            var a = Vector3.CCW(this.Start, this.End, l.Start) * Vector3.CCW(this.Start, this.End, l.End);
            var b = Vector3.CCW(l.Start, l.End, this.Start) * Vector3.CCW(l.Start, l.End, this.End);
            if (IsAlmostZero(a) || a > Vector3.EPSILON) return false;
            if (IsAlmostZero(b) || b > Vector3.EPSILON) return false;
            return true;
        }

        /// <summary>
        /// Does this line intersect the provided line in 3D?
        /// </summary>
        /// <param name="l"></param>
        /// <param name="result"></param>
        /// <param name="infinite">Treat the lines as infinite?</param>
        /// <param name="includeEnds">If the end of one line lies exactly on the other, count it as an intersection?</param>
        /// <returns>True if the lines intersect, false if they are fully collinear or do not intersect.</returns>
        public bool Intersects(Line l, out Vector3 result, bool infinite = false, bool includeEnds = false)
        {
            // check if two lines are parallel
            if (Direction().IsParallelTo(l.Direction()))
            {
                result = default(Vector3);
                return false;
            }
            // construct a plane through this line and the start or end of the other line
            Plane plane;
            Vector3 testpoint;
            if (!(new[] { Start, End, l.Start }).AreCollinear())
            {
                plane = new Plane(Start, End, l.Start);
                testpoint = l.End;

            } // this only occurs in the rare case that the start point of the other line is collinear with this line (still need to generate a plane)
            else if (!(new[] { Start, End, l.End }).AreCollinear())
            {
                plane = new Plane(Start, End, l.End);
                testpoint = l.Start;
            }
            else // they're collinear (this shouldn't occur since it should be caught by the parallel test)
            {
                result = default(Vector3);
                return false;
            }

            // check if the fourth point is in the same plane as the other 3
            if (Math.Abs(plane.SignedDistanceTo(testpoint)) > Vector3.EPSILON)
            {
                result = default(Vector3);
                return false;
            }

            // at this point they're not parallel, and they lie in the same plane, so we know they intersect, we just don't know where.
            // construct a plane
            var normal = l.Direction().Cross(plane.Normal);
            Plane intersectionPlane = new Plane(l.Start, normal);
            if (Intersects(intersectionPlane, out Vector3 planeIntersectionResult, true)) // does the line intersect the plane?
            {
                if (infinite || (l.PointOnLine(planeIntersectionResult, includeEnds) && PointOnLine(planeIntersectionResult, includeEnds)))
                {
                    result = planeIntersectionResult;
                    return true;
                }

            }
            result = default(Vector3);
            return false;
        }

        private bool IsAlmostZero(double a)
        {
            return Math.Abs(a) < Vector3.EPSILON;
        }

        /// <summary>
        /// Get the bounding box for this line.
        /// </summary>
        /// <returns>A bounding box for this line.</returns>
        public override BBox3 Bounds()
        {
            if (this.Start < this.End)
            {
                return new BBox3(this.Start, this.End);
            }
            else
            {
                return new BBox3(this.End, this.Start);
            }
        }

        /// <summary>
        /// A normalized vector representing the direction of the line.
        /// </summary>
        public Vector3 Direction()
        {
            return (this.End - this.Start).Unitized();
        }

        /// <summary>
        /// Test if a point lies within this line segment
        /// </summary>
        /// <param name="point">The point to test.</param>
        /// <param name="includeEnds">Consider a point at the endpoint as on the line.</param>
        public bool PointOnLine(Vector3 point, bool includeEnds = false)
        {
            if (includeEnds && (point.DistanceTo(Start) < Vector3.EPSILON || point.DistanceTo(End) < Vector3.EPSILON))
            {
                return true;
            }
            return (Start - point).Unitized().Dot((End - point).Unitized()) < (Vector3.EPSILON - 1);
        }

        /// <summary>
        /// Divide the line into as many segments of the provided length as possible.
        /// </summary>
        /// <param name="l">The length.</param>
        /// <param name="removeShortSegments">A flag indicating whether segments shorter than l should be removed.</param>
        public List<Line> DivideByLength(double l, bool removeShortSegments = false)
        {
            var len = this.Length();
            if (l > len)
            {
                return new List<Line>() { new Line(this.Start, this.End) };
            }

            var total = 0.0;
            var d = this.Direction();
            var lines = new List<Line>();
            while (total + l <= len)
            {
                var a = this.Start + d * total;
                var b = a + d * l;
                lines.Add(new Line(a, b));
                total += l;
            }
            if (total < len && !removeShortSegments)
            {
                var a = this.Start + d * total;
                if (!a.IsAlmostEqualTo(End))
                {
                    lines.Add(new Line(a, End));
                }
            }
            return lines;
        }


        /// <summary>
        /// Divide the line into as many segments of the provided length as possible.
        /// Divisions will be centered along the line.
        /// </summary>
        /// <param name="l">The length.</param>
        /// <returns></returns>
        public List<Line> DivideByLengthFromCenter(double l)
        {
            var lines = new List<Line>();

            var localLength = this.Length();
            if (localLength <= l)
            {
                lines.Add(this);
                return lines;
            }

            var divs = (int)(localLength / l);
            var span = divs * l;
            var halfSpan = span / 2;
            var mid = this.PointAt(0.5);
            var dir = this.Direction();
            var start = mid - dir * halfSpan;
            var end = mid + dir * halfSpan;
            if (!this.Start.IsAlmostEqualTo(start))
            {
                lines.Add(new Line(this.Start, start));
            }
            for (var i = 0; i < divs; i++)
            {
                var p1 = start + (i * l) * dir;
                var p2 = p1 + dir * l;
                lines.Add(new Line(p1, p2));
            }
            if (!this.End.IsAlmostEqualTo(end))
            {
                lines.Add(new Line(end, this.End));
            }

            return lines;
        }

        /// <summary>
        /// Divide the line into n equal segments.
        /// </summary>
        /// <param name="n">The number of segments.</param>
        public List<Line> DivideIntoEqualSegments(int n)
        {
            if (n <= 0)
            {
                throw new ArgumentException($"The number of divisions must be greater than 0.");
            }
            var lines = new List<Line>();
            var div = 1.0 / n;
            for (var t = 0.0; t < 1.0 - div + Vector3.EPSILON; t += div)
            {
                var a = PointAt(t);
                var b = PointAt(t + div);
                lines.Add(new Line(a, b));
            }
            return lines;
        }

        /// <summary>
        /// Offset the line. The offset direction will be defined by
        /// Direction X Vector3.ZAxis.
        /// </summary>
        /// <param name="distance">The distance to offset.</param>
        /// <param name="flip">Flip the offset direction.</param>
        /// <returns></returns>
        public Line Offset(double distance, bool flip)
        {
            var offsetVector = this.Direction().Cross(Vector3.ZAxis);
            if (flip)
            {
                offsetVector = offsetVector.Negate();
            }
            var a = Start + offsetVector * distance;
            var b = End + offsetVector * distance;
            return new Line(a, b);
        }

        /// <summary>
        /// Trim this line to the trimming curve.
        /// </summary>
        /// <param name="line">The curve to which to trim.</param>
        /// <param name="flip">Should the trim direction be reversed?</param>
        /// <returns>A new line, or null if this line does not intersect the trimming line.</returns>
        public Line TrimTo(Line line, bool flip = false)
        {
            if (this.Intersects(line, out Vector3 result, true))
            {
                if (flip)
                {
                    return new Line(result, this.End);
                }
                else
                {
                    return new Line(this.Start, result);
                }
            }

            return null;
        }

        /// <summary>
        /// Extend this line to the trimming curve.
        /// </summary>
        /// <param name="line">The curve to which to extend.</param>
        /// <returns>A new line, or null if these lines would never intersect if extended infinitely.</returns>
        public Line ExtendTo(Line line)
        {
            if (!Intersects(line, out var intersection, true))
            {
                return null;
            }
            if (intersection.DistanceTo(Start) > intersection.DistanceTo(End))
            {
                return new Line(this.Start, intersection);
            }
            else
            {
                return new Line(this.End, intersection);
            }
        }

        /// <summary>
        /// Extend this line to its (nearest, by default) intersection with any other line.
        /// If optional `extendToFurthest` is true, extends to furthest intersection with any other line.
        /// </summary>
        /// <param name="otherLines">The other lines to intersect with</param>
        /// <param name="bothSides">Optional — if false, will only extend in the line's direction; if true will extend in both directions.</param>
        /// <param name="extendToFurthest">Optional — if true, will extend line as far as it will go, rather than stopping at the closest intersection.</param>
        /// <param name="tolerance">Optional — The amount of tolerance to include in the extension method.</param>
        public Line ExtendTo(IEnumerable<Line> otherLines, bool bothSides = true, bool extendToFurthest = false, double tolerance = Vector3.EPSILON)
        {
            // this test line — inset slightly from the line — helps treat the ends as valid intersection points, to prevent
            // extension beyond an immediate intersection.
            var testLine = new Line(this.PointAt(0.001), this.PointAt(0.999));
            var segments = otherLines;
            var intersectionsForLine = new List<Vector3>();
            foreach (var segment in segments)
            {
                bool pointAdded = false;
                // Special case for parallel + collinear lines:
                // ____   |__________
                // We want to extend only to the first corner of the other lines,
                // not all the way through to the other end
                if (segment.Direction().IsParallelTo(testLine.Direction(), tolerance) && // if the two lines are parallel
                    (new[] { segment.End, segment.Start, testLine.Start, testLine.End }).AreCollinear())// and collinear
                {
                    if (!this.PointOnLine(segment.End, true))
                    {
                        intersectionsForLine.Add(segment.End);
                        pointAdded = true;
                    }

                    if (!this.PointOnLine(segment.Start, true))
                    {
                        intersectionsForLine.Add(segment.Start);
                        pointAdded = true;
                    }
                }
                if (extendToFurthest || !pointAdded)
                {
                    var intersects = testLine.Intersects(segment, out Vector3 intersection, true, true);

                    // if the intersection lies on the obstruction, but is beyond the segment, we collect it
                    if (segment.PointOnLine(intersection, true) && !testLine.PointOnLine(intersection, true))
                    {
                        intersectionsForLine.Add(intersection);
                    }
                }
            }

            var dir = this.Direction();
            var intersectionsOrdered = intersectionsForLine.OrderBy(i => (testLine.Start - i).Dot(dir));

            var start = this.Start;
            var end = this.End;

            var startCandidates = intersectionsOrdered
                    .Where(i => (testLine.Start - i).Dot(dir) > 0)
                    .Cast<Vector3?>();

            var endCandidates = intersectionsOrdered
                .Where(i => (testLine.Start - i).Dot(dir) < testLine.Length() * -1)
                .Reverse().Cast<Vector3?>();

            (Vector3? Start, Vector3? End) startEndCandidates = extendToFurthest ?
                (startCandidates.LastOrDefault(), endCandidates.LastOrDefault()) :
                (startCandidates.FirstOrDefault(), endCandidates.FirstOrDefault());

            if (bothSides && startEndCandidates.Start != null)
            {
                start = (Vector3)startEndCandidates.Start;
            }
            if (startEndCandidates.End != null)
            {
                end = (Vector3)startEndCandidates.End;
            }

            return new Line(start, end);
        }

        /// <summary>
        /// Extend this line to its (nearest, by default) intersection with a polyline.
        /// </summary>
        /// <param name="polyline">The polyline to intersect with</param>
        /// <param name="bothSides">Optional — if false, will only extend in the line's direction; if true will extend in both directions.</param>
        /// <param name="extendToFurthest">Optional — if true, will extend line as far as it will go, rather than stopping at the closest intersection.</param>
        public Line ExtendTo(Polyline polyline, bool bothSides = true, bool extendToFurthest = false)
        {
            return ExtendTo(polyline.Segments(), bothSides, extendToFurthest);
        }

        /// <summary>
        /// Extend this line to its (nearest, by default) intersection with a profile.
        /// </summary>
        /// <param name="profile">The profile to intersect with</param>
        /// <param name="bothSides">Optional — if false, will only extend in the line's direction; if true will extend in both directions.</param>
        /// <param name="extendToFurthest">Optional — if true, will extend line as far as it will go, rather than stopping at the closest intersection.</param>
        public Line ExtendTo(Profile profile, bool bothSides = true, bool extendToFurthest = false)
        {
            return ExtendTo(profile.Segments(), bothSides, extendToFurthest);
        }

        /// <summary>
        /// Extend this line to its (nearest, by default) intersection with a polygon.
        /// </summary>
        /// <param name="polygon">The polygon to intersect with</param>
        /// <param name="bothSides">Optional — if false, will only extend in the line's direction; if true will extend in both directions.</param>
        /// <param name="extendToFurthest">Optional — if true, will extend line as far as it will go, rather than stopping at the closest intersection.</param>
        /// <param name="tolerance">Optional — The amount of tolerance to include in the extension method.</param>
        public Line ExtendTo(Polygon polygon, bool bothSides = true, bool extendToFurthest = false, double tolerance = Vector3.EPSILON)
        {
            return ExtendTo(polygon.Segments(), bothSides, extendToFurthest, tolerance);
        }

        /// <summary>
        /// Trim a line with a polygon.
        /// </summary>
        /// <param name="polygon">The polygon to trim with.</param>
        /// <param name="outsideSegments">A list of the segment(s) of the line outside of the supplied polygon.</param>
        /// <returns>A list of the segment(s) of the line within the supplied polygon.</returns>
        public List<Line> Trim(Polygon polygon, out List<Line> outsideSegments)
        {
            // adapted from http://csharphelper.com/blog/2016/01/clip-a-line-segment-to-a-polygon-in-c/
            // Make lists to hold points of intersection
            var intersections = new List<Vector3>();

            // Add the segment's starting point.
            intersections.Add(this.Start);
            polygon.Contains(this.Start, out var containment);
            var StartsOutsidePolygon = containment == Containment.Outside;

            var hasVertexIntersections = containment == Containment.CoincidesAtVertex;

            // Examine the polygon's edges.
            for (int i1 = 0; i1 < polygon.Vertices.Count; i1++)
            {
                // Get the end points for this edge.
                int i2 = (i1 + 1) % polygon.Vertices.Count;

                // See where the edge intersects the segment.
                var segment = new Line(polygon.Vertices[i1], polygon.Vertices[i2]);
                var segmentsIntersect = Intersects(segment, out Vector3 intersection); // This will return false for intersections exactly at an end

                // See if the segment intersects the edge.
                if (segmentsIntersect)
                {
                    // Record this intersection.
                    intersections.Add(intersection);
                }
                // see if the segment intersects at a vertex
                else if (this.PointOnLine(polygon.Vertices[i1]))
                {
                    intersections.Add(polygon.Vertices[i1]);
                    hasVertexIntersections = true;
                }
            }

            // Add the segment's ending point.
            intersections.Add(End);

            var intersectionsOrdered = intersections.OrderBy(v => v.DistanceTo(Start)).ToArray();
            var inSegments = new List<Line>();
            outsideSegments = new List<Line>();
            var currentlyIn = !StartsOutsidePolygon;
            for (int i = 0; i < intersectionsOrdered.Length - 1; i++)
            {
                var A = intersectionsOrdered[i];
                var B = intersectionsOrdered[i + 1];
                if (A.IsAlmostEqualTo(B)) // skip duplicate points
                {
                    continue;
                }
                var segment = new Line(A, B);
                if (hasVertexIntersections || containment == Containment.CoincidesAtEdge) // if it passed through a vertex, or started at an edge or vertex, we can't rely on alternating, so check each midpoint
                {
                    currentlyIn = polygon.Contains((A + B) / 2);
                }
                if (currentlyIn)
                {
                    inSegments.Add(segment);
                }
                else
                {
                    outsideSegments.Add(segment);
                }
                currentlyIn = !currentlyIn;
            }

            return inSegments;
        }

        /// <summary>
        /// Create a fillet arc between this line and the target.
        /// </summary>
        /// <param name="target">The line with which to fillet.</param>
        /// <param name="radius">The radius of the fillet.</param>
        /// <returns>An arc, or null if no fillet can be calculated.</returns>
        public Arc Fillet(Line target, double radius)
        {
            var d1 = this.Direction();
            var d2 = target.Direction();
            if (d1.IsParallelTo(d2))
            {
                throw new Exception("The fillet could not be created. The lines are parallel");
            }

            var r1 = new Ray(this.Start, d1);
            var r2 = new Ray(target.Start, d2);
            if (!r1.Intersects(r2, out Vector3 result, true))
            {
                return null;
            }

            // Construct new vectors that both
            // point away from the projected intersection
            var newD1 = (this.PointAt(0.5) - result).Unitized();
            var newD2 = (target.PointAt(0.5) - result).Unitized();

            var theta = newD1.AngleTo(newD2) * Math.PI / 180.0;
            var halfTheta = theta / 2.0;
            var h = radius / Math.Sin(halfTheta);
            var centerVec = newD1.Average(newD2).Unitized();
            var arcCenter = result + centerVec * h;

            // Find the closest points from the arc
            // center to the adjacent curves.
            var p1 = arcCenter.ClosestPointOn(this);
            var p2 = arcCenter.ClosestPointOn(target);

            // Find the angle of both segments relative to the fillet arc.
            // ATan2 assumes the origin, so correct the coordinates
            // by the offset of the center of the arc.
            var angle1 = Math.Atan2(p1.Y - arcCenter.Y, p1.X - arcCenter.X) * 180.0 / Math.PI;
            var angle2 = Math.Atan2(p2.Y - arcCenter.Y, p2.X - arcCenter.X) * 180.0 / Math.PI;

            // ATan2 will provide negative angles in the "lower" quadrants
            // Ensure that these values are 180d -> 360d
            angle1 = (angle1 + 360) % 360;
            angle2 = (angle2 + 360) % 360;
            angle2 = angle2 == 0.0 ? 360.0 : angle2;

            // We only support CCW wound arcs.
            // For arcs that with start angles <1d, convert
            // the arc back to a negative value.
            var arc = new Arc(arcCenter,
                           radius,
                           angle1 > angle2 ? angle1 - 360.0 : angle1,
                           angle2);

            // Get the complimentary arc and choose
            // the shorter of the two arcs.
            var complement = arc.Complement();
            if (arc.Length() < complement.Length())
            {
                return arc;
            }
            else
            {
                return complement;
            }
        }

        /// <summary>
        /// A list of vertices describing the arc for rendering.
        /// </summary>
        internal override IList<Vector3> RenderVertices()
        {
            return new[] { this.Start, this.End };
        }

        #region WindingNumberCalcs
        internal Position RelativePositionOf(Vector3 location)
        {
            double positionCalculation =
                (End.Y - Start.Y) * (location.X - Start.X) -
                (location.Y - Start.Y) * (End.X - Start.X);

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

        internal bool AscendingRelativeTo(Vector3 location)
        {
            return Start.X <= location.X;
        }

        internal bool LocationInRange(Vector3 location, Orientation orientation)
        {
            if (orientation == Orientation.Ascending)
            {
                return End.X > location.X;
            }

            return End.X <= location.X;
        }

        internal enum Position
        {
            Left,
            Right,
            Center
        }

        internal enum Orientation
        {
            Ascending,
            Descending
        }
        #endregion
    }

    /// <summary>
    /// Geometric line comparer for use in HashSets and Dictionaries.
    /// </summary>
    public class LineComparer : IEqualityComparer<Line>
    {
        private double _precision = 5;
        private bool _directionIndependent = true;

        /// <summary>
        /// Create a comparer setting wether this comparer cares about direction and also the precision.
        /// </summary>
        public LineComparer(bool directionIndependent = true, double precision = Vector3.EPSILON)
        {
            _precision = precision;
            _directionIndependent = directionIndependent;
        }

        /// <summary>
        /// Are the two lines equal according to the LineComparer settings.
        /// </summary>
        public bool Equals(Line x, Line y)
        {
            return (x.Start.IsAlmostEqualTo(y.Start, _precision) && x.End.IsAlmostEqualTo(y.End, _precision))
                    || (_directionIndependent
                        && (x.Start.IsAlmostEqualTo(y.End, _precision) && x.End.IsAlmostEqualTo(y.Start, _precision)));
        }

        /// <summary>
        /// Retrieve a hashcode for this line that is consistent with the precision and direction dependance.
        /// </summary>
        public int GetHashCode(Line obj)
        {
            // If the direction doesn't matter, then we always sort the end points by X, then Y, then Z to have a consistent basis for forming the hash code.
            if (_directionIndependent)
            {
                if (obj.Start.X != obj.End.X)
                {
                    if (obj.Start.X > obj.End.X)
                    {
                        obj = obj.Reversed();
                    }
                }
                else if (obj.Start.Y != obj.End.Y)
                {
                    if (obj.Start.Y > obj.End.Y)
                    {
                        obj = obj.Reversed();
                    }
                }
                else if (obj.Start.Z != obj.End.Z)
                {
                    if (obj.Start.Z > obj.End.Z)
                    {
                        obj = obj.Reversed();
                    }
                }
                else
                {
                    throw new Exception("Invalid line, start and end are identical, cannot create hashcode");
                }
            }
            int hash = 17;
            hash = hash * 23 + obj.Start.Rounded(_precision).GetHashCode();
            hash = hash * 23 + obj.End.Rounded(_precision).GetHashCode();
            return hash;
        }
    }
}