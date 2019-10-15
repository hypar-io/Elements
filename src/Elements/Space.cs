using Elements.Geometry;
using Elements.Interfaces;
using System;
using Elements.Geometry.Solids;
using Newtonsoft.Json;

namespace Elements
{
    /// <summary>
    /// An extruded region of occupiable space.
    /// </summary>
    /// <example>
    /// [!code-csharp[Main](../../test/Examples/SpaceExample.cs?name=example)]
    /// </example>
    [UserElement]
    public class Space : Element, IGeometry, IMaterial
    {
        /// <summary>
        /// The profile of the space.
        /// </summary>
        public Profile Profile { get; private set; }

        /// <summary>
        /// The space's material.
        /// </summary>
        public Material Material { get; private set; }

        /// <summary>
        /// The space's height.
        /// </summary>
        public double Height { get; private set; }

        /// <summary>
        /// The space's geometry.
        /// </summary>
        public Elements.Geometry.Geometry Geometry { get; } = new Geometry.Geometry();

        /// <summary>
        /// Construct a space.
        /// </summary>
        /// <param name="profile">The profile of the space.</param>
        /// <param name="height">The height of the space.</param>
        /// <param name="elevation">The elevation of the space.</param>
        /// <param name="material">The space's material.</param>
        /// <param name="transform">The space's transform.</param>
        /// <param name="id">The id of the space.</param>
        /// <param name="name">The name of the space.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the height is less than or equal to 0.0.</exception>
        [JsonConstructor]
        public Space(Profile profile,
                     double height,
                     double elevation = 0.0,
                     Material material = null,
                     Transform transform = null,
                     Guid id = default(Guid),
                     string name = null) : base(id, name, transform)
        {
            SetProperties(height, profile, transform, material, elevation);
        }

        private void SetProperties(double height, Profile profile, Transform transform, Material material, double elevation)
        {
            if (height <= 0.0)
            {
                throw new ArgumentOutOfRangeException($"The Space could not be created. The height provided, {height}, was less than zero. The height must be greater than zero.", "height");
            }

            this.Profile = profile;
            this.Transform = transform != null ? transform : new Transform(new Vector3(0, 0, elevation));
            this.Material = material == null ? BuiltInMaterials.Mass : material;
            this.Height = height;
            this.Geometry.SolidOperations.Add(new Extrude(this.Profile, this.Height));
        }

        /// <summary>
        /// Construct a space from a solid.
        /// </summary>
        /// <param name="geometry">The solid which will be used to define the space.</param>
        /// <param name="transform">The transform of the space.</param>
        /// <param name="material">The space's material.</param>
        /// <param name="id">The id of the space.</param>
        /// <param name="name">The name of the space.</param>
        public Space(Solid geometry,
                       Transform transform = null,
                       Material material = null,
                       Guid id = default(Guid),
                       string name = null) : base(id, name, transform)
        {
            if (geometry == null)
            {
                throw new ArgumentOutOfRangeException("You must supply one IBRep to construct a Space.");
            }
            this.Transform = transform;
            this.Material = material == null ? BuiltInMaterials.Default : material;
            this.Geometry.SolidOperations.Add(new Import(geometry));
        }

        /// <summary>
        /// Get the profile of the space transformed by the space's transform.
        /// </summary>
        public Profile ProfileTransformed()
        {
            return this.Transform != null ? this.Transform.OfProfile(this.Profile) : this.Profile;
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
    }
}