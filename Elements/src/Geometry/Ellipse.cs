using System;
using System.Collections.Generic;
using Elements.Geometry.Interfaces;
using Newtonsoft.Json;
using System.Linq;
using SixLabors.ImageSharp.ColorSpaces;

namespace Elements.Geometry
{
    /// <summary>
    /// An ellipse. 
    /// Parameterization of the curve is 0 -> 2PI.
    /// </summary>
    public class Ellipse : Curve, IConic
    {
        /// <summary>
        /// The center of the ellipse.
        /// </summary>
        [JsonIgnore]
        public Vector3 Center
        {
            get
            {
                return this.Transform.Origin;
            }
        }

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
        /// The dimension of the major axis (X) of the ellipse.
        /// </summary>
        public double MajorAxis { get; set; }

        /// <summary>
        /// The dimension of the minor axis (Y) of the ellipse.
        /// </summary>
        public double MinorAxis { get; set; }

        /// <summary>
        /// The coordinate system of the plane containing the ellipse.
        /// </summary>
        public Transform Transform { get; protected set; }

        /// <summary>
        /// Create an ellipse.
        /// </summary>
        /// <param name="majorAxis">The dimension of the major axis (X) of the ellipse.</param>
        /// <param name="minorAxis">The dimension of the minor axis (Y) o the ellipse.</param>
        public Ellipse(double majorAxis = 1.0, double minorAxis = 2.0)
        {
            CheckAndThrow(minorAxis, majorAxis);

            this.Transform = new Transform();
            this.MajorAxis = majorAxis;
            this.MinorAxis = minorAxis;
        }

        /// <summary>
        /// Create an ellipse.
        /// </summary>
        /// <param name="center">The center of the ellipse.</param>
        /// <param name="majorAxis">The dimension of the major axis (X) of the ellipse.</param>
        /// <param name="minorAxis">The dimension of the minor axis (Y) of the ellipse.</param>
        public Ellipse(Vector3 center, double majorAxis = 2.0, double minorAxis = 1.0)
        {
            CheckAndThrow(minorAxis, majorAxis);

            this.Transform = new Transform(center);
            this.MajorAxis = majorAxis;
            this.MinorAxis = minorAxis;
        }

        /// <summary>
        /// Create an ellipse.
        /// </summary>
        /// <param name="transform">The coordinate system of the plane containing the ellipse.</param>
        /// <param name="majorAxis">The dimension of the major axis (X) of the ellipse.</param>
        /// <param name="minorAxis">The dimension of the minor axis (Y) of the ellipse.</param>
        [JsonConstructor]
        public Ellipse(Transform transform, double majorAxis = 2.0, double minorAxis = 1.0)
        {
            CheckAndThrow(minorAxis, majorAxis);

            this.Transform = transform;
            this.MajorAxis = majorAxis;
            this.MinorAxis = minorAxis;
        }

        private void CheckAndThrow(double minorAxis, double majorAxis)
        {
            if (minorAxis <= 0)
            {
                throw new System.ArgumentOutOfRangeException("minorAxis", "The minor axis value must be greater than 0.");
            }

            if (majorAxis <= 0)
            {
                throw new System.ArgumentOutOfRangeException("majorAxis", "The major axis value must be greater than 0.");
            }
        }

        /// <summary>
        /// Get a point along the ellipse at parameter u.
        /// </summary>
        /// <param name="u"></param>
        /// <returns>A point on the ellipse at parameter u.</returns>
        public override Vector3 PointAt(double u)
        {
            return Transform.OfPoint(PointAtUntransformed(u));
        }

        private Vector3 PointAtUntransformed(double u)
        {
            var x = this.MajorAxis * Math.Cos(u);
            var y = this.MinorAxis * Math.Sin(u);
            return new Vector3(x, y);
        }

