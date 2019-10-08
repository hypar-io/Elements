using System;
using Newtonsoft.Json;

namespace Elements.Geometry.Solids
{
    public partial class Sweep
    {
        /// <summary>
        /// Create a sweep of a profile along a curve.
        /// </summary>
        /// <param name="profile">The profile to sweep.</param>
        /// <param name="curve">The curve along which to sweep.</param>
        /// <param name="startSetback">The amount to set back from the start of the curve.</param>
        /// <param name="endSetback">The amount to set back from the end of the curve.</param>
        /// <param name="isVoid">Is the sweep a void?</param>
        public Sweep(Profile profile, Curve curve, double startSetback = 0.0, double endSetback = 0.0, bool isVoid = false): base(isVoid)
        {
            this.Profile = profile;
            this.Curve = curve;
            this.StartSetback = startSetback;
            this.EndSetback = endSetback;
        }

        /// <summary>
        /// Get the updated solid representation of the frame.
        /// </summary>
        /// <returns></returns>
        internal override Solid GetUpdatedSolid()
        {
            return Kernel.Instance.CreateSweepAlongCurve(this.Profile, this.Curve, this.StartSetback, this.EndSetback);
        }
    }
}