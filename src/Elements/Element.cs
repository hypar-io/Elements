using Newtonsoft.Json;
using System;
using Elements.Geometry;

namespace Elements
{
    /// <summary>
    /// An attribute which defines an element as a user-defined element type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class UserElement : Attribute{}

    /// <summary>
    /// Base class for all Elements.
    /// </summary>
    public abstract partial class Element : Identifiable
    {
        /// <summary>
        /// Construct an element.
        /// </summary>
        /// <param name="id">The unique identifer of the element.</param>
        /// <param name="name">The name of the element.</param>
        /// <param name="transform">The element's transform.</param>
        [JsonConstructor]
        public Element(Guid id = default(Guid), string name=null, Transform transform = null): base(id, name)
        {
            this.Transform = transform == null ? new Transform() : transform;
        }
    }
}