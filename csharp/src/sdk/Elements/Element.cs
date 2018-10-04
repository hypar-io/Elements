using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Hypar.Geometry;
using Hypar.Elements.Serialization;
using Newtonsoft.Json.Linq;

namespace Hypar.Elements
{
    /// <summary>
    /// Base class for all Elements.
    /// </summary>
    public abstract class Element : IIdentifiable
    {
        private Dictionary<string, object> _parameters = new Dictionary<string, object>();

        /// <summary>
        /// A collection of Elements aggregated by this Element.
        /// </summary>
        protected List<Element> _subElements = new List<Element>();

        /// <summary>
        /// The unique identifier of the Element.
        /// </summary>
        /// <returns></returns>
        [JsonProperty("id", Order=-2)]
        public string Id {get;internal set;}

        /// <summary>
        /// The type of the element.
        /// Used during deserialization to disambiguate derived types.
        /// </summary>
        [JsonProperty("type", Order=-1)]
        public abstract string Type{get;}

        /// <summary>
        /// A map of Parameters for the Element.
        /// </summary>
        [JsonProperty("parameters")]
        public Dictionary<string, object> Parameters
        {
            get{return _parameters;}
        }

        /// <summary>
        /// The element's material.
        /// </summary>
        [JsonProperty("material")]
        public Material Material{get; protected set;}
        
        /// <summary>
        /// A collection of Elements aggregated by this Element.
        /// </summary>
        [JsonProperty("sub_elements")]
        public IList<Element> SubElements
        {
            get{return this._subElements;}
        }

        /// <summary>
        /// The element's transform.
        /// </summary>
        [JsonIgnore]
        public Transform Transform{get; protected set;}

        /// <summary>
        /// Construct a default Element.
        /// </summary>
        public Element()
        {
            this.Id = Guid.NewGuid().ToString();
            this.Transform = new Transform();
            this.Material = BuiltInMaterials.Default;
        }

        /// <summary>
        /// Add a Parameter to the Element.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="parameter">The parameter to add.</param>
        /// <exception cref="System.Exception">Thrown when an parameter with the same name already exists.</exception>
        public void AddParameter(string name, Parameter parameter)
        {
            if(!_parameters.ContainsKey(name))
            {
                _parameters.Add(name, parameter);
                return;
            }

            throw new Exception($"The parameter, {name}, already exists");
        }

        /// <summary>
        /// Remove a Parameter from the Parameters map.
        /// </summary>
        /// <param name="name">The name of the parameter to remove.</param>
        /// <exception cref="System.Exception">Thrown when the specified parameter cannot be found.</exception>

        public void RemoveParameter(string name)
        {
            if(_parameters.ContainsKey(name))
            {
                _parameters.Remove(name);
            } 
            
            throw new Exception("The specified parameter could not be found.");
        }
    }
}