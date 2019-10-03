using Elements.ElementTypes;
using Elements.Geometry;
using Newtonsoft.Json;

namespace Elements
{
    /// <summary>
    /// A vertical structural framing element.
    /// </summary>
    /// <example>
    /// [!code-csharp[Main](../../test/Examples/ColumnExample.cs?name=example)]
    /// </example>
    public class Column : StructuralFraming
    {
        /// <summary>
        /// The location of the base of the column.
        /// </summary>
        public Vector3 Location{get;}

        /// <summary>
        /// The height of the column.
        /// </summary>
        public double Height{get;}

        /// <summary>
        /// Construct a Column.
        /// </summary>
        /// <param name="location">The location of the base of the column.</param>
        /// <param name="height">The column's height.</param>
        /// <param name="elementType">The column's structural framing type.</param>
        /// <param name="transform">The column's transform.</param>
        /// <param name="startSetback">The setback of the column's extrusion from the base of the column.</param>
        /// <param name="endSetback">The setback of the column's extrusion from the top of the column.</param>
        [JsonConstructor]
        public Column(Vector3 location, double height, StructuralFramingType elementType, 
            Transform transform = null, double startSetback = 0.0, double endSetback = 0.0) 
            : base(new Line(location, new Vector3(location.X, location.Y, location.Z + height)), elementType, startSetback, endSetback, transform)
        {
            this.Location = location;
            this.Height = height;
        }
    }
}