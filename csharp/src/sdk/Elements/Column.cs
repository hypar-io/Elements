using Hypar.Geometry;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Hypar.Elements
{
    /// <summary>
    /// A Column is a structural framing element which is often vertical.
    /// </summary>
    public class Column : StructuralFraming
    {
        /// <summary>
        /// The type of the element.
        /// </summary>
        public override string Type
        {
            get{return "column";}
        }

        /// <summary>
        /// The location of the base of the Column.
        /// </summary>
        [JsonProperty("location")]
        public Vector3 Location{get;}

        /// <summary>
        /// The height of the Column.
        /// </summary>
        /// <value></value>
        [JsonProperty("height")]
        public double Height{get;}

        /// <summary>
        /// Construct a Column.
        /// </summary>
        /// <param name="location">The location of the base of the Column.</param>
        /// <param name="height">The Column's height.</param>
        /// <param name="profile">The Column's profile.</param>
        /// <param name="material">The Column's material.</param>
        [JsonConstructor]
        public Column(Vector3 location, double height, IList<Polygon> profile, Material material = null) : base(new Line(location, new Vector3(location.X, location.Y, location.Z + height)), profile, material)
        {
            this.Location = location;
            this.Height = height;
        }
    }
}