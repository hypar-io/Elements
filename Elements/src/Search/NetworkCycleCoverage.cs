using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics;
using System.Linq;
using Elements.Geometry;
using Elements.Search;
using System.Collections.Immutable;

namespace Elements.Search
{
    internal class NetworkCycleCoverage
    {
        private readonly Dictionary<NetworkNode, List<NetworkEdge>> _adjacencyMatrix;
        public List<List<Vector3>> CyclesPoints { get; private set; }
        public List<List<int>> CyclesIndices { get; private set; }

        public NetworkCycleCoverage(Dictionary<int, List<int>> adjacencyMatrix, List<Vector3> allNodeLocations)
        {
            _adjacencyMatrix = CreateAdjacencyMatrixWithPositionInfo(adjacencyMatrix, allNodeLocations);
            RemoveLeafBranches();
            var cycles = FindAllCycles();
            CyclesPoints = new List<List<Vector3>>();
            CyclesIndices = new List<List<int>>();

            // TODO: CycleIndices are used to make the Network.FindAllClosedRegions compatible with outer code.
            // CyclesPoints is enough to build polygons in outer code.
            foreach (var cycle in cycles)
            {
                CyclesIndices.Add(cycle.Select(node => node.Id).ToList());
                CyclesPoints.Add(cycle.Select(node => node.Position).ToList());
            }
        }

        private List<List<NetworkNode>> FindAllCycles()
        {
            var regions = new List<List<NetworkNode>>();
            var nodes = _adjacencyMatrix.Keys.OrderBy(v => v.Position.X).ThenBy(v => v.Position.Y).ToList();

            // Traverse over all nodes. Edges found during the first
            // traversal path will be skipped during traversal.
            foreach (var node in nodes)
            {
                var localEdgeCount = _adjacencyMatrix[node].Count;
                if (localEdgeCount <= node.CountOfVisits)
                {
                    continue;
                }

                var path = TraversePath(node);

                if (path == null)
                {
                    continue;
                }

                // Add the visits to the corresponding nodes
                // to ensure that we don't re-traverse this loop.
                foreach (var pathNode in path)
                {
                    pathNode.MarkVisited();
                }

                regions.Add(path);
            }

            // Traverse any edges that haven't been traversed.
            // This can happen when a region is "captured" by surrounding
            // regions that have been traversed, leaving one region bounded
            // completely bounded except on one side.
            var unvisitedEdges = _adjacencyMatrix.Values.SelectMany(edges => edges).Distinct();
            foreach (var unvisitedEdge in unvisitedEdges)
            {
                var path = TraversePath(unvisitedEdge.Start, unvisitedEdge.End);
                if (path != null)
                {
                    regions.Add(path);
                }
            }

            return regions;
        }

