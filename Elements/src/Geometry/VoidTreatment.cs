namespace Elements.Geometry
{
    /// <summary>
    /// Controls the handling of internal regions in a polygon boolean operation.
    /// </summary>
    public enum VoidTreatment
    {
        /// <summary>
        /// Use an Even/Odd fill pattern to decide whether internal polygons are solid or void.
        /// This corresponds to Clipper's "EvenOdd" PolyFillType.
        /// </summary>
        PreserveInternalVoids = 0,
        /// <summary>
        /// Treat all contained or overlapping polygons as solid.
        /// This corresponds to Clipper's "Positive" PolyFillType.
        /// </summary>
        IgnoreInternalVoids = 1
    }
}