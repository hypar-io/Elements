using System;
using System.Collections.Generic;
using Elements.Geometry;
using Newtonsoft.Json;

namespace Elements
{
    /// <summary>
    /// A collection of arrows which are visible in 3D.
    /// </summary>
    /// <example>
    /// [!code-csharp[Main](../../Elements/test/ModelArrowsTests.cs?name=example)]
    /// </example>
    public class ModelArrows : GeometricElement
    {
        /// <summary>
        /// A collection of tuples specifying the origin and scale
        /// of the arrows.
        /// </summary>
        public IList<(Vector3 origin, Vector3 direction, double scale)> Vectors { get; set; }

        /// <summary>
        /// Should an arrow head be drawn at the start?
        /// </summary>
        public bool ArrowAtStart { get; set; }

        /// <summary>
        /// Should an arrow head be drawn at the end?
        /// </summary>
        public bool ArrowAtEnd { get; set; }

        /// <summary>
        /// The angle of the arrow head.
        /// </summary>
        public double ArrowAngle { get; set; }

        /// <summary>
        /// Create a collection of points.
        /// </summary>
        /// <param name="vectors">A collection of tuples specifying the 
        /// origin, direction, and the scale of the arrows.</param>
        /// <param name="arrowAtStart">Should an arrow head be drawn at the start?</param>
        /// <param name="arrowAtEnd">Should an arrow head be drawn at the end?</param>
        /// <param name="arrowAngle">The angle of the arrow head.</param>
        /// <param name="material">The material. Specular and glossiness components will be ignored.</param>
        /// <param name="transform">The model curve's transform.</param>
        /// <param name="isElementDefinition">Is this an element definition?</param>
        /// <param name="id">The id of the model curve.</param>
        /// <param name="name">The name of the model curve.</param>
        [JsonConstructor]
        public ModelArrows(IList<(Vector3, Vector3, double)> vectors = null,
                           bool arrowAtStart = false,
                           bool arrowAtEnd = true,
                           double arrowAngle = 45.0,
                           Material material = null,
                           Transform transform = null,
                           bool isElementDefinition = false,
                           Guid id = default(Guid),
                           string name = null) : base(transform != null ? transform : new Transform(),
                                                     material != null ? material : BuiltInMaterials.Points,
                                                     null,
                                                     isElementDefinition,
                                                     id != default(Guid) ? id : Guid.NewGuid(),
                                                     name)
        {
            this.Vectors = vectors != null ? vectors : new List<(Vector3, Vector3, double)>();
            this.ArrowAtEnd = arrowAtEnd;
            this.ArrowAtStart = arrowAtStart;
            this.ArrowAngle = arrowAngle;
        }
    }
}