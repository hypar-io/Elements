namespace Elements.Geometry.Interfaces
{
    /// <summary>
    /// A curve with a start and an end.
    /// Examples of bounded curves include polylines and bezier curves.
    /// </summary>
    public interface IBoundedCurve
    {
        /// <summary>
        /// The start of the curve.
        /// </summary>
        Vector3 Start { get; }

        /// <summary>
        /// The end of the curve.
        /// </summary>
        Vector3 End { get; }

        /// <summary>
        /// The mid point of the curve.
        /// </summary>
        Vector3 Mid();

        /// <summary>
        /// Calculate the length of the curve.
        /// </summary>
        double Length();

        /// <summary>
        /// Calculate the length of the curve between two parameters.
        /// </summary>
        double ArcLength(double start, double end);

        /// <summary>
        /// Get the bounding box of this curve.
        /// </summary>
        BBox3 Bounds();

        /// <summary>
        /// Get a collection of Transforms which represent frames along this curve.
        /// </summary>
        /// <param name="startSetbackDistance">The offset from the start of the ICurve.</param>
        /// <param name="endSetbackDistance">The offset from the end of the ICurve.</param>
        /// <param name="additionalRotation">An additional rotation of the frame at each point.</param>
        /// <param name="minimumChordLength">The minimum chord length allowed for subdivision of the curve. A smaller MinimumChordLength results in smoother curves. For polylines and polygons this parameter will have no effect.</param>
        /// <returns>A collection of Transforms.</returns>
        Transform[] Frames(double startSetbackDistance = 0.0,
                           double endSetbackDistance = 0.0,
                           double additionalRotation = 0.0,
                           double minimumChordLength = 0.01);

        /// <summary>
        /// The domain of the curve.
        /// </summary>
        Domain1d Domain { get; }

        /// <summary>
        /// Get parameters to be used to find points along the curve for visualization.
        /// </summary>
        /// <param name="startSetbackDistance">An optional setback from the start of the curve.</param>
        /// <param name="endSetbackDistance">An optional setback from the end of the curve.</param>
        /// <param name="minimumChordLength">The minimum chord length allowed for subdivision of the curve. A smaller MinimumChordLength results in smoother curves.</param>
        /// <returns>A collection of parameter values.</returns>
        double[] GetSubdivisionParameters(double startSetbackDistance = 0,
                                          double endSetbackDistance = 0,
                                          double minimumChordLength = 0.01);

        /// <summary>
        /// Get a point along the curve at parameter u.
        /// </summary>
        /// <param name="u">A parameter along the curve between 0.0 and 1.0.</param>
        /// <returns>A point along the curve at parameter u.</returns>
        Vector3 PointAtNormalized(double u);

        /// <summary>
        /// Get a transform whose XY plane is perpendicular to the curve, and whose
        /// positive Z axis points along the curve.
        /// </summary>
        /// <param name="u">The parameter along the curve between 0.0 and 1.0.</param>
        /// <returns>A transform.</returns>
        Transform TransformAtNormalized(double u);
    }
}