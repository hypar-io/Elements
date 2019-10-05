using Elements.Geometry;
using Elements.Geometry.Interfaces;
using Elements.Interfaces;
using Newtonsoft.Json;
using System;
using Elements.Geometry.Solids;

namespace Elements
{
    /// <summary>
    /// An extruded region of occupiable space.
    /// </summary>
    /// <example>
    /// [!code-csharp[Main](../../test/Examples/SpaceExample.cs?name=example)]
    /// </example>
    [UserElement]
    public class Space : Element, ISolid, IExtrude, IMaterial
    {
        private Guid _profileId;
        private Guid _materialId;

        /// <summary>
        /// The profile of the space.
        /// </summary>
        [JsonIgnore]
        [ReferencedByProperty("ProfileId")]
        public Profile Profile { get; private set; }

        /// <summary>
        /// The profile id of the space.
        /// </summary>
        public Guid ProfileId
        {
            get
            {
                return this.Profile != null ? this.Profile.Id : this._profileId;
            }
        }

        /// <summary>
        /// The space's geometry.
        /// </summary>
        [JsonIgnore]
        public Solid Geometry { get; internal set; }

        /// <summary>
        /// The space's material.
        /// </summary>
        [JsonIgnore]
        [ReferencedByProperty("MaterialId")]
        public Material Material { get; private set; }

        /// <summary>
        /// The space's material id.
        /// </summary>
        public Guid MaterialId
        {
            get
            {
                return this.Material != null ? this.Material.Id : this._materialId;
            }
        }

        /// <summary>
        /// The space's height.
        /// </summary>
        public double Height{get;}

        /// <summary>
        /// The extrude direction of the space.
        /// </summary>
        public Vector3 ExtrudeDirection => Vector3.ZAxis;

        /// <summary>
        /// The extrude height of the space.
        /// </summary>
        public double ExtrudeDepth => this.Height;

        /// <summary>
        /// Should the space extrude to both sides of the profile?
        /// </summary>
        public bool BothSides => false;

        /// <summary>
        /// Construct a space from a solid.
        /// </summary>
        /// <param name="geometry">The solid which will be used to define the space.</param>
        /// <param name="transform">The transform of the space.</param>
        /// <param name="material">The space's material.</param>
        internal Space(Solid geometry, Transform transform = null, Material material = null)
        {
            if (geometry == null)
            {
                throw new ArgumentOutOfRangeException("You must supply one IBRep to construct a Space.");
            }
            this.Transform = transform;
            this.Material = material == null ? BuiltInMaterials.Default : material;
            this.Geometry = geometry;
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

            this.Profile = profile;
            this.Material = material == null ? BuiltInMaterials.Default : material;
            this.Transform = transform != null ? transform : new Transform(new Vector3(0, 0, elevation));
            this.Height = height;
        }

        [JsonConstructor]
        internal Space(Guid profileId, Guid materialId, double height, double elevation = 0.0, Transform transform = null)
        {
            if (height <= 0.0)
            {
                throw new ArgumentOutOfRangeException($"The Space could not be created. The height provided, {height}, was less than zero. The height must be greater than zero.", "height");
            }

            this._profileId = profileId;
            this._materialId = materialId;
            this.Transform = transform != null ? transform : new Transform(new Vector3(0, 0, elevation));
            this.Height = height;
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
            this.Material = material == null ? BuiltInMaterials.Mass : material;
            this.Height = height;
        }

        /// <summary>
        /// Get the profile of the space transformed by the space's transform.
        /// </summary>
        public Profile ProfileTransformed()
        {
            return this.Transform != null ? this.Transform.OfProfile(this.Profile) : this.Profile;
        }

        /// <summary>
        /// Get the updated solid representation of the space.
        /// </summary>
        public Solid GetUpdatedSolid()
        {
            return Kernel.Instance.CreateExtrude(this);
        }

        /// <summary>
        /// The spaces's area.
        /// </summary>
        public double Area()
        {
            return Math.Abs(Profile.Area());
        }

        /// <summary>
        /// The spaces's volume.
        /// </summary>
        public double Volume()
        {
            return Math.Abs(Profile.Area()) * this.Height;
        }

        /// <summary>
        /// Set the profile reference.
        /// </summary>
        public void SetReference(Profile profile)
        {
            this.Profile = profile;
            this._profileId = profile.Id;
        }

        /// <summary>
        /// Set the material reference.
        /// </summary>
        public void SetReference(Material material)
        {
            this.Material = material;
            this._materialId = material.Id;
        }
    }
}