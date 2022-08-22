using System;
using System.IO;
using Elements.Geometry;
using static Elements.Units;

namespace Elements
{
    /// <summary>
    /// An element definition whose representation is provided by an imported mesh like an STL.
    /// </summary>
    /// <example>
    /// [!code-csharp[Main](../../Elements/test/ImportMeshElementTests.cs?name=example)]
    /// </example>
    public sealed class ImportMeshElement : MeshElement
    {
        /// <summary>
        /// The path to the element's mesh on disk.
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// The length unit used in the provided mesh.
        /// </summary>
        public LengthUnit LengthUnit { get; set; }

        /// <summary>
        /// Construct an import mesh element.
        /// </summary>
        /// <param name="path">The path to the element's mesh on disk.</param>
        /// <param name="lengthUnit">The length unit used in the provided mesh.</param>
        /// <param name="material">The element's material.</param>
        /// <param name="id">The element's id.</param>
        /// <param name="name">The element's name.</param>
        public ImportMeshElement(string path,
                         LengthUnit lengthUnit,
                         Material material = null,
                         Guid id = default(Guid),
                         string name = null) : base(null,
                                                    new Transform(),
                                                    material == null ? BuiltInMaterials.Default : material,
                                                    null,
                                                    true,
                                                    id == default(Guid) ? Guid.NewGuid() : id,
                                                    name)
        {
            this.Path = path;
            this.LengthUnit = lengthUnit;

            if (!File.Exists(path))
            {
                throw new Exception("The provided path does not exist.");
            }

            // TODO: Currently we only support STL. In the future we should
            // support glTF as well, and possibly others.
            this._mesh = Mesh.FromSTL(path, lengthUnit);
        }
    }
}