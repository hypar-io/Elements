using System;
using Elements.Geometry;
using Elements.Interfaces;

namespace Elements
{
    /// <summary>
    /// A curve which is visible in 3D.
    /// </summary>
    [UserElement]
    public class ModelCurve: Element, IMaterial
    {   
        /// <summary>
        /// The curve.
        /// </summary>
        public Curve Curve { get; private set;}

        /// <summary>
        /// The model curve's material.
        /// </summary>
        public Material Material { get; private set; }

        /// <summary>
        /// Create a model curve.
        /// </summary>
        /// <param name="curve">The curve.</param>
        /// <param name="material">The material. Specular and glossiness components will be ignored.</param>
        /// <param name="transform">The model curve's transform.</param>
        /// <param name="id">The id of the model curve.</param>
        /// <param name="name">The name of the model curve.</param>
        public ModelCurve(Curve curve,
                          Material material = null,
                          Transform transform = null,
                          Guid id = default(Guid),
                          string name = null) : base(id, name, transform)
        {
            this.Curve = curve;
            this.Material = material != null ? material : BuiltInMaterials.Edges;
        }
    }
}