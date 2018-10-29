using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;

namespace Hypar.Functions
{
    /// <summary>
    /// A container for Hypar configuration information.
    /// </summary>
    public class HyparConfig
    {   
        /// <summary>
        /// The description of the function.
        /// </summary>
        [JsonProperty("description")]
        public string Description{get;set;}

        /// <summary>
        /// The unique identifier of the function.
        /// </summary>
        [JsonProperty("function_id")]
        public string FunctionId{get;set;}

        /// <summary>
        /// The name of the function.
        /// </summary>
        [JsonProperty("name")]
        public string Name{get;set;}

        /// <summary>
        /// A map of input parameter data for the function.
        /// </summary>
        [JsonProperty("inputs")]
        public Dictionary<string,InputOutputBase> Inputs{get;set;}

        /// <summary>
        /// An optional git repository that stores your function.
        /// </summary>
        [JsonProperty("repository_url")]
        public string RepositoryUrl{get;set;}

        /// <summary>
        /// A map of return data for the function.
        /// </summary>
        [JsonProperty("outputs")]
        public Dictionary<string,InputOutputBase> Outputs{get;set;}

        /// <summary>
        /// Construct a HyparConfig from json.
        /// </summary>
        /// <param name="json"></param>
        public static HyparConfig FromJson(string json)
        {
            var converters = new[]{new InputOutputConverter()};
            var settings = new JsonSerializerSettings(){Converters = converters};
            var config = JsonConvert.DeserializeObject<HyparConfig>(json, settings);
            return config;
        }

        /// <summary>
        /// Serialize the configuration data to JSON.
        /// </summary>
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public HyparConfig()
        {
            this.Inputs = new Dictionary<string, InputOutputBase>();
            this.Outputs = new Dictionary<string, InputOutputBase>();
        }
    }
}