using System;
using System.Collections.Generic;
using Elements.Validators;
using Newtonsoft.Json;

namespace Elements.Geometry
{
    /// <summary>
    /// A cartesian plane.
    /// </summary>
    public partial class Plane : IEquatable<Plane>
    {
        /// <summary>The origin of the plane.</summary>
        [JsonProperty("Origin", Required = Required.Always)]
        public Vector3 Origin { get; set; }

        /// <summary>The normal of the plane.</summary>
        [JsonProperty("Normal", Required = Required.Always)]
        public Vector3 Normal { get; set; }

        /// <summary>
        /// Construct a plane.
        /// </summary>
        /// <param name="origin">The origin of the plane.</param>
        /// <param name="normal">The normal of the plane.</param>
        [JsonConstructor]
        public Plane(Vector3 @origin, Vector3 @normal)
        {
            if (!Validator.DisableValidationOnConstruction)
            {
                if (normal.IsZero())
                {
                    throw new ArgumentException($"The plane could not be constructed. The normal, {normal}, has zero length.");
                }
            }

            this.Origin = @origin;
            this.Normal = @normal;

            if (!Validator.DisableValidationOnConstruction)
            {
                if (!this.Normal.IsUnitized())
                {
                    this.Normal = this.Normal.Unitized();
                }
            }
        }

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
            var ab = (b - a).Unitized();
            var bc = (c - a).Unitized();
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
            if (points.Count < 3)
            {
                throw new ArgumentException("The plane could not be created. You must supply a minimum of 3 points.");
            }
            if (points[0].Equals(points[1]) || points[0].Equals(points) || points[1].Equals(points[2]))
            {
                throw new ArgumentException("The plane could not be created. The points must not be coincident.");
            }
            this.Origin = origin;
            this.Normal = (points[0] - points[1]).Unitized().Cross((points[2] - points[0]).Unitized());
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
            if (other == null)
            {
                return false;
            }
            return this.Normal.Equals(other.Normal) && this.Origin.Equals(other.Origin);
        }

        /// <summary>
        /// Is this plane coplanar with the provided plane?
        /// This method assumes that both planes have unit length normals.
        /// </summary>
        /// <param name="plane">The plane to test.</param>
        /// <returns>True if the plane is coplanar, otherwise false.</returns>
        public bool IsCoplanar(Plane plane)
        {
            var dot = Math.Abs(this.Normal.Dot(plane.Normal));
            return dot.ApproximatelyEquals(1) && this.Origin.DistanceTo(plane).ApproximatelyEquals(0);
        }

        /// <summary>
        /// Does this plane intersect the other two provided planes.
        /// </summary>
        /// <param name="a">The second plane.</param>
        /// <param name="b">The third plane.</param>
        /// <param name="result">The location of intersection.</param>
        /// <returns>True if an intersection exists, otherwise false.</returns>
        public bool Intersects(Plane a, Plane b, out Vector3 result)
        {
            var d1 = this.Origin.Dot(this.Normal);
            var d2 = a.Origin.Dot(a.Normal);
            var d3 = b.Origin.Dot(b.Normal);
            var denom = (this.Normal.Cross(a.Normal)).Dot(b.Normal);
            if (denom.ApproximatelyEquals(0))
            {
                // If any pair of planes is parallel,
                // then there is no intersection.
                result = default(Vector3);
                return false;
            }
            var num = d1 * (a.Normal.Cross(b.Normal)) + d2 * (b.Normal.Cross(this.Normal)) + d3 * (this.Normal.Cross(a.Normal));
            result = num / denom;
            return true;
        }

        /// <summary>
        /// Does this plane intersect the provided edge?
        /// </summary>
        /// <param name="edge">The edge to intersect.</param>
        /// <param name="result">The intersection.</param>
        /// <returns>True if an intersection occurs, otherwise false.</returns>
        public bool Intersects((Vector3 from, Vector3 to) edge, out Vector3 result)
        {
            if (Line.Intersects(this, edge.from, edge.to, out result))
            {
                return true;
            }
            result = default;
            return false;
        }

        /// <summary>
        /// Does this plane intersect the other plane?
        /// </summary>
        /// <param name="other">The other plane.</param>
        /// <param name="result">The line of intersection.</param>
        /// <returns>True if an intersection exists, otherwise false.</returns>
        public bool Intersects(Plane other, out InfiniteLine result)
        {
            var cross = this.Normal.Cross(other.Normal);
            if (cross.IsZero())
            {
                result = default;
                return false;
            }

            var dir = other.Normal.Cross(cross);
            var distance = this.Normal.Dot(dir);
            Vector3 planeDelta = Origin - other.Origin;
            var t = Normal.Dot(planeDelta) / distance;
            var p = other.Origin + t * dir;

            result = new InfiniteLine(p, cross);
            return true;
        }

        /// <summary>
        /// The world XY Plane.
        /// </summary>
        public static Plane XY => new Plane(Vector3.Origin, Vector3.ZAxis);

        /// <summary>
        /// The world YZ Plane.
        /// </summary>
        public static Plane YZ => new Plane(Vector3.Origin, Vector3.XAxis);

        /// <summary>
        /// The world XZ Plane.
        /// </summary>
        public static Plane XZ => new Plane(Vector3.Origin, Vector3.YAxis);

    }
}