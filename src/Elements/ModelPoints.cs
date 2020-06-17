using System;
using System.Collections.Generic;
using Elements.Geometry;
using Newtonsoft.Json;

namespace Elements
{
    /// <summary>
    /// A collection of points which are visible in 3D.
    /// </summary>
    /// <example>
    /// [!code-csharp[Main](../../test/Elements.Tests/ModelPointsTests.cs?name=example)]
    /// </example>
    public partial class ModelPoints
    {
        /// <summary>
        /// Create a collection of points.
        /// </summary>
        /// <param name="locations">The locations of the points.</param>
        /// <param name="material">The material. Specular and glossiness components will be ignored.</param>
        /// <param name="transform">The model curve's transform.</param>
        /// <param name="isElementDefinition">Is this an element definition?</param>
        /// <param name="id">The id of the model curve.</param>
        /// <param name="name">The name of the model curve.</param>
        public ModelPoints(IList<Vector3> locations = null,
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
            this.Locations = locations != null ? locations : new List<Vector3>();
        }
    }
}