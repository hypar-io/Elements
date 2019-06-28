namespace Elements.Geometry.Interfaces
{
    /// <summary>
    /// Sweeps an area along a directrix.
    /// </summary>
    public interface ISweepAlongCurve : IProfile, ISolid
    {
        /// <summary>
        /// The curve along which the area is swept.
        /// </summary>
        ICurve Curve { get; }

        /// <summary>
        /// The setback of the extrusion at the start.
        /// </summary>
        double StartSetback { get; }

        /// <summary>
        /// The setback of the extrusion at the end.
        /// </summary>
        double EndSetback { get; }
    }
}