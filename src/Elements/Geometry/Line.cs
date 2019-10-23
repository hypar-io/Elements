using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Elements.Geometry
{
    /// <summary>
    /// A linear curve between two points.
    /// </summary>
    public partial class Line : Curve
    {
        /// <summary>
        /// Calculate the length of the line.
        /// </summary>
        public override double Length()
        {
            return this.Start.DistanceTo(this.End);
        }

        /// <summary>
        /// Construct a line from start and end points.
        /// </summary>
        /// <param name="start">The start of the line.</param>
        /// <param name="end">The end of the line.</param>
        /// <exception cref="System.ArgumentException">Thrown when the start and end points are the same.</exception>
        [JsonConstructor]
        public Line(Vector3 start, Vector3 end)
        {
            if (start.IsAlmostEqualTo(end))
            {
                throw new ArgumentException($"The line could not be created. The start and end points of the line cannot be the same: start {start}, end {end}");
            }
            this.Start = start;
            this.End = end;
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
            this.End = start + direction.Normalized()*length;
        }

        /// <summary>
        /// Get a transform whose XY plane is perpendicular to the curve, and whose
        /// positive Z axis points along the curve.
        /// </summary>
        /// <param name="u">The parameter along the Line, between 0.0 and 1.0, at which to calculate the Transform.</param>
        /// <param name="rotation">An optional rotation in degrees around the transform's z axis.</param>
        /// <returns>A transform.</returns>
        public override Transform TransformAt(double u, double rotation = 0.0)
        {
            return new Transform(PointAt(u), (this.Start-this.End).Normalized(), rotation);
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
            if(Start.Z != End.Z)
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
        /// Does this line equal the provided line?
        /// </summary>
        /// <param name="obj">The target line.</param>
        /// <returns>True if the start and end points of the lines are equal, otherwise false.</returns>
        public override bool Equals(object obj)
        {
            var line = obj as Line;
            if (line == null)
            {
                return false;
            }
            return this.Start.Equals(line.Start) && this.End.Equals(line.End);
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
        /// Get a collection of transforms which represent frames along this line.
        /// </summary>
        /// <param name="startSetback">The offset from the start of the line.</param>
        /// <param name="endSetback">The offset from the end of the line.</param>
        /// <param name="rotation">An optional rotation in degrees around all the frames' z axes.</param>
        /// <returns>A collection of transforms.</returns>
        public override Transform[] Frames(double startSetback, double endSetback, double rotation = 0.0)
        {
            var l = this.Length();
            return new Transform[] { TransformAt(0.0 + startSetback / l, rotation), TransformAt(1.0 - endSetback / l, rotation) };
        }

        /// <summary>
        /// Intersect this line with the specified plane 
        /// </summary>
        /// <param name="p">The plane.</param>
        /// <returns>The point of intersection or null if no intersection occurs.</returns>
        public Vector3 Intersect(Plane p) {
            var d = this.Direction();

            // Test for perpendicular.
            if (p.Normal.Dot(d) == 0) {
                return null;
            }
            var t = (p.Normal.Dot(p.Origin) - p.Normal.Dot(this.Start)) / p.Normal.Dot(d);
            if(t > this.Length())
            {
                return null;
            }
            return this.Start + d * t;
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
            if (IsAlmostZero(a) || a > Vector3.Tolerance ) return false;
            if (IsAlmostZero(b) || b > Vector3.Tolerance ) return false;
            return true;
        }

        private bool IsAlmostZero(double a)
        {
            return Math.Abs(a) < Vector3.Tolerance;
        }

        /// <summary>
        /// Get the bounding box for this line.
        /// </summary>
        /// <returns>A bounding box for this line.</returns>
        public override BBox3 Bounds()
        {
            if(this.Start < this.End)
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
        /// A list of vertices describing the arc for rendering.
        /// </summary>
        internal override IList<Vector3> RenderVertices()
        {
            return new []{this.Start, this.End};
        }
    }
}