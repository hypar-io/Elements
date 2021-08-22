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
            var a = _v.DistanceTo(x.Item1);
            var b = _v.DistanceTo(y.Item1);

            if (a > b)
            {
                return 1;
            }
            else if (a < b)
            {
                return -1;
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
        /// <param name="adjacencyList">An adjacency list which describes the
        /// connectivity of the intersection points, keyed by the index of the
        /// intersection in the result.</param>
        /// <returns>A collection of unique intersection points.</returns>
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

            var segmentIntersections = new Dictionary<int, List<(Vector3 location, int segmentId)>>();
            for (var i = 0; i < segments.Count; i++)
            {
                segmentIntersections.Add(i, new List<(Vector3 location, int segmentId)>());
            }

            foreach (var e in events)
            {
                foreach (var sd in e.Segments)
                {
                    var s = segments[sd.segmentId];

                    if (sd.isLeftMostPoint)
                    {
                        segmentIntersections[sd.segmentId].Add((e.Point, sd.segmentId));

                        if (tree.Add(sd.segmentId))
                        {
                            tree.FindPredecessorSuccessors(sd.segmentId, out List<BinaryTreeNode<int>> pres, out List<BinaryTreeNode<int>> sucs);

                            foreach (var pre in pres)
                            {
                                if (s.Intersects(segments[pre.Data], out Vector3 result))
                                {
                                    segmentIntersections[sd.segmentId].Add((result, sd.segmentId));
                                    segmentIntersections[pre.Data].Add((result, pre.Data));
                                }
                            }

                            foreach (var suc in sucs)
                            {
                                if (s.Intersects(segments[suc.Data], out Vector3 result))
                                {
                                    segmentIntersections[sd.segmentId].Add((result, sd.segmentId));
                                    segmentIntersections[suc.Data].Add((result, suc.Data));
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
                                segmentIntersections[pre.Data].Add(((result, pre.Data)));
                                segmentIntersections[suc.Data].Add(((result, suc.Data)));
                            }
                        }
                        tree.Remove(sd.segmentId);
                        segmentIntersections[sd.segmentId].Add((e.Point, sd.segmentId));
                    }
                }
            }

            // Loop over all segment intersection data, sorting the 
            // data by distance from the segment's start point, and
            // creating new vertices and edges as necessary.
            foreach (var segmentData in segmentIntersections)
            {
                segmentIntersections[segmentData.Key].Sort(new EventComparer(segments[segmentData.Key].Start));
                var prevIndex = -1;
                foreach (var x in segmentIntersections[segmentData.Key])
                {
                    prevIndex = AddVertexAtEvent(x.location,
                                                 intersections,
                                                 adjacencyList,
                                                 prevIndex);
                }
            }

            return intersections;
        }

        private static int AddVertexAtEvent(Vector3 location,
                                             List<Vector3> allIntersections,
                                             AdjacencyList adj,
                                             int previousIndex)
        {
            // Find an existing intersection location, 
            // or create a new one.
            var newIndex = allIntersections.IndexOf(location);
            if (newIndex == -1)
            {
                newIndex = adj.AddVertex();
                allIntersections.Add(location);
            }

            if (previousIndex == -1)
            {
                return newIndex;
            }

            adj.AddEdgeAtEnd(previousIndex, newIndex);
            adj.AddEdgeAtEnd(newIndex, previousIndex);
            return newIndex;
        }
    }
}