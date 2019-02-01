using Elements.Geometry;
using Elements.Geometry.Interfaces;
using Newtonsoft.Json;
using System;
using Elements.Geometry.Solids;

namespace Elements
{
    /// <summary>
    /// A zero-thickness planar element defined by a perimeter.
    /// </summary>
    public class Panel : Element, IGeometry3D
    {
        /// <summary>
        /// The vertices forming the perimeter of the panel.
        /// </summary>
        [JsonProperty("perimeter")]
        public Vector3[] Perimeter { get; }

        /// <summary>
        /// The panel's geometry.
        /// </summary>
        [JsonProperty("geometry")]
        public Solid[] Geometry { get; }

        /// <summary>
        /// Create a panel.
        /// </summary>
        /// <param name="perimeter">The perimeter of the panel.</param>
        /// <param name="material">The panel's material</param>
        /// <param name="transform">The panel's transform.</param>
        /// <exception cref="System.ArgumentException">Thrown when the provided perimeter points are not coplanar.</exception>
        [JsonConstructor]
        public Panel(Vector3[] perimeter, Material material = null, Transform transform = null)
        {
            if (!perimeter.AreCoplanar())
            {
                throw new ArgumentException("The Panel could not be created. Points defining the perimeter must be coplanar.", "perimeter");
            }
            
            this.Transform = transform;
            this.Perimeter = perimeter;
            this.Geometry = new[] { Solid.CreateLamina(this.Perimeter, material == null ? BuiltInMaterials.Default : material) };
        }

        /// <summary>
        /// Create a panel.
        /// </summary>
        /// <param name="perimeter">A planar polygon defining the perimeter.</param>
        /// <param name="material">The panel's material.</param>
        /// <param name="transform">The panel's transform.</param>
        public Panel(Polygon perimeter, Material material = null, Transform transform = null)
        {
            this.Transform = transform;
            this.Perimeter = perimeter.Vertices;
            if (!this.Perimeter.AreCoplanar())
            {
                throw new ArgumentException("The Panel could not be created. Points defining the perimeter must be coplanar.", "perimeter");
            }
            this.Geometry = new[] { Solid.CreateLamina(this.Perimeter, material == null ? BuiltInMaterials.Default : material) };
        }

        /// <summary>
        /// The normal of the panel, defined using the first 3 vertices in the location.
        /// </summary>
        /// <returns>The normal vector of the panel.</returns>
        public Vector3 Normal()
        {
            return new Plane(this.Perimeter[0], this.Perimeter).Normal;
        }
    }
}