namespace Elements.Geometry.Interfaces
{
    /// <summary>
    /// ICurve is implemented by all curve types.
    /// </summary>
    public interface ICurve
    {
        /// <summary>
        /// Calculate the length of the curve.
        /// </summary>
        double Length();

        /// <summary>
        /// Get a point along the curve at parameter u.
        /// </summary>
        /// <param name="u">A parameter on the curve between 0.0 and 1.0.</param>
        /// <returns>The point on the curve.</returns>
        Vector3 PointAt(double u);

        /// <summary>
        /// Get the frame from the curve at parameter u.
        /// </summary>
        /// <param name="u">A parameter on the curve between 0.0 and 1.0.</param>
        /// <returns>The transform of the curve at parameter u, with the transform's Z axis tangent to the curve.</returns>
        Transform TransformAt(double u);

        /// <summary>
        /// Get a collection of Transforms which represent frames along this ICurve.
        /// </summary>
        /// <param name="startSetback">The offset from the start of the ICurve.</param>
        /// <param name="endSetback">The offset from the end of the ICurve.</param>
        /// <param name="additionalRotation">An additional rotation of the frame at each point.</param>
        /// <returns>A collection of Transforms.</returns>
        Transform[] Frames(double startSetback = 0.0, double endSetback = 0.0, double additionalRotation = 0.0);

        /// <summary>
        /// Get the bounding box of this curve.
        /// </summary>
        BBox3 Bounds();
    }
}