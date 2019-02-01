using Newtonsoft.Json;

namespace RevitAutomationDeploy
{
    internal class NewAppBundleRequest
    {
        [JsonProperty("id")]
        public string Id{get;set;}
        [JsonProperty("engine")]
        public string Engine{get;set;}
        [JsonProperty("description")]
        public string Description{get;set;}
    }
}