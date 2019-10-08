using Elements.Geometry;
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
    [UserElement]
    public class Mass : Element, IGeometry, IMaterial
    {
        /// <summary>
        /// The profile of the mass.
        /// </summary>
        public Profile Profile { get; private set; }

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
        public Elements.Geometry.Geometry Geometry { get; } = new Geometry.Geometry();

        /// <summary>
        /// The mass' material.
        /// </summary>
        public Material Material { get; private set; }

        /// <summary>
        /// Construct a Mass.
        /// </summary>
        /// <param name="profile">The profile of the mass.</param>
        /// <param name="height">The height of the mass from the bottom elevation.</param>
        /// <param name="material">The mass' material. The default is the built in mass material.</param>
        /// <param name="transform">The mass' transform.</param>
        /// <param name="id">The mass' id.</param>
        /// <param name="name">The mass' name.</param>
        public Mass(Profile profile, double height = 1.0, Material material = null, Transform transform = null, Guid id = default(Guid), string name=null): base(id, name, transform)
        {
            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException($"The Mass could not be created. The height provided, {height}, must be greater than zero.");
            }
            this.Profile = profile;
            this.Height = height;
            this.Material = material == null ? BuiltInMaterials.Mass : material;
            this.Geometry.SolidOperations.Add(new Extrude(this.Profile, this.Height));
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
    }
}