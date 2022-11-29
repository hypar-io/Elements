using System;
using System.Collections.Generic;
using Elements.Spatial.AdaptiveGrid;
using Elements.Geometry;
using System.Linq;
using Elements.Geometry.Solids;
using Elements.Annotations;
using System.Text;

namespace Elements.Spatial.AdaptiveGrid
{
    /// <summary>
    /// Class for routing through an AdaptiveGrid.
    /// </summary>
    public partial class AdaptiveGraphRouting
    {
        private AdaptiveGrid _grid;
        private RoutingConfiguration _configuration;

        private const double NOT_CONNECTED = double.PositiveInfinity;
        private const double CONDITIONS_NOT_MET = double.MaxValue;

        /// <summary>
        /// Filter function definition.
        /// </summary>
        /// <param name="start">Last Vertex in the route.</param>
        /// <param name="end">Candidate for the next Vertex in the route.</param>
        /// <returns></returns>
        public delegate bool RoutingFilter(Vertex start, Vertex end);
        private List<RoutingFilter> _filters = new List<RoutingFilter>();

        /// <summary>
        /// List of error messages that occur during routing.
        /// Are reset on the beginning of each routing operation.
        /// </summary>
        public List<Message> ErrorMessages = new List<Message>();

        /// <summary>
        /// Create AdaptiveGraphRouting objects and store core parameters for further use.
        /// </summary>
        /// <param name="grid">AdaptiveGrid the algorithm travels through.</param>
        /// <param name="configuration">Storage for common parameters that affect routing.</param>
        public AdaptiveGraphRouting(AdaptiveGrid grid, RoutingConfiguration configuration)
        {
            //TO DO. The process of using AdaptiveGrid/Routing pair is hard, see AdaptiveGraphRoutingTests.
            //You need to collect points manually from hint lines, obstacles, then you need to get ids
            //to pass them to routing still maintaining connection between positions and ids.
            //Need to consider extra level of abstraction on top, that will do all the work based on
            //global information line boundaries, points, lines and obstacles.
            _grid = grid;
            _configuration = configuration;

            foreach (var modifier in _configuration.WeightModifiers)
            {
                _weightModifiers.Add(modifier.Name, modifier);
            }
        }

        /// <summary>
        /// Routing supports checking if a Vertex can be added to the path.
        /// New vertex must pass all filter functions to be accepted.
        /// </summary>
        /// <param name="f">New filter function.</param>
        public void AddRoutingFilter(RoutingFilter f)
        {
            _filters.Add(f);
        }

        /// <summary>
        /// Visualize adaptive graph edges.
        /// Material depends on elevation and proximity to different hint lines.
        /// Original split points are not stored in the graph, so they need to be provided.
        /// </summary>
        /// <param name="hintLines">List of hint lines.</param>
        /// <param name="splitPoints">List of split points to visualize</param>
        /// <returns>List of graphics elements</returns>
        public IList<Element> RenderElements(IList<RoutingHintLine> hintLines,
                                             IList<Vector3> splitPoints)
        {
            var normalEdgesCheap = new List<(Line, double)>();
            var hintEdgesCheap = new List<(Line, double)>();
            var offsetEdgesCheap = new List<(Line, double)>();
            var normalEdgesExpensive = new List<(Line, double)>();
            var hintEdgesExpensive = new List<(Line, double)>();
            var offsetEdgesExpensive = new List<(Line, double)>();

            var infos = CalculateEdgeInfos(hintLines);

            var hintGroups = hintLines.GroupBy(h => h.UserDefined);
            var userHints = hintGroups.SingleOrDefault(hg => hg.Key == true);
            var defaultHints = hintGroups.SingleOrDefault(hg => hg.Key == false);

            foreach (var edge in _grid.GetEdges())
            {
                //There is only one edge for vertex pair, if this is changed -
                //we will need to check edges for uniqueness,
                var v0 = _grid.GetVertex(edge.StartId);
                var v1 = _grid.GetVertex(edge.EndId);
                Line l = new Line(v0.Point, v1.Point);

                var info = infos[edge.Id];
                if (info.HasAnyFlag(EdgeFlags.UserDefinedHint))
                {
                    if (info.Factor < 1 + _grid.Tolerance)
                    {
                        hintEdgesCheap.Add((l, info.Factor));
                    }
                    else
                    {
                        hintEdgesExpensive.Add((l, info.Factor));
                    }
                }
                else if (info.HasAnyFlag(EdgeFlags.HiddenHint))
                {
                    if (info.Factor < 1 + _grid.Tolerance)
                    {
                        offsetEdgesCheap.Add((l, info.Factor));
                    }
                    else
                    {
                        offsetEdgesExpensive.Add((l, info.Factor));
                    }
                }
                else
                {
                    if (info.Factor < 1 + _grid.Tolerance)
                    {
                        normalEdgesCheap.Add((l, info.Factor));
                    }
                    else
                    {
                        normalEdgesExpensive.Add((l, info.Factor));
                    }
                }
            }

            List<Element> visualizations = new List<Element>();
            visualizations.Add(VisualizePoints(splitPoints));

            var add = new Action<List<(Line, double)>, string, Color>((list, name, c) =>
            {
                var groups = list.GroupBy(i => i.Item2);
                foreach (var g in groups)
                {
                    var lines = g.Select(l => l.Item1).ToList();
                    var model = new ModelLines(lines, new Material(name, c));
                    model.AdditionalProperties["Factor"] = g.Key;
                    visualizations.Add(model);

                }
            });

            add(normalEdgesCheap, "Normal Edges Main", Colors.Blue);
            add(normalEdgesExpensive, "Normal Edges Other", Colors.Cobalt);
            add(offsetEdgesCheap, "Offset Edges Main", Colors.Orange);
            add(offsetEdgesExpensive, "Offset Edges Other", Colors.Yellow);
            add(hintEdgesCheap, "Hint Edges Main", Colors.Green);
            add(hintEdgesExpensive, "Hint Edges Other", Colors.Emerald);

            return visualizations;
        }

