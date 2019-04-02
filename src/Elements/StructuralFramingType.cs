using Elements.Geometry;
using Elements.Geometry.Interfaces;
using Elements.Interfaces;
using Elements.Serialization.JSON;
using Newtonsoft.Json;

namespace Elements
{
    /// <summary>
    /// A container for properties common to a type of structural framing.
    /// </summary>
    public class StructuralFramingType : ElementType, IProfile, IMaterial
    {   
        /// <summary>
        /// The profile used by the structural framing type.
        /// </summary>
        [JsonProperty("profile")]
        public Profile Profile {get;}

        /// <summary>
        /// The material used by the structural framing type.
        /// </summary>
        [JsonProperty("material")]
        public Material Material {get;}

        /// <summary>
        /// The type of the structural framing type.
        /// </summary>
        [JsonProperty("type")]
        public override string Type
        {
            get{return "structural_framing_type";}
        }

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