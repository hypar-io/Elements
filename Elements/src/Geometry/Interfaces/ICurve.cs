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
        /// <param name="u">A parameter on the curve.</param>
        /// <returns>The point on the curve.</returns>
        Vector3 PointAt(double u);

        /// <summary>
        /// Get the frame from the curve at parameter u.
        /// </summary>
        /// <param name="u">A parameter on the curve.</param>
        /// <returns>The transform of the curve at parameter u, with the transform's Z axis tangent to the curve.</returns>
        Transform TransformAt(double u);
    }
}