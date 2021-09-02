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
        [JsonProperty("element")]
        public object Element { get; set; } = null;

        [JsonProperty("override")]
        public object Override { get; set; } = null;

        [JsonConstructor]
        public Association((string id, string dependency)? element = null, (string id, string name)? ovd = null, Guid id = default(Guid), string name = null) : base(id, name)
        {
            this.Element = element;
            this.Override = ovd;
        }
    }

    public static class AssociationExtensions
    {
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