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
