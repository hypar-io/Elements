using System;
using Elements.Geometry.Interfaces;

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
        public Vector3 Center
        {
            get
            {
                return this.Transform.Origin;
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
        public Ellipse(Vector3 center, double majorAxis = 1.0, double minorAxis = 2.0)
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
        public Ellipse(Transform transform, double majorAxis = 1.0, double minorAxis = 2.0)
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

        /// <summary>
        /// Create a transformed copy of the ellipse.
        /// </summary>
        /// <param name="transform">The transform to apply.</param>
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