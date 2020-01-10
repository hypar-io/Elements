using Newtonsoft.Json;

namespace RevitAutomationDeploy
{
    internal class NewVersionResponse
    {
        [JsonProperty("package")]
        public string Package{get;set;}
        [JsonProperty("engine")]
        public string Engine{get;set;}
        [JsonProperty("description")]
        public string Description{get;set;}
        [JsonProperty("version")]
        public int Version{get;set;}
        [JsonProperty("id")]
        public string Id{get;set;}
    }
}