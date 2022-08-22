namespace Elements.Geometry
{
    /// <summary>
    /// An enumeration of relations to a plane.
    /// </summary>
    public enum RelationToPlane
    {
        /// <summary>
        /// None
        /// </summary>
        None,
        /// <summary>
        /// Intersects
        /// </summary>
        Intersects,
        /// <summary>
        /// On the normal-facing side of the plane.
        /// </summary>
        Above,
        /// <summary>
        /// On the non-normal-facing side of the plane.
        /// </summary>
        Below
    }
}