        /// <summary>
        /// Creates tree of routes between set of input Vertices and the exit Vertex.
        /// Routes merge together to form a single trunk. Starting from end, point by point,
        /// vertices are connected to the network using Dijkstra algorithm.
        /// </summary>
        /// <param name="leafVertices">Vertices to connect into the system with extra information attached.</param>
        /// <param name="trunkVertex">End vertex id.</param>
        /// <param name="hintLines">Collection of lines that routes are attracted to. At least one hint line is required.</param>
        /// <param name="order">In which order tree is constructed</param>
        /// <returns>Travel tree from inputVertices to the trunkVertex.</returns>
        public IDictionary<ulong, TreeNode> BuildSpanningTree(
            IList<RoutingVertex> leafVertices,
            ulong trunkVertex,
            IEnumerable<RoutingHintLine> hintLines,
            TreeOrder order)
        {
            ErrorMessages.Clear();
            //Excluded vertices includes inlets and vertices in certain distance around these inlets.
            //Sometimes it's not desirable for routing to go through them.
            var excludedVertices = ExcludedVertices(leafVertices);
            var allExcluded = new HashSet<ulong>();
            foreach (var item in excludedVertices)
            {
                item.Value.ForEach(v => allExcluded.Add(v));
            }

            var weights = CalculateEdgeInfos(hintLines);

            var leafsToTrunkTree = new Dictionary<ulong, TreeNode>();
            leafsToTrunkTree[trunkVertex] = new TreeNode(trunkVertex);

            var userHints = hintLines.Where(h => h.UserDefined);
            var hintVertices = NearbyVertices(userHints, leafVertices);
            //Hint lines can even go through excluded vertices
            allExcluded.ExceptWith(hintVertices.Select(hv => hv.Id));

            List<ulong> leafTerminals = leafVertices.Select(lv => lv.Id).ToList();
            RouteBranch(leafTerminals, trunkVertex, allExcluded, excludedVertices,
                weights, order, leafsToTrunkTree);

            return leafsToTrunkTree;
        }

        /// <summary>
        /// Creates tree of routes between multiple sections, each having a set of input Vertices,
        /// hint lines and local end Vertex, and the exit Vertex.
        /// Route is created by using Dijkstra algorithm locally on different segments.
        /// Segments are merged together to form a single trunk. Starting from end, point by point,
        /// segments are connected. Then, Vertices in each segments are connected as well,
        /// forming a local trunk, connected with the main one.
        /// All parameter except "trunkPathVertices" are provided per section in the same order.
        /// </summary>
        /// <param name="leafVertices">Vertices to connect into the system with extra information attached.</param>
        /// <param name="trunkVertex">End vertex id.</param>
        /// <param name="hintLines">Collection of lines that routes are attracted to. At least one hint line per group is required.</param>
        /// <param name="order">In which order tree is constructed</param>
        /// <returns>Travel tree from inputVertices to the trunkVertex.</returns>
        public IDictionary<ulong, TreeNode> BuildSpanningTree(
            IList<List<RoutingVertex>> leafVertices,
            ulong trunkVertex,
            IList<List<RoutingHintLine>> hintLines,
            TreeOrder order)
        {
            ErrorMessages.Clear();
            var allLeafs = leafVertices.SelectMany(l => l).ToList();
            var allHints = hintLines.SelectMany(h => h).ToList();

            //Excluded vertices includes inlets and vertices in certain around inlets
            //Sometimes it's not desirable for routing to go through them.
            var excludedVertices = ExcludedVertices(allLeafs);
            var allExcluded = new HashSet<ulong>();
            foreach (var item in excludedVertices)
            {
                item.Value.ForEach(v => allExcluded.Add(v));
            }

            var weights = CalculateEdgeInfos(allHints);
            var allUserHints = allHints.Where(h => h.UserDefined == true);
            var nearbyHints = NearbyVertices(allUserHints, allLeafs);
            //Hint lines can even go through excluded vertices
            allExcluded.ExceptWith(nearbyHints.Select(nh => nh.Id));

            var leafsToTrunkTree = new Dictionary<ulong, TreeNode>();
            leafsToTrunkTree[trunkVertex] = new TreeNode(trunkVertex);

            //Route tree branch independently for each input section
            for (int i = 0; i < leafVertices.Count; i++)
            {
                List<ulong> leafTerminals = leafVertices[i].Select(lv => lv.Id).ToList();
                RouteBranch(leafTerminals, trunkVertex, allExcluded, excludedVertices,
                    weights, order, leafsToTrunkTree);
            }

            return leafsToTrunkTree;
        }

        /// <summary>
        /// Create network of routes between set of input Vertices and set of exit Vertices.
        /// Each route is most efficient individually, without considering other routes.
        /// Each route is connected to the network using Dijkstra algorithm.
        /// </summary>
        /// <param name="leafVertices">Vertices to connect into the system with extra information attached.</param>
        /// <param name="exits">Possible exit vertices.</param>
        /// <param name="hintLines">Collection of lines that routes are attracted to.</param>
        /// <returns>Travel tree from inputVertices to one of the exits.</returns>
        public IDictionary<ulong, TreeNode> BuildSimpleNetwork(
            IList<RoutingVertex> leafVertices,
            IList<ulong> exits,
            IEnumerable<RoutingHintLine> hintLines = null)
        {
            ErrorMessages.Clear();
            //Excluded vertices includes inlets and vertices in certain distance around these inlets.
            //Sometimes it's not desirable for routing to go through them.
            var excludedVertices = ExcludedVertices(leafVertices);
            var allExcluded = new HashSet<ulong>();
            foreach (var item in excludedVertices)
            {
                item.Value.ForEach(v => allExcluded.Add(v));
            }

            var weights = CalculateEdgeInfos(hintLines);

            var leafsToTrunkTree = new Dictionary<ulong, TreeNode>();
            foreach (var trunk in exits)
            {
                leafsToTrunkTree[trunk] = new TreeNode(trunk);
            }

            if (hintLines != null && hintLines.Any())
            {
                var hintGroups = hintLines.GroupBy(h => h.UserDefined);
                var userHints = hintGroups.SingleOrDefault(hg => hg.Key == true);
                var hintVertices = NearbyVertices(userHints, leafVertices);
                //Hint lines can even go through excluded vertices
                allExcluded.ExceptWith(hintVertices.Select(hv => hv.Id));
            }

            foreach (var inlet in leafVertices)
            {
                var excluded = FilteredSet(allExcluded, excludedVertices[inlet.Id]);
                var connections = ShortestPathDijkstra(
                    inlet.Id, weights, out var travelCost, excluded: excluded);
                var bestCost = NOT_CONNECTED;
                var exit = FindConnectionPoint(exits, travelCost, ref bestCost);
                if (CheckAccessability(inlet.Id, bestCost, allExcluded.Any()))
                {
                    var path = GetPathTo(connections, exit);
                    AddPathToTree(inlet.Id, path, leafsToTrunkTree);
                }
            }

            return leafsToTrunkTree;
        }

