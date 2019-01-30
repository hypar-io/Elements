using System;
using Elements.Geometry;
using Elements.Geometry.Interfaces;
using Elements.Geometry.Solids;
using Newtonsoft.Json;

namespace Elements
{
    /// <summary>
    /// A rectangular opening in a wall or floor.
    /// </summary>
    public class Opening
    {
        private Polygon _perimeter;

        /// <summary>
        /// The name of the opening.
        /// </summary>
        [JsonProperty("name")]
        public string Name{get; internal set;}

        /// <summary>
        /// The perimeter of the opening.
        /// </summary>
        /// <value>A polygon of Width and Height translated by X and Y.</value>
        [JsonProperty("perimeter")]
        public Polygon Perimeter 
        {
            get => _perimeter;
            internal set => _perimeter = value;
        }

        /// <summary>
        /// The distance along the X axis of the transform of the host element to the center of the opening.
        /// </summary>
        [JsonProperty("x")]
        public double X { get; }

        /// <summary>
        /// The distance along the Y axis of the transform of the host element to the center of the opening.
        /// </summary>
        [JsonProperty("y")]
        public double Y { get; }

        /// <summary>
        /// The width of the opening.
        /// </summary>
        [JsonProperty("width")]
        public double Width { get; }

        /// <summary>
        /// The height of the opening.
        /// </summary>
        [JsonProperty("height")]
        public double Height { get; }

        /// <summary>
        /// Create an opening.
        /// </summary>
        /// <param name="x">The distance along the X axis of the transform of the host element to the center of the opening.</param>
        /// <param name="y">The distance along the Y axis of the transform of the host element to the center of the opening.</param>
        /// <param name="width">The width of the opening.</param>
        /// <param name="height">The height of the opening.</param>
        [JsonConstructor]
        public Opening(double x, double y, double width, double height)
        {
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
            this._perimeter = Polygon.Rectangle(width, height, new Vector3(x, y));
        }

        /// <summary>
        /// Create an opening.
        /// </summary>
        /// <param name="perimeter">A polygon representing the perimeter of the opening.</param>
        public Opening(Polygon perimeter)
        {
            this._perimeter = perimeter;
        }
    }
}