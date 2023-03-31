namespace Elements.Geometry.Interfaces
{
    /// <summary>
    /// A bounded curve which is a segment of basis curve between a start and end parameter.
    /// </summary>
    public interface ITrimmedCurve<TBasis> where TBasis: ICurve
    {
        /// <summary>
        /// The end parameter of the trim.
        /// </summary>
        double EndParameter { get; }

        /// <summary>
        /// The start parameter of the trim.
        /// </summary>
        double StartParameter { get; }

        /// <summary>
        /// The basis curve for this trimmed curve.
        /// </summary>
        TBasis BasisCurve { get; }
    }
}