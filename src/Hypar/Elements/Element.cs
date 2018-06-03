using System;
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
        /// <summary>
        /// The Element's material.
        /// </summary>
        /// <returns></returns>
        public Material Material{get;}

        /// <summary>
        /// The unique identifier of the Element.
        /// </summary>
        /// <returns></returns>
        public Guid Id {get;}

        public Transform Transform{get; protected set;}

        /// <summary>
        /// Construct an Element.
        /// </summary>
        /// <param name="material"></param>
        /// <param name="transform"></param>
        public Element(Material material, Transform transform)
        {
            this.Id = Guid.NewGuid();
            this.Material = material;
            this.Transform = transform;
        }
    }
}