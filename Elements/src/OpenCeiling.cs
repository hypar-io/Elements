using Elements.Geometry;
using Elements.Geometry.Solids;
using System.Text.Json.Serialization;
using System;

namespace Elements
{
    /// <summary>
    /// A ceiling that has no physical geometry, but still defines a perimeter and an elevation.
    /// </summary>
    public class OpenCeiling : BaseCeiling
    {
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
        public OpenCeiling(Polygon perimeter,
                      double elevation,
                      Material material = null,
                      Transform transform = null,
                      Representation representation = null,
                      bool isElementDefinition = false,
                      Guid id = default(Guid),
                      string name = null)
            : base(perimeter, elevation, material, transform, representation, isElementDefinition, id, name)
        {
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
        public OpenCeiling(Polygon perimeter,
                      Material material = null,
                      Transform transform = null,
                      Representation representation = null,
                      bool isElementDefinition = false,
                      Guid id = default(Guid),
                      string name = null)
            : base(perimeter, material, transform, representation, isElementDefinition, id, name)
        {
        }

        /// <summary>
        /// Construct a ceiling. It's a private constructor that doesn't add elevation to transform
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
        protected OpenCeiling(Polygon perimeter,
                      double elevation,
                      Guid id = default(Guid),
                      Material material = null,
                      Transform transform = null,
                      Representation representation = null,
                      bool isElementDefinition = false,
                      string name = null)
            : base(perimeter, elevation, id, material, transform, representation, isElementDefinition, name)
        {
        }
    }
}
