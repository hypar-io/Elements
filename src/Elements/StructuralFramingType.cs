using System;
using Elements.Geometry;
using Elements.Geometry.Interfaces;
using Elements.Interfaces;
using Newtonsoft.Json;

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
            this.Profile = profile;
            this.ProfileId = profile.Id;
            this.Material = material;
            this.MaterialId = material.Id;
        }
        
        [JsonConstructor]
        internal StructuralFramingType(string name, Guid profileId, Guid materialId) : base(name)
        {
            this.ProfileId = profileId;
            this.MaterialId = materialId;
        }

        /// <summary>
        /// The framing type's profile.
        /// </summary>
        [JsonIgnore]
        public Profile Profile { get; private set;}

        /// <summary>
        /// The framing type's material.
        /// </summary>
        [JsonIgnore]
        public Material Material { get; private set; }

        /// <summary>
        /// Set the profile.
        /// </summary>
        public void SetReference(Profile obj)
        {
            this.Profile = obj;
            this.ProfileId = obj.Id;
        }   

        /// <summary>
        /// Set the material.
        /// </summary>
        public void SetReference(Material obj)
        {
            this.Material = obj;
            this.MaterialId = obj.Id;
        }
    }
}