using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Hypar.Functions
{   
    /// <summary>
    /// Base class for Hypar configuration input parameters.
    /// </summary>
    public abstract class ParameterBase
    {
        /// <summary>
        /// A description of the parameter.
        /// </summary>
        /// <returns></returns>
        [JsonProperty("description")]
        public string Description{get;set;}

        /// <summary>
        /// The type of the parameter.
        /// </summary>
        /// <returns></returns>
        [JsonProperty("type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public virtual ParameterType Type{get;set;}

        /// <summary>
        /// Construct a ParameterBase.
        /// </summary>
        /// <param name="description">The description of the parameter.</param>
        /// <param name="type">The type of the parameter.</param>
        public ParameterBase(string description, ParameterType type)
        {
            this.Description = description;
            this.Type = type;
        }
    }
}