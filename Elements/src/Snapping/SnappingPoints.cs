using System.Collections.Generic;
using Elements.Geometry;
using Elements.Serialization.JSON;
using Newtonsoft.Json;

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
        [JsonProperty("points")]
        [JsonConverter(typeof(VectorListToByteArrayConverter))]
        public List<Vector3> Points { get; } = new List<Vector3>();

        /// <summary>
        /// The modes for creating snap edges.
        /// </summary>
        [JsonProperty("edgeMode")]
        public SnappingEdgeMode EdgeMode { get; set; }
    }
}