namespace Elements.Geometry
{
    /// <summary>
    /// Normalization type.
    /// </summary>
    public enum NormalizationType
    {
        /// <summary>
        /// During normalization move start points of segments.
        /// </summary>
        Start,
        /// <summary>
        /// During normalization move end points of segments.
        /// </summary>
        End,
        /// <summary>
        /// During normalization move both start and end vertices in approximately equivalent proportions.
        /// </summary>
        Middle
    }
}