namespace Elements.Geometry
{
    /// TODO: Rename this class to Line
    /// <summary>
    /// An infinite line.
    /// Parameterization of the line is -infinity -> 0 (Origin) -> +infinity
    /// </summary>
    public class InfiniteLine: Curve
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

        /// <summary>
        /// Construct a transformed copy of this Curve.
        /// </summary>
        /// <param name="transform">The transform to apply.</param>
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
            return new Transform(PointAt(u), Direction);
        }
    }
}