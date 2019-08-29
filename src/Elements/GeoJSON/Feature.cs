using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Elements.GeoJSON
{
    /// <summary>
    /// A GeoJSON feature.
    /// </summary>
    public class Feature
    {
        /// <summary>
        /// The type of the feature.
        /// </summary>
        [JsonProperty("type")]
        public string Type{
            get
            {
                return GetType().Name;
            }
        }

        /// <summary>
        /// All properties of the feature.
        /// </summary>
        [JsonProperty("properties", NullValueHandling=NullValueHandling.Ignore)]
        public Dictionary<string, object> Properties{get; set;}

        /// <summary>
        /// The geometry of the feature.
        /// </summary>
        [JsonProperty("geometry")]
        [JsonConverter(typeof(GeometryConverter))]
        public Geometry Geometry{get;set;}

        /// <summary>
        /// The bounding box of the feature.
        /// </summary>
        [JsonProperty("bbox", NullValueHandling=NullValueHandling.Ignore)]
        public IEnumerable<double> BBox{get;}

        /// <summary>
        /// Construct a feature.
        /// </summary>
        /// <param name="geometry"></param>
        /// <param name="properties"></param>
        public Feature(Geometry geometry, Dictionary<string,object> properties)
        {
            this.Geometry = geometry;
            this.Properties = properties;
        }
    }
}