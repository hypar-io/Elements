using System;
using System.Collections.Generic;
using System.Text;

namespace Elements.Spatial.AdaptiveGrid
{
    /// <summary>
    /// Enumeration that indicates one of two possible paths in routing.
    /// There are cases when we need to collect more than one path and
    /// only after some time we can decide which one is better.
    /// </summary>
    public enum BranchSide
    {
        /// <summary>
        /// Indicator that first, "left", path is preferred.
        /// </summary>
        Left,

        /// <summary>
        /// Indicator that second, "right" path is preferred.
       /// </summary>
        Right
    }
}
