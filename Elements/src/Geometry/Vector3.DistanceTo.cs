
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
        /// The distance from this point to the plane.
        /// The distance will be negative when this point lies
        /// "behind" the plane.
        /// </summary>
        /// <param name="p">The plane.</param>
        public double DistanceTo(Plane p)
        {
            var d = p.Origin.Dot(p.Normal);
            return this.Dot(p.Normal) - d;
        }

        /// <summary>
        /// Find the distance from this point to the line, and output the location 
        /// of the closest point on that line.
        /// Using formula from https://diego.assencio.com/?index=ec3d5dfdfc0b6a0d147a656f0af332bd
        /// </summary>
        /// <param name="line">The line to find the distance to.</param>
        /// <param name="closestPoint">The point on the line that is closest to this point.</param>
        public double DistanceTo(Line line, out Vector3 closestPoint)
        {
            var lambda = (this - line.Start).Dot(line.End - line.Start) / (line.End - line.Start).Dot(line.End - line.Start);
            if (lambda >= 1)
            {
                closestPoint = line.End;
                return this.DistanceTo(line.End);
            }
            else if (lambda <= 0)
            {
                closestPoint = line.Start;
                return this.DistanceTo(line.Start);
            }
            else
            {
                closestPoint = (line.Start + lambda * (line.End - line.Start));
                return this.DistanceTo(closestPoint);
            }
        }

        /// <summary>
        /// Find the distance from this point to the line.
        /// </summary>
        /// <param name="line"></param>
        public double DistanceTo(Line line)
        {
            return DistanceTo(line, out var _);
        }

        /// <summary>
        /// Find the shortest distance from this point to any point on the
        /// polyline, and output the location of the closest point on that polyline.
        /// </summary>
        /// <param name="polyline">The polyline for computing the distance.</param>
        /// <param name="closestPoint">The point on the polyline that is closest to this point.</param>
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

        /// <summary>
        /// Find the shortest distance from this point to any point on the
        /// polyline, and output the location of the closest point on that polyline.
        /// </summary>
        /// <param name="polyline">The polyline for computing the distance.</param>
        public double DistanceTo(Polyline polyline)
        {
            var closest = double.MaxValue;

            foreach (var line in polyline.Segments())
            {
                var distance = this.DistanceTo(line);
                if (distance < closest)
                {
                    closest = distance;
                }
            }

            return closest;
        }
    }
}