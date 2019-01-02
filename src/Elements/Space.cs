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
    public class Space : Element, IExtrude, IProfileProvider
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
        /// The Space's Material.
        /// </summary>
        [JsonProperty("material")]
        public Material Material{get;}

        /// <summary>
        /// The Thickness of the Space's extrusion.
        /// </summary>
        /// <value></value>
        [JsonIgnore]
        public double Thickness
        {
            get{return this.Height;}
        }

        /// <summary>
        /// The transformed Profile of the Space.
        /// </summary>
        [JsonIgnore]
        public IProfile ProfileTransformed
        {
            get{return this.Transform != null ? this.Transform.OfProfile(this.Profile) : this.Profile;}
        }

        /// <summary>
        /// Internal constructor for building a BRep of the Space.
        /// </summary>
        internal Space(Material material = null, Transform transform = null)
        {
            this.Material = material != null ? material : BuiltInMaterials.Default;
            this.Transform = transform != null ? transform : new Transform();
        }

        /// <summary>
        /// Construct a space.
        /// </summary>
        /// <param name="profile">The Profile of the space.</param>
        /// <param name="elevation">The elevation of the perimeter.</param>
        /// <param name="height">The height of the space above the lower elevation.</param>
        /// <param name="material">The space's material.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the height is less than or equal to 0.0.</exception>
        [JsonConstructor]
        public Space(Profile profile, double elevation = 0.0, double height = 1.0, Material material = null, Transform transform = null)
        {
            if (height <= 0.0)
            {
                throw new ArgumentOutOfRangeException(Messages.HEIGHT_EXCEPTION, "height");
            }

            this.Profile = profile;
            this.Elevation = elevation;
            this.Height = height;

            this.Material = material != null ? material : BuiltInMaterials.Default;
            this.Transform = transform != null? transform : new Transform(new Vector3(0, 0, this.Elevation), new Vector3(1, 0, 0), new Vector3(0, 0, 1));
        }

        /// <summary>
        /// Construct a Space.
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="elevation"></param>
        /// <param name="height"></param>
        /// <param name="material"></param>
        /// <param name="transform"></param>
        public Space(Polygon profile, double elevation = 0.0, double height = 1.0, Material material = null, Transform transform = null)
        {
            if (height <= 0.0)
            {
                throw new ArgumentOutOfRangeException(Messages.HEIGHT_EXCEPTION, "height");
            }

            this.Profile = new Profile(profile);
            this.Elevation = elevation;
            this.Height = height;

            this.Material = material != null ? material : BuiltInMaterials.Default;
            this.Transform = transform != null? transform : new Transform(new Vector3(0, 0, this.Elevation), new Vector3(1, 0, 0), new Vector3(0, 0, 1));
        }

        internal void SetFaces(PlanarFace[] faces)
        {
            this._faces = faces;
        }

        /// <summary>
        /// A collection of Faces which comprise this Space.
        /// </summary>
        public IFace[] Faces()
        {
            if(this._faces != null)
            {
                return this._faces;
            }
            return Extrusions.Extrude(this.Profile, this.Height, this.Elevation);
        }
    }
}