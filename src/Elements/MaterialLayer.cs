using System;
using Elements.Interfaces;
using Newtonsoft.Json;

namespace Elements
{
    /// <summary>
    /// A layer of homogeneous material.
    /// </summary>
    public partial class MaterialLayer: IMaterial
    {   
        /// <summary>
        /// The layer's material.
        /// </summary>
        [JsonIgnore]
        [ReferencedByProperty("MaterialId")]
        public Material Material { get; private set; }

        /// <summary>
        /// Construct a material layer.
        /// </summary>
        /// <param name="material">The layer's material.</param>
        /// <param name="thickness">The thickness of the layer.</param>
        public MaterialLayer(Material material, double thickness)
        {
            this.Material = material;
            this.MaterialId = material.Id;
            this.Thickness = thickness;
        }

        [JsonConstructor]
        internal MaterialLayer(Guid materialId, double thickness)
        {
            this.MaterialId = materialId;
            this.Thickness = thickness;
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