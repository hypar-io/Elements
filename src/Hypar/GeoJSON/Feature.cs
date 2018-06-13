using Newtonsoft.Json;
using System.Collections.Generic;

namespace Hypar.GeoJSON
{
    public class Feature
    {
        [JsonProperty("type")]
        public string Type{
            get
            {
                return GetType().Name;
            }
        }

        [JsonProperty("properties", NullValueHandling=NullValueHandling.Ignore)]
        public Dictionary<string, object> Properties{get; set;}

        [JsonProperty("geometry")]
        public Geometry Geometry{get;set;}

        [JsonProperty("bbox", NullValueHandling=NullValueHandling.Ignore)]
        public IEnumerable<double> BBox{get;}

        public Feature(Geometry geometry, Dictionary<string,object> properties = null)
        {
            this.Geometry = geometry;
        }
    }
}