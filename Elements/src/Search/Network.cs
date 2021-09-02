using System;
using System.Collections.Generic;
using System.Linq;
using Elements.Geometry;

namespace Elements.Search
{
    /// <summary>
    /// A network composed of nodes and edges with associated data.
    /// A network does not store spatial information. A network can
    /// index into another collection of entities which have a spatial context.
    /// </summary>
    /// <typeparam name="T">The type of data associated with the graph's edges.</typeparam>
    public class Network<T>
    {
        private AdjacencyList<T> _adjacencyList;

        /// <summary>
        /// Add a vertex to the network.
        /// </summary>
        public int AddVertex()
        {
            return _adjacencyList.AddVertex();
        }

        /// <summary>
        /// Adds an edge to the network from->to.
        /// </summary>
        /// <param name="from">The index of the start node.</param>
        /// <param name="to">The index of the end node.</param>
        /// <param name="data">The data associated with the edge.</param>
        public void AddEdgeOneWay(int from, int to, T data)
        {
            _adjacencyList.AddEdgeAtEnd(from, to, data);
        }

        /// <summary>
        /// Adds edges to the network both ways from->to and to->from.
        /// </summary>
        /// <param name="from">The index of the start node.</param>
        /// <param name="to">The index of the end node.</param>
        /// <param name="data">The data associated with the edge.</param>
        public void AddEdgeBothWays(int from, int to, T data)
        {
            _adjacencyList.AddEdgeAtEnd(from, to, data);
            _adjacencyList.AddEdgeAtEnd(to, from, data);
        }

        /// <summary>
        /// Create a network.
        /// </summary>
        public Network()
        {
            this._adjacencyList = new AdjacencyList<T>();
        }

        /// <summary>
        /// Create a network from an existing adjacency list.
        /// </summary>
        /// <param name="adjacencyList">The adjacency list.</param>
        private Network(AdjacencyList<T> adjacencyList)
        {
            this._adjacencyList = adjacencyList;
        }

        /// <summary>
        /// All leaf nodes of the network.
        /// </summary>
        /// <returns>A collection of leaf node indices.</returns>
        public List<int> LeafNodes()
        {
            return this._adjacencyList.Leaves();
        }

        /// <summary>
        /// All branch nodes of the network.
        /// </summary>
        /// <returns>A collection of branch node indices.</returns>
        public List<int> BranchNodes()
        {
            return this._adjacencyList.Branches();
        }

        /// <summary>
        /// The total number of nodes in the network.
        /// </summary>
        /// <returns></returns>
        public int NodeCount()
        {
            return this._adjacencyList.NodeCount();
        }

        /// <summary>
        /// Get all edges at the specified index.
        /// </summary>
        /// <param name="i">The index.</param>
        public List<(int, T)> EdgesAt(int i)
        {
            return this._adjacencyList[i].ToList();
        }

        /// <summary>
        /// Construct a network from the intersections of a collection
        /// of items which are segmentable.
        /// </summary>
        /// <param name="items">A collection of segmentable items.</param>
        /// <param name="getSegment">A delegate which returns a segment from an
        /// item of type T.</param>
        /// <param name="allNodeLocations">A collection of all node locations.</param>
        /// <param name="allIntersectionLocations">A collection of all intersection locations.</param>
        /// <param name="twoWayEdges">Should edges be created in both directions?</param>
        /// <returns>A network.</returns>
        public static Network<Line> FromSegmentableItems(IList<T> items,
                                                         Func<T, Line> getSegment,
                                                         out List<Vector3> allNodeLocations,
                                                         out List<Vector3> allIntersectionLocations,
                                                         bool twoWayEdges = true)
        {
            // Use a line sweep algorithm to identify intersection events.
            // https://www.geeksforgeeks.org/given-a-set-of-line-segments-find-if-any-two-segments-intersect/

            // Sort left-most points left to right according 
            // to their X coordinate.
            var events = items.SelectMany((item, i) =>
            {
                var s = getSegment(item);
                var leftMost = s.Start.X < s.End.X ? s.Start : s.End;
                return new[]{
                    (s.Start, i, s.Start == leftMost, s),
                    (s.End, i, s.End == leftMost, s)
                };
            }).GroupBy(x => x.Item1).Select(g =>
            {
                // TODO: Is there a way to make this faster?
                // We're grouping by coordinate which is SLOW and is 
                // only neccessary in the case where we have coincident points.

                // Group by the event coordinate as lines may share start 
                // or end points.
                return new LineSweepEvent<Line>(g.Key, g.Select(e => (e.i, e.Item3, e.s)).ToList());
            }).ToList();

            events.Sort();

            var segments = items.Select(item => { return getSegment(item); }).ToArray();

            // Create a binary tree to contain all segments ordered by their
            // left most point's Y coordinate
            var tree = new BinaryTree<int>(new LeftMostPointComparer(segments));

            var segmentIntersections = new Dictionary<int, List<(Vector3 location, int segmentId)>>();
            for (var i = 0; i < items.Count; i++)
            {
                segmentIntersections.Add(i, new List<(Vector3 location, int segmentId)>());
            }

            allIntersectionLocations = new List<Vector3>();

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
                                if (s.Intersects(segments[pre.Data], out Vector3 result, includeEnds: true))
                                {
                                    segmentIntersections[sd.segmentId].Add((result, sd.segmentId));
                                    segmentIntersections[pre.Data].Add((result, pre.Data));

                                    if (!allIntersectionLocations.Contains(result))
                                    {
                                        allIntersectionLocations.Add(result);
                                    }
                                }
                            }

