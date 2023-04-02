namespace Elements.Geometry.Interfaces
{
    /// <summary>
    /// A bounded curve which is a segment of basis curve between a start and end parameter.
    /// </summary>
    public interface ITrimmedCurve<TBasis> where TBasis: ICurve
    {   
        /// <summary>
        /// The domain of the trim.
        /// </summary>
        Domain1d Domain { get; }

        /// <summary>
        /// The basis curve for this trimmed curve.
        /// </summary>
        TBasis BasisCurve { get; }
    }
}