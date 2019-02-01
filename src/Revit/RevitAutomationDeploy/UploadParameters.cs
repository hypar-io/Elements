using Newtonsoft.Json;
using System.Collections.Generic;

namespace RevitAutomationDeploy
{
    internal class UploadParameters
    {
        [JsonProperty("endpointURL")]
        public string EndpointUrl{get;set;}
        [JsonProperty("formData")]
        public Dictionary<string,string> FormData{get;set;}
    }
}