                            foreach (var suc in sucs)
                            {
                                if (s.Intersects(segments[suc.Data], out Vector3 result, includeEnds: true))
                                {
                                    segmentIntersections[sd.segmentId].Add((result, sd.segmentId));
                                    segmentIntersections[suc.Data].Add((result, suc.Data));

                                    if (!allIntersectionLocations.Contains(result))
                                    {
                                        allIntersectionLocations.Add(result);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        tree.FindPredecessorSuccessor(sd.segmentId, out BinaryTreeNode<int> pre, out BinaryTreeNode<int> suc);
                        if (pre != null && suc != null)
                        {
                            if (segments[pre.Data].Intersects(segments[suc.Data], out Vector3 result, includeEnds: true))
                            {
                                segmentIntersections[pre.Data].Add(((result, pre.Data)));
                                segmentIntersections[suc.Data].Add(((result, suc.Data)));
                                if (!allIntersectionLocations.Contains(result))
                                {
                                    allIntersectionLocations.Add(result);
                                }
                            }
                        }
                        tree.Remove(sd.segmentId);
                        segmentIntersections[sd.segmentId].Add((e.Point, sd.segmentId));
                    }
                }
            }

            // A collection containing all intersection points, which
            // will be used to find an existing point if one exists.
            allNodeLocations = new List<Vector3>();

            // Loop over all segment intersection data, sorting the 
            // data by distance from the segment's start point, and
            // creating new vertices and edges as necessary.
            var adjacencyList = new AdjacencyList<Line>();
            foreach (var segmentData in segmentIntersections)
            {
                segmentIntersections[segmentData.Key].Sort(new DistanceComparer(segments[segmentData.Key].Start));
                var prevIndex = -1;
                var count = segmentIntersections[segmentData.Key].Count;
                for (var i = 0; i < count; i++)
                {
                    var x = segmentIntersections[segmentData.Key][i];

                    // We only add points as intersections if they're not at
                    // the start or end 
                    prevIndex = AddVertexAtEvent(x.location,
                                                 allNodeLocations,
                                                 adjacencyList,
                                                 segments[x.segmentId],
                                                 prevIndex,
                                                 twoWayEdges);
                }
            }

            return new Network<Line>(adjacencyList);
        }

        private static int AddVertexAtEvent(Vector3 location,
                                            List<Vector3> allNodeLocations,
                                            AdjacencyList<Line> adj,
                                            Line data,
                                            int previousIndex,
                                            bool twoWayEdges)
        {
            // Find an existing intersection location, 
            // or create a new one.
            var newIndex = allNodeLocations.IndexOf(location);
            if (newIndex == -1)
            {
                newIndex = adj.AddVertex();
                allNodeLocations.Add(location);
            }

            if (previousIndex == -1)
            {
                return newIndex;
            }

            // TODO: Figure out why this would ever happen.
            if (newIndex == previousIndex)
            {
                return newIndex;
            }

            adj.AddEdgeAtEnd(previousIndex, newIndex, data);
            if (twoWayEdges)
            {
                adj.AddEdgeAtEnd(newIndex, previousIndex, data);
            }
            return newIndex;
        }

        /// <summary>
        /// Draw the adjacency list.
        /// </summary>
        /// <param name="points">A collection of points representing the locations
        /// of the network's nodes.</param>
        /// <param name="color">The color of the resulting model geometry.</param>
        public ModelArrows ToModelArrows(IList<Vector3> points, Color? color)
        {
            var arrowData = new List<(Vector3 origin, Vector3 direction, double scale, Color? color)>();

            for (var i = 0; i < _adjacencyList.GetNumberOfVertices(); i++)
            {
                var start = points[i];
                foreach (var end in _adjacencyList[i])
                {
                    var d = (points[end.Item1] - start).Unitized();
                    var l = points[end.Item1].DistanceTo(start);
                    arrowData.Add((start, d, l, color));
                }
            }

            return new ModelArrows(arrowData, arrowAngle: 75);
        }

        /// <summary>
        /// Traverse the network from the specified node index.
        /// Traversal concludes when there are no more 
        /// available nodes to traverse.
        /// </summary>
        /// <param name="start">The starting point of the traversal.</param>
        /// <param name="next">The traversal step delegate.</param>
        /// <param name="visited">A collection of visited node indices.</param>
        /// <returns>A list of indices of the traversed nodes.</returns>
        public List<int> Traverse(int start,
                                  System.Func<List<(int, T)>, int> next,
                                  out List<int> visited)
        {
            var path = new List<int>();
            visited = new List<int>();
            var currentIndex = start;
            var prevIndex = -1;
            path.Add(currentIndex);

            while (currentIndex != -1 && !visited.Contains(currentIndex))
            {
                visited.Add(currentIndex);
                var oldIndex = currentIndex;
                currentIndex = Traverse(prevIndex, currentIndex, next);
                prevIndex = oldIndex;

                if (currentIndex != -1)
                {
                    path.Add(currentIndex);
                }
            }

            return path;
        }

        private int Traverse(int prevIndex,
                             int currentIndex,
                             System.Func<List<(int, T)>, int> next)
        {
            var edges = _adjacencyList[currentIndex];

            if (edges.Count == 0)
            {
                return -1;
            }

            if (edges.Count == 1 && edges.First.Value.Item1 != currentIndex)
            {
                // If there's only one connected vertex and 
                // it's not the current vertex, return it.
                return edges.First.Value.Item1;
            }

            return next(edges.ToList());
        }
    }
}