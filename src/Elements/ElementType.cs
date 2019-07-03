using Elements.Interfaces;
using Newtonsoft.Json;
using System;

namespace Elements
{
    /// <summary>
    /// Base class for all ElementTypes
    /// </summary>
    public abstract class ElementType : IIdentifiable
    {
        /// <summary>
        /// The unique identifier of an ElementType.
        /// </summary>
        public Guid Id{get;internal set;}

        /// <summary>
        /// The type of the ElementType.
        /// Used during serialization.
        /// </summary>
        public abstract string Type{get;}

        /// <summary>
        /// The name of the ElementType.
        /// </summary>
        public string Name{get;}

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