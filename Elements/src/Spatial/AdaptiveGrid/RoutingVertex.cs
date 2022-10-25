using System;
using System.Collections.Generic;
using System.Text;

namespace Elements.Spatial.AdaptiveGrid
{
    /// <summary>
    /// Structure that holds additional information about inlet vertex
    /// </summary>
    public struct RoutingVertex
    {
        /// <summary>
        /// Construct new RoutingVertex structure.
        /// </summary>
        /// <param name="id">Id of the vertex in the grid.</param>
        /// <param name="isolationRadius"> Distance, other sections of the route can't travel near this vertex.</param>
        public RoutingVertex(
            ulong id, double isolationRadius)
        {
            Id = id;
            IsolationRadius = isolationRadius;
        }

        /// <summary>
        /// Id of the vertex in the grid.
        /// </summary>
        public ulong Id;

        /// <summary>
        /// Distance closer than which, other sections of the route can't travel near this vertex. 
        /// Distance is in base plane of the gird, without elevation.
        /// </summary>
        public double IsolationRadius;
    }
}
