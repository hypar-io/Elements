using System.Collections.Generic;
using System.Text.Json.Serialization;
using Elements.Geometry;
using Elements.Serialization.JSON;

namespace Elements
{
    /// <summary>
    /// Provides information about snapping points.
    /// </summary>
    public class SnappingPoints
    {
        /// <summary>
        /// Initializes a new instance of SnappingPoints class.
        /// </summary>
        /// <param name="points">The set of points.</param>
        /// <param name="edgeMode">The mode for creating snap edges.</param>
        public SnappingPoints(IEnumerable<Vector3> points, SnappingEdgeMode edgeMode = SnappingEdgeMode.LineStrip)
        {
            Points.AddRange(points);
            EdgeMode = edgeMode;
        }

        /// <summary>
        /// Snapping points.
        /// </summary>
        [JsonPropertyName("points")]
        [JsonConverter(typeof(VectorListToByteArrayConverter))]
        public List<Vector3> Points { get; } = new List<Vector3>();

        /// <summary>
        /// The modes for creating snap edges.
        /// </summary>
        [JsonPropertyName("edgeMode")]
        public SnappingEdgeMode EdgeMode { get; set; }
    }
}