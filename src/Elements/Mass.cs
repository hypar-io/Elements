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
    public class Mass : Element, IExtrude
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
        /// The elevation of the bottom perimeter.
        /// </summary>
        [JsonProperty("elevation")]
        public double Elevation { get; }

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
        /// The Mass' Material.
        /// </summary>
        [JsonProperty("material")]
        public Material Material { get; }

        /// <summary>
        /// Construct a Mass.
        /// </summary>
        /// <param name="profile">The Profile of the Mass.</param>
        /// <param name="elevation">The elevation of the perimeter.</param>
        /// <param name="height">The height of the Mass from the bottom elevation.</param>
        /// <param name="material">The Mass' material. The default is the built in Mass material.</param>
        [JsonConstructor]
        public Mass(Profile profile, double elevation = 0.0, double height = 1.0, Material material = null)
        {
            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException("The Mass could not be constructed. The height must be greater than zero.");
            }
            this.Profile = profile;
            this.Elevation = elevation;
            this.Height = height;
            this.Material = material != null ? material : BuiltInMaterials.Mass;
        }

        /// <summary>
        /// Construct a Mass.
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="elevation"></param>
        /// <param name="height"></param>
        /// <param name="material"></param>
        public Mass(Polygon profile, double elevation = 0.0, double height = 1.0, Material material = null)
        {
            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException("The Mass could not be constructed. The height must be greater than zero.");
            }
            this.Profile = new Profile(profile);
            this.Elevation = elevation;
            this.Height = height;
            this.Material = material != null ? material : BuiltInMaterials.Mass;
        }

        /// <summary>
        /// The Faces of the Mass.
        /// </summary>
        public IFace[] Faces()
        {
            return Extrusions.Extrude(this.Profile, this.Height, this.Elevation);
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