using Elements.Geometry;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Elements.Search
{
    internal class NetworkCycleCoverage<T>
    {
        private readonly AdjacencyList<T> _adjacencyList;
        private readonly Dictionary<NetworkNode, List<NetworkEdge>> _adjacencyMatrix;
        public List<List<int>> CyclesIndices { get; }

        public NetworkCycleCoverage(AdjacencyList<T> adjacencyList, List<Vector3> allNodeLocations)
        {
            _adjacencyMatrix = CreateAdjacencyMatrixWithPositionInfo(adjacencyList, allNodeLocations);
            _adjacencyList = adjacencyList;
            CyclesIndices = FindAllClosedRegions(allNodeLocations);
        }

        private List<List<int>> FindAllClosedRegions(List<Vector3> allNodeLocations)
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
            var unvisitedEdges = allEdges.Where(e => e.visitDirections == VisitDirections.None);
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

        private static int TraverseLargestPlaneAngle((int currentIndex, int previousIndex, IEnumerable<int> edgeIndices) traversalData,
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

        private List<int> Traverse(int start,
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

        private static Dictionary<NetworkNode, List<NetworkEdge>> CreateAdjacencyMatrixWithPositionInfo(AdjacencyList<T> adjacencyMatrix,
                                                                                                        List<Vector3> allNodeLocations)
        {
            var nodes = new List<NetworkNode>(allNodeLocations.Count);

            for (int i = 0; i < allNodeLocations.Count; i++)
            {
                nodes.Add(new NetworkNode(i, allNodeLocations[i]));
            }

            var newAdjacencyMatrix = new Dictionary<NetworkNode, List<NetworkEdge>>(allNodeLocations.Count);
            foreach (var node in nodes)
            {
                newAdjacencyMatrix[node] = new List<NetworkEdge>();
            }

            for (int i = 0; i < allNodeLocations.Count; i++)
            {
                foreach (var neighbor in adjacencyMatrix[i])
                {
                    if (i >= neighbor.Item1)
                    {
                        continue;
                    }

                    var edge = new NetworkEdge(nodes[i], nodes[neighbor.Item1]);
                    newAdjacencyMatrix[nodes[i]].Add(edge);
                    newAdjacencyMatrix[nodes[neighbor.Item1]].Add(edge);
                }
            }

            return newAdjacencyMatrix;
        }
    }
}
