#pragma warning disable CS1591

namespace Elements.Geometry.Interfaces
{
    public interface ITessellate
    {
        /// <summary>
        /// Add the tessellated representation of this object
        /// to the provided Mesh.
        /// </summary>
        /// <param name="mesh">The mesh to which this object's representation will be added.</param>
        void Tessellate(ref Mesh mesh);
    }
}