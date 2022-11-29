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
            HasVerticalChange = false;
            Flags = EdgeFlags.None;

            if (Math.Abs(v0.Point.Z - v1.Point.Z) > grid.Tolerance)
            {
                Flags &= EdgeFlags.HasVerticalChange;
                HasVerticalChange = true;
            }
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
        [Obsolete("Use HasFlag(EdgeFlags.HasVerticalChange) instead")]
        public readonly bool HasVerticalChange;

        /// <summary>
        /// Additional information about the edge.
        /// </summary>
        internal EdgeFlags Flags;

        /// <summary>
        /// Check if edge info has a certain flag or combination of flags set.
        /// </summary>
        /// <param name="flag">Flag or combination of flags to check.
        /// For example: HasAnyFlag(Hint2D) or HasAnyFlag(Hint2D | Hint3D).</param>
        /// <returns>True if edge have the flag included.</returns>
        public bool HasAnyFlag(EdgeFlags flag)
        {
            return (Flags & flag) != EdgeFlags.None;
        }

        /// <summary>
        /// Add a flag or combinations of flags. 
        /// Adding a flag more than once has no effect.
        /// </summary>
        /// <param name="flags">Flag or combination of flags to add.</param>
        /// For example: AddFlags(Hint2D) or AddFlags(Hint2D | Hint3D).</param>
        /// <returns></returns>
        public void AddFlags(EdgeFlags flags)
        {
            Flags |= flags;
        }
    }
}
