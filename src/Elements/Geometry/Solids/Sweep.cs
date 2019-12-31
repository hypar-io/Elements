namespace Elements.Geometry.Solids
{
    public partial class Sweep
    {
        /// <summary>
        /// Get the updated solid representation of the frame.
        /// </summary>
        internal override Solid GetSolid()
        {
            return Kernel.Instance.CreateSweepAlongCurve(this.Profile, this.Curve, this.StartSetback, this.EndSetback);
        }
    }
}