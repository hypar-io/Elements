using System.Collections.Generic;
using Newtonsoft.Json;

namespace Elements
{
    /// <summary>A reference to a model, hosted at a URL.</summary>
    [JsonConverter(typeof(Elements.Serialization.JSON.JsonInheritanceConverter), "discriminator")]
    public partial class GeometryReference
    {
        /// <summary>The URL where the referenced geometry is hosted.</summary>
        [JsonProperty("GeometryUrl", Required = Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string GeometryUrl { get; set; }

        /// <summary>Any geometric data directly contained in this reference.</summary>
        [JsonProperty("InternalGeometry", Required = Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public IList<object> InternalGeometry { get; set; }

        /// <summary>
        /// Construct a geometry reference.
        /// </summary>
        /// <param name="geometryUrl">The url of the referenced geometry.</param>
        /// <param name="internalGeometry">Geometry containe in this reference.</param>
        [JsonConstructor]
        public GeometryReference(string @geometryUrl, IList<object> @internalGeometry)
        {
            this.GeometryUrl = @geometryUrl;
            this.InternalGeometry = @internalGeometry;
        }
    }
}