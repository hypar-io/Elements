using Elements.Geometry;
using System;
using Elements.Geometry.Solids;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Elements
{
    /// <summary>
    /// An extruded region of occupiable space.
    /// </summary>
    /// <example>
    /// [!code-csharp[Main](../../test/Elements.Tests/Examples/SpaceExample.cs?name=example)]
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
        /// <param name="material">The space's material.</param>
        /// <param name="transform">The space's transform.</param>
        /// <param name="representation">The space's represenation.</param>
        /// <param name="id">The id of the space.</param>
        /// <param name="name">The name of the space.</param>
        /// <exception>Thrown when the height is less than or equal to 0.0.</exception>
        [JsonConstructor]
        public Space(Profile profile,
                     double height,
                     Material material = null,
                     Transform transform = null,
                     Representation representation = null,
                     Guid id = default(Guid),
                     string name = null) : base(transform = transform != null ? transform : new Transform(),
                                                material = material != null ? material : BuiltInMaterials.Mass,
                                                representation = representation != null ? representation : new Representation(new List<SolidOperation>()),
                                                id != default(Guid) ? id : Guid.NewGuid(),
                                                name)
        {
            SetProperties(height, profile, transform);
        }

        private void SetProperties(double height, Profile profile, Transform transform)
        {
            if (height <= 0.0)
            {
                throw new ArgumentOutOfRangeException($"The Space could not be created. The height provided, {height}, was less than zero. The height must be greater than zero.", "height");
            }

            this.Profile = profile;
            this.Transform = transform ?? new Transform();
            this.Height = height;
            if(this.Representation.SolidOperations.Count == 0)
            {
                this.Representation.SolidOperations.Add(new Extrude(this.Profile, this.Height, Vector3.ZAxis, 0.0, false));
            }
        }

        /// <summary>
        /// Construct a space from a solid.
        /// </summary>
        /// <param name="geometry">The solid which will be used to define the space.</param>
        /// <param name="transform">The transform of the space.</param>
        /// <param name="material">The space's material.</param>
        /// <param name="id">The id of the space.</param>
        /// <param name="name">The name of the space.</param>
        internal Space(Solid geometry,
                       Transform transform = null,
                       Material material = null,
                       Guid id = default(Guid),
                       string name = null) : base(transform != null ? transform : new Transform(),
                                                  material != null ? material : BuiltInMaterials.Mass,
                                                  new Representation(new List<SolidOperation>()),
                                                  id != default(Guid) ? id : Guid.NewGuid(),
                                                  name)
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