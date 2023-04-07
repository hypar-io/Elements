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
        public Ellipse(Transform transform, double majorAxis = 1.0, double minorAxis=2.0)
        {
            this.Transform = transform;
            this.MajorAxis = majorAxis;
            this.MinorAxis = minorAxis;
        }

        private void CheckAndThrow(double minorAxis, double majorAxis)
        {
            if(minorAxis <= 0)
            {
                throw new System.ArgumentOutOfRangeException("minorAxis", "The minor axis value must be greater than 0.");
            }

            if(majorAxis <= 0)
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
            return new Vector3(x,y);
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
            var refVector = (p-Vector3.Origin).Unitized();

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
            if(refVector.Dot(x) < 0)
            {
                x = x.Negate();
            }
            var y = Vector3.ZAxis;

            return  new Transform(p, x, y, x.Cross(y)).Concatenated(this.Transform);
        }

        /// <summary>
        /// Create a transformed copy of the ellipse.
        /// </summary>
        /// <param name="transform">The transform to apply.</param>
        public override Curve Transformed(Transform transform)
        {
            return new Ellipse(transform.Concatenated(this.Transform), this.MajorAxis, this.MinorAxis);
        }
    }
}