using Newtonsoft.Json;

namespace Hypar.Functions
{
    /// <summary>
    /// Return data for a Hypar function.
    /// </summary>
    public class ReturnData
    {
        /// <summary>
        /// A description of the return value.
        /// </summary>
        /// <returns></returns>
        [JsonProperty("description")]
        public string Description{get;set;}

        /// <summary>
        /// The type of the return value.
        /// </summary>
        [JsonProperty("type")]
        public string Type{get;set;}
    }
}