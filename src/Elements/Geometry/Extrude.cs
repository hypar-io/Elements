using Elements.Geometry.Interfaces;
using Newtonsoft.Json;

namespace Elements.Geometry
{
    /// <summary>
    /// An profile extruded to a depth.
    /// </summary>
    public class Extrude : IExtrude
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
        /// The extrusion's faces.
        /// </summary>
        [JsonIgnore]
        public IFace[] Faces { get; }

        /// <summary>
        /// The Material applied to all faces of the extrusion.
        /// </summary>
        [JsonProperty("material")]
        public Material Material { get; }

        /// <summary>
        /// The depth of the Extrusion.
        /// </summary>
        [JsonProperty("depth")]
        public double Depth { get; }

        /// <summary>
        /// The extrusion's Profile.
        /// </summary>
        [JsonProperty("profile")]
        public IProfile Profile { get; }

        /// <summary>
        /// Construct an extrusion along a curve.
        /// </summary>
        /// <param name="profile">The profile to extrude.</param>
        /// <param name="depth">The depth of the extrusion.</param>
        /// <param name="material">The material to be applied to all extruded faces.</param>
        /// <param name="capped">A flag indicating whether the extrusion is to have end faces.</param>
        [JsonConstructor]
        public Extrude(IProfile profile, double depth, Material material, bool capped = true)
        {
            this.Profile = profile;
            this.Material = material;
            this.Depth = depth;
            this.Faces = Extrusions.Extrude(profile, depth, 0.0, capped);
        }
    }
}