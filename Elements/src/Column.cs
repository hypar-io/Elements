using System;
using Elements.Geometry;

namespace Elements
{
    /// <summary>
    /// A vertical structural framing element.
    /// </summary>
    /// <example>
    /// [!code-csharp[Main](../../Elements/test/ColumnTest.cs?name=example)]
    /// </example>
    public class Column : StructuralFraming
    {
        /// <summary>
        /// The location of the base of the column.
        /// </summary>
        public Vector3 Location { get; set; }

        /// <summary>
        /// The height of the column.
        /// </summary>
        public double Height { get; set; }

        /// <summary>
        /// Construct a Column.
        /// </summary>
        /// <param name="location">The location of the base of the column.</param>
        /// <param name="height">The column's height.</param>
        /// <param name="curve">The center line of the column. Will be ignored, so you can use 'null'. This parameter is required to support schema</param>
        /// <param name="profile">The column's profile.</param>
        /// <param name="transform">The column's transform.</param>
        /// <param name="material">The column's material.</param>
        /// <param name="representation">The column's representation.</param>
        /// <param name="isElementDefinition">Is this an element definition?</param>
        /// <param name="id">The column's id.</param>
        /// <param name="name">The column's name.</param>
        public Column(Vector3 location,
              double height,
              Curve curve,
              Profile profile,
              Transform transform = null,
              Material material = null,
              Representation representation = null,
              bool isElementDefinition = false,
              Guid id = default,
              string name = null) : base(new Line(location, new Vector3(location.X, location.Y, location.Z + height)), profile, material, 0, 0, 0, transform, representation, isElementDefinition, id, name)
        {
            this.Location = location;
            this.Height = height;
        }

        /// <summary>
        /// Construct a Column.
        /// </summary>
        /// <param name="location">The location of the base of the column.</param>
        /// <param name="height">The column's height.</param>
        /// <param name="curve">The center line of the column. Will be ignored, so you can use 'null'. This parameter is required to support schema</param>
        /// <param name="profile">The column's profile.</param>
        /// <param name="startSetback">The setback of the column's extrusion from the base of the column.</param>
        /// <param name="endSetback">The setback of the column's extrusion from the top of the column.</param>
        /// <param name="rotation">An optional rotation of the column's profile around its axis.</param>
        /// <param name="transform">The column's transform.</param>
        /// <param name="material">The column's material.</param>
        /// <param name="representation">The column's representation.</param>
        /// <param name="isElementDefinition">Is this an element definition?</param>
        /// <param name="id">The column's id.</param>
        /// <param name="name">The column's name.</param>
        public Column(Vector3 location,
              double height,
              Curve curve,
              Profile profile,
              double startSetback,
              double endSetback,
              double rotation,
              Transform transform = null,
              Material material = null,
              Representation representation = null,
              bool isElementDefinition = false,
              Guid id = default,
              string name = null) : base(new Line(location, new Vector3(location.X, location.Y, location.Z + height)), profile, material, startSetback, endSetback, rotation, transform, representation, isElementDefinition, id, name)
        {
            this.Location = location;
            this.Height = height;
        }

        /// <summary>
        /// Construct a column.
        /// </summary>
        public Column()
        {
        }
    }
}