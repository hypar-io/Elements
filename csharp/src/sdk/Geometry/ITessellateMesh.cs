using Hypar.Geometry;

namespace Hypar.Geometry
{
    /// <summary>
    /// ITessellateMesh is implemented by all types which provide a Mesh for visualization.
    /// </summary>
    public interface ITessellateMesh
    {
        /// <summary>
        /// Tessellate.
        /// </summary>
        /// <returns>An object of type T containing the tessellation results.</returns>
        Mesh Mesh();
    }
}