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
    public class Space : Element, IGeometry3D, IProfileProvider
    {
        private PlanarFace[] _faces;

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
        public IProfile Profile{get;}

        /// <summary>
        /// The transformed Profile of the Space.
        /// </summary>
        [JsonIgnore]
        public IProfile ProfileTransformed
        {
            get{return this.Transform != null ? this.Transform.OfProfile(this.Profile) : this.Profile;}
        }

        /// <summary>
        /// The Space's geometry.
        /// </summary>
        [JsonProperty("geometry")]
        public IBRep[] Geometry {get;}

        /// <summary>
        /// Internal constructor for building a BRep of the Space.
        /// </summary>
        internal Space(IBRep geometry, Transform transform = null)
        {
            this.Transform = transform != null ? transform : new Transform();
            this.Geometry = new []{geometry};
        }

        /// <summary>
        /// Construct a space.
        /// </summary>
        /// <param name="profile">The Profile of the Space.</param>
        /// <param name="height">The height of the Space above the lower elevation.</param>
        /// <param name="material">The Space's material.</param>
        /// <param name="transform">The Space's transform.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the height is less than or equal to 0.0.</exception>
        [JsonConstructor]
        public Space(Profile profile, double height = 1.0, Material material = null, Transform transform = null)
        {
            if (height <= 0.0)
            {
                throw new ArgumentOutOfRangeException(Messages.HEIGHT_EXCEPTION, "height");
            }

            this.Profile = profile;
            this.Height = height;
            this.Transform = transform != null? transform : new Transform(new Vector3(0, 0, this.Elevation), new Vector3(1, 0, 0), new Vector3(0, 0, 1));
            this.Geometry = new[]{new Extrude(this.Profile, this.Height, material == null ? BuiltInMaterials.Default : material)};
        }

        /// <summary>
        /// Construct a Space.
        /// </summary>
        /// <param name="profile">The Profile of the Space.</param>
        /// <param name="height">The height of the Space above the lower elevation.</param>
        /// <param name="material">The Space's material.</param>
        /// <param name="transform">The Space's transform.</param>
        public Space(Polygon profile, double height = 1.0, Material material = null, Transform transform = null)
        {
            if (height <= 0.0)
            {
                throw new ArgumentOutOfRangeException(Messages.HEIGHT_EXCEPTION, "height");
            }

            this.Profile = new Profile(profile);
            this.Height = height;

            this.Transform = transform != null? transform : new Transform(new Vector3(0, 0, this.Elevation), new Vector3(1, 0, 0), new Vector3(0, 0, 1));
            this.Geometry = new[]{new Extrude(this.Profile, this.Height, material == null ? BuiltInMaterials.Default : material)};
        }

        internal Space(IBRep[] geometry, Transform transform = null)
        {
            if(geometry == null || geometry.Length == 0)
            {
                throw new ArgumentOutOfRangeException("You must supply at least one IBRep to construct a Space.");
            }

            this.Transform = transform != null? transform : new Transform(new Vector3(0, 0, this.Elevation), new Vector3(1, 0, 0), new Vector3(0, 0, 1));
            this.Geometry = geometry;
        }
    }
}