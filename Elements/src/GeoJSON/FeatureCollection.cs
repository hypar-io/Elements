using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Elements.GeoJSON
{
    /// <summary>
    /// A GeoJSON feature collection.
    /// </summary>
    public class FeatureCollection
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
        /// A collection of features.
        /// </summary>
        [JsonPropertyName("features")]
        public IEnumerable<Feature> Features { get; set; }

        /// <summary>
        /// Construct a feature collection.
        /// </summary>
        /// <param name="features">A collection of features.</param>
        public FeatureCollection(IEnumerable<Feature> features)
        {
            this.Features = features;
        }
    }
}