using System;

namespace Elements.Geometry.Solids
{
    /// <summary>
    /// An extrusion operation.
    /// </summary>
    public partial class Extrude
    {
        private double _rotation = 0.0;
        
        internal static void ValidateConstructorParameters(Profile @profile, double @height, Vector3 @direction, double rotation, bool @isVoid)
        {
            if(direction.Length() == 0)
            {
                throw new ArgumentException("The extrude cannot be created. The provided direction has zero length.");
            }
        }

        /// <summary>
        /// Get the updated solid representation of the frame.
        /// </summary>
        internal override Solid GetSolid()
        {            
            return Kernel.Instance.CreateExtrude(this.Profile, this.Height, this.Direction, this._rotation);
        }
    }
}