        private void RouteBranch(
            List<ulong> leafTerminals,
            ulong trunkTerminal,
            HashSet<ulong> allExcluded,
            Dictionary<ulong, List<ulong>> excludedPerVertex,
            Dictionary<ulong, EdgeInfo> weights,
            TreeOrder order,
            Dictionary<ulong, TreeNode> leafsToTrunkTree)
        {
            //Join all individual pieces together. We start from a single connection
            //path from drop pipe and a set of connection points from the previous step.
            //One at a time we choose the connection point that is cheapest to travel to existing
            //network and its path is added to the network until all are added.
            //Magnet terminals are all of the vertices that are currently part of the tree.
            //They are used as the list of good destinations while routing the leaves so that we join the tree as soon as possible."
            HashSet<ulong> magnetTerminals = new HashSet<ulong> { trunkTerminal };

            var terminalInfo = new Dictionary<ulong, (
                Dictionary<ulong, ((ulong, BranchSide), (ulong, BranchSide))> Connections,
                Dictionary<ulong, (double, double)> Costs)>();
            List<ulong> validLeafTerminals = new List<ulong>();
            foreach (var inlet in leafTerminals)
            {
                var excluded = FilteredSet(allExcluded, excludedPerVertex[inlet]);
                //Allow travel through excluded vertices of inlet only if it not yet left it's zone
                var connections = ShortestBranchesDijkstra(inlet, weights,
                    out var travelCost, null, excluded);

                if (CheckAccessability(inlet, travelCost[trunkTerminal].Item1, allExcluded.Any()))
                {
                    terminalInfo[inlet] = (connections, travelCost);
                    validLeafTerminals.Add(inlet);
                }
            }

            List<ulong> appliedLeafs = new List<ulong>();

            //Distances are precomputed beforehand to avoid square complexity on Dijkstra algorithm.
            //When terminal is connected to the trunk - we can choose one of two routing options,
            //considering also if any of them need extra turn when connected.
            while (validLeafTerminals.Any())
            {
                ulong closestTerminal = 0;
                List<ulong> path = null;
                double bestCost = order == TreeOrder.FurthestToClosest ? double.NegativeInfinity : double.PositiveInfinity;
                foreach (var terminal in validLeafTerminals)
                {
                    var info = terminalInfo[terminal];
                    var (localClosest, branch) = FindConnectionPoint(
                        magnetTerminals, info.Costs, info.Connections, leafsToTrunkTree, weights);
                    var costs = info.Costs[localClosest];
                    var localBestCost = branch == BranchSide.Left ? costs.Item1 : costs.Item2;

                    if (order == TreeOrder.FurthestToClosest ? localBestCost > bestCost : localBestCost < bestCost)
                    {
                        path = GetPathTo(info.Connections, localClosest, branch);
                        closestTerminal = terminal;
                        bestCost = localBestCost;
                    }
                }

                if (path != null && closestTerminal != 0)
                {
                    path.ForEach(p => magnetTerminals.Add(p));
                    AddPathToTree(closestTerminal, path, leafsToTrunkTree);
                    validLeafTerminals.Remove(closestTerminal);
                    appliedLeafs.Add(closestTerminal);
                }
            }

            //Find closest connection from leafs once again.
            //New trunk sections were added after all leafs were routed first time.
            //Old connection point is first node from leaf that has more than 2 connections.
            //Leafs are reconnected in reverse order of which they were added to the tree, skipping last one.
            foreach (var leaf in appliedLeafs.Reverse<ulong>().Skip(1))
            {
                var leafNode = leafsToTrunkTree[leaf];
                var leafPath = PathToFirstBranching(leafNode);
                var localMagnets = magnetTerminals.Except(leafPath.Select(l => l.Id));
                var info = terminalInfo[leaf];
                var (localClosest, branchSide) = FindConnectionPoint(
                    localMagnets, info.Costs, info.Connections, leafsToTrunkTree, weights);
                var lastSnap = leafPath.Last().Trunk == null ? leafPath.Last().Id : leafPath.Last().Trunk.Id;
                if (localClosest != lastSnap)
                {
                    //If better connection if found - remove old path from tree.
                    foreach (var node in leafPath)
                    {
                        magnetTerminals.Remove(node.Id);
                        node.Disconnect();
                        leafsToTrunkTree.Remove(node.Id);
                    }

                    var path = GetPathTo(info.Connections, localClosest, branchSide);
                    path.ForEach(p => magnetTerminals.Add(p));
                    AddPathToTree(leaf, path, leafsToTrunkTree);
                }
            }
        }

