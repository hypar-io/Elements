using System;

namespace Elements.Geometry.Solids
{
    /// <summary>
    /// An extrusion operation.
    /// </summary>
    public partial class Extrude
    {
        private double _rotation = 0.0;
        
        /// <summary>
        /// Get the updated solid representation of the frame.
        /// </summary>
        internal override Solid GetSolid()
        {            
            return Kernel.Instance.CreateExtrude(this.Profile, this.Height, this.Direction, this._rotation);
        }
    }
}