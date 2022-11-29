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
        /// Is edge affected by user defined 2D hint line.
        /// </summary>
        UserDefinedHint2D = 1,

        /// <summary>
        /// Is edge affected by user defined 3D hint line.
        /// </summary>
        UserDefinedHint3D = 2,

        /// <summary>
        /// Is edge affected by hidden 2D hint line.
        /// </summary>
        HiddenHint2D = 4,

        /// <summary>
        /// Is edge affected by hidden 2D hint line.
        /// </summary>
        HiddenHint3D = 8,

        /// <summary>
        /// Is edge affected by hidden hint line.
        /// </summary>
        UserDefinedHint = UserDefinedHint2D | UserDefinedHint3D,

        /// <summary>
        /// Is edge affected by hidden hint line.
        /// </summary>
        HiddenHint = HiddenHint2D | HiddenHint3D,

        /// <summary>
        /// Are edge end points on different elevations.
        /// </summary>
        HasVerticalChange = 16
    }
}
