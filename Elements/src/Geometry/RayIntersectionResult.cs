namespace Elements.Geometry
{
    /// <summary>
    /// The possible types of ray intersection result.
    /// </summary>
    public enum RayIntersectionResult
    {
        /// <summary>
        /// The rays intersect.
        /// </summary>
        Intersect,
        /// <summary>
        /// The rays do not intersect.
        /// </summary>
        None,
        /// <summary>
        /// The rays are coincident.
        /// </summary>
        Coincident,
        /// <summary>
        /// The rays are parallel.
        /// </summary>
        Parallel
    }
}