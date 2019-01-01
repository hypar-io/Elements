using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Elements.Geometry;
using Elements.Interfaces;
using Elements.Serialization;

namespace Elements
{
    /// <summary>
    /// Base class for all Elements.
    /// </summary>
    public abstract class Element : IElement
    {
        private Dictionary<string, IProperty> _properties = new Dictionary<string, IProperty>();

        /// <summary>
        /// The unique identifier of the Element.
        /// </summary>
        [JsonProperty("id", Order = -101)]
        public long Id { get; internal set; }

        /// <summary>
        /// The type of the element.
        /// Used during deserialization to disambiguate derived types.
        /// </summary>
        [JsonProperty("type", Order = -100)]
        public string Type
        {
            get { return this.GetType().FullName.ToLower(); }
        }

        /// <summary>
        /// A map of Properties for the Element.
        /// </summary>
        [JsonProperty("properties"), JsonConverter(typeof(PropertyDictionaryConverter))]
        public Dictionary<string, IProperty> Properties
        {
            get { return _properties; }
        }

        /// <summary>
        /// The element's transform.
        /// </summary>
        [JsonIgnore]
        public Transform Transform { get; protected set; }

        /// <summary>
        /// Construct a default Element.
        /// </summary>
        public Element()
        {
            this.Id = IdProvider.Instance.GetNextId();
            this.Transform = new Transform();
        }

        /// <summary>
        /// Add a Property to the Element.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="property">The parameter to add.</param>
        /// <exception cref="System.Exception">Thrown when an parameter with the same name already exists.</exception>
        public void AddProperty(string name, IProperty property)
        {
            if (!_properties.ContainsKey(name))
            {
                _properties.Add(name, property);
                return;
            }

            throw new Exception($"The property, {name}, already exists");
        }

        /// <summary>
        /// Remove a Property from the Properties map.
        /// </summary>
        /// <param name="name">The name of the parameter to remove.</param>
        /// <exception cref="System.Exception">Thrown when the specified parameter cannot be found.</exception>

        public void RemoveProperty(string name)
        {
            if (_properties.ContainsKey(name))
            {
                _properties.Remove(name);
            }

            throw new Exception("The specified parameter could not be found.");
        }
    }
}