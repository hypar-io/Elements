namespace Elements.Geometry.Interfaces
{
    /// <summary>
    /// A curve with a start and an end.
    /// Examples of bounded curces include polylines and bezier curves.
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
        /// Get the bounding box of this curve.
        /// </summary>
        BBox3 Bounds();

        /// <summary>
        /// Get a collection of Transforms which represent frames along this ICurve.
        /// </summary>
        /// <param name="startSetback">The offset from the start of the ICurve.</param>
        /// <param name="endSetback">The offset from the end of the ICurve.</param>
        /// <param name="additionalRotation">An additional rotation of the frame at each point.</param>
        /// <returns>A collection of Transforms.</returns>
        Transform[] Frames(double startSetback = 0.0, double endSetback = 0.0, double additionalRotation = 0.0);
    }
}