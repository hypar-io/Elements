using LibTessDotNet.Double;

namespace Elements.Geometry.Tessellation
{
    /// <summary>
    /// An object which creates a tessellation for a tessellation target.
    /// </summary>
    internal interface ITessAdapter
    {
        /// <summary>
        /// Get the tessellation.
        /// </summary>
        Tess GetTess();
    }
}