using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Hypar.Geometry;

namespace Hypar.Elements
{
    /// <summary>
    /// Base class for all Elements.
    /// </summary>
    public abstract class Element
    {
        private Material _material;
        private Transform _transform;

        /// <summary>
        /// The Element's material.
        /// </summary>
        /// <returns></returns>
        public Material Material
        {
            get
            {
                return _material;
            }
            set
            {
                _material = value;
            }
        }

        /// <summary>
        /// The unique identifier of the Element.
        /// </summary>
        /// <returns></returns>
        public Guid Id { get; }

        /// <summary>
        /// The element's transform.
        /// </summary>
        public Transform Transform
        {
            get
            {
                return _transform;
            }
            set
            {
                _transform  = value;
            }
        }

        /// <summary>
        /// Construct an Element.
        /// </summary>
        /// <param name="material">The element's material.</param>
        /// <param name="transform">The element's transform.</param>
        public Element(Material material = null, Transform transform = null)
        {
            this.Id = Guid.NewGuid();
            this._material = material == null ? BuiltInMaterials.Default : material;
            this._transform = transform;
        }
    }
}