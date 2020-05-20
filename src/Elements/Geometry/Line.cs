using Elements.Geometry.Interfaces;
using System;
using System.Collections.Generic;

namespace Elements.Geometry
{
    /// <summary>
    /// A linear curve between two points.
    /// </summary>
    /// <example>
    /// [!code-csharp[Main](../../test/Elements.Tests/LineTests.cs?name=example)]
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
            if (u == 0.0)
            {
                return this.Start;
            }

            if (u == 1.0)
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

        public override ICurve Transformed(Transform transform)
        {
            return new Line(transform.OfPoint(this.Start), transform.OfPoint(this.End));
        }

        /// <summary>
        /// A transformed copy of this Line.
        /// </summary>
        /// <param name="transform">The transform to apply.</param>
        public override ICurve Transformed(Transform transform)
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
                if (infinite || t <= Length())
                {
                    result = location;
                    return true;
                }
            }
            else if (infinite)
            {
                var rayIntersectsBackwards = new Ray(End, Direction().Negate()).Intersects(p, out Vector3 location2, out double t2);
                if(rayIntersectsBackwards)
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
        /// <param name="infinite">Treat the lines as infinite?</param>
        /// <param name="result"></param>
        /// <returns>True if the lines intersect, false if they are fully collinear or do not intersect.</returns>
        public bool Intersects(Line l, out Vector3 result, bool infinite = false)
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
            if (Intersects(intersectionPlane, out Vector3 planeIntersectionResult, infinite))
            {
                result = planeIntersectionResult;

                return true;

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
        /// Divide the line into n+1 equal segments.
        /// </summary>
        /// <param name="n">The number of segments.</param>
        public List<Line> DivideByCount(int n)
        {
            if (n < 0)
            {
                throw new ArgumentException($"The number of divisions must be greater than 0.");
            }
            var lines = new List<Line>();
            var div = 1.0 / (n + 1);
            for (var t = 0.0; t <= 1.0 - div; t += div)
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
}