        /// <summary>
        /// Calculate weight for each edge in the graph. Information is stored as length
        /// of edge and extra factor that encourage or discourage traveling trough it.
        /// Edges that are not on main elevation have bigger factor.
        /// Edges that are near hint lines have smaller factor.
        /// Different factors are combined together.
        /// Also some edges are not allowed at all by setting factor to infinity.
        /// </summary>
        /// <param name="hintLines">Lines that affect travel factor for edges</param>
        /// <returns>For each edge - its precalculated additional information.</returns>
        private Dictionary<ulong, EdgeInfo> CalculateEdgeInfos(
            IEnumerable<RoutingHintLine> hintLines)
        {
            var weights = new Dictionary<ulong, EdgeInfo>();
            var mainAxis = _grid.Transform.XAxis;
            foreach (var e in _grid.GetEdges())
            {
                var v0 = _grid.GetVertex(e.StartId);
                var v1 = _grid.GetVertex(e.EndId);
                var vector = (v1.Point - v0.Point);
                var angle = vector.AngleTo(mainAxis);
                if (angle > 90)
                {
                    angle = 180 - angle;
                }

                EdgeFlags flags = EdgeFlags.None;

                if (_configuration.SupportedAngles != null &&
                    !_configuration.SupportedAngles.Any(a => a.ApproximatelyEquals(angle, 0.01)))
                {
                    weights[e.Id] = new EdgeInfo(_grid, e, CONDITIONS_NOT_MET);
                }
                else
                {
                    double hintFactor = 1;
                    double offsetFactor = 1;
                    double modifierFactor = ModifierFactor(v0, v1);

                    //TODO: consider unifying hint line, offset line and modifiers as single concept.
                    //There will still be functions for adding hint/offset lines but everything will be processed inside
                    //as WeightModifier. We would need to decide on the function that takes a list of these weight modifier
                    //groups and defines how multiple factors are combined together: by choosing one, combining, etc.
                    if (hintLines != null && hintLines.Any())
                    {
                        foreach (var l in hintLines)
                        {
                            if (IsAffectedBy(v0.Point, v1.Point, l))
                            {
                                //If user defined and default hints are overlapped,
                                //we want path to be aligned with default hints.
                                //To achieve this to factors are combined.
                                if (l.UserDefined)
                                {
                                    hintFactor = Math.Min(l.Factor, hintFactor);
                                    //Store the information if the edge was affected by 2D and (or) 3D hint line.
                                    flags &= l.Is2D ? EdgeFlags.UserDefinedHint2D : EdgeFlags.UserDefinedHint3D;
                                }
                                else
                                {
                                    offsetFactor = Math.Min(l.Factor, offsetFactor);
                                    flags &= l.Is2D ? EdgeFlags.HiddenHint2D : EdgeFlags.HiddenHint3D;
                                }
                            }
                        }
                    }

                    weights[e.Id] = new EdgeInfo(_grid, e, hintFactor * offsetFactor * modifierFactor);
                }
            }

            return weights;
        }

        /// <summary>
        /// Collect vertices that are less than configured Manhattan distance
        /// away from given inlets, ignoring Z difference.
        /// </summary>
        /// <returns></returns>
        private Dictionary<ulong, List<ulong>> ExcludedVertices(
            IList<RoutingVertex> inletTerminals)
        {
            var excludedVertices = new Dictionary<ulong, List<ulong>>();
            foreach (var inlet in inletTerminals)
            {
                var ip = _grid.GetVertex(inlet.Id).Point;
                var set = new List<ulong>();
                excludedVertices[inlet.Id] = set;
                if (inlet.IsolationRadius.ApproximatelyEquals(0))
                {
                    continue;
                }

                var adjustedTolerance = inlet.IsolationRadius - Vector3.EPSILON;
                foreach (var v in _grid.GetVertices())
                {
                    var p = v.Point;
                    var mhDistance2D = Math.Abs(p.X - ip.X) + Math.Abs(p.Y - ip.Y);
                    if (mhDistance2D < adjustedTolerance)
                    {
                        set.Add(v.Id);
                    }
                }
            }
            return excludedVertices;
        }

        /// <summary>
        /// This is a Dijkstra algorithm implementation.
        /// The algorithm travels from start point to all other points, gathering travel cost.
        /// Each time route turns - extra penalty is added to the cost.
        /// Higher level algorithm then decides which one of them to use as an end point.
        /// </summary>
        /// <param name="start">Start Vertex</param>
        /// <param name="edgeInfos">Dictionary of Edge Id to precomputed information about it</param>
        /// <param name="travelCost">Output dictionary where traveling cost is stored per Vertex</param>
        /// <param name="startDirection">Previous Vertex, if start Vertex is already part of the Route</param>
        /// <param name="excluded">Vertices that are not allowed to visit</param>
        /// <param name="pathDirections">Next Vertex dictionary for Vertices that are already part of the route</param>
        /// <returns>Dictionary that have travel routes from each Vertex back to start Vertex.</returns>
        public Dictionary<ulong, ulong> ShortestPathDijkstra(
            ulong start, Dictionary<ulong, EdgeInfo> edgeInfos,
            out Dictionary<ulong, double> travelCost,
            ulong? startDirection = null, HashSet<ulong> excluded = null,
            Dictionary<ulong, ulong?> pathDirections = null)
        {
            PriorityQueue<ulong> pq = PreparePriorityQueue(
                start, out Dictionary<ulong, ulong> path, out travelCost);

            while (!pq.Empty())
            {
                //At each step retrieve the vertex with the lowest travel cost and
                //remove it, so it can't be visited again.
                ulong u = pq.PopMin();

                //All vertices that can be reached from start vertex are visited.
                //Ignore once only unreachable are left.
                var cost = travelCost[u];
                if (cost == NOT_CONNECTED)
                {
                    break;
                }

                var beforeId = path[u];
                var vertex = _grid.GetVertex(u);

                foreach (var e in vertex.Edges)
                {
                    //Don't go back to where we just came from.
                    //If vertex is removed from queue - best path is already found for it.
                    var id = e.StartId == u ? e.EndId : e.StartId;
                    if (beforeId == id || !pq.Contains(id))
                    {
                        continue;
                    }

                    var edgeInfo = edgeInfos[e.Id];
                    var bestCost = travelCost[id];

                    //Edge or start vertex is not traversable.
                    if (edgeInfo.Factor == CONDITIONS_NOT_MET || cost == CONDITIONS_NOT_MET)
                    {
                        MarkExpensiveRoute(pq, travelCost, path, id, u, bestCost);
                        continue;
                    }

                    //Vertex is excluded or shortest path is already found.
                    if ((excluded != null && excluded.Contains(id)))
                    {
                        MarkExpensiveRoute(pq, travelCost, path, id, u, bestCost);
                        continue;
                    }

                    var v = _grid.GetVertex(id);
                    //Check if edge passes user defined filter functions
                    if (_filters.Any(f => !f(vertex, v)))
                    {
                        MarkExpensiveRoute(pq, travelCost, path, id, u, bestCost);
                        continue;
                    }

                    //Compute cost of each its neighbors as cost of vertex we came from plus cost of edge.
                    var newWeight = travelCost[u] + EdgeCost(edgeInfo);

                    //We need as little change of direction as possible. A penalty is added if
                    //a) We have a turn traveling to the next vertex.
                    //b) We just started and going in direction different than how we arrived from the previous segment.
                    //c) We arrived at a vertex that is used in a different path.
                    if (u == start)
                    {
                        if (startDirection.HasValue &&
                            !Vector3.AreCollinearByAngle(_grid.GetVertex(startDirection.Value).Point, vertex.Point, v.Point))
                        {
                            newWeight += TurnCost(edgeInfo, vertex, startDirection.Value, edgeInfos);
                        }
                    }
                    else
                    {
                        var vertexBefore = _grid.GetVertex(beforeId);
                        if (!Vector3.AreCollinearByAngle(vertexBefore.Point, vertex.Point, v.Point))
                        {
                            newWeight += TurnCost(edgeInfo, vertex, vertexBefore.Id, edgeInfos);
                        }
                        if (pathDirections != null &&
                            pathDirections.TryGetValue(v.Id, out var vertexAfter) && vertexAfter.HasValue &&
                            !Vector3.AreCollinearByAngle(vertex.Point, v.Point, _grid.GetVertex(vertexAfter.Value).Point))
                        {
                            newWeight += TurnCost(edgeInfo, v, vertexAfter.Value, edgeInfos);
                        }
                    }

                    // If a lower travel cost to vertex is discovered update the vertex.
                    if (newWeight < travelCost[id])
                    {
                        travelCost[id] = newWeight;
                        path[id] = u;
                        pq.UpdatePriority(id, newWeight);
                    }
                }
            }
            return path;
        }

