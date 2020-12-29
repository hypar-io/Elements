using System;
using System.IO;
using static Elements.Units;

namespace Elements.Geometry
{
    public partial class MeshRepresentation
    {
        /// <summary>
        /// Create a mesh representation from an import.
        /// </summary>
        /// <param name="stlPath">The path to an STL file.</param>
        /// <param name="lengthUnit">The length unit used in the file.</param>
        /// <param name="material">The mesh's material.</param>
        public MeshRepresentation(string stlPath, LengthUnit lengthUnit, Material material) : base(material, Guid.NewGuid(), null)
        {
            if (!File.Exists(stlPath))
            {
                throw new FileNotFoundException("The specified STL fil path could not be found.");
            }
            this.Mesh = Mesh.FromSTL(stlPath, lengthUnit);
        }

        /// <summary>
        /// Create a mesh representation.
        /// </summary>
        /// <param name="material">The material to apply to the mesh.</param>
        public MeshRepresentation(Material material) : base(material, Guid.NewGuid(), null) { }

        /// <summary>
        /// Create a mesh representation.
        /// </summary>
        /// <param name="mesh">The mesh.</param>
        /// <param name="material">The material to apply to the mesh.</param>
        public MeshRepresentation(Mesh mesh, Material material) : base(material, Guid.NewGuid(), null)
        {
            this.Mesh = mesh;
        }

        /// <summary>
        /// Create a mesh representation.
        /// </summary>
        /// <param name="mesh">The mesh.</param>
        public MeshRepresentation(Mesh mesh) : base(BuiltInMaterials.Default, Guid.NewGuid(), null)
        {
            this.Mesh = mesh;
        }
    }
}