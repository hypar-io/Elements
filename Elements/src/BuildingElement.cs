using System.Collections.Generic;

namespace Elements
{
    /// <summary>
    /// An element in a building, such as a floor, wall, or window.
    /// </summary>
    public class BuildingElement : GeometricElement
    {
        /// <summary>
        /// A collection of openings.
        /// </summary>
        public List<Opening> Openings { get; } = new List<Opening>();
    }
}