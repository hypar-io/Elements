
using System;

namespace Elements.Geometry
{
    /// <summary>
    /// A 3D vector.
    /// </summary>
    public partial struct Vector3
    {
        /// <summary>
        /// The distance from this point to b.
        /// </summary>
        /// <param name="v">The target vector.</param>
        /// <returns>The distance between this vector and the provided vector.</returns>
        public double DistanceTo(Vector3 v)
        {
            return Math.Sqrt(Math.Pow(this.X - v.X, 2) + Math.Pow(this.Y - v.Y, 2) + Math.Pow(this.Z - v.Z, 2));
        }

        /// <summary>
        /// The distance from this vector to the plane.
        /// The distance will be negative when this vector lies
        /// "behind" the plane.
        /// </summary>
        /// <param name="p">The plane.</param>
        public double DistanceTo(Plane p)
        {
            var d = p.Origin.Dot(p.Normal);
            return this.Dot(p.Normal) - d;
        }

        /// <summary>
        /// The distance from this vector to the line.
        /// </summary>
        /// <param name="line">The line to find the distance to.</param>
        public double DistanceTo(Line line)
        {
            var p = new Plane(this, line.Direction());

            if (line.Intersects(p, out var hit))
            {
                // If the line intersect the plane originating at this point
                // then the shortest distance is the distance to that intersection.
                return this.DistanceTo(hit);
            }
            else
            {
                // If the line does not intersect that plane, then the shortest 
                // distance is the distance from the point to the closest end of the line.
                var distFromStart = this.DistanceTo(line.Start);
                var distFromEnd = this.DistanceTo(line.End);
                return distFromStart < distFromEnd ? distFromStart : distFromEnd;
            }
        }

        /// <summary>
        /// The shortest distance from this vector to any point on the polyline.
        /// </summary>
        /// <param name="polyline">The polyline to </param>
        /// <returns></returns>
        public double DistanceTo(Polyline polyline)
        {
            var closest = double.MaxValue;
            foreach (var line in polyline.Segments())
            {
                var distance = this.DistanceTo(line);
                closest = distance < closest ? distance : closest;
            }
            return closest;
        }
    }
}