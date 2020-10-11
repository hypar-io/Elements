using Elements.Geometry;
using Elements.Interfaces;
using System;
using Elements.Geometry.Solids;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Elements
{
    /// <summary>
    /// A floor is a horizontal element defined by a profile.
    /// </summary>
    /// <example>
    /// [!code-csharp[Main](../../Elements/test/FloorTests.cs?name=example)]
    /// </example>
    [UserElement]
    public class Floor : GeometricElement, IHasOpenings
    {
        /// <summary>
        /// The elevation from which the floor is extruded.
        /// </summary>
        [JsonIgnore]
        public double Elevation => this.Transform.Origin.Z;

        /// <summary>
        /// The thickness of the floor.
        /// </summary>
        public double Thickness { get; set; }

        /// <summary>
        /// The untransformed profile of the floor.
        /// </summary>
        public Profile Profile { get; set; }

        /// <summary>
        /// A collection of openings in the floor.
        /// </summary>
        public List<Opening> Openings { get; } = new List<Opening>();

        /// <summary>
        /// Create a floor.
        /// </summary>
        /// <param name="profile">The perimeter of the floor.</param>
        /// <param name="thickness">The thickness of the floor.</param>
        /// <param name="transform">The floor's transform. Create a transform with a Z coordinate for the origin, to define the elevation of the floor.</param>
        /// <param name="material">The floor's material.</param>
        /// <param name="representation">The floor's representation.</param>
        /// <param name="isElementDefinition">Is this an element definition?</param>
        /// <param name="id">The floor's id.</param>
        /// <param name="name">The floor's name.</param>
        public Floor(Profile profile,
                     double thickness,
                     Transform transform = null,
                     Material material = null,
                     Representation representation = null,
                     bool isElementDefinition = false,
                     Guid id = default(Guid),
                     string name = null) : base(transform != null ? transform : new Transform(),
                                                material != null ? material : BuiltInMaterials.Concrete,
                                                representation != null ? representation : new Representation(new List<SolidOperation>()),
                                                isElementDefinition,
                                                id != default(Guid) ? id : Guid.NewGuid(),
                                                name)
        {
            SetProperties(profile, thickness);
        }

        private void SetProperties(Profile profile, double thickness)
        {
            if (thickness <= 0.0)
            {
                throw new ArgumentOutOfRangeException($"The floor could not be created. The provided thickness ({thickness}) was less than or equal to zero.");
            }

            this.Profile = profile;
            this.Thickness = thickness;
        }

        /// <summary>
        /// Get the profile of the floor transformed by the floor's transform.
        /// </summary>
        public Profile ProfileTransformed()
        {
            return this.Transform != null ? this.Transform.OfProfile(this.Profile) : this.Profile;
        }

        /// <summary>
        /// The area of the floor.
        /// </summary>
        /// <returns>The area of the floor, not including the area of openings.</returns>
        public double Area()
        {
            return Math.Abs(this.Profile.Area());
        }

        /// <summary>
        /// The area of the floor.
        /// </summary>
        /// <returns>The area of the floor, not including the area of openings.</returns>
        public double Volume()
        {
            return Math.Abs(this.Profile.Area()) * this.Thickness;
        }

        /// <summary>
        /// Update the representations.
        /// </summary>
        public override void UpdateRepresentations()
        {
            this.Representation.SolidOperations.Clear();
            foreach (var o in this.Openings)
            {
                this.Representation.SolidOperations.Add(new Extrude(o.Profile, this.Thickness, Vector3.ZAxis, true)
                {
                    LocalTransform = o.Transform
                });
            }
            this.Representation.SolidOperations.Add(new Extrude(this.Profile, this.Thickness, Vector3.ZAxis, false));
        }
    }
}