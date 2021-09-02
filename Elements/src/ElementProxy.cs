using System;
using System.Collections.Generic;
using Elements.GeoJSON;
using Newtonsoft.Json;

namespace Elements
{
    /// <summary>
    /// Proxy for an element from another function.
    /// This is used to attach additional information to upstream elements.
    /// </summary>
    public class ElementProxy : Element
    {
        /// <summary>
        /// ID of element that this is a proxy for.
        /// </summary>
        [JsonProperty("elementId")]
        public Guid ElementId { get; set; }

        /// <summary>
        /// Dependency string for the dependency that this element came from.
        /// </summary>
        [JsonProperty("dependency")]
        public string Dependency { get; set; }

        /// <summary>
        /// JSON constructor only.
        /// </summary>
        /// <returns></returns>
        [JsonConstructor]
        public ElementProxy(Element element, string dependencyName, Guid id = default(Guid), string name = null) : base(id, name)
        {
            this.ElementId = element.Id;
            this.Dependency = dependencyName;
        }
    }
}