        /// <summary>
        /// This is a Dijkstra algorithm implementation that stores up to two different paths per vertex.
        /// The algorithm travels from start point to all other points, gathering travel cost.
        /// Each time route turns - extra penalty is added to the cost.
        /// Higher level algorithm then decides which one of them to use as an end point.
        /// Produced dictionary has "Left/Right" label using which two best routes per vertex can be retried.
        /// </summary>
        /// <param name="start">Start Vertex</param>
        /// <param name="edgeInfos">Dictionary of Edge Id to precomputed information about it</param>
        /// <param name="travelCost">Output dictionary where traveling costs are stored per Vertex for two possible branches</param>
        /// <param name="startDirection">Previous Vertex, if start Vertex is already part of the Route</param>
        /// <param name="excluded">Vertices that are not allowed to visit</param>
        /// <returns>Dictionary that have two travel routes from each Vertex back to start Vertex.</returns>
        public Dictionary<ulong, ((ulong, BranchSide), (ulong, BranchSide))> ShortestBranchesDijkstra(
            ulong start, Dictionary<ulong, EdgeInfo> edgeInfos,
            out Dictionary<ulong, (double, double)> travelCost,
            ulong? startDirection = null, HashSet<ulong> excluded = null)
        {
            PriorityQueue<ulong> pq = PreparePriorityQueue(
                start, out Dictionary<ulong, ((ulong Id, BranchSide Side) Left, (ulong Id, BranchSide Side) Right)> path,
                out travelCost);

            while (!pq.Empty())
            {
                //At each step retrieve the vertex with the lowest travel cost and
                //remove it, so it can't be visited again.
                ulong u = pq.PopMin();

                //All vertices that can be reached from start vertex are visited.
                //Ignore once only unreachable are left.
                var cost = travelCost[u];
                if (cost.Item1 == NOT_CONNECTED)
                {
                    break;
                }

                var vertex = _grid.GetVertex(u);
                var before = path[u];

                foreach (var e in vertex.Edges)
                {
                    //Don't go back to where we just came from.
                    //If vertex is removed from queue - best path is already found for it.
                    var id = e.StartId == u ? e.EndId : e.StartId;
                    if (before.Left.Id == id || before.Right.Id == id || !pq.Contains(id))
                    {
                        continue;
                    }

                    var edgeInfo = edgeInfos[e.Id];
                    var bestCost = travelCost[id];

                    //Edge or start vertex is not traversable.
                    if (edgeInfo.Factor == CONDITIONS_NOT_MET || cost.Item1 == CONDITIONS_NOT_MET)
                    {
                        MarkExpensiveRoute(pq, travelCost, path, id, u, bestCost);
                        continue;
                    }

                    //Vertex is excluded or shortest path is already found.
                    if (excluded != null && excluded.Contains(id))
                    {
                        MarkExpensiveRoute(pq, travelCost, path, id, u, bestCost);
                        continue;
                    }

                    var v = _grid.GetVertex(id);
                    //Check if edge passes user defined filter functions
                    if (_filters.Any(f => !f(vertex, v)))
                    {
                        MarkExpensiveRoute(pq, travelCost, path, id, u, bestCost);
                        continue;
                    }

                    //Compute cost of each its neighbors as cost of vertex we came from plus cost of edge.
                    var newWeight = EdgeCost(edgeInfo);
                    BranchSide bestBranch = BranchSide.Left;

                    //We need as little change of direction as possible. A penalty is added if
                    //a) We have a turn traveling to the next vertex.
                    //b) We just started and going in direction different than how we arrived from the previous segment.
                    //c) We arrived at a vertex that is used in a different path.
                    if (u == start)
                    {
                        if (startDirection.HasValue &&
                            !Vector3.AreCollinearByAngle(_grid.GetVertex(startDirection.Value).Point, vertex.Point, v.Point))
                        {
                            newWeight += TurnCost(edgeInfo, vertex, startDirection.Value, edgeInfos);
                        }
                    }
                    else
                    {
                        //For each of two stored branches - edges connected to active one.
                        //Add turn cost if the direction is changed.
                        var leftBefore = _grid.GetVertex(before.Left.Id);
                        var leftCollinear = Vector3.AreCollinearByAngle(leftBefore.Point, vertex.Point, v.Point);
                        var leftCost = cost.Item1 + newWeight;
                        if (!leftCollinear)
                        {
                            leftCost += TurnCost(edgeInfo, vertex, leftBefore.Id, edgeInfos);
                        }

                        var rightCost = NOT_CONNECTED;
                        if (before.Right.Id != 0)
                        {
                            var rightBefore = _grid.GetVertex(before.Right.Id);
                            rightCost = cost.Item2 + newWeight;
                            if (!Vector3.AreCollinearByAngle(rightBefore.Point, vertex.Point, v.Point))
                            {
                                rightCost += TurnCost(edgeInfo, vertex, rightBefore.Id, edgeInfos);
                            }
                        }

                        //Then choose the path that has lower accumulated value.
                        if (leftCost < rightCost)
                        {
                            newWeight = leftCost;
                            bestBranch = BranchSide.Left;
                        }
                        else
                        {
                            newWeight = rightCost;
                            bestBranch = BranchSide.Right;
                        }
                    }


                    var oldCost = travelCost[id];
                    var oldPath = path[id];
                    //Cheaper branch is stored first.
                    //But priority for the Vertex is set by the slower branch.
                    if (newWeight < oldCost.Item1)
                    {
                        travelCost[id] = (newWeight, oldCost.Item1);
                        path[id] = ((u, bestBranch), oldPath.Left);
                        if (oldCost.Item1 == NOT_CONNECTED)
                        {
                            //When we first meet the vertex we need to slow it down to allow
                            //other slightly slower path but with potentially better turn to reach it.
                            pq.UpdatePriority(id, newWeight + _configuration.TurnCost);
                        }
                        else
                        {
                            var newPriority = Math.Min(oldCost.Item1, newWeight + _configuration.TurnCost);
                            pq.UpdatePriority(id, newPriority);
                        }
                    }
                    else if (newWeight < oldCost.Item2)
                    {
                        travelCost[id] = (oldCost.Item1, newWeight);
                        path[id] = (oldPath.Item1, (u, bestBranch));
                        var newPriority = Math.Min(oldCost.Item1 + _configuration.TurnCost, newWeight);
                        pq.UpdatePriority(id, newPriority);
                    }
                }
            }
            return path;
        }

