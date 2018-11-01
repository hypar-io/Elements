using Newtonsoft.Json;

namespace RevitAutomationDeploy
{
    internal class ActivityRequest
    {
        [JsonProperty("id")]
        public string Id{get;set;}
        [JsonProperty("commandLine")]
        public string[] CommandLine{get;set;}
        [JsonProperty("parameters")]
        public ActivityParameters Parameters{get;set;}
        [JsonProperty("engine")]
        public string Engine{get;set;}
        [JsonProperty("appbundles")]
        public string[] AppBundles{get;set;}
        [JsonProperty("description")]
        public string Description{get;set;}
    }

    internal class ActivityParameters
    {
        [JsonProperty("rvtFile")]
        public RvtFile RvtFile{get;set;}
        [JsonProperty("result")]
        public Result Result{get;set;}
    }

    internal class RvtFile
    {
        [JsonProperty("zip")]
        public bool Zip{get;set;}
        [JsonProperty("ondemand")]
        public bool OnDemand{get;set;}
        [JsonProperty("verb")]
        public string Verb{get;set;}
        [JsonProperty("description")]
        public string Description{get;set;}
        [JsonProperty("required")]
        public bool Required{get;set;}
        [JsonProperty("localName")]
        public string LocalName{get;set;}
    }

    internal class Result
    {
        [JsonProperty("zip")]
        public bool Zip{get;set;}
        [JsonProperty("ondemand")]
        public bool OnDemand{get;set;}
        [JsonProperty("verb")]
        public string Verb{get;set;}
        [JsonProperty("description")]
        public string Description{get;set;}
        [JsonProperty("required")]
        public bool Required{get;set;}
        [JsonProperty("localName")]
        public string LocalName{get;set;}
    }
}