        public bool ParameterAt(Vector3 pt, out double parameter)
        {
            var local = Transform.Inverted().OfPoint(pt);
            if (local.Z.ApproximatelyEquals(0) && OnEllipseUntransformed(pt))
            {
                parameter = ParameterAtUntransformed(local);
                return true;
            }
        
            parameter = 0;
            return false;
        }
        
        private double ParameterAtUntransformed(Vector3 pt)
        {
            return Math.Atan2(pt.Y / MinorAxis , pt.X / MajorAxis);
        }

        private bool OnEllipseUntransformed(Vector3 pt)
        {
            var h = Math.Pow(pt.X / MajorAxis, 2);
            var v = Math.Pow(pt.Y / MinorAxis, 2);
            return (h + v).ApproximatelyEquals(1);
        }

        /// <summary>
        /// Get a transform along the ellipse at parameter u.
        /// </summary>
        /// <param name="u"></param>
        /// <returns>A transform on the ellipse at parameter u.</returns>
        public override Transform TransformAt(double u)
        {
            // Code generated from chatgpt with the following prompt:
            // Can i see some c# code to calculate the normal to an ellipse at parameter t where the major axis is 5 and the minor axis is 3?

            var p = PointAtUntransformed(u);
            var refVector = (p - Vector3.Origin).Unitized();

            var a = this.MajorAxis;
            var b = this.MinorAxis;

            // Calculate slope of tangent line at point (x, y)
            double slopeTangent = -b * b * p.X / (a * a * p.Y);

            // Calculate slope of normal line at point (x, y)
            double slopeNormal = -1 / slopeTangent;

            // Calculate x and y components of the normal vector
            double nx = 1 / Math.Sqrt(1 + slopeNormal * slopeNormal);
            double ny = slopeNormal / Math.Sqrt(1 + slopeNormal * slopeNormal);
            var x = new Vector3(nx, ny);

            // Normals will naturally flip when u > pi.
            // To ensure consistent direction, flip the
            // normal if it's reversed with regards to 
            if (refVector.Dot(x) < 0)
            {
                x = x.Negate();
            }
            var y = Vector3.ZAxis;

            return new Transform(p, x, y, x.Cross(y)).Concatenated(this.Transform);
        }

        /// <inheritdoc/>
        public override Curve Transformed(Transform transform)
        {
            return new Ellipse(transform.Concatenated(this.Transform), this.MajorAxis, this.MinorAxis);
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

            var availableArcDistance = ArcLength(start, Math.PI * 2);

            if (distance >= availableArcDistance)
            {
                throw new ArgumentOutOfRangeException($"The provided distance, {distance}, is greater than the available arc length, {availableArcDistance}, from parameter {start}.");
            }

            // Start at the specified parameter and measure
            // until you reach the desired distance.
            ArcLengthUntil(start, Math.PI * 2, distance, out var end);
            return end;
        }

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

        public bool Intersects(InfiniteLine line, out List<Vector3> results)
        {
            results = new List<Vector3>();

            Plane ellipsePlane = new Plane(Center, Normal);
            Vector3 closestPoint;
            bool lineOnPlane = line.Origin.DistanceTo(ellipsePlane).ApproximatelyEquals(0) &&
                (line.Origin + line.Direction).DistanceTo(ellipsePlane).ApproximatelyEquals(0);

            if (lineOnPlane)
            {
                closestPoint = Center.ClosestPointOn(line);
            }
            else if (!line.Intersects(ellipsePlane, out closestPoint))
            {
                return false;
            }

            var transformInverted = this.Transform.Inverted();
            var localPoint = transformInverted.OfPoint(closestPoint);
            if (OnEllipseUntransformed(localPoint))
            {
                results.Add(closestPoint);
            }
            else if (lineOnPlane)
            {
                double a2 = MajorAxis * MajorAxis;
                double b2 = MinorAxis * MinorAxis;
                Vector3 localDirection = transformInverted.OfVector(line.Direction);

                double dx2 = localDirection.X * localDirection.X;
                double dy2 = localDirection.Y * localDirection.Y;

                // Coefficients for the quadratic equation
                double A = a2 * dy2 + b2 * dx2;
                double B = -2 * (localPoint.Y * a2 * localDirection.Y + localPoint.X * b2 * localDirection.X);
                double C = a2 * localPoint.Y * localPoint.Y + b2 * localPoint.X * localPoint.X - a2 * b2;

                foreach (var root in Equations.SolveQuadratic(A, B, C))
                {
                    double x = localPoint.X + root * localDirection.X;
                    double y = localPoint.Y + root * localDirection.Y;
                    results.Add(Transform.OfPoint(new Vector3(x, y)));
                }
            }

            return results.Any();
        }

