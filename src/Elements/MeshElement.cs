using System;
using Elements.Geometry;
using Elements.Geometry.Interfaces;

namespace Elements
{
    /// <summary>
    /// An element whose representation is provided by a mesh.
    /// </summary>
    /// <example>
    /// [!code-csharp[Main](../../test/Elements.Tests/MeshElementTests.cs?name=example)]
    /// </example>
    [UserElement]
    public class MeshElement : GeometricElement, ITessellate
    {
        /// <summary>
        /// The mesh.
        /// </summary>
        protected Mesh _mesh;

        /// <summary>
        /// Construct an import mesh element.
        /// </summary>
        /// <param name="mesh">The element's mesh.</param>
        /// <param name="material">The element's material.</param>
        /// <param name="transform">The element's transform.</param>
        /// <param name="isElementDefinition">Is this element a definition?</param>
        /// <param name="id">The element's id.</param>
        /// <param name="name">The element's name.</param>
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

        /// <summary>
        /// Tessellate the element.
        /// </summary>
        /// <param name="mesh"></param>
        public void Tessellate(ref Mesh mesh)
        {
            mesh = this._mesh;
        }
    }
}