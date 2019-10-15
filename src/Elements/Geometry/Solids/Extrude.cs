using System.Collections.Generic;
using System.Linq;

namespace Elements.Geometry.Solids
{
    /// <summary>
    /// An extrusion operation.
    /// </summary>
    public partial class Extrude
    {
        /// <summary>
        /// An extrusion defined by a profile, a height, and a direction.
        /// </summary>
        /// <param name="profile">The profile to be extruded.</param>
        /// <param name="height">The height of the extrusion.</param>
        /// <param name="direction">The direction of the extrusion.</param>
        /// <param name="isVoid">Is the extrusion operation a void?</param>
        public Extrude(Profile profile, double height, Vector3 direction = null, bool isVoid = false): base(isVoid)
        {
            this.Profile = profile;
            this.Height = height;
            this.Direction = direction ?? Vector3.ZAxis;
        }

        /// <summary>
        /// Get the updated solid representation of the frame.
        /// </summary>
        internal override Solid GetUpdatedSolid(IEnumerable<SolidOperation> voidOps)
        {
            if(voidOps != null)
            {
                // Find all the void ops which lie in the same plane as the profile
                var holes = voidOps.Where(op=>op is Extrude && op.IsVoid == true).Cast<Extrude>().Where(ex=>ex.Direction.IsAlmostEqualTo(this.Direction));
                if(holes.Any())
                {
                    var holeProfiles = holes.Select(ex=>ex.Profile);
                    this.Profile.Clip(holeProfiles);
                }
            }
            
            return Kernel.Instance.CreateExtrude(this.Profile, this.Height, this.Direction);
        }
    }
}