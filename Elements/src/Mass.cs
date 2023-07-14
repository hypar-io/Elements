using Elements.Geometry;
using Elements.Geometry.Solids;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Elements
{
    /// <summary>
    /// An extruded volume.
    /// </summary>
    /// <example>
    /// [!code-csharp[Main](../../Elements/test/MassTests.cs?name=example)]
    /// </example>
    public class Mass : GeometricElement
    {
        /// <summary>
        /// The profile of the mass.
        /// </summary>
        public Profile Profile { get; set; }

        /// <summary>
        /// The height of the mass.
        /// </summary>
        public double Height { get; set; }

        /// <summary>
        /// The thickness of the mass' extrusion.
        /// </summary>
        [JsonIgnore]
        public double Thickness
        {
            get { return this.Height; }
        }

        /// <summary>
        /// Construct a Mass.
        /// </summary>
        /// <param name="profile">The profile of the mass.</param>
        /// <param name="height">The height of the mass from the bottom elevation.</param>
        /// <param name="material">The mass' material. The default is the built in mass material.</param>
        /// <param name="transform">The mass' transform.</param>
        /// <param name="representation">The mass' representation.</param>
        /// <param name="isElementDefinition">Is this an element definition?</param>
        /// <param name="id">The id of the mass.</param>
        /// <param name="name">The name of the mass.</param>
        public Mass(Profile profile,
                    double height = 1.0,
                    Material material = null,
                    Transform transform = null,
                    Representation representation = null,
                    bool isElementDefinition = false,
                    Guid id = default(Guid),
                    string name = null) : base(transform != null ? transform : new Transform(),
                                               material != null ? material : BuiltInMaterials.Mass,
                                               representation != null ? representation : new Representation(new List<SolidOperation>()),
                                               isElementDefinition,
                                               id != default(Guid) ? id : Guid.NewGuid(),
                                               name)
        {
            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException($"The Mass could not be created. The height provided, {height}, must be greater than zero.");
            }
            this.Profile = profile;
            this.Height = height;
        }

        /// <summary>
        /// The volume of the mass.
        /// </summary>
        public double Volume()
        {
            return this.Profile.Area() * this.Height;
        }

        /// <summary>
        /// Get the profile of the mass transformed by the mass' transform.
        /// </summary>
        public Profile ProfileTransformed()
        {
            return this.Transform != null ? this.Transform.OfProfile(this.Profile) : this.Profile;
        }

        /// <summary>
        /// Update the representations.
        /// </summary>
        public override void UpdateRepresentations()
        {
            if (this.Representation.SolidOperations.Count == 0)
            {
                this.Representation.SolidOperations.Add(new Extrude(this.Profile, this.Height, Vector3.ZAxis, false));
            }
            else
            {
                var o = (Extrude)this.Representation.SolidOperations[0];
                o.Height = this.Height;
            }
        }
    }
}