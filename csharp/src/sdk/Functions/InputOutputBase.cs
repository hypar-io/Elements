using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Hypar.Functions
{   
    /// <summary>
    /// Base class for Hypar configuration input parameters.
    /// </summary>
    public abstract class InputOutputBase
    {
        /// <summary>
        /// A description of the parameter.
        /// </summary>
        [JsonProperty("description")]
        public string Description{get;set;}

        /// <summary>
        /// The type of the parameter.
        /// </summary>
        [JsonProperty("type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public virtual HyparParameterType Type{get;set;}

        /// <summary>
        /// Construct an InputOutputBase.
        /// </summary>
        /// <param name="description">The description of the parameter.</param>
        /// <param name="type">The type of the parameter.</param>
        public InputOutputBase(string description, HyparParameterType type)
        {
            this.Description = description;
            this.Type = type;
        }
    }
}