        /// <summary>
        /// Calculate turn cost between two edges.
        /// If turn is not vertical, turn cost should take into account cost factor of given edges.
        /// Otherwise hint paths with several turns would be ignored because of extra cost.
        /// The turn factor is multiplied by minimum of two edges factor.
        /// </summary>
        /// <param name="edgeInfo">Edge information for the first edge</param>
        /// <param name="sharedVertex">Id of the vertex, common for two edges</param>
        /// <param name="thirdVertexId">Third vertex Id</param>
        /// <param name="edgeInfos">Precalculated information for each edge</param>
        /// <returns></returns>
        private double TurnCost(
            EdgeInfo edgeInfo, Vertex sharedVertex, ulong thirdVertexId,
            IDictionary<ulong, EdgeInfo> edgeInfos)
        {
            var otherEdge = sharedVertex.GetEdge(thirdVertexId);
            var otherInfo = edgeInfos[otherEdge.Id];

            //Do not modify turn cost if either of edges is not horizontal.
            //This prevents "free to travel" loops under 2d hint lines.
            //If either of two edges are affected by 3d hint line - turn cost can be
            //discounted but still can't be bigger than TurnCost.
            if (edgeInfo.HasAnyFlag(EdgeFlags.HasVerticalChange) ||
                otherInfo.HasAnyFlag(EdgeFlags.HasVerticalChange))
            {
                var factor = 1d;
                if (edgeInfo.HasAnyFlag(EdgeFlags.UserDefinedHint3D))
                {
                    factor = Math.Min(edgeInfo.Factor, 1);
                }
                else if (otherInfo.HasAnyFlag(EdgeFlags.UserDefinedHint3D))
                {
                    factor = Math.Min(otherInfo.Factor, 1);
                }

                return factor * _configuration.TurnCost;
            }

            //Minimum factor makes algorithm prefer edges inside of hint lines even if they
            //have several turns but don't give advantage for the tiny edges that are
            //fully inside hint line influence area.
            return _configuration.TurnCost * Math.Min(edgeInfo.Factor, otherInfo.Factor);
        }

        private double EdgeCost(EdgeInfo info)
        {
            return info.Length * info.Factor;
        }

        private PriorityQueue<ulong> PreparePriorityQueue(ulong start,
            out Dictionary<ulong, ulong> path, out Dictionary<ulong, double> travelCost)
        {
            path = new Dictionary<ulong, ulong>();
            travelCost = new Dictionary<ulong, double>();

            //Travel cost of all vertices are set to infinity, except for the one we start
            //from for which is set to 0.
            var vertices = _grid.GetVertices();
            List<ulong> indices = new List<ulong>() { start };
            for (int i = 0; i < vertices.Count; i++)
            {
                if (vertices[i].Id != start)
                {
                    indices.Add(vertices[i].Id);
                    travelCost[vertices[i].Id] = NOT_CONNECTED;
                }
                path[vertices[i].Id] = 0;
            }
            travelCost[start] = 0;

            PriorityQueue<ulong> pq = new PriorityQueue<ulong>(indices);
            return pq;
        }

        private PriorityQueue<ulong> PreparePriorityQueue(ulong start,
            out Dictionary<ulong, ((ulong, BranchSide), (ulong, BranchSide))> path,
            out Dictionary<ulong, (double, double)> travelCost)
        {
            path = new Dictionary<ulong, ((ulong, BranchSide), (ulong, BranchSide))>();
            travelCost = new Dictionary<ulong, (double, double)>();

            //Travel cost of all vertices are set to infinity, except for the one we start
            //from for which is set to 0.
            var vertices = _grid.GetVertices();
            List<ulong> indices = new List<ulong>() { start };
            for (int i = 0; i < vertices.Count; i++)
            {
                if (vertices[i].Id != start)
                {
                    indices.Add(vertices[i].Id);
                    travelCost[vertices[i].Id] = (NOT_CONNECTED, NOT_CONNECTED);
                }
                path[vertices[i].Id] = ((0, BranchSide.Left), (0, BranchSide.Left));
            }
            travelCost[start] = (0, 0);

            PriorityQueue<ulong> pq = new PriorityQueue<ulong>(indices);
            return pq;
        }

        private List<ulong> GetPathTo(Dictionary<ulong, ulong> connections, ulong end)
        {
            List<ulong> shortestPath = new List<ulong>();
            shortestPath.Add(end);
            var before = connections[end];
            while (before != 0)
            {
                shortestPath.Add(before);
                before = connections[before];
            }

            return shortestPath.Reverse<ulong>().ToList();
        }

        private List<ulong> GetPathTo(
            Dictionary<ulong, ((ulong, BranchSide), (ulong, BranchSide))> connections,
            ulong end, BranchSide branch)
        {
            List<ulong> shortestPath = new List<ulong>();
            shortestPath.Add(end);
            var before = connections[end];
            var branchBefore = branch == BranchSide.Left ? before.Item1 : before.Item2;
            while (branchBefore.Item1 != 0)
            {
                shortestPath.Add(branchBefore.Item1);
                before = connections[branchBefore.Item1];
                branchBefore = branchBefore.Item2 == BranchSide.Left ? before.Item1 : before.Item2;
            }

            return shortestPath.Reverse<ulong>().ToList();
        }

