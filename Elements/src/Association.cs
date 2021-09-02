using System;
using System.Collections.Generic;
using Elements.GeoJSON;
using Newtonsoft.Json;

namespace Elements
{
    /// <summary>
    /// Association between items in one function with items from another function's results.
    /// </summary>
    public class Association : Element
    {
        /// <summary>
        /// Metadata about the element being associated.
        /// </summary>
        [JsonProperty("element")]
        public object Element { get; set; } = null;

        /// <summary>
        /// Metadata about the override being associated.
        /// </summary>
        [JsonProperty("override")]
        public object Override { get; set; } = null;

        /// <summary>
        /// JSON constructor only.
        /// </summary>
        /// <returns></returns>
        [JsonConstructor]
        internal Association((string id, string dependency)? element = null, (string id, string name)? ovd = null, Guid id = default(Guid), string name = null) : base(id, name)
        {
            this.Element = element;
            this.Override = ovd;
        }
    }

    /// <summary>
    /// Extension methods for associations.
    /// </summary>
    public static class AssociationExtensions
    {
        /// <summary>
        /// Add an association between an override and an element.
        /// </summary>
        /// <returns></returns>
        public static Association AddOverrideAssociation(this Model model, (string name, string id) ovd, (string dependency, string id) element)
        {
            var association = new Association();
            association.Override = new { name = ovd.name, id = ovd.id };
            association.Element = new { dependency = element.dependency, id = element.id };
            model.AddElement(association);
            return association;
        }
    }
}