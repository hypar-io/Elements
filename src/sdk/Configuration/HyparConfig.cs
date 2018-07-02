using Newtonsoft.Json;
using System.Collections.Generic;

namespace Hypar.Configuration
{
    /// <summary>
    /// A container for Hypar configuration information.
    /// </summary>
    public class HyparConfig
    {   
        /// <summary>
        /// The description of the function.
        /// </summary>
        /// <returns></returns>
        [JsonProperty("description")]
        public string Description{get;set;}

        /// <summary>
        /// The fully-qualified name of the function.
        /// </summary>
        /// <returns></returns>
        [JsonProperty("function")]
        public string Function{get;set;}

        /// <summary>
        /// The unique identifier of the function.
        /// </summary>
        /// <returns></returns>
        [JsonProperty("function_id")]
        public string FunctionId{get;set;}

        /// <summary>
        /// The runtime used to execute the function.
        /// </summary>
        /// <returns></returns>
        [JsonProperty("runtime")]
        public string Runtime{get;set;}

        /// <summary>
        /// A map of input parameter data for the function.
        /// </summary>
        /// <returns></returns>
        [JsonProperty("parameters")]
        public Dictionary<string,ParameterData> Parameters{get;set;}

        /// <summary>
        /// An optional git repository that stores your function.
        /// </summary>
        /// <returns></returns>
        [JsonProperty("respository_url")]
        public string RepositoryUrl{get;set;}

        /// <summary>
        /// A map of return data for the function.
        /// </summary>
        /// <returns></returns>
        [JsonProperty("returns")]
        public Dictionary<string,ReturnData> Returns{get;set;}

        /// <summary>
        /// A semantic version.
        /// </summary>
        /// <returns></returns>
        [JsonProperty("version")]
        public string Version{get;set;}

        /// <summary>
        /// The email of the function's author.
        /// </summary>
        /// <returns></returns>
        [JsonProperty("email")]
        public string Email{get;set;}

        /// <summary>
        /// Construct a HyparConfig from json.
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static HyparConfig FromJson(string json)
        {
            var converters = new[]{new ParameterDataConverter()};
            var settings = new JsonSerializerSettings(){Converters = converters};
            var config = JsonConvert.DeserializeObject<HyparConfig>(json, settings);
            return config;
        }

        /// <summary>
        /// Serialize the configuration data to JSON.
        /// </summary>
        /// <returns></returns>
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public HyparConfig()
        {
            this.Version = "0.0.1";
            this.Parameters = new Dictionary<string, ParameterData>();
            this.Returns = new Dictionary<string, ReturnData>();
        }
    }
}