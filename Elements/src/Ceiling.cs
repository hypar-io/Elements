using Elements.Geometry;
using Elements.Geometry.Solids;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Elements
{
    /// <summary>
    /// A ceiling defined by a planar profile extruded to a thickness.
    /// </summary>
    public class Ceiling : BuildingElement
    {
        /// <summary>
        /// The thickness of the ceiling.
        /// </summary>
        public double Thickness { get; protected set; }

        /// <summary>
        /// The elevation of the ceiling.
        /// </summary>
        public double Elevation { get; protected set; }

        /// <summary>
        /// The perimeter of the ceiling.
        /// </summary>
        public Polygon Perimeter { get; protected set; }

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
        public Ceiling(Polygon perimeter,
                      double thickness,
                      double elevation,
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

            if (!perimeter.Normal().IsParallelTo(Vector3.ZAxis))
            {
                throw new ArgumentOutOfRangeException("The ceiling could not be created. The perimeter polygon must lie on the XY plane");
            }

            this.Elevation = elevation;
            this.Perimeter = perimeter.Project(new Plane(Vector3.Origin, Vector3.ZAxis)); ;
            this.Thickness = thickness;
            Transform.Move(Vector3.ZAxis * Elevation);
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
        public Ceiling(Polygon perimeter,
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

            if (!perimeter.Normal().IsParallelTo(Vector3.ZAxis))
            {
                throw new ArgumentOutOfRangeException("The ceiling could not be created. The perimeter polygon must lie on the XY plane");
            }

            // we do not need null check cause id there is no vertices it will fail on calculating Normal
            this.Elevation = perimeter.Vertices.First().Z;
            this.Perimeter = perimeter.Project(new Plane(Vector3.Origin, Vector3.ZAxis));
            this.Thickness = thickness;
            Transform.Move(Vector3.ZAxis * Elevation);
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
        private Ceiling(Polygon perimeter,
                      double thickness,
                      double elevation,
                      Guid id = default(Guid),
                      Material material = null,
                      Transform transform = null,
                      Representation representation = null,
                      bool isElementDefinition = false,
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

            if (!perimeter.Normal().IsParallelTo(Vector3.ZAxis))
            {
                throw new ArgumentOutOfRangeException("The ceiling could not be created. The perimeter polygon must lie on the XY plane");
            }

            this.Elevation = elevation;
            this.Perimeter = perimeter.Project(new Plane(Vector3.Origin, Vector3.ZAxis)); ;
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