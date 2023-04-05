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
        [JsonIgnore]
        public TBasis BasisCurve { get; protected set; }
    }
}