using Elements.Geometry;
using Elements.Geometry.Interfaces;
using Newtonsoft.Json;
using Elements.Interfaces;
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
    public class Panel : Element, IMaterial, ILamina
    {
        private Guid _materialId;

        /// <summary>
        /// The perimeter of the panel.
        /// </summary>
        public Polygon Perimeter { get; }

        /// <summary>
        /// The panel's material.
        /// </summary>
        [JsonIgnore]
        public Material Material { get; private set;}

        /// <summary>
        /// The panel's material id.
        /// </summary>
        public Guid MaterialId
        {
            get
            {
                return this.Material != null ? this.Material.Id : this._materialId;
            }
        }

        /// <summary>
        /// Create a panel.
        /// </summary>
        /// <param name="perimeter">The perimeter of the panel.</param>
        /// <param name="material">The panel's material</param>
        /// <param name="transform">The panel's transform.</param>
        /// <exception cref="System.ArgumentException">Thrown when the provided perimeter points are not coplanar.</exception>
        public Panel(Polygon perimeter, Material material = null, Transform transform = null)
        {
            if(transform != null)
            {
                this.Transform = transform;
            }
            this.Perimeter = perimeter;
            this.Material = material == null ? BuiltInMaterials.Default : material;
        }

        [JsonConstructor]
        internal Panel(Polygon perimeter, Guid materialId, Transform transform = null)
        {
            if(transform != null)
            {
                this.Transform = transform;
            }
            this.Perimeter = perimeter;
            this._materialId = materialId;
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
        /// Get the updated solid representation of the panel.
        /// </summary>
        public Solid GetUpdatedSolid()
        {
            return Kernel.Instance.CreateLamina(this);
        }

        /// <summary>
        /// Set the material.
        /// </summary>
        public void SetReference(Material material)
        {
            this.Material = material;
            this._materialId = material.Id;
        }
    }
}