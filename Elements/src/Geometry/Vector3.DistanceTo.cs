
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
        /// Find the distance from this vector to the line, and output the location 
        /// of the closest point on that line.
        /// </summary>
        /// <param name="line">The line to find the distance to.</param>
        /// <param name="closestPoint">The point on the line that is closest to this point.</param>
        public double DistanceTo(Line line, out Vector3 closestPoint)
        {
            var p = new Plane(this, line.Direction());
            {

                if (line.Intersects(p, out closestPoint))
                {
                    // If the line intersect the plane originating at this point
                    // then the shortest distance is the distance to that intersection.
                    return this.DistanceTo(closestPoint);
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
        }

        /// <summary>
        /// Find the distance from this vector to the line.
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public double DistanceTo(Line line)
        {
            return DistanceTo(line, out var ignoredPoint);
        }

        /// <summary>
        /// Find the shortest distance from this vector to any point on the
        /// polyline, and output the location of the closest point on that polyline.
        /// </summary>
        /// <param name="polyline">The polyline to </param>
        /// <param name="closestPoint">The point on the polyline that is closest to this point.</param>
        /// <returns></returns>
        public double DistanceTo(Polyline polyline, out Vector3 closestPoint)
        {
            var closest = double.MaxValue;
            closestPoint = default(Vector3);

            foreach (var line in polyline.Segments())
            {
                var distance = this.DistanceTo(line, out var thisClosestPoint);
                if (distance < closest)
                {
                    closest = distance;
                    closestPoint = thisClosestPoint;
                }
            }

            return closest;
        }
    }
}