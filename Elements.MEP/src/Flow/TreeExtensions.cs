using System;
using System.Collections.Generic;
using System.Linq;
using Elements.Geometry;
using Elements.Spatial.AdaptiveGrid;

namespace Elements.Flow
{
    public static class TreeExtensions
    {
        /// <summary>
        /// Connect inlets of the graph with the outlet using path on the grid.
        /// Positions of inlets are outlet are changed to how they are set in the graph.
        /// Inlets and outlet must be set in the tree.
        /// </summary>
        /// <param name="tree">Tree with inlets and outlet set.</param>
        /// <param name="adaptiveGrid">Grid that hold vertices and edges in the route.</param>
        /// <param name="route">Vertex to next vertex dictionary, end if next is null or negative.</param>
        /// <param name="failedLeafs">Tree inlets, that are not present in the grid or can't be routed to the trunk.
        /// All inlets are added here if trunk position is not present as a vertex in the grid.</param>
        /// <param name="loops">Additional paths connecting branches on the tree in loops.</param>
        /// <returns>True if all inlets can be connected to outlet using route.</returns>
        public static bool ConnectByAdaptiveGridRoute(
            this Tree tree,
            AdaptiveGrid adaptiveGrid,
            IDictionary<ulong, TreeNode> route,
            out List<Leaf> failedLeafs,
            List<List<Spatial.AdaptiveGrid.Vertex>> loops = null)
        {
            var connectionDictionary = new Dictionary<ulong, Connection>();
            failedLeafs = new List<Leaf>();

            if (!adaptiveGrid.TryGetVertexIndex(tree.Outlet.Position, out var outletId))
            {
                failedLeafs.AddRange(tree.Inlets);
                failedLeafs.ForEach(l => tree.Disconnect(tree.GetOutgoingConnection(l)));
                return false;
            }

            //Adaptive grid is originated from Grid2d that uses Intersect.
            //Since intersect uses quantized approach - it inevitably moves positions up to its tolerance.
            //Update the positions so the FittingTree has clean pipe directions.
            tree.Outlet.Position = adaptiveGrid.GetVertex(outletId).Point;

            //Starting from the closest inlet to exit, go from inlet, one by one.
            foreach (var inlet in tree.Inlets)
            {
                var tailConnection = tree.GetOutgoingConnection(inlet);

                if (!adaptiveGrid.TryGetVertexIndex(inlet.Position, out var id))
                {
                    failedLeafs.Add(inlet);
                    tree.Disconnect(tailConnection);
                    continue;
                }

                //Same as for outlet.
                inlet.Position = adaptiveGrid.GetVertex(id).Point;
                Connection previousConnection = null;

                //Inlet is not routed in the grid.
                if (!route.TryGetValue(id, out var treeNode))
                {
                    failedLeafs.Add(inlet);
                    tree.Disconnect(tailConnection);
                    continue;
                }

                //Until there is no next vertex found (reached outlet or other trunks).
                //previousConnection is pointing to the last section.
                //tailConnection is pointing to the connection from last section end to outlet.
                while (treeNode != null && treeNode.Trunk != null)
                {
                    var position = adaptiveGrid.GetVertex(treeNode.Trunk.Id).Point;

                    //When outlet is reached - we are done. Just need to check once again if last section
                    //is collinear with outlet and if so -merge the connection with the outlet.
                    if (treeNode.Trunk.Id == outletId)
                    {
                        if (previousConnection != null && Vector3.AreCollinearByAngle(
                            previousConnection.Start.Position, previousConnection.End.Position, position))
                        {
                            tree.ShiftConnectionToNode(previousConnection, tailConnection.End);
                            tree.Disconnect(tailConnection);
                            connectionDictionary[id] = previousConnection;
                        }
                        else
                        {
                            connectionDictionary[id] = tailConnection;
                        }
                        break;
                    }

                    //If next vertex already have connection no need to go further
                    if (connectionDictionary.TryGetValue(treeNode.Trunk.Id, out Connection other))
                    {
                        var node = other.Start;
                        //If connection point is in the middle of a trunk section -
                        //the section need to be split and connection dictionary updated
                        //for the end point.
                        if (!position.IsAlmostEqualTo(node.Position))
                        {
                            node = tree.SplitConnectionThroughPoint(
                                other, position, out var newConnections);
                            var left = newConnections.First();
                            var right = newConnections.Last();
                            connectionDictionary[treeNode.Trunk.Id] = right;
                            foreach (var pair in connectionDictionary.Where(kv => kv.Value == other).ToList())
                            {
                                var p = adaptiveGrid.GetVertex(pair.Key).Point;
                                var onLeftSide = Line.PointOnLine(p, left.Start.Position, left.End.Position, true);
                                connectionDictionary[pair.Key] = onLeftSide ? left : right;
                            }
                        }

                        if (previousConnection != null && Vector3.AreCollinearByAngle(
                            previousConnection.Start.Position, previousConnection.End.Position, position))
                        {
                            tree.Disconnect(tailConnection);
                            tree.ShiftConnectionToNode(previousConnection, node);
                            connectionDictionary[id] = previousConnection;
                        }
                        else
                        {
                            tree.ShiftConnectionToNode(tailConnection, node);
                            connectionDictionary[id] = tailConnection;
                        }
                        break;
                    }
                    else
                    {
                        //Otherwise we need to build next section of the tree.
                        //If next vertex is collinear with the last section - just move
                        //end points of previousConnection and tailConnection.
                        //Two collinear sections are not allowed in the tree.
                        //Otherwise we have a turn - tail connection is split through the point.
                        if (previousConnection != null && Vector3.AreCollinearByAngle(
                            previousConnection.Start.Position, previousConnection.End.Position, position))
                        {
                            previousConnection.End.Position = position;
                            tailConnection.Start.Position = position;
                        }
                        else
                        {
                            tree.SplitConnectionThroughPoint(
                                tailConnection, position, out var newConnections);
                            previousConnection = newConnections.First();
                            tailConnection = newConnections.Last();
                        }
                        connectionDictionary[id] = previousConnection;
                    }

                    id = treeNode.Trunk.Id;
                    treeNode = treeNode.Trunk;
                }

                //One or more inlets can't reach the outlet.
                if (treeNode == null || treeNode.Trunk == null)
                {
                    failedLeafs.Add(inlet);
                    tree.Disconnect(tailConnection);
                }
            }

            if (loops != null)
            {
                var routedEdges = route.Where(p => p.Value.Trunk != null).ToDictionary(p => p.Key, p => new List<ulong>() { p.Value.Trunk.Id });
                foreach (var loop in loops)
                {
                    tree.AddLoop(loop, routedEdges, connectionDictionary, adaptiveGrid);
                }
            }

            return !failedLeafs.Any();
        }

