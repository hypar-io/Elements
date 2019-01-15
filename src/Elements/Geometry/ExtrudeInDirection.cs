using Elements.Geometry.Interfaces;
using Newtonsoft.Json;

namespace Elements.Geometry
{
    public class ExtrudeInDirection : IExtrudeInDirection
    {
        /// <summary>
        /// The depth of the extrusion.
        /// </summary>
        public double Depth{get;}

        /// <summary>
        /// The direction in which to extrude.
        /// </summary>
        public Vector3 Direction{get;}

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
        /// The faces of the extrusion.
        /// </summary>
        public IFace[] Faces{get;}

        /// <summary>
        /// The extrusion's Material.
        /// </summary>
        public Material Material{get;}

        /// <summary>
        /// The extrusion's Profile.
        /// </summary>
        public IProfile Profile{get;}

        /// <summary>
        /// Construct an ExtrudeInDirection.
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="start"></param>
        /// <param name="direction"></param>
        /// <param name="depth"></param>
        /// <param name="material"></param>
        /// <param name="capped"></param>
        public ExtrudeInDirection(IProfile profile, Transform start, Vector3 direction, double depth, Material material, bool capped = true)
        {
            this.Profile = profile;
            this.Direction = direction;
            this.Depth = depth;
            this.Material = material;
            this.Faces = Extrusions.ExtrudeInDirection(profile, start, direction, depth, capped);
        }
    }
}