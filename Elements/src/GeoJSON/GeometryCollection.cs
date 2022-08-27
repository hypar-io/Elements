using System.Text.Json.Serialization;

namespace Elements.GeoJSON
{

    /// <summary>
    /// A GeoJSON geometry collection.
    /// </summary>
    public class GeometryCollection
    {
        /// <summary>
        /// A collection of geometry.
        /// </summary>
        [JsonPropertyName("geometries")]
        public Geometry[] Geometries { get; }

        /// <summary>
        /// Construct a geometry collection.
        /// </summary>
        /// <param name="geometries">An array of geometries.</param>
        public GeometryCollection(Geometry[] geometries)
        {
            this.Geometries = geometries;
        }
    }
}