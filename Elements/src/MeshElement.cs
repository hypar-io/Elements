using System;
using Elements.Geometry;
using Elements.Geometry.Interfaces;
using Newtonsoft.Json;

namespace Elements
{
    /// <summary>
    /// An element whose representation is provided by a mesh.
    /// </summary>
    /// <example>
    /// [!code-csharp[Main](../../Elements/test/MeshElementTests.cs?name=example)]
    /// </example>
    public class MeshElement : GeometricElement, ITessellate
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
        /// <param name="transform">The element's transform.</param>
        /// <param name="material">The element's material.</param>
        /// <param name="representation">The element's representation.</param>
        /// <param name="isElementDefinition">Is this element a definition?</param>
        /// <param name="id">The element's id.</param>
        /// <param name="name">The element's name.</param>
        [JsonConstructor]
        public MeshElement(Mesh mesh,
                            Transform transform = null,
                            Material material = null,
                            Representation representation = null,
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

        /// <summary>
        /// Empty constructor for compatibility purposes. It is best to use the
        /// structured constructor with arguments, to ensure the mesh is correctly created.
        /// </summary>
        public MeshElement() : base()
        {
            _mesh = new Mesh();
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
        /// Tessellate the element.
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="transform"></param>
        /// <param name="color"></param>
        public void Tessellate(ref Mesh mesh, Transform transform = null, Color color = default(Color))
        {
            mesh = this._mesh;
        }
    }
}