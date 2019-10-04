using Elements.Interfaces;
using Newtonsoft.Json;
using System;

namespace Elements.ElementTypes
{
    /// <summary>
    /// Base class for all element types.
    /// </summary>
    public abstract partial class ElementType : IIdentifiable
    {
        /// <summary>
        /// The unique identifier of the element type.
        /// </summary>
        public Guid Id{get;internal set;}

        /// <summary>
        /// The name of the element type.
        /// </summary>
        /// <value></value>
        public string Name {get; internal set;}

        /// <summary>
        /// Construct an element type.
        /// </summary>
        /// <param name="name">A name.</param>
        public ElementType(string name): this(Guid.NewGuid(), name){}

        /// <summary>
        /// Construct an element type.
        /// </summary>
        /// <param name="id">The unique identifier of the element type.</param>
        /// <param name="name">The name of the element type.</param>
        [JsonConstructor]
        public ElementType(Guid id, string name)
        {
            this.Id = id;
            this.Name = name;
        }
    }
}