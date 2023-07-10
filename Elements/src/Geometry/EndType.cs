namespace Elements.Geometry
{
    /// <summary>
    /// Offset end types
    /// </summary>
    public enum EndType
    {
        /// <summary>
        /// Open ends are extended by the offset distance and squared off
        /// </summary>
        Square,
        /// <summary>
        /// Ends are squared off with no extension
        /// </summary>
        Butt,
        /// <summary>
        /// If open, ends are joined and treated as a closed polygon
        /// </summary>
        ClosedPolygon,
    }
}