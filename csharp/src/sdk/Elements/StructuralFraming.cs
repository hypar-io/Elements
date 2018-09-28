using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Hypar.Geometry;

namespace Hypar.Elements
{
    /// <summary>
    /// A linear structural element with a cross section.
    /// </summary>
    public abstract class StructuralFraming : Element, ITessellate<Mesh>
    {
        private readonly Line _centerLine;
        private readonly Profile _profile;

        /// <summary>
        /// The cross-section profile of the framing element.
        /// </summary>
        [JsonProperty("profile")]
        public Profile Profile
        {
            get{return this.Transform != null ? this.Transform.OfProfile(this._profile) : this._profile;}
        }

        /// <summary>
        /// The up axis of the framing element.
        /// </summary>
        [JsonProperty("up_axis")]
        public Vector3 UpAxis { get; }

        /// <summary>
        /// The center line of the framing element.
        /// </summary>
        [JsonProperty("center_line")]
        public Line CenterLine
        {
            get { return this._centerLine; }
        }

        /// <summary>
        /// The volume of the StructuralFraming element.
        /// </summary>
        [JsonProperty("volume")]
        public double Volume
        {
            get
            {
                return this._profile.Area * this._centerLine.Length;   
            }
        }

        /// <summary>
        /// Construct a beam.
        /// </summary>
        /// <param name="centerLine">The center line of the Beam.</param>
        /// <param name="profile">The structural Profile of the Beam.</param>
        /// <param name="material">The Beam's material.</param>
        /// <param name="up">The up axis of the Beam.</param>
        [JsonConstructor]
        public StructuralFraming(Line centerLine, Profile profile, Material material = null, Vector3 up = null)
        {
            this._profile = profile;
            this._centerLine = centerLine;
            this.Material = material == null ? BuiltInMaterials.Steel : material;
            var t = centerLine.GetTransform(0.0, up);
            this.UpAxis = up == null ? t.YAxis : up;
        }

        /// <summary>
        /// Tessellate the Beam.
        /// </summary>
        /// <returns>A mesh representing the tessellated Beam.</returns>
        public Mesh Tessellate()
        {
            return Mesh.ExtrudeAlongLine(this._centerLine, this._profile.Perimeter, this._profile.Voids);
        }
    }
}