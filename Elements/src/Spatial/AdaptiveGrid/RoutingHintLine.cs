using Elements.Geometry;
using System;
using System.Collections.Generic;
using System.Text;

namespace Elements.Spatial.AdaptiveGrid
{
    /// <summary>
    /// Structure that holds information about polylines that are used to guide routing.
    /// </summary>
    public struct RoutingHintLine
    {
        /// <summary>
        /// Construct new RoutingHintLine structure.
        /// </summary>
        /// <param name="polyline">Geometry of HintLine.</param>
        /// <param name="factor">Cost multiplier.</param>
        /// <param name="influence">How far it affects.</param>
        /// <param name="userDefined">Is user defined.</param>
        /// <param name="is2D">Should polyline be virtually extended by Z coordinate.</param>
        public RoutingHintLine(
            Polyline polyline, double factor, double influence, bool userDefined, bool is2D)
        {
            Polyline = polyline;
            Factor = factor;
            InfluenceDistance = influence;
            UserDefined = userDefined;
            Is2D = is2D;
        }

        /// <summary>
        /// 2D Polyline geometry representation with an influence that is extended on both sides in Z direction.
        /// </summary>
        public readonly Polyline Polyline;

        /// <summary>
        /// Cost multiplier for edges that lie within the Influence distance to the line.
        /// </summary>
        public readonly double Factor;

        /// <summary>
        /// How far away from the line, edge travel cost is affected.
        /// Both sides of an edge and its middle point should be within influence range.
        /// </summary>
        public readonly double InfluenceDistance;

        /// <summary>
        /// Is line created by the user or from internal parameters?
        /// User defined lines are preferred for input Vertex connection.
        /// </summary>
        public readonly bool UserDefined;

        /// <summary>
        /// Should polyline be virtually extended by Z coordinate.
        /// </summary>
        public readonly bool Is2D;
    }
}
