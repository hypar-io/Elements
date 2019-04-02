using Elements.Geometry;
using Elements.Geometry.Interfaces;
using Elements.Geometry.Solids;
using Elements.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Elements
{
    /// <summary>
    /// An extruded building mass.
    /// </summary>
    public class Mass : Element, IGeometry3D, IProfile, IMaterial
    {
        /// <summary>
        /// The Profile of the mass.
        /// </summary>
        public Profile Profile { get; }

        /// <summary>
        /// The transformed Profile of the mass.
        /// </summary>
        [JsonIgnore]
        public Profile ProfileTransformed
        {
            get { return this.Transform != null ? this.Transform.OfProfile(this.Profile) : this.Profile; }
        }

        /// <summary>
        /// The height of the mass.
        /// </summary>
        public double Height { get; }

        /// <summary>
        /// The thickness of the mass' extrusion.
        /// </summary>
        [JsonIgnore]
        public double Thickness
        {
            get { return this.Height; }
        }

        /// <summary>
        /// The mass' geometry.
        /// </summary>
        public Solid[] Geometry { get; }

        /// <summary>
        /// The mass' material.
        /// </summary>
        public Material Material {get;}

        /// <summary>
        /// Construct a Mass.
        /// </summary>
        /// <param name="profile">The profile of the mass.</param>
        /// <param name="height">The height of the mass from the bottom elevation.</param>
        /// <param name="material">The mass' material. The default is the built in mass material.</param>
        /// <param name="transform">The mass' transform.</param>
        [JsonConstructor]
        public Mass(Profile profile, double height = 1.0, Material material = null, Transform transform = null)
        {
            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException($"The Mass could not be created. The height provided, {height}, must be greater than zero.");
            }
            this.Profile = profile;
            this.Height = height;
            this.Transform = transform;
            this.Material = material == null ? BuiltInMaterials.Mass : material;
            this.Geometry = new[]{Solid.SweepFace(this.Profile.Perimeter, this.Profile.Voids, this.Height, this.Material)};
        }

        /// <summary>
        /// Construct a Mass.
        /// </summary>
        /// <param name="profile">The profile of the mass.</param>
        /// <param name="height">The height of the mass from the bottom elevation.</param>
        /// <param name="material">The mass' material. The default is the built in mass material.</param>
        /// <param name="transform">The mass's transform.</param>
        public Mass(Polygon profile, double height = 1.0, Material material = null, Transform transform = null)
        {
            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException($"The mass could not be created. The height provided, {height}, must be greater than zero.");
            }
            this.Profile = new Profile(profile);
            this.Height = height;
            this.Transform = transform;
            this.Material = material == null ? BuiltInMaterials.Mass : material;
            this.Geometry = new[]{Solid.SweepFace(this.Profile.Perimeter, this.Profile.Voids, this.Height, this.Material)};
        }

        /// <summary>
        /// The volume of the mass.
        /// </summary>
        public double Volume()
        {
            return this.Profile.Area() * this.Height;
        }
    }
}