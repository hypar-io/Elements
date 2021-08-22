using System.Collections.Generic;
using System.Linq;
using Elements.Search;

namespace Elements.Geometry
{
    /// <summary>
    /// Line segment extension methods.
    /// </summary>
    public static class LineSegmentExtensions
    {
        /// <summary>
        /// Find all intersections of the provided collection of lines.
        /// </summary>
        /// <param name="segments">A collection of line segments.</param>
        /// <returns>A dictionary of intersection point collections
        /// keyed by the index of the line segment in the provided collection.</returns>
        public static Dictionary<int, List<Vector3>> Intersections(this IList<Line> segments)
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

            var results = new Dictionary<int, List<Vector3>>();
            for (var i = 0; i < segments.Count; i++)
            {
                results.Add(i, new List<Vector3>());
            }

            foreach (var e in events)
            {
                foreach (var sd in e.Segments)
                {
                    var s = segments[sd.segmentId];

                    if (sd.isLeftMostPoint)
                    {
                        // Add the start point of the segment.
                        results[sd.segmentId].Add(e.Point);

                        if (tree.Add(sd.segmentId))
                        {
                            tree.FindPredecessorSuccessors(sd.segmentId, out List<BinaryTreeNode<int>> pres, out List<BinaryTreeNode<int>> sucs);

                            foreach (var pre in pres)
                            {
                                if (s.Intersects(segments[pre.Data], out Vector3 result))
                                {
                                    results[sd.segmentId].Add(result);
                                    results[pre.Data].Add(result);
                                }
                            }

                            foreach (var suc in sucs)
                            {
                                if (s.Intersects(segments[suc.Data], out Vector3 result))
                                {
                                    results[sd.segmentId].Add(result);
                                    results[suc.Data].Add(result);
                                }
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
                                results[pre.Data].Add(result);
                                results[suc.Data].Add(result);
                            }
                        }
                        tree.Remove(sd.segmentId);
                    }

                    // Add the end point of the segment.
                    results[sd.segmentId].Add(e.Point);
                }
            }

            foreach (var ptset in results)
            {
                ptset.Value.Sort(new DotComparer(segments[ptset.Key].Direction()));
            }

            return results;
        }

        /// <summary>
        /// Build an adjacency graph from ordered collections of points.
        /// Points do not need to be unique, but the 
        /// </summary>
        /// <param name="points">An ordered collection of points representing 
        /// a contiguous sequence of line segments. 
        /// e.g., [A,B,C,D] gives A->B, B->C, C->D</param>
        /// <returns>An adjacency list containing connected points.</returns>
        public static AdjacencyList<Vector3> AdjacencyList(this Dictionary<int, List<Vector3>> points)
        {
            var ptLookup = points.SelectMany(p => p.Value).Distinct().ToList();
            var adj = new AdjacencyList<Vector3>(ptLookup.Count);

            foreach (var ptSet in points)
            {
                for (var i = 0; i < ptSet.Value.Count - 1; i++)
                {
                    var a = ptLookup.IndexOf(ptSet.Value[i]);
                    var b = ptLookup.IndexOf(ptSet.Value[i + 1]);
                    if (!adj[a].Contains((b, ptLookup[a])))
                    {
                        adj.AddEdgeAtEnd(a, b, ptLookup[a]);
                    }
                    if (!adj[b].Contains((a, ptLookup[b])))
                    {
                        adj.AddEdgeAtEnd(b, a, ptLookup[b]);
                    }
                }
            }

            return adj;
        }
    }
}