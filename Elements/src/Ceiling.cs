using Elements.Geometry;
using Elements.Geometry.Solids;
using Elements.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Elements
{
    /// <summary>
    /// A ceiling defined by a planar profile extruded to a thickness.
    /// </summary>
    public class Ceiling : GeometricElement, IHasOpenings
    {
        /// <summary>
        /// The thickness of the ceiling.
        /// </summary>
        public double Thickness { get; protected set; }

        /// <summary>
        /// The profile of the ceiling.
        /// </summary>
        public Profile Profile { get; protected set; }

        /// <summary>
        /// A collection of openings in the ceiling.
        /// </summary>
        public List<Opening> Openings { get; } = new List<Opening>();

        /// <summary>
        /// Construct a ceiling by extruding a profile.
        /// </summary>
        /// <param name="profile">The plan profile of the ceiling.</param>
        /// <param name="thickness">The thickness of the ceiling.</param>
        /// <param name="material">The material of the ceiling.</param>
        /// <param name="transform">An option transform for the ceiling.</param>
        /// <param name="representation">The ceiling's representation.</param>
        /// <param name="isElementDefinition">Is this an element definition?</param>
        /// <param name="id">The id of the ceiling.</param>
        /// <param name="name">The name of the ceiling.</param>
        [JsonConstructor]
        public Ceiling(Profile profile,
                      double thickness,
                      Material material = null,
                      Transform transform = null,
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
            if (thickness <= 0.0)
            {
                throw new ArgumentOutOfRangeException("The ceiling could not be created. The thickness of the ceiling must be greater than 0.0.");
            }

            this.Profile = profile;
            this.Thickness = thickness;
        }

        /// <summary>
        /// Update the representations.
        /// </summary>
        public override void UpdateRepresentations()
        {
            this.Representation.SolidOperations.Clear();
            this.Representation.SolidOperations.Add(new Extrude(this.Profile, this.Thickness, Vector3.ZAxis, false));
        }

        /// <summary>
        /// A pass-through constructor to set the id, name, and transform.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="transform"></param>
        /// <param name="material"></param>
        /// <param name="representation"></param>
        /// <param name="isElementDefinition">Is this an element definition?</param>
        protected Ceiling(Transform transform,
                       Material material,
                       Representation representation,
                       bool isElementDefinition = false,
                       Guid id = default(Guid),
                       string name = null) : base(transform,
                                           material,
                                           representation,
                                           isElementDefinition,
                                           id == default(Guid) ? Guid.NewGuid() : id,
                                           name)
        { }

        /// <summary>
        /// Construct a ceiling from geometry.
        /// </summary>
        /// <param name="geometry">The geometry of the ceiling.</param>
        /// <param name="transform">The ceiling's Transform.</param>
        /// <param name="isElementDefinition">Is this an element definition?</param>
        internal Ceiling(Solid geometry,
                      Transform transform = null,
                      bool isElementDefinition = false) : base(transform != null ? transform : new Transform(),
                                                         BuiltInMaterials.Default,
                                                         new Representation(new List<SolidOperation>()),
                                                         isElementDefinition,
                                                         Guid.NewGuid(),
                                                         null)
        {
            if (geometry == null)
            {
                throw new ArgumentOutOfRangeException("You must supply one solid to construct a Ceiling.");
            }

            if (transform != null)
            {
                this.Transform = transform;
            }
            this.Representation.SolidOperations.Add(new ConstructedSolid(geometry));
        }
    }
}