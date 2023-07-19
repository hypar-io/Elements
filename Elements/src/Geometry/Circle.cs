using Elements.Validators;
using System;
using System.Collections.Generic;
using Elements.Geometry.Interfaces;
using Newtonsoft.Json;

namespace Elements.Geometry
{
    /// <summary>
    /// A circle.
    /// Parameterization of the circle is 0 -> 2PI.
    /// </summary>
    public class Circle : Curve, IConic, IHasArcLength
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

        /// <summary>The circumference of the circle.</summary>
        [JsonIgnore]
        [System.ComponentModel.DataAnnotations.Range(0.0D, double.MaxValue)]
        public double Circumference { get; protected set; }

        /// <summary>
        /// The domain of the curve.
        /// </summary>
        [JsonIgnore]
        public Domain1d Domain => new Domain1d(0, 2 * Math.PI);

        /// <summary>
        /// The coordinate system of the plane containing the circle.
        /// </summary>
        public Transform Transform { get; protected set; }

        /// <summary>
        /// Construct a circle.
        /// </summary>
        /// <param name="center">The center of the circle.</param>
        /// <param name="radius">The radius of the circle.</param>
        [JsonConstructor]
        public Circle(Vector3 center, double radius = 1.0)
        {
            if (!Validator.DisableValidationOnConstruction)
            {
                if (Math.Abs(radius - 0.0) < double.Epsilon ? true : false)
                {
                    throw new ArgumentException($"The circle could not be created. The radius of the circle cannot be the zero: radius {radius}");
                }
            }
            this.Radius = radius;
            this.Circumference = 2 * Math.PI * this.Radius;
            this.Transform = new Transform(center);
        }

        /// <summary>
        /// Construct a circle.
        /// </summary>
        /// <param name="radius">The radius of the circle.</param>
        public Circle(double radius = 1.0)
        {
            if (!Validator.DisableValidationOnConstruction)
            {
                if (Math.Abs(radius - 0.0) < double.Epsilon ? true : false)
                {
                    throw new ArgumentException($"The circle could not be created. The radius of the circle cannot be the zero: radius {radius}");
                }
            }
            this.Radius = radius;
            this.Circumference = 2 * Math.PI * this.Radius;
            this.Transform = new Transform();
        }

        /// <summary>
        /// Construct a circle.
        /// </summary>
        public Circle(Transform transform, double radius = 1.0)
        {
            if (!Validator.DisableValidationOnConstruction)
            {
                if (Math.Abs(radius - 0.0) < double.Epsilon ? true : false)
                {
                    throw new ArgumentException($"The circle could not be created. The radius of the circle cannot be the zero: radius {radius}");
                }
            }
            this.Transform = transform;
            this.Radius = radius;
            this.Circumference = 2 * Math.PI * this.Radius;
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
    }
}