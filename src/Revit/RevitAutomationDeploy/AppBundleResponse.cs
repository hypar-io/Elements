using Newtonsoft.Json;

namespace RevitAutomationDeploy
{
    internal class AppBundleResponse
    {
        [JsonProperty("uploadParameters")]
        public UploadParameters UploadParameters{get;set;}
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