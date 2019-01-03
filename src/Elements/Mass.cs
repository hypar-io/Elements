using Elements.Geometry;
using Elements.Geometry.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Elements
{
    /// <summary>
    /// A Mass represents an extruded building Mass.
    /// </summary>
    public class Mass : Element, IGeometry3D
    {
        private List<Polyline> _sides = new List<Polyline>();

        /// <summary>
        /// The Profile of the Mass.
        /// </summary>
        [JsonProperty("profile")]
        public IProfile Profile { get; }

        /// <summary>
        /// The transformed Profile of the Mass.
        /// </summary>
        [JsonIgnore]
        public IProfile ProfileTransformed
        {
            get { return this.Transform != null ? this.Transform.OfProfile(this.Profile) : this.Profile; }
        }

        /// <summary>
        /// The height of the Mass.
        /// </summary>
        [JsonProperty("height")]
        public double Height { get; }

        /// <summary>
        /// The thickness of the Mass' extrusion.
        /// </summary>
        [JsonIgnore]
        public double Thickness
        {
            get { return this.Height; }
        }

        /// <summary>
        /// The Mass' geometry.
        /// </summary>
        [JsonProperty("geometry")]
        public IBRep[] Geometry { get; }

        /// <summary>
        /// Construct a Mass.
        /// </summary>
        /// <param name="profile">The Profile of the Mass.</param>
        /// <param name="height">The height of the Mass from the bottom elevation.</param>
        /// <param name="material">The Mass' material. The default is the built in Mass material.</param>
        /// <param name="transform">The Mass's transform.</param>
        [JsonConstructor]
        public Mass(IProfile profile, double height = 1.0, Material material = null, Transform transform = null)
        {
            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException("The Mass could not be constructed. The height must be greater than zero.");
            }
            this.Profile = profile;
            this.Height = height;
            this.Transform = transform;
            this.Geometry = new[]{new Extrude(this.Profile, this.Height, material == null ? BuiltInMaterials.Mass : material)};
        }

        /// <summary>
        /// Construct a Mass.
        /// </summary>
        /// <param name="profile">The Profile of the Mass.</param>
        /// <param name="height">The height of the Mass from the bottom elevation.</param>
        /// <param name="material">The Mass' material. The default is the built in Mass material.</param>
        /// <param name="transform">The Mass's transform.</param>
        public Mass(Polygon profile, double height = 1.0, Material material = null, Transform transform = null)
        {
            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException("The Mass could not be constructed. The height must be greater than zero.");
            }
            this.Profile = new Profile(profile);
            this.Height = height;
            this.Transform = transform;
            this.Geometry = new[]{new Extrude(this.Profile, this.Height, material == null ? BuiltInMaterials.Mass : material)};
        }

        /// <summary>
        /// The volume of the Mass.
        /// </summary>
        public double Volume()
        {
            return this.Profile.Area() * this.Height;
        }
    }
}