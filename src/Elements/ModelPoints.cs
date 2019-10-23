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
    [UserElement]
    public class ModelPoints: Element, IMaterial
    {   
        /// <summary>
        /// The locations of the points.
        /// </summary>
        public IList<Vector3> Locations { get; private set;}

        /// <summary>
        /// The point's material.
        /// </summary>
        public Material Material { get; private set; }

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
                          string name = null) : base(id, name, transform)
        {
            this.Locations = Locations;
            this.Material = material != null ? material : BuiltInMaterials.Edges;
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
                          string name = null) : base(id, name, transform)
        {
            this.Locations = new List<Vector3>();
            this.Material = material != null ? material : BuiltInMaterials.Edges;
        }
    }
}