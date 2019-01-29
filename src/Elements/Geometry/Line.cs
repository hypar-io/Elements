using Elements.Geometry.Interfaces;
using Newtonsoft.Json;
using System;

namespace Elements.Geometry
{
    /// <summary>
    /// Line represents a linear curve between two points.
    /// </summary>
    public class Line : ICurve
    {
        /// <summary>
        /// The type of the curve.
        /// Used during deserialization to disambiguate derived types.
        /// </summary>
        [JsonProperty("type", Order = -100)]
        public string Type
        {
            get { return this.GetType().FullName.ToLower(); }
        }
        
        /// <summary>
        /// The start of the line.
        /// </summary>
        [JsonProperty("start")]
        public Vector3 Start { get; }

        /// <summary>
        /// The end of the line.
        /// </summary>
        [JsonProperty("end")]
        public Vector3 End { get; }

        /// <summary>
        /// The line's vertices.
        /// </summary>
        [JsonIgnore]
        public Vector3[] Vertices
        {
            get { return new[] { this.Start, this.End }; }
        }

        /// <summary>
        /// Calculate the length of the line.
        /// </summary>
        public double Length()
        {
            return this.Start.DistanceTo(this.End);
        }

        /// <summary>
        /// A normalized vector representing the direction of the line.
        /// </summary>
        [JsonIgnore]
        public Vector3 Direction
        {
            get
            {
                return (this.End - this.Start).Normalized();
            }
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
        /// <param name="up">The vector which will become the Y vector of the transform.</param>
        /// <returns>A transform.</returns>
        public Transform TransformAt(double u, Vector3 up = null)
        {
            return new Transform(PointAt(u), this.Start, this.End, up);
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
            return this.Start + offset * this.Direction;
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
            var offsetN = this.Direction.Cross(Vector3.ZAxis);
            var a = this.Start + (offsetN * (amount / 2));
            var b = this.End + (offsetN * (amount / 2));
            var c = this.End - (offsetN * (amount / 2));
            var d = this.Start - (offsetN * (amount / 2));
            return new Polygon(new[] { a, b, c, d });
        }

        /// <summary>
        /// Does this Line equal the provided Line?
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
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
        /// Get the hash code for the Line.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return new[] { this.Start, this.End }.GetHashCode();
        }

        /// <summary>
        /// Get a collection of Transforms which represent frames along this ICurve.
        /// </summary>
        /// <param name="startSetback">The offset from the start of the ICurve.</param>
        /// <param name="endSetback">The offset from the end of the ICurve.</param>
        /// <returns>A collection of Transforms.</returns>
        public Transform[] Frames(double startSetback, double endSetback)
        {
            var l = this.Length();
            return new Transform[] { TransformAt(0.0 + startSetback / l), TransformAt(1.0 - endSetback / l) };
        }

        /// <summary>
        /// Intersect this Line with the specified Plane 
        /// </summary>
        /// <param name="p">The Plane.</param>
        /// <returns>The point of intersection or null if no intersection occurs.</returns>
        public Vector3 Intersect(Plane p) {
            if (p.Normal.Dot(this.Direction) == 0) {
                return null;
            }
            var t = (p.Normal.Dot(p.Origin) - p.Normal.Dot(this.Start)) / p.Normal.Dot(this.Direction);
            if(t > this.Length())
            {
                return null;
            }
            return this.Start + this.Direction * t;
        }
    }
}