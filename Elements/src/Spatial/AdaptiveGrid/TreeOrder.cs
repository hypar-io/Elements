using System;
using System.Collections.Generic;
using System.Text;

namespace Elements.Spatial.AdaptiveGrid
{
    /// <summary>
    /// Order at which leaf terminal are connected into the tree.
    /// </summary>
    public enum TreeOrder
    {
        /// <summary>
        /// Closest from remaining terminals is routed first.
        /// </summary>
        ClosestToFurthest,

        /// <summary>
        /// Furthest from remaining terminals is routed first.
        /// </summary>
        FurthestToClosest
    }
}
