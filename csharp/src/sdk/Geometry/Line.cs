using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Hypar.Geometry
{
    /// <summary>
    /// Line represents a linear curve between two points.
    /// </summary>
    public class Line : ICurve
    {
        /// <summary>
        /// The start of the line.
        /// </summary>
        [JsonProperty("start")]
        public Vector3 Start{get;}

        /// <summary>
        /// The end of the line.
        /// </summary>
        [JsonProperty("end")]
        public Vector3 End{get;}

        /// <summary>
        /// The line's vertices.
        /// </summary>
        [JsonIgnore]
        public IList<Vector3> Vertices
        {
            get{return new[]{this.Start, this.End};}
        }
        
        /// <summary>
        /// Get the length of the line.
        /// </summary>
        [JsonProperty("length")]
        public double Length
        {
            get{return this.Start.DistanceTo(this.End);}
        }

        /// <summary>
        /// Get a normalized vector representing the direction of the line.
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
                throw new ArgumentException($"The line could not be constructed. The start and end points of the line cannot be the same: start {start}, end {end}");
            }
            this.Start = start;
            this.End = end;
        }

        /// <summary>
        /// Get a transform whose XY plane is perpendicular to the curve, and whose
        /// positive Z axis points along the curve.
        /// </summary>
        /// <param name="u">The parameter along the Line, between 0.0 and 1.0, at which to calculate the Transform.</param>
        /// <param name="up">The vector which will become the Y vector of the transform.</param>
        /// <returns>A transform.</returns>
        public Transform GetTransform(double u, Vector3 up = null)
        {
            var v = Direction;
            var x = new Vector3(1, 0, 0);
            var y = new Vector3(0, 1, 0);
            var z = new Vector3(0, 0, 1);

            if (up == null)
            {
                up = z;
            }
            if (up.IsParallelTo(v))
            {
                if(v.IsAlmostEqualTo(x))
                {
                    up = y;
                }
                else
                {
                    up = x;
                }
            }

            var xAxis = v.Cross(up).Normalized();
            var t = new Transform(PointAt(u), xAxis, v);

            return t;
        }

        /// <summary>
        /// Get a point along the line at parameter u.
        /// </summary>
        /// <param name="u"></param>
        /// <returns>A point on the curve at parameter u.</returns>
        public Vector3 PointAt(double u)
        {
            if (u > 1.0 || u < 0.0)
            {
                throw new Exception("The parameter t must be between 0.0 and 1.0.");
            }
            var offset = this.Length* u;
            return this.Start + offset * this.Direction;
        }

        /// <summary>
        /// Get a new line that is the reverse of the original line.
        /// </summary>
        /// <returns></returns>
        public Line Reversed()
        {
            return new Line(End, Start);
        }

        /// <summary>
        /// Tessellate the curve.
        /// </summary>
        /// <returns>A collection of points sampled along the curve.</returns>
        public IList<IList<Vector3>> Curves()
        {
            return new[]{new[]{this.Start, this.End}};
        }

        /// <summary>
        /// Thicken a line by the specified amount.
        /// </summary>
        /// <param name="amount">The amount to thicken the line.</param>
        /// <returns></returns>
        public Polygon Thicken(double amount)
        {
            var offsetN = this.Direction.Cross(Vector3.ZAxis);
            var a = this.Start + (offsetN * (amount/2));
            var b = this.End + (offsetN * (amount/2));
            var c = this.End - (offsetN * (amount/2));
            var d = this.Start - (offsetN * (amount/2));
            return new Polygon(new[]{a, b, c, d});
        }

        /// <summary>
        /// Does this Line equal the provided Line?
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            var line = obj as Line;
            if(line == null)
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
            return new[]{this.Start, this.End}.GetHashCode();
        }
    }
}