        private void AddPathToTree(
            ulong start,
            List<ulong> path,
            Dictionary<ulong, TreeNode> tree)
        {
            if (path.First() != start)
            {
                path.Reverse();
            }

            for (int i = path.Count - 2; i >= 0; i--)
            {
                //Path is composed from end to inlets. If tree already has next vertex
                //from this one recorded, we don't want to override it. This way we join
                //the flow that is already created, removing unnecessary loops.
                if (!tree.TryGetValue(path[i], out var oldNode) || oldNode.Trunk == null)
                {
                    var node = oldNode ?? new TreeNode(path[i]);
                    var nextNode = tree[path[i + 1]];
                    node.SetTrunk(nextNode);
                    tree[path[i]] = node;
                }
            }
        }

        private List<TreeNode> PathToFirstBranching(TreeNode node)
        {
            List<TreeNode> path = new List<TreeNode>();
            while (node != null && node.Leafs.Count < 2)
            {
                path.Add(node);
                node = node.Trunk;
            }
            return path;
        }

        private List<Vertex> NearbyVertices(
            IEnumerable<RoutingHintLine> hints,
            IEnumerable<RoutingVertex> excluded)
        {
            if (hints == null || !hints.Any())
            {
                return new List<Vertex>();
            }

            return _grid.GetVertices().Where(
                v => !excluded.Any(e => e.Id == v.Id) && IsNearby(v.Point, hints)).ToList();
        }

