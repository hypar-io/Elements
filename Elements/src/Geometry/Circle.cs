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

        /// <summary>
        /// Return a new circle transformed by the provided transform.
        /// </summary>
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
    }
}