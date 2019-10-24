using Elements.Geometry;
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
    public class Space : GeometricElement
    {
        /// <summary>
        /// The profile of the space.
        /// </summary>
        public Profile Profile { get; private set; }

        /// <summary>
        /// The space's height.
        /// </summary>
        public double Height { get; private set; }

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
                     string name = null) : base(material, transform, id, name)
        {
            SetProperties(height, profile, transform, elevation);
        }

        private void SetProperties(double height, Profile profile, Transform transform, double elevation)
        {
            if (height <= 0.0)
            {
                throw new ArgumentOutOfRangeException($"The Space could not be created. The height provided, {height}, was less than zero. The height must be greater than zero.", "height");
            }

            this.Profile = profile;
            this.Transform = transform != null ? transform : new Transform(new Vector3(0, 0, elevation));
            this.Height = height;
            this.Representation.SolidOperations.Add(new Extrude(this.Profile, this.Height));
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
                       string name = null) : base(material, transform, id, name)
        {
            if (geometry == null)
            {
                throw new ArgumentOutOfRangeException("You must supply one IBRep to construct a Space.");
            }
            this.Transform = transform;
            this.Representation.SolidOperations.Add(new Import(geometry));

            // TODO(Ian): When receiving a Space as a solid, as we do with IFC,
            // we won't have a profile. This will cause problems with JSON 
            // serialization later.
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

        /// <summary>
        /// Update the representations.
        /// </summary>
        public override void UpdateRepresentations()
        {
            return;
        }
    }
}