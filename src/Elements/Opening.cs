using System;
using Elements.Geometry;
using Elements.Geometry.Interfaces;
using Newtonsoft.Json;

namespace Elements
{
    /// <summary>
    /// An Opening in a Wall or Floor.
    /// </summary>
    public class Opening : Element, IGeometry3D
    {
        /// <summary>
        /// The distance along the X axis of the Transform of the host Element to the center of the opening.
        /// </summary>
        [JsonProperty("perimeter")]
        public Polygon Perimeter { get; }

        /// <summary>
        /// The depth of the extrusion which creates the Opening.
        /// </summary>
        public double Depth { get; }

        /// <summary>
        /// The Opening's geometry.
        /// </summary>
        public IBRep[] Geometry { get; }

        /// <summary>
        /// Construct an Opening.
        /// </summary>
        /// <param name="perimeter">The perimeter of the Opening.</param>
        /// <param name="depth">The depth of the extrusion which creates the Opening.</param>
        /// <param name="transform">The Transform of the Opening.</param>
        public Opening(Polygon perimeter, double depth, Transform transform = null)
        {
            this.Perimeter = perimeter;
            this.Transform = transform;
            this.Geometry = new[] { new Extrude(new Profile(perimeter), depth, BuiltInMaterials.Void)};
        }
    }
}