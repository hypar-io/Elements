using System.Collections.Generic;
using Elements.Search;

namespace Elements.Geometry
{
    /// <summary>
    /// Line segment extension methods.
    /// </summary>
    public static class LineSegmentExtensions
    {
        /// <summary>
        /// Find all intersections of the provided collection of lines.
        /// </summary>
        /// <param name="items">A collection of lines.</param>
        /// <returns>A collection of unique intersection points.</returns>
        public static List<Vector3> Intersections(this IList<Line> items)
        {
            var network = Network<Line>.FromSegmentableItems(items, (line) => { return line; }, out _, out List<Vector3> allIntersectionLocations);
            return allIntersectionLocations;
        }
    }
}