        private bool IsNearby(Vector3 v, IEnumerable<RoutingHintLine> hints)
        {
            if (hints != null)
            {
                foreach (var hint in hints)
                {
                    var influenceDistance = Math.Max(hint.InfluenceDistance, _grid.Tolerance);
                    var target = hint.Is2D ? new Vector3(v.X, v.Y) : v;
                    if (target.DistanceTo(hint.Polyline) <= influenceDistance)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool IsAffectedBy(
            Vector3 start, Vector3 end, IEnumerable<RoutingHintLine> hints)
        {
            return hints != null && hints.Any(h => IsAffectedBy(start, end, h));
        }

        private bool IsAffectedBy(Vector3 start, Vector3 end, RoutingHintLine hint)
        {
            var influenceDistance = Math.Max(hint.InfluenceDistance, _grid.Tolerance);
            Vector3 vs = hint.Is2D ? new Vector3(start.X, start.Y) : start;
            Vector3 ve = hint.Is2D ? new Vector3(end.X, end.Y) : end;
            //Vertical edges are not affected by 2D hint lines
            if (!hint.Is2D || !vs.IsAlmostEqualTo(ve, _grid.Tolerance) &&
                Math.Abs(start.Z - end.Z) < _grid.Tolerance)
            {
                foreach (var segment in hint.Polyline.Segments())
                {
                    double lowClosest = 1;
                    double hiClosest = 0;

                    var dot = segment.Direction().Dot((ve - vs).Unitized());
                    if (!Math.Abs(dot).ApproximatelyEquals(1))
                    {
                        continue;
                    }

                    if (vs.DistanceTo(segment) <= influenceDistance)
                    {
                        lowClosest = 0;
                    }

                    if (ve.DistanceTo(segment) <= influenceDistance)
                    {
                        hiClosest = 1;
                    }

                    if (lowClosest < hiClosest)
                    {
                        return true;
                    }

                    var edgeLine = new Line(vs, ve);
                    Action<Vector3> check = (Vector3 p) =>
                    {
                        if (p.DistanceTo(edgeLine, out var closest) <= influenceDistance)
                        {
                            var t = (closest - vs).Length() / edgeLine.Length();
                            if (t < lowClosest)
                            {
                                lowClosest = t;
                            }

                            if (t > hiClosest)
                            {
                                hiClosest = t;
                            }
                        }
                    };

                    check(segment.Start);
                    check(segment.End);

                    if (hiClosest > lowClosest &&
                        (hiClosest - lowClosest) * edgeLine.Length() > influenceDistance)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void Compare(ulong index, IDictionary<ulong, double> travelCost,
            ref double bestCost, ref ulong bestIndex)
        {
            if (travelCost.TryGetValue(index, out var cost))
            {
                if (cost < bestCost)
                {
                    bestCost = cost;
                    bestIndex = index;
                }
            }
        }

        private (ulong, BranchSide) FindConnectionPoint(
            IEnumerable<ulong> collection,
            IDictionary<ulong, (double, double)> travelCost,
            IDictionary<ulong, ((ulong, BranchSide), (ulong, BranchSide))> connections,
            IDictionary<ulong, TreeNode> tree,
            IDictionary<ulong, EdgeInfo> weights)

        {
            ulong bestIndex = 0;
            double bestCost = NOT_CONNECTED;
            BranchSide bestBranch = BranchSide.Left;
            double bestCostToTrunk = -1;

            foreach (var index in collection)
            {
                if (travelCost.TryGetValue(index, out var costs))
                {
                    double candidateCost = costs.Item1;
                    BranchSide candidateBranch = BranchSide.Left;
                    if (costs.Item2 - costs.Item1 < Vector3.EPSILON)
                    {
                        candidateCost = costs.Item2;
                        candidateBranch = BranchSide.Right;
                    }

                    //Connection point with the lower cost is better.
                    if (candidateCost > bestCost + Vector3.EPSILON)
                    {
                        continue;
                    }

                    //If two ways to travel from point A to point B has the same cost,
                    //the one is used that joins the tree without a turn.
                    //We both need/don't need a turn - first path is chosen.
                    if (costs.Item1.ApproximatelyEquals(costs.Item2) &&
                        tree.TryGetValue(index, out var node) && node.Trunk != null)
                    {
                        var activeV = _grid.GetVertex(index);
                        var nextV = _grid.GetVertex(node.Trunk.Id);
                        var before = connections[index];
                        bool needTurn1 = false;
                        bool needTurn2 = false;

                        if (before.Item1.Item1 != 0)
                        {
                            var beforeV1 = _grid.GetVertex(before.Item1.Item1);
                            needTurn1 = !Vector3.AreCollinearByAngle(beforeV1.Point, activeV.Point, nextV.Point);
                        }

                        if (before.Item2.Item1 != 0)
                        {
                            var beforeV2 = _grid.GetVertex(before.Item2.Item1);
                            needTurn2 = !Vector3.AreCollinearByAngle(beforeV2.Point, activeV.Point, nextV.Point);
                        }

                        if (needTurn1 && !needTurn2)
                        {
                            (candidateCost, candidateBranch) = (costs.Item2, BranchSide.Right);
                        }
                        else
                        {
                            (candidateCost, candidateBranch) = (costs.Item1, BranchSide.Left);
                        }
                    }

                    //If several connection points on the tree have the same cost,
                    //one is chosen that is closer to the end.
                    if (candidateCost.ApproximatelyEquals(bestCost))
                    {
                        if (bestCostToTrunk < 0)
                        {
                            bestCostToTrunk = CostToTrunk(tree[bestIndex], weights);
                        }
                        var candidateToTrunk = CostToTrunk(tree[index], weights);
                        if (bestCostToTrunk - candidateToTrunk > Vector3.EPSILON)
                        {
                            bestIndex = index;
                            bestCost = candidateCost;
                            bestBranch = candidateBranch;
                            bestCostToTrunk = candidateToTrunk;
                        }
                    }
                    else
                    {
                        bestIndex = index;
                        bestCost = candidateCost;
                        bestBranch = candidateBranch;
                        bestCostToTrunk = -1;
                    }
                }
            }
            return (bestIndex, bestBranch);
        }

        private ulong FindConnectionPoint(IEnumerable<ulong> collection,
            IDictionary<ulong, double> travelCost,
            ref double bestCost)
        {
            ulong bestIndex = 0;
            foreach (var v in collection)
            {
                Compare(v, travelCost, ref bestCost, ref bestIndex);
            }
            return bestIndex;
        }

        private double CostToTrunk(
            TreeNode start,
            IDictionary<ulong, EdgeInfo> weights)
        {
            double cost = 0;
            Vertex before = null;
            Vertex vertex = _grid.GetVertex(start.Id);
            TreeNode node = start;

            while (node != null && node.Trunk != null)
            {
                var edge = vertex.GetEdge(node.Trunk.Id);
                Vertex next = _grid.GetVertex(node.Trunk.Id);
                var edgeInfo = weights[edge.Id];
                cost += EdgeCost(edgeInfo);

                if (before != null &&
                    !Vector3.AreCollinearByAngle(before.Point, vertex.Point, next.Point))
                {
                    cost += TurnCost(edgeInfo, vertex, before.Id, weights);
                }

                before = vertex;
                vertex = next;
                node = node.Trunk;
            }
            return cost;
        }

        private void MarkExpensiveRoute(PriorityQueue<ulong> pq,
                                       IDictionary<ulong, double> travelCost,
                                       IDictionary<ulong, ulong> path,
                                       ulong id, ulong before, double bestCost)
        {
            if (bestCost == NOT_CONNECTED)
            {
                travelCost[id] = CONDITIONS_NOT_MET;
                pq.UpdatePriority(id, CONDITIONS_NOT_MET);
                path[id] = before;
            }
        }

        private void MarkExpensiveRoute(
            PriorityQueue<ulong> pq,
            Dictionary<ulong, (double Left, double Right)> travelCost,
            Dictionary<ulong, ((ulong Id, BranchSide Side) Left, (ulong Id, BranchSide Side) Right)> path,
            ulong id, ulong before, (double Left, double Right) bestCost)
        {
            if (bestCost.Left == NOT_CONNECTED)
            {
                travelCost[id] = (CONDITIONS_NOT_MET, NOT_CONNECTED);
                pq.UpdatePriority(id, CONDITIONS_NOT_MET);
                path[id] = ((before, BranchSide.Left), (0, BranchSide.Left));
            }
        }

        private HashSet<ulong> FilteredSet(HashSet<ulong> hashSet, IEnumerable<ulong> exceptions)
        {
            var setCopy = new HashSet<ulong>(hashSet);
            setCopy.ExceptWith(exceptions);
            return setCopy;
        }

        private ModelPoints VisualizePoints(IList<Vector3> points)
        {
            List<SolidOperation> so = new List<SolidOperation>();
            double sideLength = 0.2;
            var baseRectangle = Polygon.Rectangle(sideLength, sideLength);
            foreach (var p in points)
            {
                var rectangle = baseRectangle.TransformedPolygon(
                    new Transform(p - new Vector3(0, 0, sideLength / 2)));
                var extrude = new Extrude(rectangle, sideLength, Vector3.ZAxis, false);
                so.Add(extrude);
            }

            var mp = new ModelPoints(points)
            {
                Representation = new Representation(so),
                Material = new Material("Grid Key Points", new Color(0.6, 0.2, 0.8, 0.5)) //Dark Orchid
            };
            return mp;
        }

        private bool CheckAccessability(
            ulong terminal,
            double cost,
            bool haveExcludedVertices)
        {
            if (cost == NOT_CONNECTED)
            {
                var vertex = _grid.GetVertex(terminal);
                ErrorMessages.Add(Message.FromPoint(
                    $"Input vertex {vertex.Id} is not connected with any exit vertices in the grid.",
                    vertex.Point));
                return false;
            }
            else if (cost == CONDITIONS_NOT_MET)
            {
                var vertex = _grid.GetVertex(terminal);
                bool userFilters = _filters.Any();
                bool angleFilters = _configuration.SupportedAngles != null &&
                                    _configuration.SupportedAngles.Any();
                List<string> reasons = new List<string>();
                if (haveExcludedVertices)
                {
                    reasons.Add("isolation distance around other inputs");
                }

                if (userFilters)
                {
                    reasons.Add("user defined filter functions");
                }

                if (angleFilters)
                {
                    reasons.Add("list of supported angles");
                }

                StringBuilder sb = new StringBuilder();
                sb.Append($"Input vertex {vertex.Id} does not have correct route to any exit vertices");
                if (reasons.Any())
                {
                    sb.Append(" due to");
                    for (int i = 0; i < reasons.Count; i++)
                    {
                        if (i != 0)
                        {
                            if (i == reasons.Count - 1)
                            {
                                sb.Append(" or ");
                            }
                            else if (i > 0)
                            {
                                sb.Append(", ");
                            }
                        }

                        sb.Append(reasons[i]);
                    }
                    sb.Append(".");
                }

                ErrorMessages.Add(Message.FromPoint(sb.ToString(), vertex.Point));
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}