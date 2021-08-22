using System.Collections.Generic;
using System.Linq;
using Elements.Search;

namespace Elements.Geometry
{
    internal class EventComparer : IComparer<(Vector3, int)>
    {
        private Vector3 _v;

        /// <summary>
        /// Construct an event comparer.
        /// </summary>
        /// <param name="v">The vector against which to compare.</param>
        public EventComparer(Vector3 v)
        {
            this._v = v;
        }

        public int Compare((Vector3, int) x, (Vector3, int) y)
        {
            var a = _v.Dot(x.Item1);
            var b = _v.Dot(y.Item1);

            if (a > b)
            {
                return -1;
            }
            else if (a < b)
            {
                return 1;
            }
            return 0;
        }
    }

    /// <summary>
    /// Line segment extension methods.
    /// </summary>
    public static class LineSegmentExtensions
    {
        /// <summary>
        /// Find all intersections of the provided collection of lines.
        /// </summary>
        /// <param name="segments">A collection of line segments.</param>
        /// <param name="adjacencyList"></param>
        /// <returns>A dictionary of intersection point collections
        /// keyed by the index of the line segment in the provided collection.</returns>
        public static List<Vector3> Intersections(this IList<Line> segments, out AdjacencyList adjacencyList)
        {
            // https://www.geeksforgeeks.org/given-a-set-of-line-segments-find-if-any-two-segments-intersect/

            // Sort left-most points left to right according 
            // to their X coordinate.
            var events = segments.SelectMany((s, i) =>
            {
                var leftMost = s.Start.X < s.End.X ? s.Start : s.End;
                return new[]{
                    (s.Start, i, s.Start == leftMost),
                    (s.End, i, s.End == leftMost)
                };
            }).GroupBy(x => x.Item1).Select(g =>
            {
                // TODO: Is there a way to make this faster?
                // We're grouping by coordinate which is SLOW and is 
                // only neccessary in the case where we have coincident points.

                // Group by the event coordinate as lines may share start 
                // or end points.
                return new LineSweepEvent(g.Key, g.Select(e => (e.i, e.Item3)).ToList());
            }).ToList();

            events.Sort();

            // Create a binary tree to contain all segments ordered by their
            // left most point's Y coordinate
            var tree = new BinaryTree<int>(new LineSweepSegmentComparer(segments));

            adjacencyList = new AdjacencyList();

            // A collection containing all intersection points, which
            // will be used to find an existing point if one exists.
            var intersections = new List<Vector3>();

            // A dictionary to track last intersection node index
            // along all curves.
            var segmentIndices = new Dictionary<int, int>();
            for (var i = 0; i < segments.Count; i++)
            {
                segmentIndices.Add(i, -1);
            }

            foreach (var e in events)
            {
                foreach (var sd in e.Segments)
                {
                    var s = segments[sd.segmentId];

                    if (sd.isLeftMostPoint)
                    {
                        AddVertexAtSegmentStart(e.Point, sd.segmentId, intersections, adjacencyList, segmentIndices);

                        if (tree.Add(sd.segmentId))
                        {
                            tree.FindPredecessorSuccessors(sd.segmentId, out List<BinaryTreeNode<int>> pres, out List<BinaryTreeNode<int>> sucs);

                            // Intersect lines above and below and sort the 
                            // results so that when we add graph information
                            // the graph edges flow in the direction of the segment.
                            var localIntersections = new List<(Vector3 location, int segmentId)>();

                            foreach (var pre in pres)
                            {
                                if (s.Intersects(segments[pre.Data], out Vector3 result))
                                {
                                    localIntersections.Add((result, pre.Data));
                                }
                            }

                            foreach (var suc in sucs)
                            {
                                if (s.Intersects(segments[suc.Data], out Vector3 result))
                                {
                                    localIntersections.Add((result, suc.Data));
                                }
                            }

                            localIntersections.Sort(new EventComparer(Vector3.XAxis));
                            foreach (var x in localIntersections)
                            {
                                AddVertexAtIntersection(x.location,
                                                        intersections,
                                                        adjacencyList,
                                                        segmentIndices,
                                                        sd.segmentId,
                                                        x.segmentId);
                            }

                        }
                    }
                    else
                    {
                        tree.FindPredecessorSuccessor(sd.segmentId, out BinaryTreeNode<int> pre, out BinaryTreeNode<int> suc);
                        if (pre != null && suc != null)
                        {
                            if (segments[pre.Data].Intersects(segments[suc.Data], out Vector3 result))
                            {
                                AddVertexAtIntersection(result,
                                                                   intersections,
                                                                   adjacencyList,
                                                                   segmentIndices,
                                                                   pre.Data,
                                                                   suc.Data);
                            }
                        }
                        tree.Remove(sd.segmentId);
                        AddVertexAtSegmentEnd(e.Point, sd.segmentId, adjacencyList, intersections, segmentIndices);
                    }
                }
            }

            return intersections;
        }

