using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Hypar.Geometry;

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
        public Guid Id {get;}

        /// <summary>
        /// A map of Parameters for the Element.
        /// </summary>
        /// <value></value>
        [JsonProperty("parameters")]
        public Dictionary<string, object> Parameters
        {
            get{return _parameters;}
        }

        /// <summary>
        /// Construct a default Element.
        /// </summary>
        public Element()
        {
            this.Id = Guid.NewGuid();
        }

        /// <summary>
        /// Add a Parameter to the Element.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="parameter">The parameter to add.</param>
        /// <returns></returns>
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
        /// <param name="name"></param>
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