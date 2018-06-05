using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Hypar.Geometry;

namespace Hypar.Elements
{
    public interface IMeshProvider
    {
        Mesh Tessellate();
    }

    public interface IDataProvider
    {
        Dictionary<string,double> Data();
    }

    /// <summary>
    /// Base class for all Elements.
    /// </summary>
    public abstract class Element
    {
        protected Material _material;
        protected Transform _transform;

        /// <summary>
        /// The Element's material.
        /// </summary>
        /// <returns></returns>
        public Material Material => _material;

        /// <summary>
        /// The unique identifier of the Element.
        /// </summary>
        /// <returns></returns>
        public Guid Id { get; }

        public Transform Transform => _transform;

        /// <summary>
        /// Construct an Element.
        /// </summary>
        /// <param name="material"></param>
        /// <param name="transform"></param>
        public Element(Material material = null, Transform transform = null)
        {
            this.Id = Guid.NewGuid();
            this._material = material == null ? BuiltIntMaterials.Default : material;
            this._transform = transform;
        }


        public Element WithTransform(Transform t)
        {
            this._transform = t;
            return this;
        }
    }
}