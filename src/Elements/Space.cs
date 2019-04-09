using Elements.Geometry;
using Elements.Geometry.Interfaces;
using Elements.Interfaces;
using Newtonsoft.Json;
using System;
using Elements.Geometry.Solids;

namespace Elements
{
    /// <summary>
    /// A boundary of an occupiable region.
    /// </summary>
    public class Space : Element, IGeometry3D, IProfile, IMaterial
    {
        /// <summary>
        /// The profile of the space.
        /// </summary>
        public Profile Profile { get; }

        /// <summary>
        /// The space's geometry.
        /// </summary>
        public Solid[] Geometry { get; internal set; }

        /// <summary>
        /// The space's material.
        /// </summary>
        public Material Material { get; }

        /// <summary>
        /// Construct a space from a solid.
        /// </summary>
        /// <param name="geometry">The BRep which will be used to define the space.</param>
        /// <param name="transform">The transform of the space.</param>
        /// <param name="material">The space's material.</param>
        public Space(Solid geometry, Transform transform = null, Material material = null)
        {
            this.Transform = transform;
            this.Material = material == null ? BuiltInMaterials.Default : material;
            this.Geometry = new[] { geometry };
        }

        /// <summary>
        /// Construct a space.
        /// </summary>
        /// <param name="profile">The profile of the space.</param>
        /// <param name="height">The height of the space.</param>
        /// <param name="elevation">The elevation of the space.</param>
        /// <param name="material">The space's material.</param>
        /// <param name="transform">The space's transform.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the height is less than or equal to 0.0.</exception>
        public Space(Profile profile, double height, double elevation = 0.0, Material material = null, Transform transform = null)
        {
            if (height <= 0.0)
            {
                throw new ArgumentOutOfRangeException($"The Space could not be created. The height provided, {height}, was less than zero. The height must be greater than zero.", "height");
            }

            this.Material = material == null ? BuiltInMaterials.Default : material;
            this.Transform = transform != null ? transform : new Transform(new Vector3(0, 0, elevation));
            this.Geometry = new[] { Solid.SweepFace(profile.Perimeter, profile.Voids, height, this.Material) };
        }

        /// <summary>
        /// Construct a Space.
        /// </summary>
        /// <param name="profile">The profile of the space.</param>
        /// <param name="height">The height of the space above the lower elevation.</param>
        /// <param name="elevation">The elevation of the space.</param>
        /// <param name="material">The space's material.</param>
        /// <param name="transform">The space's transform.</param>
        public Space(Polygon profile, double height, double elevation = 0.0, Material material = null, Transform transform = null)
        {
            if (height <= 0.0)
            {
                throw new ArgumentOutOfRangeException($"The Space could not be created. The height provided, {height}, was less than zero. The height must be greater than zero.", "height");
            }

            this.Profile = new Profile(profile);
            this.Transform = transform != null ? transform : new Transform(new Vector3(0, 0, elevation));
            this.Material = this.Material == null ? BuiltInMaterials.Default : material;
            this.Geometry = new[] { Solid.SweepFace(this.Profile.Perimeter, this.Profile.Voids, height, this.Material) };
        }

        /// <summary>
        /// Construct a space from an array of solids.
        /// </summary>
        /// <param name="geometry">An array of solids which will be used to define the space.</param>
        /// <param name="transform">The space's Transform.</param>
        /// <param name="material">The space's Material.</param>
        [JsonConstructor]
        public Space(Solid[] geometry, Transform transform = null, Material material = null)
        {
            if (geometry == null || geometry.Length == 0)
            {
                throw new ArgumentOutOfRangeException("You must supply at least one IBRep to construct a Space.");
            }

            // TODO: Remove this when the Profile is no longer available
            // as a property on the Element. 
            // foreach(var g in geometry)
            // {
            //     var extrude = g as Extrude;
            //     if(extrude != null)
            //     {
            //         this.Profile = extrude.Profile;
            //     }
            // }
            this.Material = this.Material == null ? BuiltInMaterials.Default : material;
            this.Transform = transform;
            this.Geometry = geometry;
        }
    
        /// <summary>
        /// Get the profile of the space transformed by the space's transform.
        /// </summary>
        public Profile ProfileTransformed()
        {
            return this.Transform != null ? this.Transform.OfProfile(this.Profile) : this.Profile;
        }
    }
}