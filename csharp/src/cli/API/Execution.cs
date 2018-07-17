using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Hypar.API
{
    public class LambdaData
    {
        [JsonProperty("start")]
        public DateTime Start{get;set;}
        [JsonProperty("end")]
        public DateTime End{get;set;}
        [JsonProperty("elapsed")]
        public double Elapsed{get;set;}
    }

    public class Execution
    {
        [JsonProperty("function_id")]
        public string FunctionId{get;set;}
        [JsonProperty("args")]
        public Dictionary<string,object> Args{get;set;}
        [JsonProperty("execution_id")]
        public string Id{get;set;}
        [JsonProperty("computed")]
        public Dictionary<string,double> Computed{get;set;}
        [JsonProperty("model_url")]
        public string ModelUrl{get;set;}
        [JsonProperty("thumbnail_url")]
        public string ThumbnailUrl{get;set;}
        [JsonProperty("lambda")]
        public LambdaData Lambda{get;set;}
        [JsonProperty("version")]
        public string Version{get;set;}

        public Execution(){}
    }
}