        public bool Intersects(Circle circle, out List<Vector3> results)
        {
            results = new List<Vector3>();

            Plane planeA = new Plane(Center, Normal);
            Plane planeB = new Plane(circle.Center, circle.Normal);

            // Check if circle and ellipse are on the same plane. 
            if (Normal.IsAlmostEqualTo(circle.Normal) &&
                circle.Center.DistanceTo(planeA).ApproximatelyEquals(0))
            {
                // Circle and Ellipse are the same.
                if (Center.IsAlmostEqualTo(circle.Center) &&
                    MajorAxis.ApproximatelyEquals(MinorAxis) &&
                    MajorAxis.ApproximatelyEquals(circle.Radius))
                {
                    return false;
                }

                if (Center.DistanceTo(circle.Center) > MajorAxis + circle.Radius)
                {
                    return false;
                }

                var localCenter = Transform.Inverted().OfPoint(circle.Center);
                var roots = Equations.SolveIterative(0, Math.PI * 2,
                    new Func<double, double>((t) =>
                    {
                        var d = PointAtUntransformed(t) - localCenter;
                        return d.LengthSquared() - circle.Radius * circle.Radius;
                    }));
                foreach (var root in roots)
                {
                    Vector3 intersection = PointAtUntransformed(root);
                    var global = Transform.OfPoint(intersection);
                    if (!results.Any() || !global.IsAlmostEqualTo(results.Last()))
                    {
                        results.Add(global);
                    }
                }
            }
            // Ignore parallel planes.
            // Find intersection line between two planes.
            else if (planeA.Intersects(planeB, out var line) &&
                     Intersects(line, out var candidates))
            {
                foreach (var item in candidates)
                {
                    // Check each point that lays on intersection line and one of the circles.
                    // They are on both if they have correct distance to circle centers.
                    if (item.DistanceTo(circle.Center).ApproximatelyEquals(circle.Radius))
                    {
                        results.Add(item);
                    }
                }
            }

            return results.Any();
        }

