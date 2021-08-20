using System.Collections.Generic;
using System.Linq;
using Elements.Geometry;

namespace Elements.Search
{
    public static class LineSweep
    {
        /// <summary>
        /// Conduct a line sweep across segments, finding all intersections.
        /// </summary>
        /// <param name="segments"></param>
        public static List<Vector3> FromSegments(IList<Line> segments)
        {
            // https://www.geeksforgeeks.org/given-a-set-of-line-segments-find-if-any-two-segments-intersect/

            // Sort left-most points left to right according 
            // to their X coordinate.
            var events = segments.SelectMany(s =>
            {
                var leftMost = s.Start.X < s.End.X ? s.Start : s.End;
                return new[]{
                    (s.Start, s, s.Start == leftMost),
                    (s.End, s, s.End == leftMost)
                };
            }).GroupBy(x => x.Item1).Select(g =>
            {
                // Group by the event coordinate as lines may share start 
                // or end points.
                return new LineSweepEvent(g.Key, g.Select(e => (e.s, e.Item3)).ToList());
            }).ToList();

            events.Sort();

            // Create a binary tree to contain all segments ordered by their
            // left most point's Y coordinate
            var tree = new BinaryTree<Line>(new LineSweepSegmentComparer());

            var results = new List<Vector3>();

            foreach (var e in events)
            {
                if (e.Segments.Count > 1)
                {
                    // A beginning or end where multiple
                    // lines meet.
                    results.Add(e.Point);
                }

                // If the line is vertical intersect with all lines
                // currently in the tree and don't add the line


                foreach (var sd in e.Segments)
                {
                    if (sd.isLeftMostPoint)
                    {
                        if (tree.Add(sd.segment))
                        {
                            tree.FindPredecessorSuccessors(sd.segment, out List<Node<Line>> pres, out List<Node<Line>> sucs);

                            foreach (var pre in pres)
                            {
                                if (sd.segment.Intersects(pre.Data, out Vector3 result))
                                {
                                    results.Add(result);
                                }
                            }

                            foreach (var suc in sucs)
                            {
                                if (sd.segment.Intersects(suc.Data, out Vector3 result))
                                {
                                    results.Add(result);
                                }
                            }
                        }
                    }
                    else
                    {
                        tree.FindPredecessorSuccessor(sd.segment, out Node<Line> pre, out Node<Line> suc);
                        if (pre != null && suc != null)
                        {
                            if (pre.Data.Intersects(suc.Data, out Vector3 result))
                            {
                                results.Add(result);
                            }
                        }

                        tree.Remove(sd.segment);
                    }
                }
            }

            return results;
        }
    }
}