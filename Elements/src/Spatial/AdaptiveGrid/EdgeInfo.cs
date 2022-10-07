using System;
using System.Collections.Generic;
using System.Text;

namespace Elements.Spatial.AdaptiveGrid
{
    /// <summary>
    /// Precalculated information about the edge.
    /// </summary>
    public struct EdgeInfo
    {
        /// <summary>
        /// Construct new EdgeInfo structure.
        /// </summary>
        /// <param name="grid">Grid, edge belongs to.</param>
        /// <param name="edge">The edge.</param>
        /// <param name="factor">Edge traveling factor.</param>
        public EdgeInfo(AdaptiveGrid grid, Edge edge, double factor = 1)
        {
            Edge = edge;
            var v0 = grid.GetVertex(edge.StartId);
            var v1 = grid.GetVertex(edge.EndId);
            var vector = (v1.Point - v0.Point);
            Length = vector.Length();
            Factor = factor;
            HasVerticalChange = Math.Abs(v0.Point.Z - v1.Point.Z) > grid.Tolerance;
        }

        /// <summary>
        /// The Edge.
        /// </summary>
        public readonly Edge Edge;

        /// <summary>
        /// Length of the edge.
        /// </summary>
        public readonly double Length;

        /// <summary>
        /// Edge traveling factor.
        /// </summary>
        public readonly double Factor;

        /// <summary>
        /// Are edge end points on different elevations.
        /// </summary>
        public readonly bool HasVerticalChange;
    }
}
