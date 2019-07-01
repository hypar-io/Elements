using System.Collections.Generic;

namespace Elements.Interfaces
{
    /// <summary>
    /// Has a collection of opening.
    /// </summary>
    public interface IHasOpenings
    {
        /// <summary>
        /// A collection of openings which are transformed in the coordinate system of their host element.
        /// </summary>
        List<Opening> Openings{get;}
    }
}