using System.Collections.Generic;
using Elements.Geometry;

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
        public SnappingPoints(IEnumerable<Vector3> points)
        {
            Points.AddRange(points);
        }

        /// <summary>
        /// Snapping points.
        /// </summary>
        public List<Vector3> Points { get; } = new List<Vector3>();
    }
}