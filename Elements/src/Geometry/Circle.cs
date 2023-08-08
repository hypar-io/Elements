using System;
using System.Collections.Generic;
using System.Linq;
using Elements.Geometry.Interfaces;
using Newtonsoft.Json;

namespace Elements.Geometry
{
    /// <summary>
    /// A circle. 
    /// Parameterization of the circle is 0 -> 2PI.
    /// </summary>
    public class Circle : Curve, IConic
    {
        /// <summary>The center of the circle.</summary>
        [JsonProperty("Center", Required = Required.AllowNull)]
        public Vector3 Center
        {
            get
            {
                return this.Transform.Origin;
            }
        }

        /// <summary>The radius of the circle.</summary>
        [JsonProperty("Radius", Required = Required.Always)]
        [System.ComponentModel.DataAnnotations.Range(0.0D, double.MaxValue)]
        public double Radius { get; protected set; }

        /// <summary>
        /// The coordinate system of the plane containing the circle.
        /// </summary>
        public Transform Transform { get; protected set; }

        /// <summary>The normal direction of the circle.</summary>
        [JsonIgnore]
        public Vector3 Normal
        {
            get
            {
                return this.Transform.ZAxis;
            }
        }

        /// <summary>
        /// Construct a circle.
        /// </summary>
        /// <param name="center">The center of the circle.</param>
        /// <param name="radius">The radius of the circle.</param>
        [JsonConstructor]
        public Circle(Vector3 center, double radius = 1.0)
        {
            this.Radius = radius;
            this.Transform = new Transform(center);
        }

        /// <summary>
        /// Construct a circle.
        /// </summary>
        /// <param name="radius">The radius of the circle.</param>
        public Circle(double radius = 1.0)
        {
            this.Radius = radius;
            this.Transform = new Transform();
        }

        /// <summary>
        /// Construct a circle.
        /// </summary>
        public Circle(Transform transform, double radius = 1.0)
        {
            this.Transform = transform;
            this.Radius = radius;
        }

        /// <summary>
        /// Create a polygon through a set of points along the circle.
        /// </summary>
        /// <param name="divisions">The number of divisions of the circle.</param>
        /// <returns>A polygon.</returns>
        public Polygon ToPolygon(int divisions = 10)
        {
            var pts = new List<Vector3>();
            var twoPi = Math.PI * 2;
            var step = twoPi / divisions;
            // We use epsilon here because for larger numbers of divisions,
            // we can creep right up to 2pi, close enough that we'll
            // get a point at not exactly 2pi, but other code will see the point
            // found as equivalent to the point at parameter 0.
            for (var t = 0.0; t < twoPi - Vector3.EPSILON; t += step)
            {
                pts.Add(this.PointAt(t));
            }
            return new Polygon(pts, true);
        }

        /// <summary>
        /// Convert a circle to a circular arc.
        /// </summary>
        public static implicit operator Arc(Circle c) => new Arc(c, 0, Math.PI * 2);

        /// <summary>
        /// Convert a circle to a circular model curve.
        /// </summary>
        /// <param name="c">The bounded curve to convert.</param>
        public static implicit operator ModelCurve(Circle c) => new ModelCurve(c);

        /// <summary>
        /// Return the point at parameter u on the arc.
        /// </summary>
        /// <param name="u">A parameter on the arc.</param>
        /// <returns>A Vector3 representing the point along the arc.</returns>
        public override Vector3 PointAt(double u)
        {
            return Transform.OfPoint(PointAtUntransformed(u));
        }

        private Vector3 PointAtUntransformed(double u)
        {
            var x = this.Radius * Math.Cos(u);
            var y = this.Radius * Math.Sin(u);
            return new Vector3(x, y);
        }

        public bool ParameterAt(Vector3 pt, out double parameter)
        {
            var local = Transform.Inverted().OfPoint(pt);
            if (local.Z.ApproximatelyEquals(0) &&
                local.LengthSquared().ApproximatelyEquals(
                    Radius * Radius, Vector3.EPSILON * Vector3.EPSILON))
            {
                parameter = ParameterAtUntransformed(local);
                return true;
            }

            parameter = 0;
            return false;
        }

        private double ParameterAtUntransformed(Vector3 pt)
        {
            var v = (pt - this.Center) / Radius;
            return Math.Atan2(v.Y, v.X);
        }

