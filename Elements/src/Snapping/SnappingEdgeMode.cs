namespace Elements
{
    /// <summary>
    /// Enumerates the modes for creating snap edges.
    /// </summary>
    public enum SnappingEdgeMode
    {
        /// <summary>
        /// No edges are created; only individual point snaps.
        /// </summary>
        Points,
        /// <summary>
        /// A snap edge is drawn between every pair of points, creating a network of edges.
        /// </summary>
        Lines,
        /// <summary>
        /// Snap edges connect each subsequent point and also close the shape by connecting the last to the first point.
        /// </summary>
        LineLoop,
        /// <summary>
        /// Snap edges connect each subsequent point, without closing the shape.
        /// </summary>
        LineStrip
    }
}