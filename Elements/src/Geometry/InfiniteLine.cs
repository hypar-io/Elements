using System;
using System.Collections.Generic;

namespace Elements.Geometry
{
    /// TODO: Rename this class to Line
    /// <summary>
    /// An infinite line.
    /// Parameterization of the line is -infinity -> 0 (Origin) -> +infinity
    /// </summary>
    public class InfiniteLine : Curve
    {
        /// <summary>
        /// The origin of the line.
        /// </summary>
        public Vector3 Origin { get; }

        /// <summary>
        /// The direction of the line.
        /// </summary>
        public Vector3 Direction { get; }

        /// <summary>
        /// Create an infinite line.
        /// </summary>
        public InfiniteLine(Vector3 origin, Vector3 direction)
        {
            this.Origin = origin;
            this.Direction = direction.Unitized();
        }

        /// <inheritdoc/>
        public override Curve Transformed(Transform transform)
        {
            if (transform == null)
            {
                return this;
            }

            return new InfiniteLine(transform.OfPoint(this.Origin), transform.OfVector(this.Direction));
        }

        /// <summary>
        /// Get the point on the line at the parameter.
        /// </summary>
        public override Vector3 PointAt(double u)
        {
            return Origin + Direction * u;
        }

        /// <summary>
        /// Get a transform whose XY plane is perpendicular to the curve, and whose
        /// positive Z axis points along the curve.
        /// </summary>
        /// <param name="u">The parameter along the Line at which to calculate the Transform.</param>
        /// <returns>A transform.</returns>
        public override Transform TransformAt(double u)
        {
            return new Transform(PointAt(u), Direction.Negate());
        }

        public bool ParameterAt(Vector3 pt, out double t)
        { 
            t = (pt - Origin).Dot(Direction) / Direction.LengthSquared();
            var pointOnLine = Origin + Direction * t;
            if (pointOnLine.IsAlmostEqualTo(pt))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Get the parameter at a distance from the start parameter along the curve.
        /// </summary>
        /// <param name="distance">The distance from the start parameter.</param>
        /// <param name="start">The parameter from which to measure the distance.</param>
        public override double ParameterAtDistanceFromParameter(double distance, double start)
        {
            return start + distance;
        }

        public bool Intersects(Plane plane, out Vector3 result)
        {
            result = default;
            
            // Test for perpendicular.
            if (plane.Normal.Dot(Direction).ApproximatelyEquals(0))
            {
                return false;
            }

            var t = (plane.Normal.Dot(plane.Origin) - plane.Normal.Dot(Origin)) / plane.Normal.Dot(Direction);
            result = Origin + Direction * t;
            return true;
        }

        public bool Intersects(InfiniteLine other, out List<Vector3> results)
        {
            results = new List<Vector3>();

            // Check and return if two lines are parallel.
            var normal = Direction.Cross(other.Direction);
            if (normal.LengthSquared() < Vector3.EPSILON * Vector3.EPSILON)
            {
                return false;
            }

            // Check and return if two lines are not coplanar 
            if (Math.Abs((Origin - other.Origin).Dot(normal)) > Vector3.EPSILON)
            {
                return false;
            }

            normal = other.Direction.Cross(normal);
            var t = (normal.Dot(other.Origin) - normal.Dot(Origin)) / normal.Dot(Direction);
            results.Add(Origin + Direction * t);
            return true;
        }
    }
}