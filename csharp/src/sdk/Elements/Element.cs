using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Hypar.Geometry;
using Newtonsoft.Json.Linq;

namespace Hypar.Elements
{
    /// <summary>
    /// Base class for all Elements.
    /// </summary>
    public abstract class Element
    {
        private Dictionary<string, object> _parameters = new Dictionary<string, object>();

        /// <summary>
        /// The unique identifier of the Element.
        /// </summary>
        /// <returns></returns>
        [JsonProperty("id")]
        public string Id {get;internal set;}

        /// <summary>
        /// The type of the eleme]]
        /// </summary>
        [JsonProperty("type")]
        public virtual string Type
        {
            get{ return "element";}
        }

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
        /// The element's transform.
        /// </summary>
        [JsonProperty("transform")]
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
        public void AddParameter<T>(string name, Parameter<T> parameter)
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