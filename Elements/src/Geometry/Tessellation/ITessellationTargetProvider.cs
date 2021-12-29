using System.Collections.Generic;

namespace Elements.Geometry.Tessellation
{
    /// <summary>
    /// An object which provides tessellation targets.
    /// </summary>
    internal interface ITessellationTargetProvider
    {
        /// <summary>
        /// Get the tessellation targets.
        /// </summary>
        IEnumerable<ITessAdapter> GetTessellationTargets();
    }
}