using Elements.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Elements.Spatial;

namespace Elements.Geometry
{
    /// <summary>
    /// A line segment.
    /// Parameterization of the line is  0 (start) -> length (end)
    /// </summary>
    /// <example>
    /// [!code-csharp[Main](../../Elements/test/LineTests.cs?name=example)]
    /// </example>
    /// TODO: Rename this class to LineSegment
    public class Line : TrimmedCurve<InfiniteLine>, IEquatable<Line>
    {
        /// <summary>
        /// The domain of the curve.
        /// </summary>
        [JsonIgnore]
        public override Domain1d Domain => new Domain1d(this.Start.DistanceTo(BasisCurve.Origin), this.End.DistanceTo(BasisCurve.Origin));

        /// <summary>
        /// Create a line of one unit length along the X axis.
        /// </summary>
        public Line()
        {
            this.Start = Vector3.Origin;
            this.End = new Vector3(1, 0, 0);
            this.BasisCurve = new InfiniteLine(this.Start, (this.End - this.Start).Unitized());
        }

        /// <summary>
        /// Create a line.
        /// </summary>
        /// <param name="start">The start of the line.</param>
        /// <param name="end">The end of the line.</param>
        [JsonConstructor]
        public Line(Vector3 @start, Vector3 @end) : base()
        {
            if (!Validator.DisableValidationOnConstruction)
            {
                if (start.IsAlmostEqualTo(end))
                {
                    throw new ArgumentException($"The line could not be created. The start and end points of the line cannot be the same: start {start}, end {end}");
                }
            }

            this.Start = @start;
            this.End = @end;
            this.BasisCurve = new InfiniteLine(this.Start, (this.End - this.Start).Unitized());
        }

        /// <summary>
        /// Create a line of length from a start along direction.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="direction"></param>
        /// <param name="length"></param>
        public Line(Vector3 start, Vector3 direction, double length)
        {
            this.Start = start;
            this.End = start + direction.Unitized() * length;
            this.BasisCurve = new InfiniteLine(this.Start, direction);
        }

        /// <summary>
        /// Create a line from a trimmed segment of an infinite line.
        /// </summary>
        /// <param name="line">The infinite line from which this segment is trimmed.</param>
        public Line(InfiniteLine line)
        {
            this.BasisCurve = line;
            this.Start = this.BasisCurve.Origin + this.Domain.Min * this.BasisCurve.Direction;
            this.End = this.BasisCurve.Origin + this.Domain.Max * this.BasisCurve.Direction;
        }

        /// <summary>
        /// Calculate the length of the line.
        /// </summary>
        public override double Length()
        {
            return this.Start.DistanceTo(this.End);
        }

        /// <summary>
        /// Calculate the length of the line between two parameters.
        /// </summary>
        public override double ArcLength(double start, double end)
        {
            return Math.Abs(end - start);
        }

        /// <summary>
        /// Get a transform whose XY plane is perpendicular to the curve, and whose
        /// positive Z axis points along the curve.
        /// </summary>
        /// <param name="u">The parameter along the Line, between 0.0 and 1.0, at which to calculate the Transform.</param>
        /// <returns>A transform.</returns>
        public override Transform TransformAt(double u)
        {
            if (!Domain.Includes(u, true))
            {
                throw new Exception($"The parameter {u} is not on the trimmed portion of the basis curve.");
            }
            return this.BasisCurve.TransformAt(u);
        }

        /// <summary>
        /// Get a point along the line at parameter u.
        /// </summary>
        /// <param name="u">A parameter on the curve between 0.0 and length.</param>
        /// <returns>A point on the curve at parameter u.</returns>
        public override Vector3 PointAt(double u)
        {
            if (!Domain.Includes(u, true))
            {
                throw new Exception($"The parameter {u} is not on the trimmed portion of the basis curve. The parameter must be between {Domain.Min} and {Domain.Max}.");
            }
            return this.BasisCurve.PointAt(u);
        }

        /// <inheritdoc/>
        public override Curve Transformed(Transform transform)
        {
            if (transform == null)
            {
                return this;
            }

            return new Line(transform.OfPoint(this.Start), transform.OfPoint(this.End));
        }

        /// <summary>
        /// Construct a transformed copy of this Line.
        /// </summary>
        /// <param name="transform">The transform to apply.</param>
        public Line TransformedLine(Transform transform)
        {
            if (transform == null)
            {
                return this;
            }

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
            if (!Start.Z.ApproximatelyEquals(End.Z))
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
            // https://stackoverflow.com/questions/263400/what-is-the-best-algorithm-for-overriding-gethashcode
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + this.Start.GetHashCode();
                hash = hash * 23 + this.End.GetHashCode();
                return hash;
            }
        }

