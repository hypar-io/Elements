using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Elements.Geometry;

namespace Elements.Search
{
    /// <summary>
    /// Provides graph edge info
    /// </summary>
    public class LocalEdge
    {
        [Flags]
        internal enum VisitDirections
        {
            None,
            Straight,
            Opposite
        }

        /// <summary>
        /// Creates a new instance of Edge class
        /// </summary>
        /// <param name="vertexIndex1">The index of the first vertex.</param>
        /// <param name="vertexIndex2">The index of the second vertex.</param>
        public LocalEdge(int vertexIndex1, int vertexIndex2)
        {
            visitDirections = VisitDirections.None;
            Start = vertexIndex1;
            End = vertexIndex2;
        }

        /// <summary>
        /// The index of the first vertex.
        /// </summary>
        public int Start { get; }

        /// <summary>
        /// The index of the second vertex.
        /// </summary>
        public int End { get; }

        /// <summary>
        /// Mark a vertex as having been visited from the specified index.
        /// </summary>
        /// <param name="start">The index of the vertex from which the edge is visited.</param>
        public void MarkAsVisited(int start)
        {
            if (start == Start)
            {
                visitDirections |= VisitDirections.Straight;
            }
            else if (start == End)
            {
                visitDirections |= VisitDirections.Opposite;
            }
        }

        /// <summary>
        /// Is this edge between the provided vertices?
        /// </summary>
        /// <param name="start">The index of the first vertex.</param>
        /// <param name="end">The index of the second vertex.</param>
        /// <returns>Returns true if the edge is between the provided vertex indices.</returns>
        public bool IsBetweenVertices(int start, int end)
        {
            return (Start == start && End == end) ||
                (Start == end && End == start);
        }

        /// <summary>
        /// Is this edge visited from the provided vertex?
        /// </summary>
        /// <param name="vertexIndex">The index of the vertex from which the vertex is visited.</param>
        /// <returns>Returns true if the edge was visited from the vertex.</returns>
        public bool IsVisitedFromVertex(int vertexIndex)
        {
            if (Start == vertexIndex)
            {
                return visitDirections.HasFlag(VisitDirections.Straight);
            }

            if (End == vertexIndex)
            {
                return visitDirections.HasFlag(VisitDirections.Opposite);
            }

            return false;
        }

        internal VisitDirections visitDirections;
    }

    /// <summary>
    /// A network composed of nodes and edges with associated data.
    /// A network does not store spatial information. A network can
    /// index into another collection of entities which have a spatial context.
    /// </summary>
    /// <typeparam name="T">The type of data associated with the graph's edges.</typeparam>
    public class Network<T>
    {
        private readonly AdjacencyList<T> _adjacencyList;

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
        public IEnumerable<(int, T)> EdgesAt(int i)
        {
            return this._adjacencyList[i];
        }

        private class XEqualityWithFallbackToYEqualityComparer : IComparer<Vector3>
        {
            public int Compare(Vector3 x, Vector3 y)
            {
                if (x.X.ApproximatelyEquals(y.X))
                {
                    return y.Y.CompareTo(x.Y);
                }

                return x.X.CompareTo(y.X);
            }
        }

