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
    /// The base class for all ceilings.
    /// </summary>
    public abstract class BaseCeiling : GeometricElement
    {
        /// <summary>
        /// The elevation of the ceiling.
        /// </summary>
        public double Elevation { get; protected set; }

        /// <summary>
        /// The perimeter of the ceiling.
        /// </summary>
        public Polygon Perimeter { get; protected set; }

        /// <summary>
        /// Construct a ceiling from perimeter and an elevation.
        /// </summary>
        /// <param name="perimeter">The plan profile of the ceiling. It must lie on the XY plane.
        /// The Z coordinate will be ignored</param>
        /// <param name="elevation">The elevation of the ceiling.</param>
        /// <param name="material">The material of the ceiling.</param>
        /// <param name="transform">An optional transform for the ceiling.</param>
        /// <param name="representation">The ceiling's representation.</param>
        /// <param name="isElementDefinition">Is this an element definition?</param>
        /// <param name="id">The id of the ceiling.</param>
        /// <param name="name">The name of the ceiling.</param>
        protected BaseCeiling(Polygon perimeter,
                      double elevation,
                      Material material = null,
                      Transform transform = null,
                      Representation representation = null,
                      bool isElementDefinition = false,
                      Guid id = default(Guid),
                      string name = null)
            : base(transform ?? new Transform(),
                   material ?? BuiltInMaterials.Concrete,
                   representation ?? new Representation(new List<SolidOperation>()),
                   isElementDefinition,
                   id != default(Guid) ? id : Guid.NewGuid(),
                   name)
        {
            if (!perimeter.Normal().IsParallelTo(Vector3.ZAxis))
            {
                throw new ArgumentException("The ceiling could not be created. The perimeter polygon must lie on the XY plane");
            }

            this.Elevation = elevation;
            this.Perimeter = perimeter.Project(new Plane(Vector3.Origin, Vector3.ZAxis));
            Transform.Move(Vector3.ZAxis * Elevation);
        }

        /// <summary>
        /// Construct a ceiling from perimeter.
        /// </summary>
        /// <param name="perimeter">The plan perimeter of the ceiling. It must lie on the XY plane.
        /// Z coordinate will be used as elevation</param>
        /// <param name="material">The material of the ceiling.</param>
        /// <param name="transform">An optional transform for the ceiling.</param>
        /// <param name="representation">The ceiling's representation.</param>
        /// <param name="isElementDefinition">Is this an element definition?</param>
        /// <param name="id">The id of the ceiling.</param>
        /// <param name="name">The name of the ceiling.</param>
        protected BaseCeiling(Polygon perimeter,
                      Material material = null,
                      Transform transform = null,
                      Representation representation = null,
                      bool isElementDefinition = false,
                      Guid id = default(Guid),
                      string name = null)
            : base(transform ?? new Transform(),
                   material ?? BuiltInMaterials.Concrete,
                   representation ?? new Representation(new List<SolidOperation>()),
                   isElementDefinition,
                   id != default(Guid) ? id : Guid.NewGuid(),
                   name)
        {
            if (!perimeter.Normal().IsParallelTo(Vector3.ZAxis))
            {
                throw new ArgumentException("The ceiling could not be created. The perimeter polygon must lie on the XY plane");
            }

            // we do not need null check cause id there is no vertices it will fail on calculating Normal
            this.Elevation = perimeter.Vertices.First().Z;
            this.Perimeter = perimeter.Project(new Plane(Vector3.Origin, Vector3.ZAxis));
            Transform.Move(Vector3.ZAxis * Elevation);
        }

        /// <summary>
        /// Construct a ceiling. It's a private constructor that doesn't add elevation to transform.
        /// </summary>
        /// <param name="perimeter">The plan profile of the ceiling. It must lie on the XY plane.
        /// The Z coordinate will be ignored</param>
        /// <param name="elevation">The elevation of the ceiling.</param>
        /// <param name="material">The material of the ceiling.</param>
        /// <param name="transform">An optional transform for the ceiling.</param>
        /// <param name="representation">The ceiling's representation.</param>
        /// <param name="isElementDefinition">Is this an element definition?</param>
        /// <param name="id">The id of the ceiling.</param>
        /// <param name="name">The name of the ceiling.</param>
        [JsonConstructor]
        protected BaseCeiling(Polygon perimeter,
                      double elevation,
                      Guid id = default(Guid),
                      Material material = null,
                      Transform transform = null,
                      Representation representation = null,
                      bool isElementDefinition = false,
                      string name = null)
            : base(transform ?? new Transform(),
                   material ?? BuiltInMaterials.Concrete,
                   representation ?? new Representation(new List<SolidOperation>()),
                   isElementDefinition,
                   id != default(Guid) ? id : Guid.NewGuid(),
                   name)
        {
            if (!perimeter.Normal().IsParallelTo(Vector3.ZAxis))
            {
                throw new ArgumentException("The ceiling could not be created. The perimeter polygon must lie on the XY plane");
            }

            this.Elevation = elevation;
            this.Perimeter = perimeter.Project(new Plane(Vector3.Origin, Vector3.ZAxis));
        }

        private protected BaseCeiling(Transform transform = null, bool isElementDefinition = false)
            : base(transform ?? new Transform(),
                   BuiltInMaterials.Default,
                   new Representation(new List<SolidOperation>()),
                   isElementDefinition,
                   Guid.NewGuid(),
                   null)
        {
        }
    }
}
