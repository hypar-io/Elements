namespace Elements.Geometry.Interfaces
{
    /// <summary>
    /// A trimmed curve.
    /// </summary>
    public interface ITrimmedCurve<TBasis> where TBasis: ICurve
    {
        /// <summary>
        /// The basis curve for this trimmed curve.
        /// </summary>
        TBasis BasisCurve { get; }
    }
}