        /// <summary>
        /// Construct a network from the intersections of a collection
        /// of items which provide segments in a shared plane.
        /// </summary>
        /// <param name="items">A collection of segmentable items.</param>
        /// <param name="getSegment">A delegate which returns a segment from an
        /// item of type T.</param>
        /// <param name="allNodeLocations">A collection of all node locations.</param>
        /// <param name="allIntersectionLocations">A collection of all intersection locations.</param>
        /// <param name="twoWayEdges">Should edges be created in both directions?</param>
        /// <returns>A network.</returns>
        public static Network<T> FromSegmentableItems(IList<T> items,
                                                         Func<T, Line> getSegment,
                                                         out List<Vector3> allNodeLocations,
                                                         out List<Vector3> allIntersectionLocations,
                                                         bool twoWayEdges = true)
        {
            // Use a line sweep algorithm to identify intersection events.
            // https://www.geeksforgeeks.org/given-a-set-of-line-segments-find-if-any-two-segments-intersect/

            // Order the line sweep events from top to bottom then left to right.
            // The practical result of this is that the sweep line is not exactly
            // horizontal but moves as if at a slight incline. This solves for
            // all perpendicular cases.
            var events = items.SelectMany((item, i) =>
            {
                var segment = getSegment(item);
                var leftMost = segment.Start;
                if (segment.Start.X > segment.End.X)
                {
                    leftMost = segment.End;
                }
                else if (segment.Start.X.ApproximatelyEquals(segment.End.X))
                {
                    leftMost = segment.Start.Y < segment.End.Y ? segment.End : segment.Start;
                }
                return new (Vector3 location, int index, bool isLeftMost, T item)[]{
                    (segment.Start, i, segment.Start == leftMost, item),
                    (segment.End, i, segment.End == leftMost, item)
                };
            }).GroupBy(x => x.location).Select(g =>
            {
                // TODO: Is there a way to make this faster?
                // We're grouping by coordinate which is SLOW and is
                // only necessary in the case where we have coincident points.

                // Group by the event coordinate as lines may share start
                // or end points.
                return new LineSweepEvent<T>(g.Key, g.Select(e => (e.index, e.isLeftMost, e.item)));
            });

            events = events.OrderBy(e => -e.Point.Y).OrderBy(e => e.Point, new XEqualityWithFallbackToYEqualityComparer());
            var segments = items.Select(item => { return getSegment(item); }).ToArray();

            // Create a binary tree to contain all segments ordered by their
            // left most point's Y coordinate
            var tree = new BinaryTree<T>(new LeftMostPointComparer<T>(getSegment));

            var segmentIntersections = new Dictionary<T, List<Vector3>>();
            for (var i = 0; i < items.Count; i++)
            {
                if (!segmentIntersections.ContainsKey(items[i]))
                {
                    segmentIntersections.Add(items[i], new List<Vector3>());
                }
            }

            allIntersectionLocations = new List<Vector3>();

            foreach (var e in events)
            {
                foreach (var (segmentId, isLeftMostPoint, data) in e.Segments)
                {
                    var s = segments[segmentId];

                    if (isLeftMostPoint)
                    {
                        segmentIntersections[data].Add(e.Point);

                        if (tree.Add(data))
                        {
                            tree.FindPredecessorSuccessors(data, out List<BinaryTreeNode<T>> pres, out List<BinaryTreeNode<T>> sucs);

                            foreach (var pre in pres)
                            {
                                if (s.Intersects(getSegment(pre.Data), out Vector3 result, includeEnds: true))
                                {
                                    if (PointIsUniqueIntersectionAlongLine(result, s, segmentIntersections[data]))
                                    {
                                        segmentIntersections[data].Add(result);
                                    }
                                    if (PointIsUniqueIntersectionAlongLine(result, getSegment(pre.Data), segmentIntersections[pre.Data]))
                                    {
                                        segmentIntersections[pre.Data].Add(result);
                                    }

                                    // TODO: Come up with a better solution for
                                    // storing only the intersection points without
                                    // needing Contains().
                                    if (!allIntersectionLocations.Contains(result))
                                    {
                                        allIntersectionLocations.Add(result);
                                    }
                                }
                            }

                            foreach (var suc in sucs)
                            {
                                if (s.Intersects(getSegment(suc.Data), out Vector3 result, includeEnds: true))
                                {
                                    if (PointIsUniqueIntersectionAlongLine(result, s, segmentIntersections[data]))
                                    {
                                        segmentIntersections[data].Add(result);
                                    }
                                    if (PointIsUniqueIntersectionAlongLine(result, getSegment(suc.Data), segmentIntersections[suc.Data]))
                                    {
                                        segmentIntersections[suc.Data].Add(result);
                                    }

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
                        tree.FindPredecessorSuccessor(data, out BinaryTreeNode<T> pre, out BinaryTreeNode<T> suc);

                        if (pre != null && suc != null)
                        {
                            if (getSegment(pre.Data).Intersects(getSegment(suc.Data), out Vector3 result, includeEnds: true))
                            {
                                if (PointIsUniqueIntersectionAlongLine(result, s, segmentIntersections[data]))
                                {
                                    segmentIntersections[data].Add(result);
                                }
                                if (PointIsUniqueIntersectionAlongLine(result, getSegment(suc.Data), segmentIntersections[suc.Data]))
                                {
                                    segmentIntersections[suc.Data].Add(result);
                                }
                                if (PointIsUniqueIntersectionAlongLine(result, getSegment(pre.Data), segmentIntersections[pre.Data]))
                                {
                                    segmentIntersections[pre.Data].Add(result);
                                }
                                if (!allIntersectionLocations.Contains(result))
                                {
                                    allIntersectionLocations.Add(result);
                                }
                            }
                        }

                        if (pre != null)
                        {
                            if (s.Intersects(getSegment(pre.Data), out Vector3 result, includeEnds: true))
                            {
                                if (PointIsUniqueIntersectionAlongLine(result, s, segmentIntersections[data]))
                                {
                                    segmentIntersections[data].Add(result);
                                }
                                if (PointIsUniqueIntersectionAlongLine(result, getSegment(pre.Data), segmentIntersections[pre.Data]))
                                {
                                    segmentIntersections[pre.Data].Add(result);
                                }
                                if (!allIntersectionLocations.Contains(result))
                                {
                                    allIntersectionLocations.Add(result);
                                }
                            }
                        }

                        if (suc != null)
                        {
                            if (s.Intersects(getSegment(suc.Data), out Vector3 result, includeEnds: true))
                            {
                                if (PointIsUniqueIntersectionAlongLine(result, s, segmentIntersections[data]))
                                {
                                    segmentIntersections[data].Add(result);
                                }
                                if (PointIsUniqueIntersectionAlongLine(result, getSegment(suc.Data), segmentIntersections[suc.Data]))
                                {
                                    segmentIntersections[suc.Data].Add(result);
                                }
                                if (!allIntersectionLocations.Contains(result))
                                {
                                    allIntersectionLocations.Add(result);
                                }
                            }
                        }

                        //NOTE: Custom data comparer is used inside Find method. Sometimes it fails to compare elements and
                        // as result the wrong element can be deleted. It shouldn't be the case after changes inside LeftMostPointComparer,
                        // but I left this code here as a precaution
                        if (tree.Find(data) != null && tree.Find(data).Data.Equals(data))
                        {
                            tree.Remove(data);
                        }

                        if (!segmentIntersections[data].Any(p => p.IsAlmostEqualTo(e.Point)))
                            segmentIntersections[data].Add(e.Point);
                    }
                }
                Debug.WriteLine(tree.ToString());
            }

            // A collection containing all intersection points, which
            // will be used to find an existing point if one exists.
            allNodeLocations = new List<Vector3>();

            // Loop over all segment intersection data, sorting the
            // data by distance from the segment's start point, and
            // creating new vertices and edges as necessary.
            var adjacencyList = new AdjacencyList<T>();
            foreach (var segmentData in segmentIntersections)
            {
                var line = getSegment(segmentData.Key);
                segmentIntersections[segmentData.Key].Sort(new DistanceComparer(line.Start));
                var prevIndex = -1;
                var count = segmentIntersections[segmentData.Key].Count;
                for (var i = 0; i < count; i++)
                {
                    var x = segmentIntersections[segmentData.Key][i];

                    // We only add points as intersections if they're not at
                    // the start or end
                    prevIndex = AddVertexAtEvent(x,
                                                 allNodeLocations,
                                                 adjacencyList,
                                                 segmentData.Key,
                                                 prevIndex,
                                                 twoWayEdges);
                }
            }

            return new Network<T>(adjacencyList);
        }

        /// <summary>
        /// Find all the closed regions in the network.
        /// This method uses the Traverse method internally with a traversal
        /// function that uses the maximal plane angle to determine the direction
        /// of traversal.
        /// </summary>
        /// <param name="allNodeLocations">A collection of all node locations in the network.</param>
        /// <returns>A collection of integers representing the indices of the nodes
        /// forming closed regions in the network.</returns>
        public List<List<int>> FindAllClosedRegions(List<Vector3> allNodeLocations)
        {
            var regions = new List<List<int>>();

            // TODO: This code is a mess. We use several methods for tracking
            // traversal data: LocalEdge instances, nodeVisits, and structures
            // internal to the traversal methods. These can be combined so that
            // we are only using LocalEdges.

            var leafNodes = new List<int>();
            var allEdges = new List<LocalEdge>();

            for (var i = 0; i < this.NodeCount(); i++)
            {
                var localEdges = EdgesAt(i);
                var edgeCount = localEdges.Count();
                // Leaf nodes
                if (edgeCount == 1)
                {
                    leafNodes.Add(i);
                }

                // TODO: This is slow to set up because we need to find each
                // edge, potentially scanning the entire list of edges. This is
                // because we might have two-way edges in the network, but we
                // only want one local edge for tracking purposes.
                foreach (var edge in localEdges)
                {
                    var foundEdge = allEdges.FirstOrDefault(e => e.IsBetweenVertices(i, edge.Item1));
                    if (foundEdge == null)
                    {
                        allEdges.Add(new LocalEdge(i, edge.Item1));
                    }
                }
            }

            var nodeVisits = new int[NodeCount()];

            // Traverse from leaves first. This will capture paths where
            // a leaf edge traverses into our out of a closed region.
            foreach (var leafIndex in leafNodes)
            {
                var path = TraversePath(leafIndex, allNodeLocations, allEdges);
                if (path != null)
                {
                    regions.Add(path);
                }
            }

            // Traverse over all nodes. Edges found during the first
            // traversal path will be skipped during traversal.
            for (var i = 0; i < nodeVisits.Length; i++)
            {
                var localEdgeCount = EdgesAt(i).Count();
                if (localEdgeCount > 1 && localEdgeCount > nodeVisits[i])
                {
                    var path = TraversePath(i, allNodeLocations, allEdges);

                    if (path != null)
                    {
                        // Add the visits to the corresponding nodes
                        // to ensure that we don't re-traverse this loop.
                        foreach (var index in path)
                        {
                            nodeVisits[index] = nodeVisits[index] + 1;
                        }

                        regions.Add(path);
                    }
                }
            }

            // Traverse any edges that haven't been traversed.
            // This can happen when a region is "captured" by surrounding
            // regions that have been traversed, leaving one region bounded
            // completely bounded except on one side.
            var unvisitedEdges = allEdges.Where(e => e.visitDirections == LocalEdge.VisitDirections.None);
            foreach (var unvisitedEdge in unvisitedEdges)
            {
                var path = TraversePath(unvisitedEdge.End, allNodeLocations, allEdges, unvisitedEdge.Start);
                if (path != null)
                {
                    regions.Add(path);
                }
            }

            return regions;
        }

        private List<int> TraversePath(int i, List<Vector3> allNodeLocations, List<LocalEdge> allEdges, int prevIndex = -1)
        {
            Debug.WriteLine($"STARTING PATH AT INDEX: {i}");

            List<int> path = Traverse(i, TraverseLargestPlaneAngle, allNodeLocations, allEdges, out List<int> visited, prevIndex);

            if (IsNotClosed(path))
            {
                Debug.WriteLine($"EXITING NON CLOSED PATH");
                Debug.WriteLine(string.Empty);
                return null;
            }

            MarkVisitedEdges(allEdges, path);

            if (IsTooShort(path))
            {
                Debug.WriteLine($"EXITING PATH TOO SHORT");
                Debug.WriteLine(string.Empty);
                return null;
            }

            Debug.WriteLine($"FOUND PATH: {string.Join(",", path)}");
            Debug.WriteLine(string.Empty);

            return path;
        }

        private bool IsTooShort(List<int> path)
        {
            return path.Count < 3;
        }

        private bool IsNotClosed(List<int> path)
        {
            return path[0] != path[path.Count - 1];
        }

        /// <summary>
        /// Traverse a network following the smallest plane angle between the current
        /// edge and the next candidate edge.
        /// </summary>
        /// <param name="traversalData">Data about the current step of the traversal.</param>
        /// <param name="allNodeLocations">A collection of all node locations in the network.</param>
        /// <param name="visitedEdges">A collection of previously visited edges.</param>
        /// <returns>The next index to traverse.</returns>
        public static int TraverseSmallestPlaneAngle((int currentIndex, int previousIndex, IEnumerable<int> edgeIndices) traversalData,
                                               List<Vector3> allNodeLocations,
                                               List<LocalEdge> visitedEdges)
        {
            var minAngle = double.MaxValue;
            var minIndex = -1;
            var baseEdge = traversalData.previousIndex == -1 ? Vector3.XAxis : (allNodeLocations[traversalData.currentIndex] - allNodeLocations[traversalData.previousIndex]).Unitized();
            var edgeIndices = traversalData.edgeIndices.Distinct().ToList();
            foreach (var e in edgeIndices)
            {
                if (e == traversalData.previousIndex)
                {
                    Debug.WriteLine($"Skipping index {e} as previous.");
                    continue;
                }

                var visitedEdge = visitedEdges.FirstOrDefault(edge => edge.IsBetweenVertices(e, traversalData.currentIndex));
                if (visitedEdge?.IsVisitedFromVertex(traversalData.currentIndex) == true)
                {
                    Debug.WriteLine($"Skipping index {e} as visited.");
                    continue;
                }

                var localEdge = (allNodeLocations[e] - allNodeLocations[traversalData.currentIndex]).Unitized();
                var angle = localEdge.PlaneAngleTo(baseEdge);

                // The angle of traversal is not actually zero here,
                // it's 180 (unless the path is invalid). We want to
                // ensure that traversal happens along the straight
                // edge if possible.
                if (angle == 0)
                {
                    angle = 180.0;
                }

                Debug.WriteLine($"{traversalData.currentIndex}:{e}:{angle}");

                if (angle < minAngle)
                {
                    Debug.WriteLine("Found minimum.");
                    minAngle = angle;
                    minIndex = e;
                }
            }
            return minIndex;
        }

        /// <summary>
        /// Traverse a network following the smallest plane angle between the current
        /// edge and the next candidate edge.
        /// </summary>
        /// <param name="traversalData">Data about the current step of the traversal.</param>
        /// <param name="allNodeLocations">A collection of all node locations in the network.</param>
        /// <param name="visitedEdges">A collection of previously visited edges.</param>
        /// <returns>The next index to traverse.</returns>
        public static int TraverseLargestPlaneAngle((int currentIndex, int previousIndex, IEnumerable<int> edgeIndices) traversalData,
                                               List<Vector3> allNodeLocations,
                                               List<LocalEdge> visitedEdges)
        {
            var maxAngle = double.MinValue;
            var maxIndex = -1;
            var baseEdge = traversalData.previousIndex == -1 ? Vector3.XAxis : (allNodeLocations[traversalData.currentIndex] - allNodeLocations[traversalData.previousIndex]).Unitized();
            var edgeIndices = traversalData.edgeIndices.Distinct().ToList();
            foreach (var e in edgeIndices)
            {
                if (e == traversalData.previousIndex)
                {
                    Debug.WriteLine($"Skipping index {e} as previous.");
                    continue;
                }

                var visitedEdge = visitedEdges.FirstOrDefault(edge => edge.IsBetweenVertices(e, traversalData.currentIndex));
                if (visitedEdge?.IsVisitedFromVertex(traversalData.currentIndex) == true)
                {
                    Debug.WriteLine($"Skipping index {e} as visited.");
                    continue;
                }

                var localEdge = (allNodeLocations[e] - allNodeLocations[traversalData.currentIndex]).Unitized();
                var angle = localEdge.PlaneAngleTo(baseEdge);

                // The angle of traversal is not actually zero here,
                // it's 180 (unless the path is invalid). We want to
                // ensure that traversal happens along the straight
                // edge if possible.
                if (angle == 0)
                {
                    angle = 180.0;
                }

                Debug.WriteLine($"{traversalData.currentIndex}:{e}:{angle}");

                if (angle > maxAngle)
                {
                    Debug.WriteLine("Found maximum.");
                    maxAngle = angle;
                    maxIndex = e;
                }
            }
            return maxIndex;
        }

        private static void MarkVisitedEdges(List<LocalEdge> visitedEdges, List<int> path)
        {
            for (int j = 0; j < path.Count - 1; j++)
            {
                var edge = visitedEdges.FirstOrDefault(e => e.IsBetweenVertices(path[j], path[j + 1]));

                // if (edge == null)
                // {
                //     edge = new LocalEdge(path[j], path[j + 1]);
                //     visitedEdges.Add(edge);
                // }

                edge.MarkAsVisited(path[j]);
            }
        }

        private static int AddVertexAtEvent(Vector3 location,
                                            List<Vector3> allNodeLocations,
                                            AdjacencyList<T> adj,
                                            T data,
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
        /// Draw the network as model arrows.
        /// </summary>
        /// <param name="nodeLocations">The locations of the network's nodes.</param>
        /// <param name="color">The color of the resulting model geometry.</param>
        public ModelArrows ToModelArrows(IList<Vector3> nodeLocations, Color? color)
        {
            var arrowData = new List<(Vector3 origin, Vector3 direction, double scale, Color? color)>();

            for (var i = 0; i < _adjacencyList.GetNumberOfVertices(); i++)
            {
                var start = nodeLocations[i];
                foreach (var end in _adjacencyList[i])
                {
                    var d = (nodeLocations[end.Item1] - start).Unitized();
                    var l = nodeLocations[end.Item1].DistanceTo(start);
                    arrowData.Add((start, d, l, color));
                }
            }

            return new ModelArrows(arrowData, arrowAngle: 75);
        }

        /// <summary>
        /// Draw the network as model curves.
        /// </summary>
        /// <param name="nodeLocations"></param>
        public List<ModelCurve> ToModelCurves(List<Vector3> nodeLocations)
        {
            var curves = new List<ModelCurve>();
            for (var i = 0; i < this.NodeCount(); i++)
            {
                var start = nodeLocations[i];
                foreach (var end in this.EdgesAt(i))
                {
                    curves.Add(new ModelCurve(new Line(start, nodeLocations[end.Item1]), BuiltInMaterials.YAxis));
                }
            }

            return curves;
        }

        /// <summary>
        /// Draw node indices and connected node indices at each node.
        /// </summary>
        /// <param name="nodeLocations">A collection of node locations.</param>
        /// <param name="color">The color of the model text.</param>
        public List<ModelText> ToModelText(List<Vector3> nodeLocations, Color color)
        {
            var textData = new List<(Vector3 location, Vector3 facingDirection, Vector3 lineDirection, string text, Color? color)>();
            var texts = new List<ModelText>();

            var count = NodeCount();
            for (var i = 0; i < NodeCount(); i++)
            {
                var start = nodeLocations[i];
                var indexStr = $"{i}: {string.Join(",", EdgesAt(i).Select(e => e.Item1.ToString()))}";
                textData.Add((nodeLocations[i], Vector3.ZAxis, Vector3.XAxis, indexStr, color));

                // Break up text data objects to avoid overflowing maximum
                // texture and geometry buffer sizes.
                if (textData.Count > 100 || i == count - 1)
                {
                    texts.Add(new ModelText(textData, FontSize.PT24));
                    textData = new List<(Vector3 location, Vector3 facingDirection, Vector3 lineDirection, string text, Color? color)>();
                }
            }

            return texts;
        }

        /// <summary>
        /// Draw bounded areas of the network as panels.
        /// </summary>
        /// <param name="allNodeLocations">All node locations in the network.</param>
        public List<Panel> ToBoundedAreaPanels(List<Vector3> allNodeLocations)
        {
            var regions = FindAllClosedRegions(allNodeLocations);
            var r = new Random();
            var panels = new List<Panel>();

            foreach (var region in regions)
            {
                var vertices = region.Select(i => allNodeLocations[i]).ToList();
                Polygon poly = null;
                try
                {
                    poly = new Polygon(vertices);
                }
                catch
                {
                    // This will happen for traversals of
                    // straight edges.
                    continue;
                }
                panels.Add(new Panel(poly, r.NextMaterial()));
            }

            return panels;
        }

        /// <summary>
        /// Traverse the network from the specified node index.
        /// Traversal concludes when there are no more
        /// available nodes to traverse.
        /// </summary>
        /// <param name="start">The starting point of the traversal.</param>
        /// <param name="next">The traversal step delegate.</param>
        /// <param name="allNodeLocations">A collection of all node locations in the network.</param>
        /// <param name="visitedEdges">A collection of all visited edges.</param>
        /// <param name="visited">A collection of visited node indices.</param>
        /// <returns>A list of indices of the traversed nodes.</returns>
        public List<int> Traverse(int start,
                                  Func<(int, int, IEnumerable<int>), List<Vector3>, List<LocalEdge>, int> next,
                                  List<Vector3> allNodeLocations,
                                  List<LocalEdge> visitedEdges,
                                  out List<int> visited,
                                  int prevIndex = -1)
        {
            var path = new List<int>();
            visited = new List<int>();
            var currentIndex = start;

            // Track the trailing edge from a specific index.
            // This will be used to compare traversal to avoid passing
            // over where the path has previously traveled.
            var lastIndexMap = new Dictionary<int, (int start, int end)>();

            if (prevIndex != -1)
            {
                // If a previous index has been supplied, we're starting from
                // an edge. Add the starting point of that edge to the path.
                path.Add(prevIndex);
                visited.Add(prevIndex);
            }

            while (currentIndex != -1)
            {
                path.Add(currentIndex);
                visited.Add(currentIndex);
                var oldIndex = currentIndex;
                currentIndex = Traverse(prevIndex, currentIndex, next, allNodeLocations, visitedEdges);
                prevIndex = oldIndex;

                // After at least one traversal step, if the current index
                // is the start, we've achieved a loop.
                if (currentIndex == start)
                {
                    break;
                }

                if (lastIndexMap.ContainsKey(currentIndex))
                {
                    var firstSegmentStart = lastIndexMap[currentIndex].start;
                    var firstSegmentEnd = lastIndexMap[currentIndex].end;

                    var secondSegmentStart = oldIndex;
                    var secondSegmentEnd = currentIndex;

                    // Check if the segments are the same.
                    if (firstSegmentStart == secondSegmentStart && firstSegmentEnd == secondSegmentEnd)
                    {
                        // Snip the "tail" by taking only everything up to the last segment.
                        path = path.Take(path.LastIndexOf(firstSegmentEnd)).ToList();
                        break;
                    }
                }

                if (lastIndexMap.ContainsKey(currentIndex))
                {
                    lastIndexMap[currentIndex] = (oldIndex, currentIndex);
                }
                else
                {
                    lastIndexMap.Add(currentIndex, (oldIndex, currentIndex));
                }
            }

            // Allow closing a loop.
            if (visited[0] == currentIndex)
            {
                path.Add(currentIndex);
            }

            return path;
        }

        private int Traverse(int prevIndex,
                             int currentIndex,
                             Func<(int, int, IEnumerable<int>), List<Vector3>, List<LocalEdge>, int> next,
                             List<Vector3> allNodeLocations,
                             List<LocalEdge> visitedEdges)
        {
            var edges = _adjacencyList[currentIndex];

            if (edges.Count == 0)
            {
                return -1;
            }

            if (edges.Count == 1)
            {
                if (edges.First.Value.Item1 == prevIndex)
                {
                    // Don't traverse backwards.
                    return -1;
                }

                if (edges.First.Value.Item1 != currentIndex)
                {
                    // If there's only one connected vertex and
                    // it's not the current vertex, return it.
                    return edges.First.Value.Item1;
                }
            }

            return next((currentIndex, prevIndex, edges.Select(e => e.Item1)), allNodeLocations, visitedEdges);
        }

        private static bool PointIsUniqueIntersectionAlongLine(Vector3 point, Line line, List<Vector3> intersections)
        {
            return !intersections.Any(p => p.IsAlmostEqualTo(point)) && line.PointOnLine(point);
        }
    }
}