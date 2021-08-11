using System;
using System.Collections.Generic;
using Elements.Geometry;
using Elements.Interfaces;
using Newtonsoft.Json;

namespace Elements
{
    /// <summary>
    /// A collection of points which are visible in 3D.
    /// </summary>
    /// <example>
    /// [!code-csharp[Main](../../Elements/test/ModelPointsTests.cs?name=example)]
    /// </example>
    public class ModelPoints : GeometricElement, IVisualizePoints3d
    {
        /// <summary>
        /// The locations of the points.
        /// </summary>
        public IList<Vector3> Locations { get; set; }

        /// <summary>
        /// Create a collection of points.
        /// </summary>
        /// <param name="locations">The locations of the points.</param>
        /// <param name="material">The material. Specular and glossiness components will be ignored.</param>
        /// <param name="transform">The model curve's transform.</param>
        /// <param name="isElementDefinition">Is this an element definition?</param>
        /// <param name="id">The id of the model curve.</param>
        /// <param name="name">The name of the model curve.</param>
        [JsonConstructor]
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

        /// <summary>
        /// Visualize these points in 3d.
        /// </summary>
        public GraphicsBuffers VisualizePoints3d()
        {
            if (this.Locations.Count == 0)
            {
                return null;
            }

            var gb = new GraphicsBuffers();

            for (var i = 0; i < this.Locations.Count; i++)
            {
                var l = this.Locations[i];
                gb.AddVertex(l, default(Vector3), default(UV), null);
                gb.AddIndex((ushort)(i));
            }

            return gb;
        }
    }
}