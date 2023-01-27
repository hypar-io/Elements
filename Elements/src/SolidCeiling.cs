using Elements.Geometry;
using Elements.Geometry.Solids;
using Elements.Interfaces;
using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Elements
{
    /// <summary>
    /// A ceiling defined by a planar profile extruded to a thickness.
    /// </summary>
    public class SolidCeiling : BaseCeiling, IHasOpenings
    {
        /// <summary>
        /// The thickness of the ceiling.
        /// </summary>
        public double Thickness { get; protected set; }

        /// <summary>
        /// A collection of openings in the ceiling.
        /// </summary>
        public List<Opening> Openings { get; } = new List<Opening>();

        /// <summary>
        /// Construct a ceiling by extruding a profile.
        /// </summary>
        /// <param name="perimeter">The plan profile of the ceiling. It must lie on the XY plane.
        /// The Z coordinate will be ignored</param>
        /// <param name="thickness">The thickness of the ceiling.</param>
        /// <param name="elevation">The elevation of the ceiling.</param>
        /// <param name="material">The material of the ceiling.</param>
        /// <param name="transform">An optional transform for the ceiling.</param>
        /// <param name="representation">The ceiling's representation.</param>
        /// <param name="isElementDefinition">Is this an element definition?</param>
        /// <param name="id">The id of the ceiling.</param>
        /// <param name="name">The name of the ceiling.</param>
        public SolidCeiling(Polygon perimeter,
                      double thickness,
                      double elevation,
                      Material material = null,
                      Transform transform = null,
                      Representation representation = null,
                      bool isElementDefinition = false,
                      Guid id = default(Guid),
                      string name = null)
            : base(perimeter, elevation, material, transform, representation, isElementDefinition, id, name)
        {
            if (thickness <= 0.0)
            {
                throw new ArgumentOutOfRangeException(nameof(thickness), "The ceiling could not be created. The thickness of the ceiling must be greater than 0.0.");
            }

            this.Thickness = thickness;
        }

        /// <summary>
        /// Construct a ceiling by extruding a profile.
        /// </summary>
        /// <param name="perimeter">The plan perimeter of the ceiling. It must lie on the XY plane.
        /// Z coordinate will be used as elevation</param>
        /// <param name="thickness">The thickness of the ceiling.</param>
        /// <param name="material">The material of the ceiling.</param>
        /// <param name="transform">An optional transform for the ceiling.</param>
        /// <param name="representation">The ceiling's representation.</param>
        /// <param name="isElementDefinition">Is this an element definition?</param>
        /// <param name="id">The id of the ceiling.</param>
        /// <param name="name">The name of the ceiling.</param>
        public SolidCeiling(Polygon perimeter,
                      double thickness,
                      Material material = null,
                      Transform transform = null,
                      Representation representation = null,
                      bool isElementDefinition = false,
                      Guid id = default(Guid),
                      string name = null)
            : base(perimeter, material, transform, representation, isElementDefinition, id, name)
        {
            if (thickness <= 0.0)
            {
                throw new ArgumentOutOfRangeException(nameof(thickness), "The ceiling could not be created. The thickness of the ceiling must be greater than 0.0.");
            }

            this.Thickness = thickness;
        }

        /// <summary>
        /// Construct a ceiling by extruding a profile. It's a private constructor that doesn't add elevation to transform
        /// </summary>
        /// <param name="perimeter">The plan profile of the ceiling. It must lie on the XY plane.
        /// The Z coordinate will be ignored</param>
        /// <param name="thickness">The thickness of the ceiling.</param>
        /// <param name="elevation">The elevation of the ceiling.</param>
        /// <param name="material">The material of the ceiling.</param>
        /// <param name="transform">An optional transform for the ceiling.</param>
        /// <param name="representation">The ceiling's representation.</param>
        /// <param name="isElementDefinition">Is this an element definition?</param>
        /// <param name="id">The id of the ceiling.</param>
        /// <param name="name">The name of the ceiling.</param>
        [JsonConstructor]
        protected SolidCeiling(Polygon perimeter,
                      double thickness,
                      double elevation,
                      Guid id = default(Guid),
                      Material material = null,
                      Transform transform = null,
                      Representation representation = null,
                      bool isElementDefinition = false,
                      string name = null)
            : base(perimeter, elevation, id, material, transform, representation, isElementDefinition, name)
        {
            if (thickness <= 0.0)
            {
                throw new ArgumentOutOfRangeException(nameof(thickness), "The ceiling could not be created. The thickness of the ceiling must be greater than 0.0.");
            }

            this.Thickness = thickness;
        }

        /// <summary>
        /// The Profile of the Ceiling computed from its Perimeter and the Openings.
        /// </summary>
        /// <returns></returns>
        public Profile GetProfile()
        {
            return new Profile(Perimeter, Openings?.Select(o => o.Perimeter).ToList());
        }

        /// <summary>
        /// Add an Opening to the Ceiling.
        /// </summary>
        /// <param name="perimeter">The plan perimeter of the ceiling. It must lie on the XY plane.
        /// The Z coordinate will be ignored</param>
        public void AddOpening(Polygon perimeter)
        {
            if (!perimeter.Normal().IsParallelTo(Vector3.ZAxis))
            {
                throw new ArgumentOutOfRangeException("The opening could not be created. The perimeter polygon must lie on the XY plane");
            }

            // TODO: add support of the openings with custom depthFront/depthBack
            var opening = new Opening(perimeter.Project(new Plane(Vector3.Origin, Vector3.ZAxis)), perimeter.Normal(), Thickness, 0);
            this.Openings.Add(opening);
        }

        /// <summary>
        /// Update the representations.
        /// </summary>
        public override void UpdateRepresentations()
        {
            this.Representation.SolidOperations.Clear();
            var extrude = new Extrude(GetProfile(), this.Thickness, Vector3.ZAxis, false);
            this.Representation.SolidOperations.Add(extrude);
        }

        /// <summary>
        /// Construct a ceiling from geometry.
        /// </summary>
        /// <param name="geometry">The geometry of the ceiling.</param>
        /// <param name="transform">The ceiling's Transform.</param>
        /// <param name="isElementDefinition">Is this an element definition?</param>
        internal SolidCeiling(Solid geometry,
                      Transform transform = null,
                      bool isElementDefinition = false) : base(transform, isElementDefinition)
        {
            if (geometry == null)
            {
                throw new ArgumentNullException(nameof(geometry), "You must supply one solid to construct a Ceiling.");
            }

            this.Representation.SolidOperations.Add(new ConstructedSolid(geometry));
        }
    }
}