        /// <summary>
        /// Return transform on the arc at parameter u.
        /// </summary>
        /// <param name="u">A parameter on the arc.</param>
        /// <returns>A transform with its origin at u along the curve and its Z axis tangent to the curve.</returns>
        public override Transform TransformAt(double u)
        {
            var p = PointAtUntransformed(u);
            var x = (p - Vector3.Origin).Unitized();
            var y = Vector3.ZAxis;
            return new Transform(p, x, y, x.Cross(y)).Concatenated(this.Transform);
        }

        /// <inheritdoc/>
        public override Curve Transformed(Transform transform)
        {
            return new Circle(transform.Concatenated(this.Transform), this.Radius);
        }

        /// <summary>
        /// Get the parameter at a distance from the start parameter along the curve.
        /// </summary>
        /// <param name="distance">The distance from the start parameter.</param>
        /// <param name="start">The parameter from which to measure the distance.</param>
        public override double ParameterAtDistanceFromParameter(double distance, double start)
        {
            if (distance == 0.0)
            {
                return start;
            }

            // s = r * theta
            // theta = s/r
            var theta = distance / this.Radius;

            return start + theta;
        }

        public bool Intersects(Circle other, out List<Vector3> results)
        {
            results = new List<Vector3>();

            Plane planeA = new Plane(Center, Normal);
            Plane planeB = new Plane(other.Center, other.Normal);

            // Check if two circles are on the same plane. 
            if (Normal.IsAlmostEqualTo(other.Normal) &&
                other.Center.DistanceTo(planeA).ApproximatelyEquals(0))
            {
                var delta = other.Center - Center;
                var dist = delta.Length();
                // Check if circles are on correct distance for intersection to happen.
                if (dist.ApproximatelyEquals(0) ||
                    dist > Radius + other.Radius || dist < Math.Abs(Radius - other.Radius))
                {
                    return false;
                }

                // Build triangle with center of one circle and two intersection points.
                var r1squre = Radius * Radius;
                var r2squre = other.Radius * other.Radius;
                var lineDist = (r1squre - r2squre + dist * dist) / (2 * dist);
                var linePoint = Center + lineDist * delta.Unitized();
                double perpDistance = Math.Sqrt(r1squre - lineDist * lineDist);
                // If triangle side is 0 - circles touches. Only one intersection recorded.
                if (perpDistance.ApproximatelyEquals(0))
                {
                    results.Add(linePoint);
                }
                else
                {
                    Vector3 perpDirection = delta.Cross(Normal).Unitized();
                    results.Add(linePoint + perpDirection * perpDistance);
                    results.Add(linePoint - perpDirection * perpDistance);
                }
            }
            // Ignore circles on parallel planes.
            // Find intersection line between two planes.
            else if (planeA.Intersects(planeB, out var line) &&
                     Intersects(line, out var candidates))
            {
                foreach (var item in candidates)
                {
                    // Check each point that lays on intersection line and one of the circles.
                    // They are on both if they have correct distance to circle centers.
                    if (item.DistanceTo(other.Center).ApproximatelyEquals(other.Radius))
                    {
                        results.Add(item);
                    }
                }
            }

            return results.Any();
        }

        public bool Intersects(InfiniteLine line, out List<Vector3> results)
        {
            results = new List<Vector3>();

            Plane circlePlane = new Plane(Center, Normal);
            Vector3 closestPoint;
            bool lineOnPlane = line.Origin.DistanceTo(circlePlane).ApproximatelyEquals(0) &&
                line.Direction.Dot(Normal).ApproximatelyEquals(0);

            // If line share a plane with circle - find closest point on it to circle center.
            // If not - check if there an intersection between line and circle plane.
            if (lineOnPlane)
            {
                closestPoint = Center.ClosestPointOn(line);
            }
            else if (!line.Intersects(circlePlane, out closestPoint))
            {
                return false;
            }

            var delta = closestPoint - Center;
            var lengthSquared = delta.LengthSquared();
            var radiusSquared = Radius * Radius;
            var toleranceSquared = Vector3.EPSILON * Vector3.EPSILON;
            // if line not on circle plane - only one intersection is possible if it's radius away.
            // this will also happen if line is on plane but only touches the circle.
            if (lengthSquared.ApproximatelyEquals(radiusSquared, toleranceSquared))
            {
                results.Add(closestPoint);
            }
            else if (lineOnPlane && lengthSquared < radiusSquared)
            {
                var distance = Math.Sqrt(radiusSquared - lengthSquared);
                results.Add(closestPoint + line.Direction * distance);
                results.Add(closestPoint - line.Direction * distance);
            }

            return results.Any();
        }
    }
}