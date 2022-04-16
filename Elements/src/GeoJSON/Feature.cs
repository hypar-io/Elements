using System.Text.Json.Serialization;
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
        [JsonPropertyName("type")]
        public string Type
        {
            get
            {
                return GetType().Name;
            }
        }

        /// <summary>
        /// All properties of the feature.
        /// </summary>
        [JsonPropertyName("properties")]
        public Dictionary<string, object> Properties { get; set; }

        /// <summary>
        /// The geometry of the feature.
        /// </summary>
        [JsonPropertyName("geometry")]
        [JsonConverter(typeof(GeometryConverter))]
        public Geometry Geometry { get; set; }

        /// <summary>
        /// The bounding box of the feature.
        /// </summary>
        [JsonPropertyName("bbox")]
        public IEnumerable<double> BBox { get; }

        /// <summary>
        /// Construct a feature.
        /// </summary>
        /// <param name="geometry"></param>
        /// <param name="properties"></param>
        public Feature(Geometry geometry, Dictionary<string, object> properties)
        {
            this.Geometry = geometry;
            this.Properties = properties;
        }
    }
}