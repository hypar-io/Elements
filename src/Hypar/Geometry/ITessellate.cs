using Hypar.Geometry;

namespace Hypar.Geometry
{
    /// <summary>
    /// ITessellate is implemented by all types for which visualization is required.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ITessellate<T>
    {
        /// <summary>
        /// Tessellate.
        /// </summary>
        /// <returns>An object of type T containing the tessellation results.</returns>
        T Tessellate();
    }
}