        /// <summary>
        /// Are the two lines almost equal?
        /// </summary>
        public bool IsAlmostEqualTo(Line other, bool directionDependent, double tolerance = Vector3.EPSILON)
        {
            return (Start.IsAlmostEqualTo(other.Start, tolerance) && End.IsAlmostEqualTo(other.End, tolerance))
                    || (!directionDependent
                        && (Start.IsAlmostEqualTo(other.End, tolerance) && End.IsAlmostEqualTo(other.Start, tolerance)));
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
            return Intersects(p, this.Start, this.End, out result, infinite);
        }

        /// <summary>
        /// Intersect a segment defined by two points with a plane.
        /// </summary>
        /// <param name="p">The plane.</param>
        /// <param name="start">The start of the segment.</param>
        /// <param name="end">The end of the segment.</param>
        /// <param name="result">The location of intersection.</param>
        /// <param name="infinite">Whether the segment should instead be considered infinite.</param>
        /// <returns>True if an intersection is found, otherwise false.</returns>
        public static bool Intersects(Plane p, Vector3 start, Vector3 end, out Vector3 result, bool infinite = false)
        {
            var d = (end - start).Unitized();
            var rayIntersects = new Ray(start, d).Intersects(p, out Vector3 location, out double t);
            if (rayIntersects)
            {
                var l = start.DistanceTo(end);
                if (infinite || t.ApproximatelyEquals(l) || t < l)
                {
                    result = location;
                    return true;
                }
            }
            else if (infinite)
            {
                var rayIntersectsBackwards = new Ray(end, d.Negate()).Intersects(p, out Vector3 location2, out double t2);
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
            return Intersects2d(Start, End, l.Start, l.End);
        }

        /// <summary>
        /// Does the first line intersect with the second line in 2D?
        /// </summary>
        /// <param name="start1">Start point of the first line</param>
        /// <param name="end1">End point of the first line</param>
        /// <param name="start2">Start point of the second line</param>
        /// <param name="end2">End point of the second line</param>
        /// <returns>Return true if the lines intersect,
        /// false if the lines have coincident vertices or do not intersect.</returns>
        public static bool Intersects2d(Vector3 start1, Vector3 end1, Vector3 start2, Vector3 end2)
        {
            var a = Vector3.CCW(start1, end1, start2) * Vector3.CCW(start1, end1, end2);
            var b = Vector3.CCW(start2, end2, start1) * Vector3.CCW(start2, end2, end1);
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
            return Line.Intersects(this.Start, this.End, l.Start, l.End, out result, infinite, includeEnds);
        }

        /// <summary>
        /// Do two lines intersect in 3d?
        /// </summary>
        /// <param name="start1">Start point of the first line</param>
        /// <param name="end1">End point of the first line</param>
        /// <param name="start2">Start point of the second line</param>
        /// <param name="end2">End point of the second line</param>
        /// <param name="result"></param>
        /// <param name="infinite">Treat the lines as infinite?</param>
        /// <param name="includeEnds">If the end of one line lies exactly on the other, count it as an intersection?</param>
        /// <returns>True if the lines intersect, false if they are fully collinear or do not intersect.</returns>
        public static bool Intersects(Vector3 start1, Vector3 end1, Vector3 start2, Vector3 end2, out Vector3 result, bool infinite = false, bool includeEnds = false)
        {
            // check if two lines are parallel
            var direction1 = Direction(start1, end1);
            var direction2 = Direction(start2, end2);
            if (direction1.IsParallelTo(direction2))
            {
                result = default(Vector3);
                return false;
            }
            // construct a plane through this line and the start or end of the other line
            Plane plane;
            Vector3 testpoint;
            if (!(new[] { start1, end1, start2 }).AreCollinearByDistance())
            {
                plane = new Plane(start1, end1, start2);
                testpoint = end2;

            } // this only occurs in the rare case that the start point of the other line is collinear with this line (still need to generate a plane)
            else if (!(new[] { start1, end1, end2 }).AreCollinearByDistance())
            {
                plane = new Plane(start1, end1, end2);
                testpoint = start2;
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
            var normal = direction2.Cross(plane.Normal);
            Plane intersectionPlane = new Plane(start2, normal);
            if (Intersects(intersectionPlane, start1, end1, out Vector3 planeIntersectionResult, true)) // does the line intersect the plane?
            {
                if (infinite || (PointOnLine(planeIntersectionResult, start2, end2, includeEnds) && PointOnLine(planeIntersectionResult, start1, end1, includeEnds)))
                {
                    result = planeIntersectionResult;
                    return true;
                }

            }
            result = default(Vector3);
            return false;
        }

        /// <summary>
        /// Does this line touches or intersects the provided box in 3D?
        /// </summary>
        /// <param name="box">Axis aligned box to intersect.</param>
        /// <param name="results">Up to two intersection points.</param>
        /// <param name="infinite">Treat the line as infinite?</param>
        /// <param name="tolerance">An optional distance tolerance.</param>
        /// <returns>True if the line touches or intersects the  box at least at one point, false otherwise.</returns>
        public bool Intersects(BBox3 box, out List<Vector3> results, bool infinite = false, double tolerance = Vector3.EPSILON)
        {
            var d = End - Start;
            results = new List<Vector3>();

            // Solving the t parameter on line were it intersects planes of box in different coordinates.
            // If vector has no change in particular coordinate - just skip it as infinity.
            var t0x = double.NegativeInfinity;
            var t1x = double.PositiveInfinity;
            if (Math.Abs(d.X) > 1e-6)
            {
                t0x = (box.Min.X - Start.X) / d.X;
                t1x = (box.Max.X - Start.X) / d.X;
                // Line can reach min plane of box before reaching max.
                if (t1x < t0x)
                {
                    (t0x, t1x) = (t1x, t0x);
                }
            }

            var t0y = double.NegativeInfinity;
            var t1y = double.PositiveInfinity;
            if (Math.Abs(d.Y) > 1e-6)
            {
                t0y = (box.Min.Y - Start.Y) / d.Y;
                t1y = (box.Max.Y - Start.Y) / d.Y;
                if (t1y < t0y)
                {
                    (t0y, t1y) = (t1y, t0y);
                }
            }

            // If max hit of one coordinate is smaller then min hit of other - line hits planes outside the box.
            // In other words line just goes by.
            var length = d.Length();
            if ((t0x - t1y) * length > tolerance || (t0y - t1x) * length > tolerance)
            {
                return false;
            }

            var tMin = Math.Max(t0x, t0y);
            var tMax = Math.Min(t1x, t1y);

            if (Math.Abs(d.Z) > 1e-6)
            {
                var t0z = (box.Min.Z - Start.Z) / d.Z;
                var t1z = (box.Max.Z - Start.Z) / d.Z;

                if (t1z < t0z)
                {
                    (t0z, t1z) = (t1z, t0z);
                }

                if (t0z > tMax || t1z < tMin)
                {
                    return false;
                }

                tMin = Math.Max(t0z, tMin);
                tMax = Math.Min(t1z, tMax);
            }

            if (tMin == double.NegativeInfinity || tMin == double.PositiveInfinity)
            {
                return false;
            }

            var dMin = tMin * length;
            var dMax = tMax * length;

            // Check if found parameters are within normalized line range.
            if (infinite || (dMin > -tolerance && dMin < length + tolerance))
            {
                results.Add(Start + d * tMin);
            }

            if (Math.Abs(dMax - dMin) > tolerance &&
                (infinite || (dMax > -tolerance && dMax < length + tolerance)))
            {
                results.Add(Start + d * tMax);
            }

            return results.Any();
        }

        private static bool IsAlmostZero(double a)
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
            return Direction(Start, End);
        }

        /// <summary>
        /// A normalized vector representing the direction of a line, represented by a start and end point.
        /// <param name="start">The start point of the line.</param>
        /// <param name="end">The end point of the line.</param>
        /// </summary>
        public static Vector3 Direction(Vector3 start, Vector3 end)
        {
            return (end - start).Unitized();
        }

        /// <summary>
        /// Test if a point lies within tolerance of this line segment.
        /// </summary>
        /// <param name="point">The point to test.</param>
        /// <param name="includeEnds">Consider a point at the endpoint as on the line.</param>
        /// <param name="tolerance">An optional distance tolerance.
        /// When true, any point within tolerance of the end points will be considered on the line.
        /// When false, points precisely at the ends of the line will not be considered on the line.</param>
        public bool PointOnLine(Vector3 point, bool includeEnds = false, double tolerance = Vector3.EPSILON)
        {
            return Line.PointOnLine(point, Start, End, includeEnds, tolerance);
        }

        /// <summary>
        /// Test if a point lies within tolerance of a given line segment.
        /// </summary>
        /// <param name="point">The point to test.</param>
        /// <param name="start">The start point of the line segment.</param>
        /// <param name="end">The end point of the line segment.</param>
        /// <param name="includeEnds">Consider a point at the endpoint as on the line.</param>
        /// <param name="tolerance">An optional distance tolerance.
        /// When true, any point within tolerance of the end points will be considered on the line.
        /// When false, points precisely at the ends of the line will not be considered on the line.</param>
        public static bool PointOnLine(Vector3 point, Vector3 start, Vector3 end, bool includeEnds = false, double tolerance = Vector3.EPSILON)
        {
            if (includeEnds && (point.IsAlmostEqualTo(start, tolerance) || point.IsAlmostEqualTo(end, tolerance)))
            {
                return true;
            }

            var delta = end - start;
            var lambda = (point - start).Dot(delta) / (end - start).Dot(delta);
            if (lambda > 0 && lambda < 1)
            {
                var pointOnLine = start + lambda * delta;
                return pointOnLine.IsAlmostEqualTo(point, tolerance);
            }
            return false;
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
        /// The middle of the curve's parameter spaces
        /// which is also the mid point of the line.
        /// </summary>
        public override Vector3 Mid()
        {
            return Start.Average(End);
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
            var mid = this.Mid();
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
            var div = Length() / n;
            var a = Start;
            var t = div;
            for (var i = 0; i < n - 1; i++)
            {
                var b = PointAt(t);
                lines.Add(new Line(a, b));

                t += div;
                a = b;
            }
            lines.Add(new Line(a, End));
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
            return ExtendTo(otherLines, double.MaxValue, bothSides, extendToFurthest, tolerance);
        }

        /// <summary>
        /// Extend this line to its (nearest, by default) intersection with any other line, but no further than maxDistance.
        /// If optional `extendToFurthest` is true, extends to furthest intersection with any other line, but no further than maxDistance.
        /// If the distance to the intersection with the lines is greater than the maximum, the line will be returned unchanged.
        /// </summary>
        /// <param name="otherLines">The other lines to intersect with.</param>
        /// <param name="maxDistance">Maximum extension distance.</param>
        /// <param name="bothSides">Optional — if false, will only extend in the line's direction; if true will extend in both directions.</param>
        /// <param name="extendToFurthest">Optional — if true, will extend line as far as it will go, rather than stopping at the closest intersection.</param>
        /// <param name="tolerance">Optional — The amount of tolerance to include in the extension method.</param>
        public Line ExtendTo(IEnumerable<Line> otherLines, double maxDistance, bool bothSides = true, bool extendToFurthest = false, double tolerance = Vector3.EPSILON)
        {
            // this test line — inset slightly from the line — helps treat the ends as valid intersection points, to prevent
            // extension beyond an immediate intersection.
            var testLine = new Line(this.Start + this.BasisCurve.Direction * 0.001, this.End - this.BasisCurve.Direction * 0.001);
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
                    (new[] { segment.End, segment.Start, testLine.Start, testLine.End }).AreCollinearByDistance())// and collinear
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
                    if (intersects && segment.PointOnLine(intersection, true) && !testLine.PointOnLine(intersection, true))
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
                (startCandidates.LastOrDefault(p => p.GetValueOrDefault().DistanceTo(start) < maxDistance), endCandidates.LastOrDefault(p => p.GetValueOrDefault().DistanceTo(end) < maxDistance)) :
                (startCandidates.FirstOrDefault(p => p.GetValueOrDefault().DistanceTo(start) < maxDistance), endCandidates.FirstOrDefault(p => p.GetValueOrDefault().DistanceTo(end) < maxDistance));

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
        /// Extend this line to its (nearest, by default) intersection with a polyline, but no further than maxDistance.
        /// </summary>
        /// <param name="polyline">The polyline to intersect with</param>
        /// <param name="maxDistance">Maximum extension distance.</param>
        /// <param name="bothSides">Optional — if false, will only extend in the line's direction; if true will extend in both directions.</param>
        /// <param name="extendToFurthest">Optional — if true, will extend line as far as it will go, rather than stopping at the closest intersection.</param>
        public Line ExtendTo(Polyline polyline, double maxDistance, bool bothSides = true, bool extendToFurthest = false)
        {
            return ExtendTo(polyline.Segments(), maxDistance, bothSides, extendToFurthest);
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
        /// Extend this line to its (nearest, by default) intersection with a profile, but no further than maxDistance.
        /// </summary>
        /// <param name="profile">The profile to intersect with</param>
        /// <param name="maxDistance">Maximum extension distance.</param>
        /// <param name="bothSides">Optional — if false, will only extend in the line's direction; if true will extend in both directions.</param>
        /// <param name="extendToFurthest">Optional — if true, will extend line as far as it will go, rather than stopping at the closest intersection.</param>
        public Line ExtendTo(Profile profile, double maxDistance, bool bothSides = true, bool extendToFurthest = false)
        {
            return ExtendTo(profile.Segments(), maxDistance, bothSides, extendToFurthest);
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
        /// Extend this line to its (nearest, by default) intersection with a polygon, but no further than maxDistance.
        /// </summary>
        /// <param name="polygon">The polygon to intersect with</param>
        /// <param name="maxDistance">Maximum extension distance.</param>
        /// <param name="bothSides">Optional — if false, will only extend in the line's direction; if true will extend in both directions.</param>
        /// <param name="extendToFurthest">Optional — if true, will extend line as far as it will go, rather than stopping at the closest intersection.</param>
        /// <param name="tolerance">Optional — The amount of tolerance to include in the extension method.</param>
        public Line ExtendTo(Polygon polygon, double maxDistance, bool bothSides = true, bool extendToFurthest = false, double tolerance = Vector3.EPSILON)
        {
            return ExtendTo(polygon.Segments(), maxDistance, bothSides, extendToFurthest, tolerance);
        }

        /// <summary>
        /// Measure the distance between two lines.
        /// </summary>
        /// <param name="other">The line to measure the distance to.</param>
        public double DistanceTo(Line other)
        {
            Vector3 dStartStart = this.Start - other.Start;
            Vector3 vThis = this.End - this.Start;
            Vector3 vOther = other.End - other.Start;
            Vector3 cross = vThis.Cross(vOther);
            // line vectors are collinear - two segments share the same infinite line on the infinite line.
            if (cross.IsZero())
            {
                // if start of this line is "before" start of the other line.
                if (vOther.Dot(dStartStart) < 0)
                {
                    Vector3 dEndStart = this.End - other.Start;
                    // and end of this line is "before" start of the other line - line are not overlapping,
                    // otherwise the projection of this contains other.Start
                    if (vOther.Dot(dEndStart) < 0)
                    {
                        // the projection of this line is outside of the other, closer to other.Start
                        return Math.Sqrt(Math.Min(dStartStart.LengthSquared(), dEndStart.LengthSquared()));
                    }
                }
                else
                {
                    Vector3 dStartEnd = this.Start - other.End;
                    // if end of this line is "after" end of the other line,
                    // otherwise the projection of this.Start is inside other.
                    if (vOther.Dot(dStartEnd) > 0)
                    {
                        Vector3 dEndEnd = this.End - other.End;
                        // and start of this line is "after" end of the other line.
                        if (vOther.Dot(dEndEnd) > 0)
                        {
                            // the projection of this line is outside of the other, closer to other.End
                            return Math.Sqrt(Math.Min(dStartEnd.LengthSquared(), dEndEnd.LengthSquared()));
                        }
                    }
                }
                dStartStart -= dStartStart.ProjectOnto(vThis);
            }
            // line vectors are not collinear, their directions share the common plane.
            else
            {
                // dStartStart length is distance to the common plane.
                dStartStart = dStartStart.ProjectOnto(cross);
                Vector3 vStartStart = other.Start + dStartStart - this.Start;
                Vector3 vStartEnd = other.Start + dStartStart - this.End;
                Vector3 vEndStart = other.End + dStartStart - this.Start;
                Vector3 vEndEnd = other.End + dStartStart - this.End;
                // other + dStartStart and this are now in the same plane
                // check if other is fully on one side of this or vice versa,
                // in other words, if  this and `other + dStartStart` intersect on a shred plane
                if (vStartStart.Cross(vStartEnd).Dot(vEndStart.Cross(vEndEnd)) > 0 ||
                    vStartStart.Cross(vEndStart).Dot(vStartEnd.Cross(vEndEnd)) > 0)
                {
                    // if not intersect - shortest distance is minimum distance from a point to other line.
                    return Math.Min(Math.Min(this.Start.DistanceTo(other), this.End.DistanceTo(other)),
                                    Math.Min(other.Start.DistanceTo(this), other.End.DistanceTo(this)));
                }
            }

            return dStartStart.Length();
        }

        /// <summary>
        /// Trim a line with a polygon.
        /// </summary>
        /// <param name="polygon">The polygon to trim with.</param>
        /// <param name="outsideSegments">A list of the segment(s) of the line outside of the supplied polygon.</param>
        /// <param name="includeCoincidenceAtEdge">Include coincidence at edge as inner segment.</param>
        /// <param name="infinite">Treat the line as infinite?</param>
        /// <returns>A list of the segment(s) of the line within the supplied polygon.</returns>
        public List<Line> Trim(Polygon polygon, out List<Line> outsideSegments, bool includeCoincidenceAtEdge = false, bool infinite = false)
        {
            // adapted from http://csharphelper.com/blog/2016/01/clip-a-line-segment-to-a-polygon-in-c/
            // Make lists to hold points of intersection
            var intersections = new List<Vector3>();

            var startsOutsidePolygon = false;
            var hasVertexIntersections = false;
            var containment = Containment.Outside;

            if (!infinite)
            {
                // Add the segment's starting point.
                intersections.Add(this.Start);
                polygon.Contains(this.Start, out containment);
                startsOutsidePolygon = containment == Containment.Outside;
                hasVertexIntersections = containment == Containment.CoincidesAtVertex;
            }

            // Examine the polygon's edges.
            for (int i1 = 0; i1 < polygon.Vertices.Count; i1++)
            {
                // Get the end points for this edge.
                int i2 = (i1 + 1) % polygon.Vertices.Count;

                // See where the edge intersects the segment.
                var segment = new Line(polygon.Vertices[i1], polygon.Vertices[i2]);
                // This will return false for intersections exactly at an end if line is not infinite
                var segmentsIntersect = Intersects(segment, out Vector3 intersection, infinite);

                if (infinite)
                {
                    if (segmentsIntersect)
                    {
                        intersections.Add(intersection);
                        if (Vector3.AreCollinearByDistance(Start, End, polygon.Vertices[i1]))
                        {
                            hasVertexIntersections = true;
                        }
                    }
                }
                else
                {
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
            }

            if (!infinite)
            {
                // Add the segment's ending point.
                intersections.Add(End);
            }

            var intersectionsOrdered = intersections.OrderBy(v => (v - Start).Dot(Direction())).ToArray();
            var inSegments = new List<Line>();
            outsideSegments = new List<Line>();
            var currentlyIn = !startsOutsidePolygon;
            for (int i = 0; i < intersectionsOrdered.Length - 1; i++)
            {
                var A = intersectionsOrdered[i];
                var B = intersectionsOrdered[i + 1];
                if (A.IsAlmostEqualTo(B)) // skip duplicate points
                {
                    // it's possible that A is outside, but B is at an edge, even
                    // if they are within tolerance of each other.
                    // This can happen due to floating point error when the point is almost exactly
                    // epsilon distance from the edge.
                    // so if we have duplicate points, we have to update the containment value.
                    polygon.Contains(B, out containment);
                    continue;
                }
                var segment = new Line(A, B);
                if (hasVertexIntersections || containment == Containment.CoincidesAtEdge) // if it passed through a vertex, or started at an edge or vertex, we can't rely on alternating, so check each midpoint
                {
                    polygon.Contains((A + B) / 2, out var containmentInPolygon);
                    currentlyIn = containmentInPolygon == Containment.Inside;
                    if (includeCoincidenceAtEdge)
                    {
                        currentlyIn = currentlyIn || containmentInPolygon == Containment.CoincidesAtEdge;
                    }
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
            return Arc.Fillet(this, target, radius);
        }

        /// <summary>
        /// Check if this line is collinear with another line.
        /// </summary>
        /// <param name="line">Line to check.</param>
        /// <param name="tolerance">If points are within this distance of a fit line, they will be considered collinear.</param>
        /// <returns></returns>
        public bool IsCollinear(Line line, double tolerance = Vector3.EPSILON)
        {
            var vectors = new Vector3[] { Start, End, line.Start, line.End };
            return vectors.AreCollinearByDistance(tolerance);
        }

        /// <summary>
        /// Check if this line overlaps with another line.
        /// </summary>
        /// <param name="line">Line to check.</param>
        /// <param name="overlap">Overlapping line or null when lines do not overlap.</param>
        /// <returns>Returns true when lines overlap and false when they do not.</returns>
        public bool TryGetOverlap(Line line, out Line overlap)
        {
            return TryGetOverlap(line, Vector3.EPSILON, out overlap);
        }

        /// <summary>
        /// Check if this line overlaps with another line.
        /// </summary>
        /// <param name="line">Line to check</param>
        /// <param name="tolerance">Tolerance for distance-based checks.</param>
        /// <param name="overlap">Overlapping line or null when lines do not overlap.</param>
        /// <returns>Returns true when lines overlap and false when they do not.</returns>
        public bool TryGetOverlap(Line line, double tolerance, out Line overlap)
        {
            overlap = null;

            if (line == null)
                return false;

            if (!IsCollinear(line, tolerance))
                return false;

            //order vertices of lines
            var vectors = new List<Vector3>() { Start, End, line.Start, line.End };
            var direction = Direction();
            var orderedVectors = vectors.OrderBy(v => (v - Start).Dot(direction)).ToList();

            //check if 2nd point lies on both lines
            if (!PointOnLine(orderedVectors[1], Start, End, true, tolerance) || !PointOnLine(orderedVectors[1], line.Start, line.End, true, tolerance))
                return false;

            //check if 3rd point lies on both lines
            if (!PointOnLine(orderedVectors[2], Start, End, true, tolerance) || !PointOnLine(orderedVectors[2], line.Start, line.End, true, tolerance))
                return false;

            //edge case when lines share only point
            if (orderedVectors[1].IsAlmostEqualTo(orderedVectors[2], tolerance))
                return false;

            var overlappingLine = new Line(orderedVectors[1], orderedVectors[2]);

            //keep the same direction as original line
            overlap = direction.Dot(overlappingLine.Direction()) > 0
                ? overlappingLine
                : overlappingLine.Reversed();

            return true;
        }

        /// <summary>
        /// Calculate U parameter for point on line
        /// </summary>
        /// <param name="point">Point on line</param>
        /// <returns>Returns U parameter for point on line</returns>
        public double GetParameterAt(Vector3 point)
        {
            return GetParameterAt(point, Start, End);
        }

        /// <summary>
        /// Calculate U parameter for point between two other points
        /// </summary>
        /// <param name="point">Point for which parameter is calculated</param>
        /// <param name="start">First point</param>
        /// <param name="end">Second point</param>
        /// <returns>Returns U parameter for point between two other points</returns>
        public static double GetParameterAt(Vector3 point, Vector3 start, Vector3 end)
        {
            if (!PointOnLine(point, start, end, true))
            {
                return -1;
            }

            if (point.IsAlmostEqualTo(start))
            {
                return 0;
            }

            if (point.IsAlmostEqualTo(end))
            {
                return end.DistanceTo(start);
            }

            return point.DistanceTo(start);
        }

        /// <summary>
        /// Creates new line with vertices of current and joined line
        /// </summary>
        /// <param name="line">Collinear line</param>
        /// <returns>New line containing vertices of all merged lines</returns>
        /// <exception cref="ArgumentException">Throws exception when lines are not collinear</exception>
        public Line MergedCollinearLine(Line line)
        {
            if (!IsCollinear(line))
            {
                throw new ArgumentException("Lines needs to be collinear");
            }

            //order vertices of lines
            var vectors = new List<Vector3>() { Start, End, line.Start, line.End };
            var direction = Direction();
            var orderedVectors = vectors.OrderBy(v => (v - Start).Dot(direction)).ToList();

            var joinedLine = new Line(orderedVectors.First(), orderedVectors.Last());

            //keep the same direction as original line
            return joinedLine.Direction().IsAlmostEqualTo(Direction())
                ? joinedLine
                : joinedLine.Reversed();
        }

        /// <summary>
        /// Projects current line onto a plane
        /// </summary>
        /// <param name="plane">Plane to project</param>
        /// <returns>New line on a plane</returns>
        public Line Projected(Plane plane)
        {
            var start = Start.Project(plane);
            var end = End.Project(plane);
            return new Line(start, end);
        }

        /// <summary>
        /// Return an approximate fit line through a set of points using the least squares method.
        /// </summary>
        /// <param name="points">The points to fit. Should have at least 2 distinct points.</param>
        /// <returns>An approximate fit line through a set of points using the least squares method.
        /// If there is less than 2 distinct points, returns null.</returns>
        public static Line BestFit(IList<Vector3> points)
        {
            var distinctPoints = points.Distinct().ToList();
            if (distinctPoints.Count < 2)
            {
                return null;
            }
            else if (distinctPoints.Count == 2)
            {
                return new Line(distinctPoints[0], distinctPoints[1]);
            }

            // find the coefficients of the straight line equation (y = m * x + b) using the least squares method
            var m = FindMCoefficient(points);
            var b = FindBCoefficient(points, m);
            var areInfiniteCoefficients = double.IsInfinity(m) || double.IsInfinity(b);
            Line line = null;
            if (m.ApproximatelyEquals(0) || areInfiniteCoefficients)
            {
                // find the coefficients of the straight line equation (x = b0)
                var b0 = FindBCoefficient(points.Select(t => new Vector3(t.Y, t.X)).ToList(), 0);
                var currentLine = new Line(new Vector3(b0, 0), new Vector3(b0, 10));
                if (areInfiniteCoefficients)
                {
                    line = currentLine;
                }
                else
                {
                    var sum1 = points.Sum(t => Math.Abs(t.Y - b));
                    var sum2 = points.Sum(t => Math.Abs(t.X - b0));
                    // select currentLine, if the sum of all distances from points to this line is minimal
                    if (sum2 < sum1)
                    {
                        line = currentLine;
                    }
                }
            }
            // substitute the values x=0 and x=10 into the equation of a straight line for getting y value of points
            line = line ?? new Line(new Vector3(0, b), new Vector3(10, m * 10 + b));

            var closestPointsOnLine = points.Select(p => p.ClosestPointOn(line, true)).Select(p =>
            {
                var vector = p - line.Start;
                var parameterizedPosition = vector.Length();
                if (line.Direction().AngleTo(vector) > 90)
                {
                    parameterizedPosition *= -1;
                }
                return (p, parameterizedPosition);
            }).OrderBy(t => t.parameterizedPosition);

            var resultLine = new Line(closestPointsOnLine.First().p, closestPointsOnLine.Last().p);
            return resultLine;
        }

        /// <summary>
        /// Find the 'm' coefficient of the straight line equation (y = m * x + b) using the least squares method
        /// </summary>
        /// <param name="points">Points for which best fit line should be found.</param>
        /// <returns>The 'm' coefficient of the straight line equation.</returns>
        private static double FindMCoefficient(IList<Vector3> points)
        {
            double sumxy = points.Sum(p => p.X * p.Y);
            var sumx = points.Sum(p => p.X);
            var sumy = points.Sum(p => p.Y);
            var sumx2 = points.Sum(p => p.X * p.X);
            var m = (sumxy - sumx * sumy / points.Count) / (sumx2 - sumx * sumx / points.Count);
            return m;
        }

        /// <summary>
        /// Find the 'b' coefficient of the straight line equation (y = m * x + b) using the least squares method
        /// </summary>
        /// <param name="points">Points for which best fit line should be found.</param>
        /// <param name="m">'m' coefficient of the straight line equation.</param>
        /// <returns>The 'b' coefficient of the straight line equation.</returns>
        private static double FindBCoefficient(IList<Vector3> points, double m)
        {
            var sumx = points.Sum(p => p.X);
            var sumy = points.Sum(p => p.Y);
            var b = (sumy - m * sumx) / points.Count;
            return b;
        }

        /// <summary>
        /// Checks if line lays on plane
        /// </summary>
        /// <param name="plane">Plane to check</param>
        /// <param name="tolerance">Optional tolerance value</param>
        /// <returns>The result of check if line lays on plane</returns>
        public bool IsOnPlane(Plane plane, double tolerance = 1E-05)
        {
            return Start.DistanceTo(plane).ApproximatelyEquals(0, tolerance)
                && End.DistanceTo(plane).ApproximatelyEquals(0, tolerance);
        }

        /// <summary>
        /// A string representation of the line.
        /// </summary>
        public override string ToString()
        {
            return $"start: {Start}, end: {End}";
        }

        /// <summary>
        /// Get the parameter at a distance from the start parameter along the curve.
        /// </summary>
        /// <param name="distance">The distance from the start parameter.</param>
        /// <param name="start">The parameter from which to measure the distance.</param>
        public override double ParameterAtDistanceFromParameter(double distance, double start)
        {
            if (!Domain.Includes(start, true))
            {
                throw new Exception($"The parameter {start} is not on the trimmed portion of the basis curve. The parameter must be between {Domain.Min} and {Domain.Max}.");
            }

            if (distance == 0.0)
            {
                return start;
            }

            return start + distance;
        }

        /// <inheritdoc/>
        public override double[] GetSubdivisionParameters(double startSetbackDistance = 0,
                                                          double endSetbackDistance = 0)
        {
            return new[] { ParameterAtDistanceFromParameter(startSetbackDistance, this.Domain.Min), ParameterAtDistanceFromParameter(this.Length() - endSetbackDistance, this.Domain.Min) };
        }
    }

    /// <summary>
    /// Line extension methods.
    /// </summary>
    public static class LineExtensions
    {
        /// <summary>
        /// Offset the lines. The resulting polygon will have acute angles.
        /// </summary>
        /// <param name="lines">List of lines to offset.</param>
        /// <param name="distance">The distance to offset.</param>
        /// <returns></returns>
        public static List<Polygon> Offset(this List<Line> lines, double distance)
        {
            if (lines == null || lines.Count == 0)
                return new List<Polygon>();

            var heg = HalfEdgeGraph2d.Construct(lines, true);
            var polylines = heg.Polylinize();
            var offsets = polylines.SelectMany(l => l.OffsetWithAcuteAngle(distance / 2)).ToList();

            return offsets;
        }
    }
}