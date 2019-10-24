using System;
using System.Collections.Generic;
using Elements.Geometry;
using Newtonsoft.Json;

namespace Elements
{
    /// <summary>
    /// A collection of points which are visible in 3D.
    /// </summary>
    [UserElement]
    public class ModelPoints: GeometricElement
    {   
        /// <summary>
        /// The locations of the points.
        /// </summary>
        public IList<Vector3> Locations { get; private set;}

        /// <summary>
        /// Create a collection of points.
        /// </summary>
        /// <param name="locations">The locations of the points.</param>
        /// <param name="material">The material. Specular and glossiness components will be ignored.</param>
        /// <param name="transform">The model curve's transform.</param>
        /// <param name="id">The id of the model curve.</param>
        /// <param name="name">The name of the model curve.</param>
        [JsonConstructor]
        public ModelPoints(IList<Vector3> locations,
                          Material material = null,
                          Transform transform = null,
                          Guid id = default(Guid),
                          string name = null) : base(material, transform, id, name)
        {
            this.Locations = Locations;
            this.Material = material ?? BuiltInMaterials.Points;
        }

        /// <summary>
        /// Create a collection of points.
        /// </summary>
        /// <param name="material">The material. Specular and glossiness components will be ignored.</param>
        /// <param name="transform">The model curve's transform.</param>
        /// <param name="id">The id of the model curve.</param>
        /// <param name="name">The name of the model curve.</param>
        public ModelPoints(Material material = null,
                          Transform transform = null,
                          Guid id = default(Guid),
                          string name = null) : base(material, transform, id, name)
        {
            this.Locations = new List<Vector3>();
            this.Material = material ?? BuiltInMaterials.Points;
        }

        /// <summary>
        /// Update the geometry.
        /// </summary>
        public override void UpdateRepresentations()
        {
            return;
        }
    }
}