        private static void AddVertexAtSegmentStart(Vector3 location, int segmentId, List<Vector3> intersections, AdjacencyList adj, Dictionary<int, int> segmentIndices)
        {
            var newIndex = intersections.IndexOf(location);
            if (newIndex == -1)
            {
                newIndex = adj.AddVertex();
                intersections.Add(location);
            }
            segmentIndices[segmentId] = newIndex;
        }

        private static void AddVertexAtSegmentEnd(Vector3 location, int segmentId, AdjacencyList adj, List<Vector3> intersections, Dictionary<int, int> segmentIndices)
        {
            var newIndex = intersections.IndexOf(location);
            if (newIndex == -1)
            {
                newIndex = adj.AddVertex();
                intersections.Add(location);
            }

            var oldIndex = segmentIndices[segmentId];
            if (oldIndex != -1)
            {
                adj.AddEdgeAtEnd(oldIndex, newIndex);
            }
            segmentIndices[segmentId] = newIndex;
        }

        private static void AddVertexAtIntersection(Vector3 location,
                                                  List<Vector3> intersections,
                                                  AdjacencyList adj,
                                                  Dictionary<int, int> segmentIndices,
                                                  params int[] connectTo)
        {
            // Find an existing intersection location, 
            // or create a new one.
            var newIndex = intersections.IndexOf(location);
            if (newIndex == -1)
            {
                newIndex = adj.AddVertex();
                intersections.Add(location);
            }

            foreach (var i in connectTo)
            {
                if (segmentIndices[i] != -1)
                {
                    var lastIndex = segmentIndices[i];
                    adj.AddEdgeAtEnd(lastIndex, newIndex);
                }

                // Point the intersection map for the segment
                // to the new index.
                segmentIndices[i] = newIndex;
            }
        }

        /// <summary>
        /// Build an adjacency graph from ordered collections of points.
        /// Points do not need to be unique, but the 
        /// </summary>
        /// <param name="points">An ordered collection of points representing 
        /// a contiguous sequence of line segments. 
        /// e.g., [A,B,C,D] gives A->B, B->C, C->D</param>
        /// <returns>An adjacency list containing connected points.</returns>
        // public static AdjacencyList AdjacencyList(this Dictionary<int, List<Vector3>> points)
        // {
        //     var ptLookup = points.SelectMany(p => p.Value).Distinct().ToList();
        //     var adj = new AdjacencyList<Vector3>(ptLookup.Count);

        //     foreach (var ptSet in points)
        //     {
        //         for (var i = 0; i < ptSet.Value.Count - 1; i++)
        //         {
        //             var a = ptLookup.IndexOf(ptSet.Value[i]);
        //             var b = ptLookup.IndexOf(ptSet.Value[i + 1]);
        //             if (!adj[a].Contains((b, ptLookup[a])))
        //             {
        //                 adj.AddEdgeAtEnd(a, b, ptLookup[a]);
        //             }
        //             if (!adj[b].Contains((a, ptLookup[b])))
        //             {
        //                 adj.AddEdgeAtEnd(b, a, ptLookup[b]);
        //             }
        //         }
        //     }

        //     return adj;
        // }
    }
}