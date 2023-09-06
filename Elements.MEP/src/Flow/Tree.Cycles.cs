using System.Collections.Generic;
using System.Linq;

namespace Elements.Flow
{
    public partial class Tree
    {
        /// <summary>
        /// Finds connections of cycles.
        /// </summary>
        /// <returns>List of all connections in all cycles.</returns>
        public List<Connection> FindAllConnectionsOfCycles()
        {
            return FindAllCyclesOfConnections().SelectMany(c => c).ToList();
        }

        /// <summary>
        /// Finds cycles of connections.
        /// List of returned cycles may not be complete and may include cycles that contain other cycles within them.
        /// But all possible cycles will be covered by resulting list of cycles.
        /// </summary>
        /// <returns>List of connections cycles.</returns>
        public List<List<Connection>> FindAllCyclesOfConnections()
        {
            var nodeCycles = FindAllNodesCycles();
            var connectionCycles = new List<List<Connection>>();
            foreach (var nodeCycle in nodeCycles)
            {
                var connectionCycle = new List<Connection>();
                for (var i = 0; i < nodeCycle.Count; i++)
                {
                    var firstNode = nodeCycle[i];
                    var secondNode = nodeCycle[(i + 1) % nodeCycle.Count];
                    var connection = GetOutgoingConnections(firstNode).FirstOrDefault(c => c.End == secondNode)
                                     ?? GetIncomingConnections(firstNode).FirstOrDefault(c => c.Start == secondNode);
                    connectionCycle.Add(connection);
                }

                connectionCycles.Add(connectionCycle);
            }

            return connectionCycles;
        }

        /// <summary>
        /// Finds cycles of nodes.
        /// List of returned cycles may not be complete and may include cycles that contain other cycles within them.
        /// But all possible cycles will be covered by resulting list of cycles.
        /// </summary>
        /// <returns>List of nodes cycles.</returns>
        public List<List<Node>> FindAllNodesCycles()
        {
            // 0 - WHITE - Vertex is not processed yet. Initially, all vertices are WHITE.
            // 1 - GRAY - Vertex is being processed (DFS for this vertex has started, but not finished which means that all descendants (in DFS tree) of this vertex are not processed yet (or this vertex is in the function call stack)
            // 2 - BLACK - Vertex and all its descendants are processed. While doing DFS, if an edge is encountered from current vertex to a GRAY vertex, then this edge is back edge and hence there is a cycle. 
            var colors = InternalNodes.ToDictionary(n => n, n => 0);
            colors.Add(Outlet, 0);
            foreach (var inlet in Inlets)
            {
                colors.Add(inlet, 0);
            }

            var nodeToParentLookup = new Dictionary<Node, Node>();
            var nodeCycles = new List<List<Node>>();
            RecursiveFindCycles(Outlet, null, colors, nodeToParentLookup, nodeCycles);
            return nodeCycles;
        }

        private void RecursiveFindCycles(Node currentNode,
                                         Node parentNode,
                                         Dictionary<Node, int> colors,
                                         Dictionary<Node, Node> nodeToParentLookup,
                                         List<List<Node>> nodeCycles)
        {
            // Already (completely) visited vertex.
            if (colors[currentNode] == 2)
            {
                return;
            }

            // Seen vertex, but was not completely visited -> cycle detected.
            // Backtrack based on parents to find the complete cycle.
            if (colors[currentNode] == 1)
            {
                var route = new List<Node>();
                var node = parentNode;
                route.Add(node);

                // Backtrack the vertex which are in the current cycle thats found
                while (node != currentNode)
                {
                    node = nodeToParentLookup[node];
                    route.Add(node);
                }
                nodeCycles.Add(route);
                return;
            }
            nodeToParentLookup[currentNode] = parentNode;

            // Partially visited.
            colors[currentNode] = 1;

            // Simple dfs on graph
            var connectedNodes = GetOutgoingConnections(currentNode).Select(c => c.End).ToList();
            connectedNodes.AddRange(GetIncomingConnections(currentNode).Select(c => c.Start));
            foreach (var v in connectedNodes)
            {
                // If it has not been visited previously
                if (v == nodeToParentLookup[currentNode])
                {
                    continue;
                }
                RecursiveFindCycles(v, currentNode, colors, nodeToParentLookup, nodeCycles);
            }

            // Completely visited.
            colors[currentNode] = 2;
        }
    }
}