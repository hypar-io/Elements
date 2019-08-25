using Elements.Geometry;
using Elements.Geometry.Interfaces;
using Elements.Geometry.Solids;
using Elements.Interfaces;
using Newtonsoft.Json;
using System;

namespace Elements
{
    /// <summary>
    /// An extruded volume.
    /// </summary>
    /// <example>
    /// [!code-csharp[Main](../../test/Examples/MassExample.cs?name=example)]
    /// </example>
    public class Mass : Element, ISolid, IExtrude, IMaterial
    {
        /// <summary>
        /// The Profile of the mass.
        /// </summary>
        public Profile Profile { get; }

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
        public Solid Geometry { get; }

        /// <summary>
        /// The mass' material.
        /// </summary>
        public Material Material {get;}

        /// <summary>
        /// The direction of the mass's extrusion.
        /// </summary>
        public Vector3 ExtrudeDirection => Vector3.ZAxis;

        /// <summary>
        /// The depth of the mass' extrusion.
        /// </summary>
        public double ExtrudeDepth => this.Height;
        
        /// <summary>
        /// Should the mass extrude to both sides of the profile?
        /// </summary>
        public bool BothSides => false;

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
            if(transform != null)
            {
                this.Transform = transform;
            }
            this.Material = material == null ? BuiltInMaterials.Mass : material;
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
            if(transform != null)
            {
                this.Transform = transform;
            }
            this.Material = material == null ? BuiltInMaterials.Mass : material;
        }

        /// <summary>
        /// The volume of the mass.
        /// </summary>
        public double Volume()
        {
            return Math.Abs(this.Profile.Area()) * this.Height;
        }
    
        /// <summary>
        /// Get the profile of the mass transformed by the mass' transform.
        /// </summary>
        public Profile ProfileTransformed()
        {
            return this.Transform != null ? this.Transform.OfProfile(this.Profile) : this.Profile;
        }

        /// <summary>
        /// Get the updated solid representation of the Mass.
        /// </summary>
        public Solid GetUpdatedSolid()
        {
            return Kernel.Instance.CreateExtrude(this);
        }
    }
}