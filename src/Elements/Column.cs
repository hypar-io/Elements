using System;
using Elements.Geometry;

namespace Elements
{
    /// <summary>
    /// A vertical structural framing element.
    /// </summary>
    /// <example>
    /// [!code-csharp[Main](../../test/Elements.Tests/Examples/ColumnExample.cs?name=example)]
    /// </example>
    [UserElement]
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
        /// <param name="profile">The column's profile.</param>
        /// <param name="material">The column's material.</param>
        /// <param name="transform">The column's transform.</param>
        /// <param name="startSetback">The setback of the column's extrusion from the base of the column.</param>
        /// <param name="endSetback">The setback of the column's extrusion from the top of the column.</param>
        /// <param name="rotation">An optional rotation of the column's profile around its axis.</param>
        /// <param name="id">The column's id.</param>
        /// <param name="name">The column's name.</param>
        public Column(Vector3 location,
                      double height,
                      Profile profile,
                      Material material = null,
                      Transform transform = null,
                      double startSetback = 0.0,
                      double endSetback = 0.0,
                      double rotation = 0.0,
                      Guid id = default(Guid),
                      string name = null) 
            : base(new Line(new Vector3(location.X, location.Y, location.Z + height), location), profile, material, 
                startSetback, endSetback, rotation, transform, null, id, name)
        {
            this.Location = location;
            this.Height = height;
        }
    }
}