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
        /// <param name="transform">An optional transform to apply to each vertex.</param>
        /// <param name="color">An optional color to apply to each vertex.</param>
        void Tessellate(ref Mesh mesh, Transform transform = null, Color color = default(Color));
    }
}