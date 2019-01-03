using Elements.Geometry.Interfaces;
using Newtonsoft.Json;

namespace Elements.Geometry
{
    /// <summary>
    /// An profile extruded along a curve.
    /// </summary>
    public class ExtrudeAlongCurve : IExtrudeAlongCurve
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
        /// A distance along the directrix from the start, where the extrusion should begin.
        /// </summary>
        [JsonProperty("start_setback")]
        public double StartSetback { get; }

        /// <summary>
        /// A distnace along the directrix from the end, where the extrusion should end.
        /// </summary>
        [JsonProperty("end_setback")]
        public double EndSetback { get; }

        /// <summary>
        /// The directrix of the ExtrudeAlongCurve.
        /// </summary>
        [JsonProperty("curve")]
        public ICurve Curve { get; }

        /// <summary>
        /// The extrusion's Profile.
        /// </summary>
        [JsonProperty("profile")]
        public IProfile Profile { get; }

        /// <summary>
        /// Construct an extrusion along a curve.
        /// </summary>
        /// <param name="profile">The profile to extrude.</param>
        /// <param name="curve">The directrix of the extrusion.</param>
        /// <param name="material">The material to be applied to all extruded faces.</param>
        /// <param name="capped">A flag indicating whether the extrusion is to have end faces.</param>
        /// <param name="startSetback">A distance along the directrix from the start, where the extrusion should begin.</param>
        /// <param name="endSetback">A distnace along the directrix from the end, where the extrusion should end.</param>
        [JsonConstructor]
        public ExtrudeAlongCurve(IProfile profile, ICurve curve, Material material, bool capped = true, double startSetback = 0, double endSetback = 0)
        {
            this.Profile = profile;
            this.Material = material;
            this.Curve = curve;
            this.StartSetback = startSetback;
            this.EndSetback = endSetback;
            this.Faces = Extrusions.ExtrudeAlongCurve(profile, curve, capped, startSetback, endSetback);
        }
    }
}