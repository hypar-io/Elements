using Elements.Geometry;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Elements
{
    /// <summary>
    /// A vertical structural framing element.
    /// </summary>
    public class Column : StructuralFraming
    {
        /// <summary>
        /// The location of the base of the column.
        /// </summary>
        [JsonProperty("location")]
        public Vector3 Location{get;}

        /// <summary>
        /// The height of the column.
        /// </summary>
        [JsonProperty("height")]
        public double Height{get;}

        /// <summary>
        /// Construct a Column.
        /// </summary>
        /// <param name="location">The location of the base of the column.</param>
        /// <param name="height">The column's height.</param>
        /// <param name="profile">The column's profile.</param>
        /// <param name="material">The column's material.</param>
        /// <param name="transform">The column's transform.</param>
        /// <param name="startSetback">The setback of the column's extrusion from the base of the column.</param>
        /// <param name="endSetback">The setback of the column's extrusion from the top of the column.</param>
        [JsonConstructor]
        public Column(Vector3 location, double height, Profile profile, Material material = null, Transform transform = null, double startSetback = 0.0, double endSetback = 0.0) : base(new Line(location, new Vector3(location.X, location.Y, location.Z + height)), profile, material, startSetback, endSetback, transform)
        {
            this.Location = location;
            this.Height = height;
        }
    }
}