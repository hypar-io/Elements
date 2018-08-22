using Newtonsoft.Json;
using System.Collections.Generic;

namespace Hypar.API
{
    public class Function
    {   
        [JsonProperty("description")]
        public string Description{get;set;}

        [JsonProperty("email")]
        public string Email{get;set;}

        [JsonProperty("function")]
        public string EntryPoint{get;set;}

        [JsonProperty("function_id")]
        public string Id{get;set;}

        [JsonProperty("name")]
        public string Name{get;set;}

        [JsonProperty("parameters")]
        public Dictionary<string,object> Parameters{get;set;}

        [JsonProperty("repository_url")]
        public string RepositoryUrl{get;set;}

        [JsonProperty("returns")]
        public Dictionary<string,object> Returns{get;set;}

        [JsonProperty("runtime")]
        public string Runtime{get;set;}

        public Function(){}
    }
}