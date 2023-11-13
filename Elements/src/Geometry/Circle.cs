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
        /// Calculate the length of the circle between two parameters.
        /// </summary>
        public double ArcLength(double start, double end)
        {
            // Convert start and end parameters from radians to degrees
            double _startAngle = start * 180.0 / Math.PI;
            double _endAngle = end * 180.0 / Math.PI;

            // Ensure the start angle is within the valid domain range of 0 to 360 degrees
            double startAngle = _startAngle % 360;
            if (startAngle < 0)
            {
                startAngle += 360;
            }

            // Ensure the end angle is within the valid domain range of 0 to 360 degrees
            double endAngle = _endAngle % 360;
            if (endAngle < 0)
            {
                endAngle += 360;
            }
            else if (endAngle == 0 && Math.Abs(_endAngle) >= 2 * Math.PI)
            {
                endAngle = 360;
            }

            // Calculate the difference in angles
            double angleDifference = endAngle - startAngle;

            // Adjust the angle difference if it crosses the 360-degree boundary
            if (angleDifference < 0)
            {
                angleDifference += 360;
            }
            else if (angleDifference >= 2 * Math.PI)
            {
                return Circumference; // Full circle, return circumference
            }

            // Convert the angle difference back to radians
            double angleDifferenceRadians = angleDifference * Math.PI / 180.0;

            // Calculate the arc length using the formula: arc length = radius * angle
            double arcLength = Radius * angleDifferenceRadians;

            return arcLength;
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
        /// Are the two circles almost equal?
        /// </summary>
        public bool IsAlmostEqualTo(Circle other, double tolerance = Vector3.EPSILON)
        {
            return (Center.IsAlmostEqualTo(other.Center, tolerance) && Math.Abs(Radius - other.Radius) < tolerance ? true : false);
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
        /// Calculates and returns the midpoint of the circle.
        /// </summary>
        /// <returns>The midpoint of the circle.</returns>
        public Vector3 MidPoint()
        {
            return PointAt(Math.PI);
        }

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

        /// <summary>
        /// Calculates and returns the point on the circle at a specific arc length.
        /// </summary>
        /// <param name="length">The arc length along the circumference of the circle.</param>
        /// <returns>The point on the circle at the specified arc length.</returns>
        public Vector3 PointAtLength(double length)
        {
            double parameter = (length / Circumference) * 2 * Math.PI;
            return PointAt(parameter);
        }

        /// <summary>
        /// Calculates and returns the point on the circle at a normalized arc length.
        /// </summary>
        /// <param name="normalizedLength">The normalized arc length between 0 and 1.</param>
        /// <returns>The point on the circle at the specified normalized arc length.</returns>
        public Vector3 PointAtNormalizedLength(double normalizedLength)
        {
            double parameter = normalizedLength * 2 * Math.PI;
            return PointAt(parameter);
        }

        /// <summary>
        /// Calculates the parameter within the range of 0 to 2π at a given point on the circle.
        /// </summary>
        /// <param name="point">The point on the circle.</param>
        /// <returns>The parameter within the range of 0 to 2π at the given point on the circle.</returns>
        public double GetParameterAt(Vector3 point)
        {
            Vector3 relativePoint = point - Center;

            double theta = Math.Atan2(relativePoint.Y, relativePoint.X);

            if (theta < 0)
            {
                theta += 2 * Math.PI;
            }
            return theta;
        }

        /// <summary>
        /// Check if certain point is on the circle.
        /// </summary>
        /// <param name="pt">Point to check.</param>
        /// <param name="t">Calculated parameter of point on circle.</param>
        /// <returns>True if point lays on the circle.</returns>
        public bool ParameterAt(Vector3 pt, out double t)
        {
            var local = Transform.Inverted().OfPoint(pt);
            if (local.Z.ApproximatelyEquals(0) &&
                local.LengthSquared().ApproximatelyEquals(
                    Radius * Radius, Vector3.EPSILON * Vector3.EPSILON))
            {
                t = ParameterAtUntransformed(local);
                return true;
            }

            t = 0;
            return false;
        }

        /// <summary>
        /// Checks if a given point lies on a circle within a specified tolerance.
        /// </summary>
        /// <param name="point">The point to be checked.</param>
        /// <param name="circle">The circle to check against.</param>
        /// <param name="tolerance">The tolerance value (optional). Default is 1E-05.</param>
        /// <returns>True if the point lies on the circle within the tolerance, otherwise false.</returns>
        public static bool PointOnCircle(Vector3 point, Circle circle, double tolerance = 1E-05)
        {
            Vector3 centerToPoint = point - circle.Center;
            double distanceToCenter = centerToPoint.Length();

            // Check if the distance from the point to the center is within the tolerance of the circle's radius
            return Math.Abs(distanceToCenter - circle.Radius) < tolerance;
        }

        private double ParameterAtUntransformed(Vector3 pt)
        {
            var v = pt / Radius;
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

        /// <summary>
        /// Divides the circle into segments of the specified length and returns a list of points representing the division.
        /// </summary>
        /// <param name="length">The length of each segment.</param>
        /// <returns>A list of points representing the division of the circle.</returns>
        public Vector3[] DivideByLength(double length)
        {
            List<Vector3> points = new List<Vector3>();
            double circumference = 2 * Math.PI * Radius;
            int segmentCount = (int)Math.Ceiling(circumference / length);
            double segmentLength = circumference / segmentCount;

            for (int i = 0; i < segmentCount; i++)
            {
                double parameter = i * segmentLength / circumference;
                points.Add(PointAtNormalizedLength(parameter));
            }

            return points.ToArray();
        }

        /// <inheritdoc/>
        public override bool Intersects(ICurve curve, out List<Vector3> results)
        {
            switch (curve)
            {
                case BoundedCurve boundedCurve:
                    return boundedCurve.Intersects(this, out results);
                case InfiniteLine line:
                    return Intersects(line, out results);
                case Circle circle:
                    return Intersects(circle, out results);
                case Ellipse elliplse:
                    return Intersects(elliplse, out results);
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Does this circle intersects with other circle?
        /// Circles with the same positions and radii are not considered as intersecting.
        /// </summary>
        /// <param name="other">Other circle to intersect.</param>
        /// <param name="results">List containing up to two intersection points.</param>
        /// <returns>True if any intersections exist, otherwise false.</returns>
        public bool Intersects(Circle other, out List<Vector3> results)
        {
            results = new List<Vector3>();

            Plane planeA = new Plane(Center, Normal);
            Plane planeB = new Plane(other.Center, other.Normal);

            // Check if two circles are on the same plane.
            if (Normal.IsParallelTo(other.Normal, Vector3.EPSILON * Vector3.EPSILON) &&
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

        /// <summary>
        /// Does this circle intersects with an infinite line?
        /// </summary>
        /// <param name="line">Infinite line to intersect.</param>
        /// <param name="results">List containing up to two intersection points.</param>
        /// <returns>True if any intersections exist, otherwise false.</returns>
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

        /// <summary>
        /// Does this circle intersects with an ellipse?
        /// Circle and ellipse that are coincides are not considered as intersecting.
        /// <see cref="Ellipse.Intersects(Circle, out List{Vector3})"/>
        /// </summary>
        /// <param name="ellipse">Ellipse to intersect.</param>
        /// <param name="results">List containing up to four intersection points.</param>
        /// <returns>True if any intersections exist, otherwise false.</returns>
        public bool Intersects(Ellipse ellipse, out List<Vector3> results)
        {
            return ellipse.Intersects(this, out results);
        }

        /// <summary>
        /// Does this circle intersects with a bounded curve?
        /// </summary>
        /// <param name="curve">Curve to intersect.</param>
        /// <param name="results">List containing intersection points.</param>
        /// <returns>True if any intersections exist, otherwise false.</returns>
        public bool Intersects(BoundedCurve curve, out List<Vector3> results)
        {
            return curve.Intersects(this, out results);
        }
    }
}