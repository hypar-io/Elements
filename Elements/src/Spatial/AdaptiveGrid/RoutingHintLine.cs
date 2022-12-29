using Elements.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Elements.Spatial.AdaptiveGrid
{
    /// <summary>
    /// Structure that holds information about polylines that are used to guide routing.
    /// </summary>
    public class RoutingHintLine
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
            Factor = factor;
            InfluenceDistance = influence;
            UserDefined = userDefined;
            Is2D = is2D;
            if (Is2D)
            {
                Polyline = new Polyline(polyline.Vertices.Select(v => new Vector3(v.X, v.Y)).ToList());
            }
            else
            {
                Polyline = polyline;
            }
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

        /// <summary>
        /// Check if point is within influence of the hint line.
        /// If hint line is 2D than only 2D distance is calculated, ignoring Z coordinate.
        /// </summary>
        /// <param name="point">Point to check.</param>
        /// <param name="tolerance">Minimum allowed distance to polyline, even if influence is 0.</param>
        /// <returns>True if point is close enough to the polyline.</returns>
        public bool IsNearby(Vector3 point, double tolerance = Vector3.EPSILON)
        {
            var influenceDistance = Math.Max(InfluenceDistance, tolerance);
            var target = Is2D ? new Vector3(point.X, point.Y) : point;
            return target.DistanceTo(Polyline) <= influenceDistance;
        }

        /// <summary>
        /// Check if hint line affects the line represented by two points.
        /// Both points must be withing influence radius of the polyline.
        /// Only edges parallel to polyline segments are affected.
        /// </summary>
        /// <param name="start">Start of the line point.</param>
        /// <param name="end">End of the line point.</param>
        /// <param name="tolerance">Minimum allowed distance to polyline, even if influence is 0.</param>
        /// <returns></returns>
        public bool Affects(Vector3 start, Vector3 end, double tolerance = Vector3.EPSILON)
        {
            var influenceDistance = Math.Max(InfluenceDistance, tolerance);
            Vector3 vs = Is2D ? new Vector3(start.X, start.Y) : start;
            Vector3 ve = Is2D ? new Vector3(end.X, end.Y) : end;
            //Vertical edges are not affected by 2D hint lines
            if (!Is2D || !vs.IsAlmostEqualTo(ve, tolerance) && Math.Abs(start.Z - end.Z) < tolerance)
            {
                foreach (var segment in Polyline.Segments())
                {
                    double lowClosest = 1;
                    double hiClosest = 0;

                    var dot = segment.Direction().Dot((ve - vs).Unitized());
                    if (!Math.Abs(dot).ApproximatelyEquals(1))
                    {
                        continue;
                    }

                    if (vs.DistanceTo(segment) <= influenceDistance)
                    {
                        lowClosest = 0;
                    }

                    if (ve.DistanceTo(segment) <= influenceDistance)
                    {
                        hiClosest = 1;
                    }

                    if (lowClosest < hiClosest)
                    {
                        return true;
                    }

                    var edgeLine = new Line(vs, ve);
                    Action<Vector3> check = (Vector3 p) =>
                    {
                        if (p.DistanceTo(edgeLine, out var closest) <= influenceDistance)
                        {
                            var t = (closest - vs).Length() / edgeLine.Length();
                            if (t < lowClosest)
                            {
                                lowClosest = t;
                            }

                            if (t > hiClosest)
                            {
                                hiClosest = t;
                            }
                        }
                    };

                    check(segment.Start);
                    check(segment.End);

                    if (hiClosest > lowClosest &&
                        (hiClosest - lowClosest) * edgeLine.Length() > influenceDistance)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
