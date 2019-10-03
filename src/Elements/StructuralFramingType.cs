using Elements.Geometry;
using Elements.Geometry.Interfaces;
using Elements.Interfaces;

namespace Elements.ElementTypes
{
    /// <summary>
    /// A container for properties common to a type of structural framing.
    /// </summary>
    public partial class StructuralFramingType : ElementType, IProfile, IMaterial
    {
        /// <summary>
        /// Construct a structural framing type.
        /// </summary>
        /// <param name="name">The name of the structural framing type.</param>
        /// <param name="profile">The profile used by the structural framing type.</param>
        /// <param name="material">The material used by the structural framing type.</param>
        public StructuralFramingType(string name, Profile profile, Material material) : base(name)
        {
            this.Profile= profile;
            this.Material = material;
        }
    }
}