        private static void AddLoop(
            this Tree tree,
            List<Spatial.AdaptiveGrid.Vertex> loop,
            Dictionary<ulong, List<ulong>> routedEdges,
            Dictionary<ulong, Connection> connectionDictionary,
            AdaptiveGrid adaptiveGrid)
        {
            Spatial.AdaptiveGrid.Vertex startVertex = null;
            var pathInBetween = new List<Spatial.AdaptiveGrid.Vertex>();
            for (var i = 0; i < loop.Count; i++)
            {
                var vertex = loop[i];
                if (connectionDictionary.TryGetValue(vertex.Id, out var connection))
                {
                    if (i > 0 && startVertex == loop[i - 1])
                    {
                        if (!routedEdges[startVertex.Id].Any(trunkId => trunkId.Equals(vertex.Id))
                            && !routedEdges[vertex.Id].Any(trunkId => trunkId.Equals(startVertex.Id)))
                        {
                            tree.CreateLoop(
                                startVertex,
                                vertex,
                                connectionDictionary[startVertex.Id],
                                connection,
                                pathInBetween,
                                connectionDictionary,
                                adaptiveGrid);
                            AddRoutedPath(routedEdges, startVertex, pathInBetween, vertex);
                        }
                    }
                    else if (startVertex != null)
                    {
                        tree.CreateLoop(
                            startVertex,
                            vertex,
                            connectionDictionary[startVertex.Id],
                            connection,
                            pathInBetween,
                            connectionDictionary,
                            adaptiveGrid);
                        AddRoutedPath(routedEdges, startVertex, pathInBetween, vertex);
                    }
                    pathInBetween.Clear();
                    startVertex = vertex;
                }
                else
                {
                    pathInBetween.Add(vertex);
                }
            }

            tree.UpdateSections();
        }

