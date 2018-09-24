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

        /// <summary>
        /// The cross-section profile of the Beam.
        /// </summary>
        [JsonProperty("profile")]
        public IList<Polygon> Profile { get; }

        /// <summary>
        /// The up axis of the Beam.
        /// </summary>
        [JsonProperty("up_axis")]
        public Vector3 UpAxis { get; }

        /// <summary>
        /// The center line of the Beam.
        /// </summary>
        [JsonProperty("center_line")]
        public Line CenterLine
        {
            get{return this.Transform != null ? this.Transform.OfLine(this._centerLine) : this._centerLine;}
        }

        /// <summary>
        /// Construct a beam.
        /// </summary>
        /// <param name="centerLine">The center line of the Beam.</param>
        /// <param name="profile">The structural profile of the Beam.</param>
        /// <param name="material">The Beam's material.</param>
        /// <param name="up">The up axis of the Beam.</param>
        [JsonConstructor]
        public StructuralFraming(Line centerLine, IList<Polygon> profile, Material material = null, Vector3 up = null)
        {
            this.Profile = profile;
            this._centerLine = centerLine;
            this.Material = material == null ? BuiltInMaterials.Steel : material;

            var t = centerLine.GetTransform(up);
            this.UpAxis = up == null ? t.YAxis : up;
            this.Transform = t;
        }

        /// <summary>
        /// Tessellate the Beam.
        /// </summary>
        /// <returns>A mesh representing the tessellated Beam.</returns>
        public Mesh Tessellate()
        {
            return Mesh.ExtrudeAlongLine(this._centerLine, this.Profile);
        }
    }
}