using Elements.Geometry;
using Elements.Geometry.Interfaces;
using Newtonsoft.Json;
using System;
using System.Linq;
using Elements.Geometry.Solids;

namespace Elements
{
    /// <summary>
    /// A zero-thickness planar Panel.
    /// </summary>
    public class Panel : Element, IGeometry3D
    {
        /// <summary>
        /// The vertices forming the perimeter of the panel.
        /// </summary>
        [JsonProperty("perimeter")]
        public Vector3[] Perimeter { get; }

        /// <summary>
        /// The Panel's geometry.
        /// </summary>
        [JsonProperty("geometry")]
        public Solid[] Geometry { get; }

        /// <summary>
        /// Construct a Panel.
        /// </summary>
        /// <param name="perimeter">The perimeter of the Panel.</param>
        /// <param name="material">The Panel's material</param>
        /// <exception cref="System.ArgumentException">Thrown when the provided perimeter points are not coplanar.</exception>
        [JsonConstructor]
        public Panel(Vector3[] perimeter, Material material = null)
        {
            var vCount = perimeter.Count();
            if (!perimeter.AreCoplanar())
            {
                throw new ArgumentException("The Panel could not be constructed. Points defining the perimeter must be coplanar.", "perimeter");
            }
            
            this.Perimeter = perimeter;
            this.Geometry = new[] { Solid.CreateLamina(this.Perimeter, material == null ? BuiltInMaterials.Default : material) };
        }

        /// <summary>
        /// The normal of the Panel, defined using the first 3 vertices in the location.
        /// </summary>
        public Vector3 Normal()
        {
            return new Plane(this.Perimeter[0], this.Perimeter).Normal;
        }
    }
}