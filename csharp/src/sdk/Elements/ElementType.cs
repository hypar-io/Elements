using Newtonsoft.Json;
using System;

namespace Hypar.Elements
{
    /// <summary>
    /// Base class for all ElementTypes
    /// </summary>
    public abstract class ElementType
    {
        /// <summary>
        /// The unique identifier of an ElementType.
        /// </summary>
        [JsonProperty("id")]
        public string Id{get;internal set;}

        /// <summary>
        /// The type of the ElementType.
        /// Used during serialization.
        /// </summary>
        [JsonProperty("type")]
        public abstract string Type{get;}

        /// <summary>
        /// The name of the ElementType.
        /// </summary>
        [JsonProperty("name")]
        public string Name{get;}

        /// <summary>
        /// A description of the ElementType.
        /// </summary>
        [JsonProperty("description")]
        public string Description{get;}

        /// <summary>
        /// Construct an ElementType.
        /// </summary>
        /// <param name="name">A name.</param>
        /// <param name="description">A description.</param>
        public ElementType(string name, string description = null)
        {
            this.Id = Guid.NewGuid().ToString();
            this.Name = name;
            this.Description = description;
        }
    }
}