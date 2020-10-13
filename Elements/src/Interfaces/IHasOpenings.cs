#pragma warning disable CS1591

using System.Collections.Generic;
using Elements.Geometry;

namespace Elements.Interfaces
{
    public interface IHasOpenings
    {
        /// <summary>
        /// A collection of openings.
        /// </summary>
        List<Opening> Openings { get; }
    }
}