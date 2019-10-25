using Elements.Serialization.JSON;
using Newtonsoft.Json;
using System;

namespace Elements
{
    /// <summary>
    /// Base class for all Elements.
    /// </summary>
    [JsonInheritanceAttribute("Elements.GeometricElement", typeof(GeometricElement))]
    public abstract partial class Element : Identifiable
    {
        /// <summary>
        /// Construct an element.
        /// </summary>
        /// <param name="id">The unique identifer of the element.</param>
        /// <param name="name">The name of the element.</param>
        [JsonConstructor]
        public Element(Guid id = default(Guid), string name=null): base(id, name){}
    }
}