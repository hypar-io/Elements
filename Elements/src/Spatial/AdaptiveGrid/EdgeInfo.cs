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
        public EdgeFlags Flags;

        /// <summary>
        /// Check if edge info has certain flag or combination of flags set.
        /// </summary>
        /// <param name="flag">Flag or combination of flags to check.</param>
        /// <returns>True if edge have the flag included.</returns>
        public bool HasFlag(EdgeFlags flag)
        {
            return (Flags & flag) != EdgeFlags.None;
        }
    }

    /// <summary>
    /// Bit set of flags storing describing information about edge.
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
