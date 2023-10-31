using System.Collections.Generic;
using Elements.Geometry;
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
        /// <param name="isPolygon">Indicates if snapping points create polygon.</param>
        public SnappingPoints(IEnumerable<Vector3> points, bool isPolygon = false)
        {
            Points.AddRange(points);
            IsPolygon = isPolygon;
        }

        /// <summary>
        /// Snapping points.
        /// </summary>
        [JsonProperty("points")]
        public List<Vector3> Points { get; } = new List<Vector3>();

        /// <summary>
        /// Indicates if snapping points create polygon.
        /// If true, the first and the last points will be connected.
        /// </summary>
        [JsonProperty("isPolygon")]
        public bool IsPolygon { get; set; }
    }
}