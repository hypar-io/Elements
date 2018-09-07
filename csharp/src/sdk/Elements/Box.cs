using Hypar.Geometry;
using Newtonsoft.Json;

namespace Hypar.Elements
{
    /// <summary>
    /// Box represents a unit square box.
    /// </summary>
    public class Box: Element, ILocateable<Vector3>, ITessellate<Mesh>, IMaterialize
    {   
        /// <summary>
        /// The material of the box.
        /// </summary>
        /// <value></value>
        [JsonIgnore]
        public Material Material{get;set;}

        /// <summary>
        /// The origin of the box.
        /// </summary>
        /// <value></value>
        [JsonProperty("location")]
        public Vector3 Location{get;}

        /// <summary>
        /// Construct a unit square box.
        /// </summary>
        public Box(Vector3 origin)
        {
            this.Location = new Vector3();
            this.Material = new Material("box", new Color(1.0f, 0.0f, 0.0f, 1.0f), 0.0f, 0.0f);
        }

        /// <summary>
        /// Tessellate the box.
        /// </summary>
        /// <returns>A mesh representing the tessellated box.</returns>
        public Mesh Tessellate() 
        {
            return Mesh.Extrude(new[] { Profiles.Rectangular() }, 1);
        }
    }
}