        private static Node GetNodeForVertex(
            this Tree tree,
            Spatial.AdaptiveGrid.Vertex vertex,
            Connection connection,
            Dictionary<ulong, Connection> connectionDictionary,
            AdaptiveGrid adaptiveGrid)
        {
            Node node;
            if (connection.Start.Position.IsAlmostEqualTo(vertex.Point))
            {
                node = connection.Start;
            }
            else
            {
                node = tree.SplitConnectionThroughPoint(connection, vertex.Point, out var newConnections);

                var left = newConnections.First();
                var right = newConnections.Last();
                connectionDictionary[vertex.Id] = right;
                foreach (var pair in connectionDictionary.Where(kv => kv.Value == connection).ToList())
                {
                    var p = adaptiveGrid.GetVertex(pair.Key).Point;
                    var onLeftSide = Line.PointOnLine(p, left.Start.Position, left.End.Position, true);
                    connectionDictionary[pair.Key] = onLeftSide ? left : right;
                }
            }

            return node;
        }

        private static void CreateLoop(
            this Tree tree,
            Spatial.AdaptiveGrid.Vertex startVertex,
            Spatial.AdaptiveGrid.Vertex endVertex,
            Connection startConnection,
            Connection endConnection,
            List<Spatial.AdaptiveGrid.Vertex> pathInBetween,
            Dictionary<ulong, Connection> connectionDictionary,
            AdaptiveGrid adaptiveGrid)
        {
            Node startNode = tree.GetNodeForVertex(startVertex, startConnection, connectionDictionary, adaptiveGrid);
            Node endNode = tree.GetNodeForVertex(endVertex, endConnection, connectionDictionary, adaptiveGrid);
            var loopConnection = tree.AddLoopConnection(startNode, endNode);

            var currentConnection = loopConnection;
            for (int i = 0; i < pathInBetween.Count; i++)
            {
                if (Vector3.AreCollinearByAngle(currentConnection.Start.Position, currentConnection.End.Position, pathInBetween[i].Point))
                {
                    connectionDictionary[pathInBetween[i].Id] = currentConnection;
                    continue;
                }

                tree.SplitConnectionThroughPoint(currentConnection, pathInBetween[i].Point, out var createdConnections);
                if (i > 0)
                {
                    foreach (var vertex in pathInBetween.Take(i).Where(v => connectionDictionary[v.Id] == currentConnection))
                    {
                        connectionDictionary[vertex.Id] = createdConnections[0];
                    }
                }
                connectionDictionary[pathInBetween[i].Id] = createdConnections[1];

                currentConnection = createdConnections[1];
            }
        }

        private static void AddRoutedPath(Dictionary<ulong, List<ulong>> routedEdges, Spatial.AdaptiveGrid.Vertex startVertex, List<Spatial.AdaptiveGrid.Vertex> pathInBetween, Spatial.AdaptiveGrid.Vertex endVertex)
        {
            if (!pathInBetween.Any())
            {
                routedEdges[startVertex.Id].Add(endVertex.Id);
                return;
            }

            for (int i = 0; i < pathInBetween.Count; i++)
            {
                if (i == 0)
                {
                    routedEdges[startVertex.Id].Add(pathInBetween[i].Id);
                }
                else
                {
                    if (!routedEdges.TryGetValue(pathInBetween[i - 1].Id, out var trunkIds))
                    {
                        trunkIds = new List<ulong>();
                        routedEdges[pathInBetween[i - 1].Id] = trunkIds;
                    }
                    trunkIds.Add(pathInBetween[i].Id);
                }
                if (i == pathInBetween.Count - 1)
                {
                    if (!routedEdges.TryGetValue(pathInBetween[i].Id, out var trunkIds))
                    {
                        trunkIds = new List<ulong>();
                        routedEdges[pathInBetween[i].Id] = trunkIds;
                    }
                    trunkIds.Add(endVertex.Id);
                }
            }
        }
    }
}
