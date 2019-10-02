using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Elements.Interfaces;
using Elements.Serialization.JSON;
using System.Linq;

namespace Elements
{
    /// <summary>
    /// Base class for all Elements.
    /// </summary>
    public partial class Element : IElement
    {
        private Dictionary<string, IProperty> _properties = new Dictionary<string, IProperty>();

        /// <summary>
        /// The type of the element.
        /// Used during deserialization to disambiguate derived types.
        /// </summary>
        [JsonProperty(Order = -100)]
        public string Type
        {
            get { return this.GetType().FullName.ToLower(); }
        }

        /// <summary>
        /// A map of properties for the element.
        /// </summary>
        [JsonProperty(NullValueHandling=NullValueHandling.Ignore), 
            JsonConverter(typeof(PropertyDictionaryConverter))]
        public Dictionary<string, IProperty> Properties
        {
            get { return _properties; }
        }

        /// <summary>
        /// The element's transform.
        /// </summary>
        public Transform Transform { get; protected set; }

        /// <summary>
        /// The element's name.
        /// </summary>
        public string Name { get; set; }

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

        /// <summary>
        /// Get the first component of the provided type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>The first component of type T or null.</returns>
        public Component FirstComponentOfType<T>() where T: Component
        {
            return this.Components.FirstOrDefault(c=>c is T);
        }

        /// <summary>
        /// Get all components of the provided type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>A collection of components.</returns>
        public IEnumerable<Component> AllComponentsOfType<T>() where T: Component
        {
            return this.Components.OfType<T>();
        }
        
        /// <summary>
        /// Gets the component with the specified name.
        /// </summary>
        /// <param name="name">The name of the component.</param>
        /// <returns>The first component with the specified name or null.</returns>
        public Component GetComponentByName(string name)
        {
            return this.Components.FirstOrDefault(c=>c.Name == name);
        }
    }
}