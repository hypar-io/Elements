using System;

namespace Elements.Geometry
{
    public partial class CurveRepresentation
    {
        /// <summary>
        /// Create a curve representation.
        /// </summary>
        /// <param name="curve">The curve.</param>
        /// <param name="material">The material to apply to the curve.</param>
        public CurveRepresentation(Curve curve, Material material) : base(material, Guid.NewGuid(), null)
        {
            this.Curve = curve;
        }
    }
}