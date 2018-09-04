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
        /// <returns></returns>
        [JsonProperty("start")]
        public Vector3 Start{get;}

        /// <summary>
        /// The end of the line.
        /// </summary>
        /// <returns></returns>
        [JsonProperty("end")]
        public Vector3 End{get;}

        /// <summary>
        /// Construct a line from start and end points.
        /// </summary>
        /// <param name="start">The start of the line.</param>
        /// <param name="end">The end of the line.</param>
        public Line(Vector3 start, Vector3 end)
        {
            if (start.IsAlmostEqualTo(end))
            {
                throw new Exception("The start and end of the Line cannot be the same.");
            }
            this.Start = start;
            this.End = end;
        }

        /// <summary>
        /// Construct a line from a start point, pointing in a direction, with the specified length.
        /// </summary>
        /// <param name="start">The start of the line.</param>
        /// <param name="direction">The direction of the line.</param>
        /// <param name="length">The length of the line.</param>
        public Line(Vector3 start, Vector3 direction, double length)
        {
            this.Start = start;
            this.End = start + direction.Normalized() * length;
        }
        
        /// <summary>
        /// Get the length of the line.
        /// </summary>
        /// <returns></returns>
        public double Length()
        {
            return Math.Sqrt(Math.Pow(this.Start.X - this.End.X, 2) + Math.Pow(this.Start.Y - this.End.Y, 2) + Math.Pow(this.Start.Z - this.End.Z, 2));
        }

        /// <summary>
        /// Get a transform whose XY plane is perpendicular to the curve, and whose
        /// positive Z axis points along the curve.
        /// </summary>
        /// <param name="up">The vector which will become the Y vector of the transform.</param>
        /// <returns>A transform.</returns>
        public Transform GetTransform(Vector3 up = null)
        {
            var v = Direction();
            var x = new Vector3(1, 0, 0);
            var y = new Vector3(0, 1, 0);
            var z = new Vector3(0, 0, 1);

            if (up == null)
            {
                up = z;
            }
            if (up.IsParallelTo(v))
            {
                up = x;
            }

            var xAxis = v.Cross(up).Normalized();
            var t = new Transform(this.Start, xAxis, v);

            return t;
        }

        /// <summary>
        /// Get a normalized vector representing the direction of the line.
        /// </summary>
        /// <returns>A vector representing the direction of the line.</returns>
        public Vector3 Direction()
        {
            return (this.End - this.Start).Normalized();
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
            var offset = this.Length() * u;
            return this.Start + offset * this.Direction();
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
        public IEnumerable<Vector3> Tessellate()
        {
            return new[]{Start, End};
        }

        /// <summary>
        /// Thicken a line by the specified amount.
        /// </summary>
        /// <param name="amount">The amount to thicken the line.</param>
        /// <returns></returns>
        public Polyline Thicken(double amount)
        {
            var offsetN = this.Direction().Cross(Vector3.ZAxis());
            var a = this.Start + (offsetN * (amount/2));
            var b = this.End + (offsetN * (amount/2));
            var c = this.End - (offsetN * (amount/2));
            var d = this.Start - (offsetN * (amount/2));
            return new Polyline(new[]{a,b,c,d});
        }
    }
}