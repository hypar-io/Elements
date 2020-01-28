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
            this.End = new Vector3(1,0,0);
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
            this.End = start + direction.Normalized() * length;
        }

        /// <summary>
        /// Get a transform whose XY plane is perpendicular to the curve, and whose
        /// positive Z axis points along the curve.
        /// </summary>
        /// <param name="u">The parameter along the Line, between 0.0 and 1.0, at which to calculate the Transform.</param>
        /// <returns>A transform.</returns>
        public override Transform TransformAt(double u)
        {
            return new Transform(PointAt(u), (this.Start - this.End).Normalized());
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
        /// <returns>True if the line intersects the plane, false if no intersection occurs.</returns>
        public bool Intersects(Plane p, out Vector3 result)
        {
            result = default(Vector3);

            var d = this.Direction();

            // Test for perpendicular.
            if (p.Normal.Dot(d) == 0)
            {
                return false;
            }
            var t = (p.Normal.Dot(p.Origin) - p.Normal.Dot(this.Start)) / p.Normal.Dot(d);

            // If t > the length of the line, the point
            // of intersection is past the end of the line.
            // If t < 0, the point of intersection is behind
            // the start of the line.
            if (t > this.Length() || t < 0)
            {
                return false;
            }
            result = this.Start + d * t;
            return true;
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
            if (IsAlmostZero(a) || a > Vector3.Epsilon) return false;
            if (IsAlmostZero(b) || b > Vector3.Epsilon) return false;
            return true;
        }

        private bool IsAlmostZero(double a)
        {
            return Math.Abs(a) < Vector3.Epsilon;
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
            return (this.End - this.Start).Normalized();
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
                lines.Add(new Line(a, End));
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
            if(localLength <= l)
            {
                lines.Add(this);
                return lines; 
            }

            var divs = (int)(localLength/l);
            var span = divs * l;
            var halfSpan = span/2;
            var mid = this.PointAt(0.5);
            var dir = this.Direction();
            var start = mid - dir * halfSpan;
            var end = mid + dir * halfSpan;
            if(!this.Start.IsAlmostEqualTo(start))
            {
                lines.Add(new Line(this.Start, start));
            }
            for(var i=0; i<divs; i++)
            {
                var p1 = start + (i * l) * dir;
                var p2 = p1 + dir * l;
                lines.Add(new Line(p1, p2));
            }
            if(!this.End.IsAlmostEqualTo(end))
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
            var d1 = this.Direction();
            var d2 = line.Direction();

            var n = d2.Cross(Vector3.ZAxis);
            var p = new Plane(line.Start, n);

            if (this.Intersects(p, out Vector3 result))
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
        /// <returns>A new line, or null if this line does not intersect the trimming line.</returns>
        public Line ExtendTo(Line line)
        {
            var d1 = this.Direction();
            var d2 = line.Direction();
            
            // Extend the line and trim in one direction.
            var d3 = this.Start + d1 * 1000000;
            var temp = new Line(this.Start, d3);
            var trim = temp.TrimTo(line);
            if(trim != null)
            {
                return trim;
            }

            // Extend the line and trim in the other direction.
            d3 = this.Start - d1 * 1000000;
            temp = new Line(this.End, d3);
            trim = temp.TrimTo(line);
            return trim;
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
            if(!r1.Intersects(r2, out Vector3 result, true))
            {
                return null;
            }

            // Construct new vectors that both
            // point away from the projected intersection
            var newD1 = (this.PointAt(0.5) - result).Normalized();
            var newD2 = (target.PointAt(0.5) - result).Normalized();

            var theta = newD1.AngleTo(newD2) * Math.PI / 180.0;
            var halfTheta = theta/2.0;
            var h = radius / Math.Sin(halfTheta);
            var centerVec = newD1.Average(newD2).Normalized();
            var arcCenter = result + centerVec * h;
            
            // Find the closest points from the arc
            // center to the adjacent curves.
            var p1 = arcCenter.ClosestPointOn(this);
            var p2 = arcCenter.ClosestPointOn(target);

            // Find the angle of both segments relative to the fillet arc.
            // ATan2 assumes the origin, so correct the coordinates
            // by the offset of the center of the arc.
            var angle1 = Math.Atan2(p1.Y - arcCenter.Y, p1.X - arcCenter.X) * 180.0/Math.PI;
            var angle2 = Math.Atan2(p2.Y - arcCenter.Y, p2.X - arcCenter.X) * 180.0/Math.PI;
            
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
            if(arc.Length() < complement.Length())
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
    }
}