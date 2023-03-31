using Elements.Geometry.Interfaces;
using Newtonsoft.Json;

namespace Elements.Geometry
{
    /// <summary>
    /// A trimmed curve.
    /// </summary>
    public abstract class TrimmedCurve<TBasis> : BoundedCurve, ITrimmedCurve<TBasis> where TBasis: ICurve
    {
        /// <summary>
        /// The basis curve for this bounded curve.
        /// </summary>
        public TBasis BasisCurve { get; protected set; }
        
        /// <summary>
        /// The end parameter of the trim.
        /// </summary>
        [JsonIgnore]
        public double EndParameter { get; protected set; }

        /// <summary>
        /// The start parameter of the trim.
        /// </summary>
        [JsonIgnore]
        public double StartParameter { get; protected set; }
    }
}