namespace Elements.Geometry.Solids
{
    public partial class Sweep
    {
        private double _rotation = 0.0;

        /// <summary>
        /// Get the updated solid representation of the frame.
        /// </summary>
        /// <returns></returns>
        internal override Solid GetSolid()
        {
            return Kernel.Instance.CreateSweepAlongCurve(this.Profile, this.Curve, this.StartSetback, this.EndSetback, _rotation);
        }
    }
}