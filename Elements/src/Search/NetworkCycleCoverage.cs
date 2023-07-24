using Elements.Geometry;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Elements.Search
{
    internal class NetworkCycleCoverage
    {
        private readonly Dictionary<NetworkNode, List<NetworkEdge>> _adjacencyMatrix;
        public List<List<int>> CyclesIndices { get; }

        public NetworkCycleCoverage(Dictionary<int, List<int>> adjacencyMatrix, List<Vector3> allNodeLocations)
        {
            _adjacencyMatrix = CreateAdjacencyMatrixWithPositionInfo(adjacencyMatrix, allNodeLocations);
            var cycles = FindAllClosedRegionsTraversingEdges();
            CyclesIndices = new List<List<int>>();

            foreach (var cycle in cycles)
            {
                CyclesIndices.Add(cycle.Select(node => node.Id).ToList());
            }
        }

        private List<List<NetworkNode>> FindAllClosedRegions()
        {
            var regions = new List<List<NetworkNode>>();
            var leafNodes = new List<NetworkNode>();

            foreach (var node in _adjacencyMatrix.Keys)
            {
                var localEdges = _adjacencyMatrix[node];
                var edgeCount = localEdges.Count();
                // Leaf nodes
                if (edgeCount == 1)
                {
                    leafNodes.Add(node);
                }
            }

            // Traverse from leaves first. This will capture paths where
            // a leaf edge traverses into our out of a closed region.
            foreach (var leafNode in leafNodes)
            {
                var path = TraversePath(leafNode);
                if (path != null)
                {
                    regions.Add(path);
                }
            }

            // Traverse over all nodes. Edges found during the first
            // traversal path will be skipped during traversal.
            foreach (var node in _adjacencyMatrix.Keys)
            {
                var localEdgeCount = _adjacencyMatrix[node].Count();
                if (localEdgeCount > 1 && localEdgeCount > node.CountOfVisits)
                {
                    var path = TraversePath(node);

                    if (path != null)
                    {
                        // Add the visits to the corresponding nodes
                        // to ensure that we don't re-traverse this loop.
                        foreach (var pathNode in path)
                        {
                            pathNode.MarkVisited();
                        }

                        regions.Add(path);
                    }
                }
            }

            // Traverse any edges that haven't been traversed.
            // This can happen when a region is "captured" by surrounding
            // regions that have been traversed, leaving one region bounded
            // completely bounded except on one side.
            var allEdges = _adjacencyMatrix.Values.SelectMany(edges => edges).Distinct().ToList();
            var unvisitedEdges = allEdges.Where(e => e.visitDirections == VisitDirections.None);
            foreach (var unvisitedEdge in unvisitedEdges)
            {
                var path = TraversePath(unvisitedEdge.End, unvisitedEdge.Start);
                if (path != null)
                {
                    regions.Add(path);
                }
            }

            return regions;
        }

        private List<NetworkNode> TraversePath(NetworkNode currNode, NetworkNode prevNode = null)
        {
            Debug.WriteLine($"STARTING PATH AT INDEX: {currNode.Id}");

            List<NetworkNode> path = Traverse(currNode, out List<NetworkNode> visited, prevNode);

            if (IsNotClosed(path))
            {
                Debug.WriteLine($"EXITING NON CLOSED PATH");
                Debug.WriteLine(string.Empty);
                return null;
            }

            MarkVisitedEdges(path);

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

        private static bool IsTooShort(List<NetworkNode> path)
        {
            return path.Count < 3;
        }

        private static bool IsNotClosed(List<NetworkNode> path)
        {
            return !path[0].Equals(path[path.Count - 1]);
        }

        /// <summary>
        /// Traverse a network following the largest plane angle between the current
        /// edge and the next candidate edge.
        /// </summary>
        /// <param name="prevNode">Previous traversed node.</param>
        /// <param name="currNode">Current node. Edges adjacent to this node will be considered as next edge candidates.</param>
        /// <returns>The next node to traverse.</returns>
        private NetworkNode TraverseLargestPlaneAngle(NetworkNode prevNode, NetworkNode currNode)
        {
            var maxAngle = double.MinValue;
            NetworkNode maxAngleNode = null;
            var baseEdgeDir = prevNode == null ? Vector3.XAxis : (currNode.Position - prevNode.Position).Unitized();
            var edges = _adjacencyMatrix[currNode];
            foreach (var edge in edges)
            {
                var neighbor = edge.GetOppositeNode(currNode);

                if (neighbor.Equals(prevNode))
                {
                    Debug.WriteLine($"Skipping index {neighbor.Id} as previous.");
                    continue;
                }

                if (edge?.IsVisitedFromVertex(currNode) == true)
                {
                    Debug.WriteLine($"Skipping index {neighbor.Id} as visited.");
                    continue;
                }

                var localEdgeDir = edge.GetDirectionFrom(currNode);
                var angle = baseEdgeDir.Negate().PlaneAngleTo(localEdgeDir);

                Debug.WriteLine($"{currNode.Id}:{neighbor.Id}:{angle}");

                if (angle > maxAngle)
                {
                    Debug.WriteLine("Found maximum.");
                    maxAngle = angle;
                    maxAngleNode = neighbor;
                }
            }
            return maxAngleNode;
        }

        private List<List<NetworkNode>> FindAllClosedRegionsTraversingEdges()
        {
            var regions = new List<List<NetworkNode>>();

            var allEdges = _adjacencyMatrix.Values.SelectMany(lst => lst).Distinct().ToList();

            foreach (var startEdge in allEdges)
            {
                Debug.WriteLine($"STARTING PATH AT EDGE: {startEdge}");
                if (startEdge.IsVisited)
                {
                    Debug.WriteLine($"{startEdge} IS VISITED ALREADY. EXITING PATH.");
                    continue;
                }

                var closedRegion = new List<NetworkEdge>();
                var currEdge = startEdge;
                do
                {
                    closedRegion.Add(currEdge);
                    currEdge.IsVisited = true;
                    currEdge = TraverseLargestPlaneAngle(currEdge);
                } while (!currEdge.IsVisited && currEdge != startEdge);

                if (currEdge != startEdge)
                {
                    Debug.WriteLine($"{currEdge} IS VISITED ALREADY. EXITING PATH.");
                    continue;
                }

                var path = ToListOfNodes(closedRegion);
                regions.Add(path);
                Debug.WriteLine($"FOUND PATH: {string.Join(",", path)}");
                Debug.WriteLine(string.Empty);
            }

            return regions;
        }

        private static List<NetworkNode> ToListOfNodes(List<NetworkEdge> closedRegion)
        {
            var closedRegionNodes = new List<NetworkNode>
            {
                closedRegion.First().Start
            };
            closedRegionNodes.AddRange(closedRegion.Select(e => e.End).ToList());
            return closedRegionNodes;
        }

        private NetworkEdge TraverseLargestPlaneAngle(NetworkEdge edge)
        {
            var nextCandidates = _adjacencyMatrix[edge.End];
            var negatedDir = edge.Direction.Negate();

            double maxAngle = double.MinValue;
            NetworkEdge next = null;
            foreach (var candidate in nextCandidates)
            {
                double angle = negatedDir.PlaneAngleTo(candidate.Direction);

                Debug.WriteLine($"{candidate.Start.Id}:{candidate.End.Id}:{angle}");

                if (angle > maxAngle)
                {
                    Debug.WriteLine("Found maximum.");
                    maxAngle = angle;
                    next = candidate;
                }
            }

            // Make sure that there are no more edges that start from the same node
            // and have the same direction.
            Debug.Assert(nextCandidates.Where(candidate => negatedDir.PlaneAngleTo(candidate.Direction).ApproximatelyEquals(maxAngle)).Count() < 2);

            return next;
        }

        private void MarkVisitedEdges(List<NetworkNode> path)
        {
            for (int j = 0; j < path.Count - 1; j++)
            {
                var edge = GetEdge(path[j], path[j + 1]);
                edge.MarkAsVisited(path[j]);
            }
        }

        private NetworkEdge GetEdge(NetworkNode node1, NetworkNode node2)
        {
            if (!(_adjacencyMatrix.ContainsKey(node1) && _adjacencyMatrix.ContainsKey(node2)))
            {
                Debug.Assert(false, "An attempt to get an edge that is adjacent to an unexisting node.");
                return null;
            }

            return _adjacencyMatrix[node1].First(e => e.IsAdjacentToNode(node2));
        }

        /// <summary>
        /// Traverse the network from the specified node.
        /// Traversal concludes when there are no more
        /// available nodes to traverse.
        /// </summary>
        /// <param name="start">The starting node of the traversal.</param>
        /// <param name="visited">A collection of visited node indices.</param>
        /// <param name="prevNode">The previous node found during the traversal</param>
        /// <returns>A list of the traversed nodes.</returns>
        private List<NetworkNode> Traverse(NetworkNode start,
                                  out List<NetworkNode> visited,
                                  NetworkNode prevNode = null)
        {
            var path = new List<NetworkNode>();
            visited = new List<NetworkNode>();
            var currentNode = start;

            // Track the trailing edge from a specific index.
            // This will be used to compare traversal to avoid passing
            // over where the path has previously traveled.
            var lastIndexMap = new Dictionary<NetworkNode, (NetworkNode start, NetworkNode end)>();

            if (prevNode != null)
            {
                // If a previous index has been supplied, we're starting from
                // an edge. Add the starting point of that edge to the path.
                path.Add(prevNode);
                visited.Add(prevNode);
            }

            while (currentNode != null)
            {
                path.Add(currentNode);
                visited.Add(currentNode);
                var oldNode = currentNode;
                currentNode = Traverse(prevNode, currentNode);

                if (currentNode == null)
                {
                    break;
                }

                prevNode = oldNode;

                // After at least one traversal step, if the current index
                // is the start, we've achieved a loop.
                if (start.Equals(currentNode))
                {
                    break;
                }

                if (lastIndexMap.ContainsKey(currentNode))
                {
                    var firstSegmentStart = lastIndexMap[currentNode].start;
                    var firstSegmentEnd = lastIndexMap[currentNode].end;

                    var secondSegmentStart = oldNode;
                    var secondSegmentEnd = currentNode;

                    // Check if the segments are the same.
                    if (firstSegmentStart.Equals(secondSegmentStart) && firstSegmentEnd.Equals(secondSegmentEnd))
                    {
                        // Snip the "tail" by taking only everything up to the last segment.
                        path = path.Take(path.LastIndexOf(firstSegmentEnd)).ToList();
                        break;
                    }
                }

                if (lastIndexMap.ContainsKey(currentNode))
                {
                    lastIndexMap[currentNode] = (oldNode, currentNode);
                }
                else
                {
                    lastIndexMap.Add(currentNode, (oldNode, currentNode));
                }
            }

            // Allow closing a loop.
            if (visited[0].Equals(currentNode))
            {
                path.Add(currentNode);
            }

            return path;
        }

        private NetworkNode Traverse(NetworkNode prevNode, NetworkNode currentNode)
        {
            var edges = _adjacencyMatrix[currentNode];

            if (edges.Count == 0)
            {
                return null;
            }

            if (edges.Count == 1)
            {
                var opposite = edges[0].GetOppositeNode(currentNode);

                if (opposite.Equals(prevNode))
                {
                    // Don't traverse backwards.
                    return null;
                }

                if (!opposite.Equals(currentNode))
                {
                    // If there's only one connected vertex and
                    // it's not the current vertex, return it.
                    return opposite;
                }
            }

            return TraverseLargestPlaneAngle(prevNode, currentNode);
        }

        private static Dictionary<NetworkNode, List<NetworkEdge>> CreateAdjacencyMatrixWithPositionInfo(Dictionary<int, List<int>> adjacencyMatrix,
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
                    if (i >= neighbor)
                    {
                        continue;
                    }

                    var edge = new NetworkEdge(nodes[i], nodes[neighbor]);
                    newAdjacencyMatrix[nodes[i]].Add(edge);
                    newAdjacencyMatrix[nodes[neighbor]].Add(edge.Opposite);
                }
            }

            return newAdjacencyMatrix;
        }

        public static NetworkCycleCoverage FromNetwork<T>(Network<T> network, List<Vector3> allNodeLocations)
        {
            var adjacencyMatrix = new Dictionary<int, List<int>>();

            for (int i = 0; i < allNodeLocations.Count; i++)
            {
                adjacencyMatrix.Add(i, network.EdgesAt(i).Select(e => e.Item1).Distinct().ToList());
            }

            return new NetworkCycleCoverage(adjacencyMatrix, allNodeLocations);
        }
    }
}