        private List<NetworkNode> TraversePath(NetworkNode node, NetworkNode prevNode = null)
        {
            Debug.WriteLine($"STARTING PATH AT INDEX: {node.Id}");

            // TODO: visited is not used here.
            List<NetworkNode> path = Traverse(node, out List<NetworkNode> visited, prevNode);

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

        /// <summary>
        /// Traverse the network from the specified node.
        /// Traversal concludes when there are no more
        /// available nodes to traverse.
        /// </summary>
        /// <param name="startNode">The starting node of the traversal.</param>
        /// <param name="visited">A collection of visited node indices.</param>
        /// <param name="prevNode">The previous node found during the traversal</param>
        /// <returns>A list of the traversed nodes.</returns>
        public List<NetworkNode> Traverse(NetworkNode startNode,
                                  out List<NetworkNode> visited,
                                  NetworkNode prevNode = null)
        {
            var path = new List<NetworkNode>();
            visited = new List<NetworkNode>();
            var currNode = startNode;

            // Track the trailing edge from a specific node.
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

            while (currNode != null)
            {
                path.Add(currNode);
                visited.Add(currNode);
                var oldIndex = currNode;
                currNode = TraverseLargestPlaneAngle(prevNode, currNode);

                // TODO: If currentIndex is null, the cycle will break here
                // and cycle condition will never be checked.
                if (currNode == null)
                {
                    break;
                }

                prevNode = oldIndex;

                // After at least one traversal step, if the current node
                // is the start, we've achieved a loop.
                if (startNode.Equals(currNode))
                {
                    break;
                }

                if (lastIndexMap.ContainsKey(currNode))
                {
                    var firstSegmentStart = lastIndexMap[currNode].start;
                    var firstSegmentEnd = lastIndexMap[currNode].end;

                    var secondSegmentStart = oldIndex;
                    var secondSegmentEnd = currNode;

                    // Check if the segments are the same.
                    if (firstSegmentStart.Equals(secondSegmentStart) && firstSegmentEnd.Equals(secondSegmentEnd))
                    {
                        // Snip the "tail" by taking only everything up to the last segment.
                        path = path.Take(path.LastIndexOf(firstSegmentEnd)).ToList();
                        break;
                    }
                }

                if (lastIndexMap.ContainsKey(currNode))
                {
                    lastIndexMap[currNode] = (oldIndex, currNode);
                }
                else
                {
                    lastIndexMap.Add(currNode, (oldIndex, currNode));
                }
            }

            // Allow closing a loop.
            if (visited[0] == currNode)
            {
                path.Add(currNode);
            }

            return path;
        }

        /// <summary>
        /// Traverse a network following the largest plane angle between the current
        /// edge and the next candidate edge.
        /// </summary>
        /// <param name="prevNode">Previous traversed node.</param>
        /// <param name="currNode">Current node. Edges adjacent to this node will be considered as next edge candidates.</param>
        /// <returns>The next index to traverse.</returns>
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
                var angle = localEdgeDir.PlaneAngleTo(baseEdgeDir);

                // The angle of traversal is not actually zero here,
                // it's 180 (unless the path is invalid). We want to
                // ensure that traversal happens along the straight
                // edge if possible.
                if (angle == 0)
                {
                    angle = 180.0;
                }

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

        private static bool IsTooShort(List<NetworkNode> path)
        {
            return path.Count < 3;
        }

        private static bool IsNotClosed(List<NetworkNode> path)
        {
            return !path[0].Equals(path[path.Count - 1]);
        }

        private NetworkEdge GetEdge(NetworkNode node1, NetworkNode node2)
        {
            if (!(_adjacencyMatrix.ContainsKey(node1) && _adjacencyMatrix.ContainsKey(node2)))
            {
                Debug.Assert(false, "An attempt to get an edge that is adjacent to unexisting node.");
            }

            return _adjacencyMatrix[node1].First(e => e.IsAdjacentToNode(node2));
        }

        private void MarkVisitedEdges(List<NetworkNode> path)
        {
            for (int j = 0; j < path.Count - 1; j++)
            {
                var edge = GetEdge(path[j], path[j + 1]);

                edge.MarkAsVisited(path[j]);
            }
        }

        private static Dictionary<NetworkNode, List<NetworkEdge>> CreateAdjacencyMatrixWithPositionInfo(Dictionary<int, List<int>> adjacencyMatrix,
                                                                                                  List<Vector3> allNodeLocations)
        {
            var nodes = new List<NetworkNode>();

            for (int i = 0; i < allNodeLocations.Count; i++)
            {
                nodes.Add(new NetworkNode(i, allNodeLocations[i]));
            }

            var newAdjacencyMatrix = new Dictionary<NetworkNode, List<NetworkEdge>>();
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
                    newAdjacencyMatrix[nodes[neighbor]].Add(edge);
                }
            }

            return newAdjacencyMatrix;
        }

        private void RemoveNode(NetworkNode node)
        {
            if (!_adjacencyMatrix.ContainsKey(node))
            {
                Debug.Assert(false, "Trying to remove node that has already been removed.");
                return;
            }

            foreach (var edge in _adjacencyMatrix[node])
            {
                var neighborNode = edge.GetOppositeNode(node);
                _adjacencyMatrix[neighborNode].Remove(edge);

                if (!_adjacencyMatrix[neighborNode].Any())
                {
                    _adjacencyMatrix.Remove(neighborNode);
                }
            }

            _adjacencyMatrix.Remove(node);
        }

