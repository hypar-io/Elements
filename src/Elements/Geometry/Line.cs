using Elements.Geometry.Interfaces;
using Newtonsoft.Json;
using System;

namespace Elements.Geometry
{
    /// <summary>
    /// A linear curve between two points.
    /// </summary>
    public class Line : ICurve
    {
        /// <summary>
        /// The type of the curve.
        /// Used during deserialization to disambiguate derived types.
        /// </summary>
        [JsonProperty(Order = -100)]
        public string Type
        {
            get { return this.GetType().FullName.ToLower(); }
        }
        
        /// <summary>
        /// The start of the line.
        /// </summary>
        public Vector3 Start { get; }

        /// <summary>
        /// The end of the line.
        /// </summary>
        public Vector3 End { get; }

        /// <summary>
        /// Calculate the length of the line.
        /// </summary>
        public double Length()
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
        /// <returns>A transform.</returns>
        public Transform TransformAt(double u)
        {
            return new Transform(PointAt(u), this.Start, this.End, null);
        }

        /// <summary>
        /// Get a point along the line at parameter u.
        /// </summary>
        /// <param name="u"></param>
        /// <returns>A point on the curve at parameter u.</returns>
        public Vector3 PointAt(double u)
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
        /// <returns></returns>
        public ICurve Reversed()
        {
            return new Line(End, Start);
        }

        /// <summary>
        /// Thicken a line by the specified amount.
        /// </summary>
        /// <param name="amount">The amount to thicken the line.</param>
        /// <returns></returns>
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
        /// <returns>A collection of transforms.</returns>
        public Transform[] Frames(double startSetback, double endSetback)
        {
            var l = this.Length();
            return new Transform[] { TransformAt(0.0 + startSetback / l), TransformAt(1.0 - endSetback / l) };
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
        /// Get the bounding box for this line.
        /// </summary>
        /// <returns>A bounding box for this line.</returns>
        public BBox3 Bounds()
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
    }
}