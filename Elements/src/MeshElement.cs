using System;
using Elements.Geometry;
using Newtonsoft.Json;

namespace Elements
{
    /// <summary>
    /// An element whose representation is provided by a mesh.
    /// </summary>
    /// <example>
    /// [!code-csharp[Main](../../Elements/test/MeshElementTests.cs?name=example)]
    /// </example>
    [UserElement]
    public class MeshElement : GeometricElement
    {
        /// <summary>
        /// The mesh.
        /// </summary>
        protected Mesh _mesh;

        /// <summary>
        /// The element's mesh.
        /// </summary>
        [JsonIgnore]
        public Mesh Mesh => this._mesh;

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
                                                       new[] { new MeshRepresentation(mesh, material == null ? BuiltInMaterials.Default : material) },
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
                                                       new[] { new MeshRepresentation(material == null ? BuiltInMaterials.Default : material) },
                                                       isElementDefinition,
                                                       id == default(Guid) ? Guid.NewGuid() : id,
                                                       name)
        {
            this._mesh = new Mesh();
        }

        /// <summary>
        /// Update the mesh element's representations.
        /// </summary>
        public override void UpdateRepresentations()
        {
            var rep = this.FirstRepresentationOfType<MeshRepresentation>();
            rep.Mesh = this.Mesh;
        }
    }
}