        private void RemoveLeafBranches()
        {
            var nodesToRemove = new Queue<NetworkNode>();

            foreach (var node in _adjacencyMatrix.Keys)
            {
                if (_adjacencyMatrix[node].Count == 1)
                {
                    nodesToRemove.Enqueue(node);
                }
            }

            while (nodesToRemove.Any())
            {
                NetworkNode currNode = nodesToRemove.Dequeue();

                if (!_adjacencyMatrix.ContainsKey(currNode))
                {
                    continue;
                }

                Debug.Assert(_adjacencyMatrix[currNode].Count == 1, "Node with more than 1 neighbor is considered as dead end.");

                var nextNode = _adjacencyMatrix[currNode].Select(e => e.GetOppositeNode(currNode)).Single();
                RemoveNode(currNode);

                if (!_adjacencyMatrix.ContainsKey(nextNode))
                {
                    continue;
                }

                var nextNeighbors = _adjacencyMatrix[nextNode].Select(e => e.GetOppositeNode(nextNode)).ToList();

                if (!nextNeighbors.Any())
                {
                    Debug.Assert(false, "Node with no neighbors.");
                    RemoveNode(nextNode);
                    continue;
                }

                if (nextNeighbors.Count == 1)
                {
                    nodesToRemove.Enqueue(nextNode);
                }
            }
        }

        public static NetworkCycleCoverage FromNetwork<T>(Network<T> network, List<Vector3> allNodeLocations)
        {
            var adjacencyMatrix = new Dictionary<int, List<int>>();

            for (int i = 0; i < allNodeLocations.Count; i++)
            {
                adjacencyMatrix.Add(i, network.EdgesAt(i).Select(e => e.Item1).ToList());
            }

            return new NetworkCycleCoverage(adjacencyMatrix, allNodeLocations);
        }
    }

    internal class NetworkNode
    {
        public int Id { get; private set; }
        public Vector3 Position { get; private set; }
        public int CountOfVisits { get; private set; }

        public NetworkNode(int id, Vector3 pos)
        {
            Id = id;
            Position = pos;
            CountOfVisits = 0;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is NetworkNode))
            {
                return false;
            }

            return Id == ((NetworkNode)obj).Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public void MarkVisited()
        {
            CountOfVisits++;
        }

        public override string ToString()
        {
            return $"{Id}: {Position}";
        }
    }

    internal class NetworkEdge
    {
        [Flags]
        internal enum VisitDirections
        {
            None,
            Straight,
            Opposite
        }

        public NetworkNode Start { get; private set; }
        public NetworkNode End { get; private set; }

        public NetworkEdge(NetworkNode start, NetworkNode end)
        {
            Start = start;
            End = end;
        }

        public NetworkNode GetOppositeNode(NetworkNode node)
        {
            if (node.Equals(Start))
            {
                return End;
            }

            if (node.Equals(End))
            {
                return Start;
            }

            Debug.Assert(false, "The node isn't an ending of an edge.");
            return null;
        }

        public Vector3 GetDirectionFrom(NetworkNode node)
        {
            if (node.Equals(Start))
            {
                return (End.Position - Start.Position).Unitized();
            }

            if (node.Equals(End))
            {
                return (Start.Position - End.Position).Unitized();
            }

            return new Vector3();
        }

        public bool IsAdjacentToNode(NetworkNode node)
        {
            return Start.Equals(node) || End.Equals(node);
        }

        /// <summary>
        /// Mark a vertex as having been visited from the specified index.
        /// </summary>
        /// <param name="start">The index of the vertex from which the edge is visited.</param>
        public void MarkAsVisited(NetworkNode start)
        {
            if (Start.Equals(start))
            {
                visitDirections |= VisitDirections.Straight;
            }
            else if (End.Equals(start))
            {
                visitDirections |= VisitDirections.Opposite;
            }
        }

        /// <summary>
        /// Is this edge visited from the provided vertex?
        /// </summary>
        /// <param name="node">The node from which the vertex is visited.</param>
        /// <returns>Returns true if the edge was visited from the vertex.</returns>
        public bool IsVisitedFromVertex(NetworkNode node)
        {
            if (Start.Equals(node))
            {
                return visitDirections.HasFlag(VisitDirections.Straight);
            }

            if (End.Equals(node))
            {
                return visitDirections.HasFlag(VisitDirections.Opposite);
            }

            return false;
        }

        internal VisitDirections visitDirections;

        public override string ToString()
        {
            return $"({Start.Id}; {End.Id})";
        }
    }
}
