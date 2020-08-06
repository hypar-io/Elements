using System;
using System.Collections.Generic;

namespace Elements.Geometry
{
    /// <summary>
    /// A cartesian plane.
    /// </summary>
    public partial class Plane: IEquatable<Plane>
    {
        /// <summary>
        /// Construct a plane by three points.
        /// The plane is constructed as a->b * b->c.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <exception>Thrown when any of a, b, or c are null.</exception>
        public Plane(Vector3 a, Vector3 b, Vector3 c)
        {
            this.Origin = a;
            var ab = (b-a).Unitized();
            var bc = (c-a).Unitized();
            this.Normal = ab.Cross(bc).Unitized();
        }

        /// <summary>
        /// Construct a plane.
        /// Only the first three points of the points array will be used.
        /// </summary>
        /// <param name="origin">The origin of the plane.</param>
        /// <param name="points">An array of vectors to be used to determine the normal of the plane.</param>
        /// <exception>Thrown when less than three points are provided.</exception>
        /// <exception>Thrown when coincident points are provided.</exception>
        public Plane(Vector3 origin, IList<Vector3> points)
        {
            if(points.Count < 3)
            {
                throw new ArgumentException("The plane could not be created. You must supply a minimum of 3 points.");
            }
            if(points[0].Equals(points[1]) || points[0].Equals(points) || points[1].Equals(points[2]))
            {
                throw new ArgumentException("The plane could not be created. The points must not be coincident.");
            }
            this.Origin = origin;
            this.Normal = (points[0]-points[1]).Unitized().Cross((points[2] - points[0]).Unitized());
        }

        /// <summary>
        /// Find the closest point on this plane from a given sample point.
        /// </summary>
        /// <param name="point">The sample point.</param>
        /// <returns>The closest point to the sample point on this plane.</returns>
        public Vector3 ClosestPoint(Vector3 point)
        {
            var fromPointToOrigin = Origin - point;
            var projectionVector = fromPointToOrigin.Dot(Normal) * Normal;
            return point + projectionVector;
            
        }

        /// <summary>
        /// Find the signed distance from a sample point to a plane.
        /// If positive, the point is on the "Normal" side of the plane,
        /// otherwise it is on the opposite side. 
        /// </summary>
        /// <param name="point">The sample point.</param>
        /// <returns>The signed distance between this plane and the sample point.</returns>
        public double SignedDistanceTo(Vector3 point)
        {
            var fromOriginToPoint = point - Origin;
            return fromOriginToPoint.Dot(Normal);
        }

        /// <summary>
        /// Is this plane equal to the provided plane?
        /// </summary>
        /// <param name="other">The plane to test.</param>
        /// <returns>Returns true if the two planes are equal, otherwise false.</returns>
        public bool Equals(Plane other)
        {
            if(other == null)
            {
                return false;
            }
            return this.Normal.Equals(other.Normal) && this.Origin.Equals(other.Origin);
        }
    }
}