namespace Elements
{
    /// <summary>
    /// A layer of homogeneous material.
    /// </summary>
    public partial class MaterialLayer
    {
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