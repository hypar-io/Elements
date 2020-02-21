using System;
namespace Elements.Geometry.Solids
{
    /// <summary>
    /// A Solid Operation based on a mesh. 
    /// </summary>
    public class MeshSolid : SolidOperation
    {
        /// <summary>
        /// The internal Mesh
        /// </summary>
        public Mesh Mesh { get; }

        /// <summary>
        /// Create a MeshSolid.
        /// </summary>
        /// <param name="mesh"></param>
        public MeshSolid(Mesh mesh) : base(false)
        {
            Mesh = mesh;
        }

        /// <summary>
        /// Get the updated solid representation of the mesh.
        /// </summary>
        /// <returns></returns>
        internal override Solid GetSolid()
        {
            return Kernel.Instance.CreateMeshSolid(this.Mesh);
        }
    }
}
