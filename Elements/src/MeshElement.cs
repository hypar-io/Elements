using System;
using Elements.Geometry;
using Elements.Interfaces;
using Newtonsoft.Json;

namespace Elements
{
    /// <summary>
    /// An element whose representation is provided by a mesh.
    /// </summary>
    /// <example>
    /// [!code-csharp[Main](../../Elements/test/MeshElementTests.cs?name=example)]
    /// </example>
    public class MeshElement : GeometricElement, IVisualize3d
    {
        /// <summary>
        /// The mesh.
        /// </summary>
        protected Mesh _mesh;

        /// <summary>
        /// The element's mesh.
        /// </summary>
        public Mesh Mesh
        {
            get { return this._mesh; }
            set { this._mesh = value; }
        }

        /// <summary>
        /// Construct an import mesh element.
        /// </summary>
        /// <param name="mesh">The element's mesh.</param>
        /// <param name="material">The element's material.</param>
        /// <param name="transform">The element's transform.</param>
        /// <param name="isElementDefinition">Is this element a definition?</param>
        /// <param name="id">The element's id.</param>
        /// <param name="name">The element's name.</param>
        [JsonConstructor]
        public MeshElement(Mesh mesh,
                            Material material = null,
                            Transform transform = null,
                            bool isElementDefinition = false,
                            Guid id = default(Guid),
                            string name = null) : base(transform == null ? new Transform() : transform,
                                                       material == null ? BuiltInMaterials.Default : material,
                                                       null,
                                                       isElementDefinition,
                                                       id == default(Guid) ? Guid.NewGuid() : id,
                                                       name)
        {
            this._mesh = mesh;
        }

        internal MeshElement(Material material = null,
                            Transform transform = null,
                            bool isElementDefinition = false,
                            Guid id = default(Guid),
                            string name = null) : base(transform == null ? new Transform() : transform,
                                                       material == null ? BuiltInMaterials.Default : material,
                                                       null,
                                                       isElementDefinition,
                                                       id == default(Guid) ? Guid.NewGuid() : id,
                                                       name)
        {
            this._mesh = new Mesh();
        }

        /// <summary>
        /// Visualize this mesh in 3d.
        /// </summary>
        public override GraphicsBuffers Visualize3d()
        {
            return this._mesh.GetBuffers();
        }
    }
}