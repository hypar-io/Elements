using System;
using Elements.Geometry;
using Elements.Geometry.Interfaces;
using Elements.Geometry.Solids;
using Newtonsoft.Json;

namespace Elements
{
    /// <summary>
    /// An Opening in a Wall or Floor.
    /// </summary>
    public class Opening : Element
    {
        /// <summary>
        /// The distance along the X axis of the Transform of the host Element to the center of the opening.
        /// </summary>
        [JsonProperty("perimeter")]
        public Polygon Perimeter { get; }

        /// <summary>
        /// The depth of the extrusion which creates the Opening.
        /// </summary>
        [JsonProperty("depth")]
        public double Depth { get; }

        /// <summary>
        /// Construct an Opening.
        /// </summary>
        /// <param name="perimeter">The perimeter of the Opening.</param>
        /// <param name="depth">The depth of the extrusion which creates the Opening.</param>
        /// <param name="transform">The Transform of the Opening.</param>
        [JsonConstructor]
        public Opening(Polygon perimeter, double depth, Transform transform = null)
        {
            this.Perimeter = perimeter;
            this.Transform = transform;
        }
    }
}