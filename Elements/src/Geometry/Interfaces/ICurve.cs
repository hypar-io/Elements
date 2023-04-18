namespace Elements.Geometry.Interfaces
{
    /// <summary>
    /// An infinite curve.
    /// </summary>
    public interface ICurve
    {
        /// <summary>
        /// Get a point along the curve at parameter u.
        /// </summary>
        /// <param name="u">A parameter on the curve between domain.min and domain.max.</param>
        /// <returns>The point on the curve.</returns>
        Vector3 PointAt(double u);

        /// <summary>
        /// Get a point along the curve at parameter u.
        /// </summary>
        /// <param name="u">A parameter on the curve between 0.0 and 1.0.</param>
        /// <returns>The point on the curve.</returns>
        Vector3 PointAtNormalized(double u);

        /// <summary>
        /// Get the frame from the curve at parameter u.
        /// </summary>
        /// <param name="u">A parameter on the curve.</param>
        /// <returns>The transform of the curve at parameter u, with the transform's Z axis tangent to the curve.</returns>
        Transform TransformAt(double u);

        /// <summary>
        /// Get the parameter at a distance from the specified parameter along the curve.
        /// </summary>
        /// <param name="distance">The distance from the start parameter.</param>
        /// <param name="parameter">The parameter from which to measure the distance.</param>
        double ParameterAtDistanceFromParameter(double distance, double parameter);
    }
}