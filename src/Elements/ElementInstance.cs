using System;
using Elements.Geometry;

namespace Elements
{
    /// <summary>
    /// An instance of an element in the model.
    /// Instances point to one instance of a type, but have
    /// individual ids and transforms.
    /// </summary>
    public class ElementInstance : Element
    {
        /// <summary>
        /// The element from which this instance is derived.
        /// </summary>
        public GeometricElement Parent { get; }

        /// <summary>
        /// The transform of the instance.
        /// </summary>
        public Transform Transform { get; }
        
        /// <summary>
        /// Construct an element instance.
        /// </summary>
        /// <param name="parent">The element from which this instance is derived.</param>
        /// <param name="transform">The transform of the instance.</param>
        /// <param name="name">The name of the instance.</param>
        /// <param name="id">The id of the instance.</param>
        public ElementInstance(GeometricElement parent,
                               Transform transform,
                               string name = null,
                               Guid id = default(Guid)) : base (id == default(Guid) ? Guid.NewGuid() : id, name)
        {
            this.Parent = parent;
            this.Transform = transform;
        }
    } 
}