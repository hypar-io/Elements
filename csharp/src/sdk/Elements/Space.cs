using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Hypar.Geometry;

namespace Hypar.Elements
{
    /// <summary>
    /// A space represents the extruded boundary of an occupiable region.
    /// </summary>
    public class Space : Element, ITessellate<Mesh>
    {
        private readonly Profile _profile;

        /// <summary>
        /// The type of the element.
        /// </summary>
        public override string Type
        {
            get { return "space"; }
        }

        /// <summary>
        /// The elevation of the Space perimeter.
        /// </summary>
        [JsonProperty("elevation")]
        public double Elevation { get; }

        /// <summary>
        /// The height of the Space above its perimeter elevation.
        /// </summary>
        [JsonProperty("height")]
        public double Height { get; }

        /// <summary>
        /// The Profile of the Space.
        /// </summary>
        [JsonProperty("profile")]
        public Profile Profile
        {
            get { return this.Transform != null? this.Transform.OfProfile(this._profile) : this._profile; }
        }

        /// <summary>
        /// Construct a space.
        /// </summary>
        /// <param name="profile">The Profile of the space.</param>
        /// <param name="elevation">The elevation of the perimeter.</param>
        /// <param name="height">The height of the space above the lower elevation.</param>
        /// <param name="material">The space's material.</param>
        [JsonConstructor]
        public Space(Profile profile, double elevation = 0.0, double height = 1.0, Material material = null)
        {
            if (height <= 0.0)
            {
                throw new ArgumentOutOfRangeException(Messages.HEIGHT_EXCEPTION, "height");
            }

            this._profile = profile;
            this.Elevation = elevation;
            this.Height = height;

            this.Material = material != null ? material : BuiltInMaterials.Default;
            this.Transform = new Transform(new Vector3(0, 0, this.Elevation), new Vector3(1, 0, 0), new Vector3(0, 0, 1));
        }

        /// <summary>
        /// Tessellate the Space.
        /// </summary>
        /// <returns>The Mesh representing the Space.</returns>
        public Mesh Tessellate()
        {
            return Mesh.Extrude(this._profile.Perimeter, this.Height, this._profile.Voids, true);
        }
    }
}