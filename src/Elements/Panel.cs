using Elements.Geometry;
using Elements.Geometry.Solids;
using System;

namespace Elements
{
    /// <summary>
    /// A zero-thickness planar element defined by a perimeter.
    /// </summary>
    /// <example>
    /// [!code-csharp[Main](../../test/Examples/PanelExample.cs?name=example)]
    /// </example>
    [UserElement]
    public class Panel : GeometricElement
    {
        /// <summary>
        /// The perimeter of the panel.
        /// </summary>
        public Polygon Perimeter { get; }

        /// <summary>
        /// Create a panel.
        /// </summary>
        /// <param name="perimeter">The perimeter of the panel.</param>
        /// <param name="material">The panel's material</param>
        /// <param name="transform">The panel's transform.</param>
        /// <param name="id">The id of the panel.</param>
        /// <param name="name">The name of the panel.</param>
        /// <exception cref="System.ArgumentException">Thrown when the provided perimeter points are not coplanar.</exception>
        public Panel(Polygon perimeter,
                     Material material = null,
                     Transform transform = null,
                     Guid id = default(Guid),
                     string name = null) : base(material, transform, id, name)
        {
            this.Perimeter = perimeter;
            this.Representation.SolidOperations.Add(new Lamina(this.Perimeter));
        }

        /// <summary>
        /// The panel's area.
        /// </summary>
        public double Area()
        {
            return Math.Abs(Perimeter.Area());
        }

        /// <summary>
        /// The normal of the panel, defined using the first 3 vertices in the location.
        /// </summary>
        /// <returns>The normal vector of the panel.</returns>
        public Vector3 Normal()
        {
            return this.Perimeter.Plane().Normal;
        }

        /// <summary>
        /// Update the representations.
        /// </summary>
        public override void UpdateRepresentations()
        {
            return;
        }
    }
}