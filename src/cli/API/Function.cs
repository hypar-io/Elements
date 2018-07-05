using Newtonsoft.Json;
using System.Collections.Generic;

namespace Hypar.API
{
    public class Version
    {
        [JsonProperty("parameters")]
        public Dictionary<string,object> Parameters{get;set;}

        [JsonProperty("returns")]
        public Dictionary<string,object> Returns{get;set;}

        [JsonProperty("created")]
        public string Created{get;set;}
    }

    public class Function
    {
        [JsonProperty("runtime")]
        public string Runtime{get;set;}
        
        [JsonProperty("repository_url")]
        public string RepositoryUrl{get;set;}

        
        [JsonProperty("function")]
        public string EntryPoint{get;set;}

        [JsonProperty("description")]
        public string Description{get;set;}

        [JsonProperty("function_id")]
        public string Id{get;set;}

        [JsonProperty("name")]
        public string Name{get;set;}

        [JsonProperty("versions")]
        public Dictionary<string,Version> Versions{get;set;}

        public Function(){}
    }
}