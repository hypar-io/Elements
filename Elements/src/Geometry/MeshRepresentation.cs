using System;

namespace Elements.Geometry
{
    public partial class MeshRepresentation
    {
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
    }
}