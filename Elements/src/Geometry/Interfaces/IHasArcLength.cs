namespace Elements.Geometry.Interfaces
{
    /// <summary>
    /// Represents a curve with arc length-based operations.
    /// Implementing classes define methods for computing points and performing operations based on arc length.
    /// </summary>
    public interface IHasArcLength
    {
        /// <summary>
        /// Returns the point on the curve at the specified arc length.
        /// </summary>
        /// <param name="length">The arc length along the curve.</param>
        /// <returns>The point on the curve at the specified arc length.</returns>
        Vector3 PointAtLength(double length);

        /// <summary>
        /// Returns the point on the curve at the specified normalized length.
        /// The normalized length is a value between 0 and 1 representing the relative position along the curve.
        /// </summary>
        /// <param name="normalizedLength">The normalized length along the curve.</param>
        /// <returns>The point on the curve at the specified normalized length.</returns>
        Vector3 PointAtNormalizedLength(double normalizedLength);

        /// <summary>
        /// Returns the midpoint of the curve.
        /// </summary>
        /// <returns>The midpoint of the curve.</returns>
        Vector3 MidPoint();

        /// <summary>
        /// Divides the curve into segments of the specified length and returns the points along the curve at those intervals.
        /// </summary>
        /// <param name="length">The desired length for dividing the curve.</param>
        /// <returns>A list of points representing the divisions along the curve.</returns>
        Vector3[] DivideByLength(double length);
    }

}