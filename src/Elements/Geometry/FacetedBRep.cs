using Elements.Geometry.Interfaces;
using Newtonsoft.Json;

namespace Elements.Geometry
{
    /// <summary>
    /// A connected set of planar faces.
    /// </summary>
    public class FacetedBRep : IBRep
    {
        /// <summary>
        /// The type of the element.
        /// Used during deserialization to disambiguate derived types.
        /// </summary>
        [JsonProperty("type", Order = -100)]
        public string Type
        {
            get { return this.GetType().FullName.ToLower(); }
        }
        
        /// <summary>
        /// The FacetedBRep's faces.
        /// </summary>
        [JsonIgnore]
        public IFace[] Faces { get; }

        /// <summary>
        /// The Material applied to all faces of the FacetedBRep.
        /// </summary>
        [JsonProperty("material")]
        public Material Material { get; }

        /// <summary>
        /// Construct a FacetedBRep
        /// </summary>
        /// <param name="faces">A collection of Faces.</param>
        /// <param name="material">A Material to apply to all faces.</param>
        public FacetedBRep(IFace[] faces, Material material)
        {
            this.Faces = faces;
            this.Material = material;
        }
    }
}