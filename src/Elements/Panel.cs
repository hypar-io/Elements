using Elements.Geometry;
using Elements.Geometry.Interfaces;
using Newtonsoft.Json;
using Elements.Interfaces;
using Elements.Geometry.Solids;

namespace Elements
{
    /// <summary>
    /// A zero-thickness planar element defined by a perimeter.
    /// </summary>
    /// <example>
    /// [!code-csharp[Main](../../test/Examples/PanelExample.cs?name=example)]
    /// </example>
    public class Panel : Element, IMaterial, ILamina
    {
        /// <summary>
        /// The perimeter of the panel.
        /// </summary>
        public Polygon Perimeter { get; }

        /// <summary>
        /// The panel's material.
        /// </summary>
        public Material Material {get;}

        /// <summary>
        /// Create a panel.
        /// </summary>
        /// <param name="perimeter">The perimeter of the panel.</param>
        /// <param name="material">The panel's material</param>
        /// <param name="transform">The panel's transform.</param>
        /// <exception cref="System.ArgumentException">Thrown when the provided perimeter points are not coplanar.</exception>
        [JsonConstructor]
        public Panel(Polygon perimeter, Material material = null, Transform transform = null)
        {
            if(transform != null)
            {
                this.Transform = transform;
            }
            this.Perimeter = perimeter;
            this.Material = material == null ? BuiltInMaterials.Default : material;
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
        /// Get the updated solid representation of the panel.
        /// </summary>
        public Solid GetUpdatedSolid()
        {
            return Kernel.Instance.CreateLamina(this);
        }
    }
}