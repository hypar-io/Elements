using Elements.Interfaces;
using Elements.Geometry.Interfaces;
using Elements.Geometry;
using Elements.Geometry.Solids;
using Newtonsoft.Json;
using System;

namespace Elements
{
    /// <summary>
    /// An element defined by a perimeter and a cross section swept along that perimeter.
    /// </summary>
    public class Frame : Element, IProfile, IMaterial, ISweepAlongCurve
    {
        /// <summary>
        /// The frame's profile.
        /// </summary>
        [JsonIgnore]
        public Profile Profile { get; private set;}

        /// <summary>
        /// The frame's profile id.
        /// </summary>
        public Guid ProfileId { get; private set;}

        /// <summary>
        /// The frame's material.
        /// </summary>
        [JsonIgnore]
        public Material Material { get; private set; }

        /// <summary>
        /// The frame's material id.
        /// </summary>
        public Guid MaterialId { get; private set;}

        /// <summary>
        /// The perimeter of the frame.
        /// </summary>
        public Curve Curve { get; }

        /// <summary>
        /// The start setback of the sweep along the curve.
        /// </summary>
        public double StartSetback => 0.0;

        /// <summary>
        /// The end setback of the sweep along the curve.
        /// </summary>
        public double EndSetback => 0.0;

        /// <summary>
        /// Create a frame.
        /// </summary>
        /// <param name="curve">The frame's perimeter.</param>
        /// <param name="profile">The frame's profile.</param>
        /// <param name="offset">The amount which the perimeter will be offset internally.</param>
        /// <param name="material">The frame's material.</param>
        /// <param name="transform">The frame's transform.</param>
        public Frame(Polygon curve, Profile profile, double offset = 0.0, Material material = null, Transform transform = null)
        {
            this.Curve = curve.Offset(-offset)[0];
            this.Profile = profile;
            this.Transform = transform;
            this.Material = material != null ? material : BuiltInMaterials.Default;
        }

        [JsonConstructor]
        internal Frame(Polygon curve, Guid profileId, Guid materialId, double offset = 0.0, Transform transform = null)
        {
            this.Curve = curve.Offset(-offset)[0];
            this.ProfileId = profileId;
            this.Transform = transform;
            this.MaterialId = materialId;
        }

        /// <summary>
        /// Get the updated solid representation of the frame.
        /// </summary>
        /// <returns></returns>
        public Solid GetUpdatedSolid()
        {
            return Kernel.Instance.CreateSweepAlongCurve(this);
        }

        /// <summary>
        /// Set the material.
        /// </summary>
        public void SetReference(Material material)
        {
            this.Material = material;
            this.MaterialId = material.Id;
        }

        /// <summary>
        /// Set the profile.
        /// </summary>
        public void SetReference(Profile profile)
        {
            this.Profile = profile;
            this.MaterialId = profile.Id;
        }
    }
}