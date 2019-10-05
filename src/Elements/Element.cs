using Newtonsoft.Json;
using System;
using Elements.Geometry;
using Elements.Interfaces;

namespace Elements
{
    /// <summary>
    /// An attribute which defines an element as a user-defined element type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class UserElement : Attribute{}

    /// <summary>
    /// An attribute which defines a property for which the decorated property is the id.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ReferencedByProperty : Attribute
    {
        string _propertyName;

        /// <summary>
        /// Create an ReferencedByProperty.
        /// </summary>
        /// <param name="propertyName">The name of the id property corresponding 
        /// to this property.</param>
        public ReferencedByProperty(string propertyName)
        {
            this._propertyName = propertyName;
        }
    }

    /// <summary>
    /// Base class for all Elements.
    /// </summary>
    public abstract partial class Element : IElement
    {
        /// <summary>
        /// Construct an element.
        /// </summary>
        public Element()
        {
            this.Id = Guid.NewGuid();
            this.Transform = new Transform();
        }

        /// <summary>
        /// Construct an element.
        /// </summary>
        /// <param name="id">The unique identifer of the element.</param>
        [JsonConstructor]
        public Element(Guid id)
        {
            this.Id = id;
            this.Transform = new Transform();
        }
    }
}