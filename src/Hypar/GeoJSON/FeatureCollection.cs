using System.Collections.Generic;
using Newtonsoft.Json;

namespace Hypar.GeoJSON
{
    public class FeatureCollection
    {
        [JsonProperty("type")]
        public string Type{
            get
            {
                return GetType().Name;
            }
        }
        
        [JsonProperty("features")]
        public IEnumerable<Feature> Features{get;set;}

        public FeatureCollection(IEnumerable<Feature> features)
        {
            this.Features = features;
        }
    }
}