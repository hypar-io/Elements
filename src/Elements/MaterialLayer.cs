using Elements.Geometry;
using Newtonsoft.Json;

namespace Elements
{
    /// <summary>
    /// A layer of homogeneous material.
    /// </summary>
    public class MaterialLayer
    {
        /// <summary>
        /// The thickness of the layer.
        /// </summary>
        [JsonProperty("thickness")]
        public double Thickness {get;}

        /// <summary>
        /// The layer's material.
        /// </summary>
        [JsonProperty("material")]
        public Material Material{get;}

        /// <summary>
        /// Construct a material layer.
        /// </summary>
        /// <param name="material">The layer's material.</param>
        /// <param name="thickness">The thickness of the layer.</param>
        public MaterialLayer(Material material, double thickness)
        {
            this.Material = material;
            this.Thickness = thickness;
        }
    }
}