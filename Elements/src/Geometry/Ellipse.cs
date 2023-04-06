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
        
        /// <summary>
        /// Get a point along the ellipse at parameter u.
        /// </summary>
        /// <param name="u"></param>
        /// <returns>A point on the ellipse at parameter u.</returns>
        public override Vector3 PointAt(double u)
        {
            var x = this.MajorAxis * Math.Cos(u);
            var y = this.MinorAxis * Math.Sin(u);
            return Transform.OfPoint(new Vector3(x, y));
        }

        /// <summary>
        /// Get a transform along the ellipse at parameter u.
        /// </summary>
        /// <param name="u"></param>
        /// <returns>A transform on the ellipse at parameter u.</returns>
        public override Transform TransformAt(double u)
        {
            var p = PointAt(u);
            var x = (p - this.Transform.Origin).Unitized();
            var y = Vector3.ZAxis;
            return new Transform(p, x, x.Cross(y));
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