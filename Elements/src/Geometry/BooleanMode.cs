namespace Elements.Geometry
{
    /// <summary>
    /// Mode to apply a boolean operation
    /// </summary>
    public enum BooleanMode
    {
        /// <summary>
        /// A and not B
        /// </summary>
        Difference,
        /// <summary>
        /// A or B
        /// </summary>
        Union,
        /// <summary>
        /// A and B
        /// </summary>
        Intersection,
        /// <summary>
        /// Exclusive or — either A or B but not both.
        /// </summary>
        XOr
    }
}