using Elements.Geometry.Interfaces;
using Elements.Interfaces;
using Newtonsoft.Json;
using System;
using Elements.Geometry;
using System.Collections.Generic;

namespace Elements
{
    /// <summary>
    /// A space represents the extruded boundary of an occupiable region.
    /// </summary>
    public class Space : Element, IGeometry3D
    {   
        /// <summary>
        /// The Profile of the Space.
        /// </summary>
        [JsonProperty("profile")]
        public IProfile Profile { get; }

        /// <summary>
        /// The transformed Profile of the Space.
        /// </summary>
        [JsonIgnore]
        public IProfile ProfileTransformed
        {
            get { return this.Transform != null ? this.Transform.OfProfile(this.Profile) : this.Profile; }
        }

        /// <summary>
        /// The Space's geometry.
        /// </summary>
        [JsonProperty("geometry")]
        public IBRep[] Geometry { get; internal set;}

        /// <summary>
        /// Construct a space from an IBRep.
        /// </summary>
        /// <param name="geometry">The BRep which will be used to define the space.</param>
        /// <param name="transform">The Transform of the Space.</param>
        public Space(IBRep geometry, Transform transform = null)
        {
            this.Transform = transform;
            this.Geometry = new[] { geometry };
        }

        /// <summary>
        /// Construct a space.
        /// </summary>
        /// <param name="profile">The Profile of the Space.</param>
        /// <param name="height">The height of the Space.</param>
        /// <param name="elevation">The elevation of the Space.</param>
        /// <param name="material">The Space's material.</param>
        /// <param name="transform">The Space's transform.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the height is less than or equal to 0.0.</exception>
        // [JsonConstructor]
        public Space(Profile profile, double height, double elevation = 0.0, Material material = null, Transform transform = null)
        {
            if (height <= 0.0)
            {
                throw new ArgumentOutOfRangeException(Messages.HEIGHT_EXCEPTION, "height");
            }

            this.Profile = profile;
            // this.Height = height;
            this.Transform = transform != null ? transform : new Transform(new Vector3(0, 0, elevation));
            this.Geometry = new[] { new Extrude(this.Profile, height, material == null ? BuiltInMaterials.Default : material) };
        }

        /// <summary>
        /// Construct a Space.
        /// </summary>
        /// <param name="profile">The Profile of the Space.</param>
        /// <param name="height">The height of the Space above the lower elevation.</param>
        /// <param name="elevation">The elevation of the Space.</param>
        /// <param name="material">The Space's material.</param>
        /// <param name="transform">The Space's transform.</param>
        public Space(Polygon profile, double height, double elevation = 0.0, Material material = null, Transform transform = null)
        {
            if (height <= 0.0)
            {
                throw new ArgumentOutOfRangeException(Messages.HEIGHT_EXCEPTION, "height");
            }

            this.Profile = new Profile(profile);
            // this.Height = height;

            this.Transform = transform != null ? transform : new Transform(new Vector3(0, 0, elevation));
            this.Geometry = new[] { new Extrude(this.Profile, height, material == null ? BuiltInMaterials.Default : material) };
        }

        /// <summary>
        /// Construct a space from an array of IBRep.
        /// </summary>
        /// <param name="geometry">The BReps which will be used to define the space.</param>
        /// <param name="transform"></param>
        [JsonConstructor]
        public Space(IBRep[] geometry, Transform transform = null)
        {
            if (geometry == null || geometry.Length == 0)
            {
                throw new ArgumentOutOfRangeException("You must supply at least one IBRep to construct a Space.");
            }

            // TODO: Remove this when the Profile is no longer available
            // as a property on the Element. 
            foreach(var g in geometry)
            {
                var extrude = g as Extrude;
                if(extrude != null)
                {
                    this.Profile = extrude.Profile;
                }
            }

            this.Transform = transform;
            this.Geometry = geometry;
        }
    }
}