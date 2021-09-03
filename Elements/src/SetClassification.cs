namespace Elements
{
    /// <summary>
    /// A set containment classification.
    /// </summary>
    public enum SetClassification
    {
        /// <summary>
        /// A segments inside B
        /// </summary>
        AInsideB,
        /// <summary>
        /// A segments outside B
        /// </summary>
        AOutsideB,
        /// <summary>
        /// B segments inside A
        /// </summary>
        BInsideA,
        /// <summary>
        /// B segments outside A
        /// </summary>
        BOutsideA,
        /// <summary>
        /// A coplanar with B
        /// </summary>
        ACoplanarB,
        /// <summary>
        /// B coplanar with A
        /// </summary>
        BCoplanarA,
        /// <summary>
        /// None
        /// </summary>
        None,
    }
}