        public bool Intersects(Ellipse other, out List<Vector3> results)
        {
            results = new List<Vector3>();

            Plane planeA = new Plane(Center, Normal);
            Plane planeB = new Plane(other.Center, other.Normal);

            // Check if circle and ellipse are on the same plane. 
            if (Normal.IsAlmostEqualTo(other.Normal) &&
                other.Center.DistanceTo(planeA).ApproximatelyEquals(0))
            {
                // Ellipses are the same.
                if (Center.IsAlmostEqualTo(other.Center))
                {
                    if (MajorAxis.ApproximatelyEquals(other.MajorAxis) &&
                        Transform.XAxis.IsParallelTo(other.Transform.XAxis) &&
                        MinorAxis.ApproximatelyEquals(other.MinorAxis) &&
                        Transform.YAxis.IsParallelTo(other.Transform.YAxis))
                    {
                        return false;
                    }

                    if (MajorAxis.ApproximatelyEquals(other.MinorAxis) &&
                        Transform.XAxis.IsParallelTo(other.Transform.YAxis) &&
                        MinorAxis.ApproximatelyEquals(other.MajorAxis) &&
                        Transform.YAxis.IsParallelTo(other.Transform.XAxis))
                    {
                        return false;
                    }
                }

                // Too far away
                if (Center.DistanceTo(other.Center) > MajorAxis + other.MajorAxis)
                {
                    return false;
                }

                var inverted = Transform.Inverted();
                var ellipseToEllipse = Transform.Concatenated(other.Transform.Inverted());

                var roots = Equations.SolveIterative(0, Math.PI * 2,
                    new Func<double, double>((t) =>
                    {
                        var d = PointAtUntransformed(t);
                        var otherD = ellipseToEllipse.OfPoint(d);
                        var dx = Math.Pow(otherD.X / other.MajorAxis, 2);
                        var dy = Math.Pow(otherD.Y / other.MinorAxis, 2);
                        return dx + dy - 1;
                    }));
                foreach (var root in roots)
                {
                    Vector3 intersection = PointAtUntransformed(root);
                    var global = Transform.OfPoint(intersection);
                    if (!results.Any() || !global.IsAlmostEqualTo(results.Last()))
                    {
                        results.Add(global);
                    }
                }
            }
            // Ignore parallel planes.
            // Find intersection line between two planes.
            else if (planeA.Intersects(planeB, out var line) &&
                     Intersects(line, out var candidates))
            {
                var inverted = other.Transform.Inverted();
                foreach (var item in candidates)
                {
                    // Check each point that lays on intersection line and one of the circles.
                    // They are on both if they have correct distance to circle centers.
                    var local = inverted.OfPoint(item);
                    if (other.OnEllipseUntransformed(local))
                    {
                        results.Add(item);
                    }
                }
            }

            return results.Any();
        }

        public bool Intersects(BoundedCurve bounded, out List<Vector3> results)
        {
            return bounded.Intersects(this, out results);
        }

        internal double ArcLength(double t0,
                                  double t1,
                                  int n = 1000)
        {
            var a = this.MajorAxis;
            var b = this.MinorAxis;

            double h = ((a - b) * (a - b)) / ((a + b) * (a + b));
            double arcLength = 0.0;
            double dt = (t1 - t0) / n;
            for (double t = t0; t < t1; t += dt)
            {
                var sampleLength = Step(a, b, t, dt, h);
                arcLength += sampleLength;
            }

            return arcLength;
        }

        internal double ArcLengthUntil(double t0,
                                       double tmax,
                                       double distance,
                                       out double end,
                                       int n = 1000)
        {
            var a = this.MajorAxis;
            var b = this.MinorAxis;

            // Calculate arc length
            double h = ((a - b) * (a - b)) / ((a + b) * (a + b));
            double arcLength = 0.0;
            double dt = (tmax - t0) / n;

            end = tmax;

            for (double t = t0; t < tmax; t += dt)
            {
                var sampleLength = Step(a, b, t, dt, h);
                if (arcLength + sampleLength > distance)
                {
                    // TODO: This is an approximation.
                    // This will return the parameter before the 
                    // actual value that we want. Implement a more
                    // precise strategy.
                    end = t;
                    return arcLength;
                }
                arcLength += sampleLength;
            }

            return arcLength;
        }

        private double Step(double a, double b, double t, double dt, double h)
        {
            // Full derivation shown in comments for reference.
            // double x = a * Math.Cos(t);
            // double y = b * Math.Sin(t);
            double dxdt = -a * Math.Sin(t);
            double dydt = b * Math.Cos(t);
            double dsdt = Math.Sqrt(dxdt * dxdt + dydt * dydt);
            double ds = dsdt * dt / Math.Sqrt(1 - h * Math.Sin(t) * Math.Sin(t));
            // double next_x = a * Math.Cos(t + dt);
            // double next_y = b * Math.Sin(t + dt);
            double next_dxdt = -a * Math.Sin(t + dt);
            double next_dydt = b * Math.Cos(t + dt);
            double next_dsdt = Math.Sqrt(next_dxdt * next_dxdt + next_dydt * next_dydt);
            double next_ds = next_dsdt * dt / Math.Sqrt(1 - h * Math.Sin(t + dt) * Math.Sin(t + dt));
            return (ds + next_ds) / 2;
        }
    }
}