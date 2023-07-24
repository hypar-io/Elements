using Elements.Geometry;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
            RemoveCodirectionalEdges();
            var cycles = FindAllClosedRegionsTraversingEdges();
            CyclesIndices = new List<List<int>>();

            foreach (var cycle in cycles)
            {
                CyclesIndices.Add(cycle.Select(node => node.Id).ToList());
            }
        }

        // Sometimes several edges with the same start have the same direction.
        // In such cases the shortest edge stays, and other edges are replaced
        // with smaller edges between consecutive points.
        // For example:
        // A ----- B ----- C ----- D
        // There are edges AB, AC, AD. They will be replaced with AB, BC and CD.
        private void RemoveCodirectionalEdges()
        {
            foreach (var instance in _adjacencyMatrix)
            {
                var startNode = instance.Key;
                var sameDirEdgesGroups = GroupEdgesByDirection(instance.Value);

                foreach (var sameDirEdgesGroup in sameDirEdgesGroups)
                {
                    if (sameDirEdgesGroup.Value.Count < 2)
                    {
                        continue;
                    }

                    var edgesOrdered = sameDirEdgesGroup.Value.OrderBy(edge => (edge.End.Position - edge.Start.Position).LengthSquared()).ToList();

                    for (int i = 1; i < edgesOrdered.Count; i++)
                    {
                        var currEdge = edgesOrdered[i];
                        var currNode = currEdge.End;

                        var prevEdge = edgesOrdered[i - 1];
                        var prevNode = prevEdge.End;

                        _adjacencyMatrix[startNode].Remove(currEdge);
                        _adjacencyMatrix[currNode].Remove(currEdge.Opposite);

                        if (!_adjacencyMatrix[prevNode].Where(e => e.IsAdjacentToNode(currNode)).Any())
                        {
                            var newEdge = new NetworkEdge(prevNode, currNode);
                            _adjacencyMatrix[prevNode].Add(newEdge);
                            _adjacencyMatrix[currNode].Add(newEdge.Opposite);
                        }
                    }
                }
            }
        }

        private static Dictionary<Vector3, List<NetworkEdge>> GroupEdgesByDirection(List<NetworkEdge> edges)
        {
            var result = new Dictionary<Vector3, List<NetworkEdge>>();

            foreach (var edge in edges)
            {
                if (!result.Where(p => p.Key.IsAlmostEqualTo(edge.Direction)).Any())
                {
                    result.Add(edge.Direction, new List<NetworkEdge>() { edge });
                    continue;
                }

                var foundDir = result.Keys.First(v => v.IsAlmostEqualTo(edge.Direction));
                result[foundDir].Add(edge);
            }

            return result;
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

                if (IsBacktrackingPath(closedRegion))
                {
                    Debug.WriteLine($"THE PATH IS NOT A CLOSED REGION. EXITING PATH.");
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

        private static bool IsBacktrackingPath(List<NetworkEdge> cycle)
        {
            if (cycle.Count % 2 == 1)
            {
                return false;
            }

            var edgesSet = cycle.ToImmutableHashSet();

            foreach (var edge in cycle)
            {
                if (!edgesSet.Contains(edge.Opposite))
                {
                    return false;
                }
            }

            return true;
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
