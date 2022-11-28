using System;
using System.Collections.Generic;
using System.Text;

namespace Elements.Spatial.AdaptiveGrid
{
    /// <summary>
    /// Bit set of flags storing describing information about edge.
    /// Each flag is set to it's own number - power of 2, so they can be safely combined.
    /// Use | or |= to combine flags: flag = Hint2D | HasVerticalChange = 1 + 4 = 001 + 100 = 101 = 5.
    /// Use & or &= to check of one or more flags: flags & Hint3D == 101 & 010 == 0 == None, but 
    /// but flags & Hint2D == 101 & 001 == 001 == Hint2D.
    /// </summary>
    [Flags]
    public enum EdgeFlags
    {
        /// <summary>
        /// No flags set.
        /// </summary>
        None = 0,

        /// <summary>
        /// Is edge affected by any 2D hint line.
        /// </summary>
        Hint2D = 1,

        /// <summary>
        /// Is edge affected by any 3D hint line.
        /// </summary>
        Hint3D = 2,

        /// <summary>
        /// Are edge end points on different elevations.
        /// </summary>
        HasVerticalChange = 4
    }
}
