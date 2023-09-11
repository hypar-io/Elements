using System;
using System.Collections.Generic;
using System.Linq;
using Elements;
using Elements.Geometry;
using Elements.Geometry.Solids;

namespace Elements.Flow
{
    public partial class Tree : ICloneable
    {
        public static Material CollectionMaterial = new Material("Collection", new Color(1, 0.4, 0, 1));
        public static Material GravityMaterial = new Material("Gravity Pipe", new Color(0.2, 0.2, 0.2, 1.0));

        public static bool SkipConnectionsInRepresentation { get; set; }

        /// <summary>
        /// Create a Tree for a set of regions.
        /// Region references are sorted before they are stored.
        /// Outlet position is set to origin.
        /// </summary>
        /// <param name="regionsServed">List of region references used in the tree.</param>
        public Tree(IEnumerable<string> regionsServed)
            : base(new Transform(),
                   CollectionMaterial,
                   new Representation(new List<SolidOperation> { }),
                   false,
                   Guid.NewGuid(),
                   "")
        {
            Init(regionsServed);
            this.Purpose = string.Empty;
        }

        /// <summary>
        /// Create a Tree from initial set of parameters.
        /// Region references are sorted before they are stored.
        /// Outlet position is set to origin.
        /// </summary>
        /// <param name="regionsServed">List of region references used in the tree.</param>
        /// <param name="outletFlow">The total outlet flow.</param>
        /// <param name="material">Tree material.</param>
        /// <param name="name">Name of the tree. Treated differently if contains 'Emergency'.</param>
        /// <param name="purpose">The purpose tag of the tree.</param>
        public Tree(string name, string purpose, List<string> regionsServed, double outletFlow, Material material)
            : base(new Transform(),
                   material,
                   new Representation(new List<SolidOperation> { }),
                   false,
                   Guid.NewGuid(),
                   name)
        {
            Init(regionsServed);
            this.OutletFlow = outletFlow;
            this.Purpose = purpose;
        }

        private void Init(IEnumerable<string> regionsServed)
        {
            this.Inlets = new List<Leaf>();
            this.Connections = new List<Connection>();
            this.InternalNodes = new List<Node>();
            this.Outlet = this.SetOutletPosition(new Vector3(0, 0, 0));
            this.RegionReferences = regionsServed.OrderBy(r => r, new LexiNumericalComparer()).ToList();
        }

        /// <summary>
        /// Does this tree contain this Node?
        /// </summary>
        /// <param name="node"></param>
        /// <param name="excludeInlets"></param>
        /// <returns></returns>
        public bool HasNode(Node node, bool excludeInlets = false)
        {
            return this.Outlet == node
                    || this.InternalNodes.Contains(node)
                    || (!excludeInlets && this.Inlets.Contains(node));
        }

        /// <summary>
        /// Does this tree contain loops
        /// </summary>
        /// <returns>True if this tree has loops.</returns>
        public bool HasLoops()
        {
            return Connections.Any(c => c.IsLoop == true);
        }

        public static string NetworkRefFromRegionRefs(IEnumerable<string> regionRefs)
        {
            return String.Join(",", regionRefs);
        }

        public string GetNetworkReference()
        {
            var networkRef = string.Empty;
            if (RegionReferences != null && RegionReferences.Any())
            {
                networkRef = NetworkRefFromRegionRefs(RegionReferences);
            }
            return networkRef;
        }

        private Connection Connect(Node start, Node end, bool allowDisconnect = false, bool? isLoop = false)
        {
            if (!this.HasNode(start))
            {
                throw new ArgumentException("That start node is not in this network.");
            }
            if (!this.HasNode(end))
            {
                throw new ArgumentException("That end node is not in this network.");
            }
            if (this.Inlets.Contains(end))
            {
                throw new ArgumentException("The end node provided is an Inlet and inlets cannot have incoming connections");
            }
            if (this.Outlet == start)
            {
                throw new Exception("The start node provided is the outlet, and the outlet cannot have outgoing connections.");
            }
            if (start.Position.IsAlmostEqualTo(end.Position))
            {
                throw new Exception("The start and end of this connection are in the same place, this is not allowed.");
            }

            var connection = new Connection(start, end, 0, 0);
            if (this._nodesOutgoingConnectionLookup.TryGetValue(start, out var existingConnections))
            {
                var existingMainConnection = existingConnections.FirstOrDefault(c => c.IsLoop != true);
                if (existingMainConnection != null && isLoop != true)
                {
                    if (allowDisconnect)
                    {
                        this.Disconnect(existingMainConnection);
                    }
                    else
                    {
                        throw new Exception("You are trying to connect from a node that is already connected.  You must disconnect first or send in allowDisconnect = true in this method.");
                    }
                }
            }
            if (!_nodesIncomingConnectionLookup.ContainsKey(end))
            {
                _nodesIncomingConnectionLookup[end] = new HashSet<Connection>();
            }
            if (!_nodesOutgoingConnectionLookup.ContainsKey(start))
            {
                _nodesOutgoingConnectionLookup[start] = new HashSet<Connection>();
            }
            _nodesOutgoingConnectionLookup[start].Add(connection);

            _nodesIncomingConnectionLookup[end].Add(connection);

            this.Connections.Add(connection);
            connection.IsLoop = isLoop;

            return connection;
        }

        public Connection AddLoopConnection(Node start, Node end)
        {
            var connection = Connect(start, end, isLoop: true);
            return connection;
        }

        internal bool _alreadyTriedInit = false;

        private Dictionary<Node, HashSet<Connection>> _nodesIncomingConnectionLookup = new Dictionary<Node, HashSet<Connection>>();

        private Dictionary<Node, HashSet<Connection>> _nodesOutgoingConnectionLookup = new Dictionary<Node, HashSet<Connection>>();

        /// <summary>
        /// Adds a new inlet or leaf node to the tree.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="flow"></param>
        /// <param name="connectVia"></param>
        /// <param name="terminalServed"></param>
        /// <returns></returns>
        public Leaf AddInlet(Vector3 position = new Vector3(), double flow = 0, Node connectVia = null, Guid? terminalServed = null)
        {
            if (this.Connections == null)
            {
                throw new Exception($"This tree has not been initialized yet, use the {nameof(Tree)} constructor that takes in regionReferences.");
            }
            var incomingFromLookup = this._nodesIncomingConnectionLookup.SelectMany(kvp => kvp.Value).ToList();
            var extraFromLookup = incomingFromLookup.ToList().Except(this.Connections);
            var extraFromConnections = this.Connections.ToList().Except(incomingFromLookup);
            var newInlet = new Leaf(flow, terminalServed, position, Guid.NewGuid(), "");
            this.Inlets.Add(newInlet);
            if (connectVia != null)
            {
                var connection = this.Connect(newInlet, connectVia);
                var present2 = incomingFromLookup.Contains(connection);
                var extraFromLookup2 = incomingFromLookup.ToList().Except(this.Connections);
                var extraFromConnections2 = this.Connections.ToList().Except(incomingFromLookup);
            }

            else if (this.Outlet != null)
            {
                var connection = this.Connect(newInlet, this.Outlet);
                var incomingFromLookup2 = this._nodesIncomingConnectionLookup.SelectMany(kvp => kvp.Value);
                var present2 = incomingFromLookup.Contains(connection);
                var extraFromLookup2 = incomingFromLookup.ToList().Except(this.Connections);
                var extraFromConnections2 = this.Connections.ToList().Except(incomingFromLookup);
            }

            return newInlet;
        }

        /// <summary>
        /// Chamfers the connections at the given node. The node must have exactly one
        /// incoming connection and one outgoing connection.  The chamfer points are an equal
        /// distance from the original nodes, while being separated by the chamferThickness.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="chamferThickness"></param>
        /// <param name="newIncomingSideNode"></param>
        /// <param name="newOutgoingSideNode"></param>
        public void ChamferAtNode(Node node, double chamferThickness, out Node newIncomingSideNode, out Node newOutgoingSideNode)
        {
            if (!this.HasNode(node))
            {
                throw new System.ArgumentException("That node is not in this flow tree.");
            }
            var incomingConnections = this.GetIncomingConnections(node);
            if (incomingConnections.Count != 1)
            {
                throw new System.ArgumentException($"The node must have exactly 1 incoming connection, {incomingConnections.Count} were found.");
            }
            var outgoingConnections = this.GetOutgoingConnections(node);
            if (outgoingConnections.Count == 0)
            {
                throw new System.ArgumentException($"The node must have an outgoing connection, but none was found.");
            }
            if (outgoingConnections.Count > 1)
            {
                throw new ArgumentException($"The node must have one outgoing connection, but {outgoingConnections.Count} was found.");
            }
            var outgoingConnection = outgoingConnections.First();
            var isLoop = incomingConnections.First().IsLoop == true && outgoingConnection.IsLoop == true;
            var incomingDirection = incomingConnections.First().Start.Position - incomingConnections.First().End.Position;
            var outgoingDirection = outgoingConnection.End.Position - outgoingConnection.Start.Position;

            var angle = incomingDirection.AngleTo(outgoingDirection);

            var distanceToNewConnections = chamferThickness / (2 * Math.Sin(angle / 2));

            var newIncomingPoint = node.Position + incomingDirection.Unitized() * distanceToNewConnections;
            newIncomingSideNode = new Node(newIncomingPoint, Guid.NewGuid(), "");
            var incomingNode = incomingConnections.First().Start;
            this.Disconnect(incomingConnections.First());
            this.InternalNodes.Add(newIncomingSideNode);
            this.Connect(incomingNode, newIncomingSideNode, isLoop: isLoop);

            var newOutgoingPoint = node.Position + outgoingDirection.Unitized() * distanceToNewConnections;
            newOutgoingSideNode = new Node(newOutgoingPoint, Guid.NewGuid(), "");
            var outgoingNode = outgoingConnection.End;
            this.Disconnect(outgoingConnection);
            this.InternalNodes.Add(newOutgoingSideNode);
            this.Connect(newOutgoingSideNode, outgoingNode, isLoop: isLoop);

            this.Connect(newIncomingSideNode, newOutgoingSideNode, isLoop: isLoop);
        }

        /// <summary>
        /// Returns all of the connections from this connection to the outlet, including the connection itself.
        /// </summary>
        /// <param name="startingConnection"></param>
        /// <returns></returns>
        public IEnumerable<Connection> AllConnectionsHereToTrunk(Connection startingConnection)
        {
            var allConnections = new List<Connection>();
            var currentConnection = startingConnection;
            while (currentConnection != null)
            {
                allConnections.Add(currentConnection);
                var nextNode = currentConnection.End;
                currentConnection = GetOutgoingConnection(nextNode);
            }
            return allConnections;
        }

        /// <summary>
        /// Get a list of all of the connections that go into this flow node.
        /// </summary>
        /// <param name="node">The node used to lookup incoming connections.</param>
        public List<Connection> GetIncomingConnections(Node node)
        {
            if (this.Inlets.Contains(node))
            {
                return new List<Connection>();
                // TODO not sure if we should succeed and return null or throw the error.
                // throw new Exception("That node is an inlet, and therefor cannot have any incoming connections");
            }
            if (_nodesIncomingConnectionLookup.Count == 0 && !_alreadyTriedInit)
            {
                InitializeTree();
            }
            if (this._nodesIncomingConnectionLookup.TryGetValue(node, out var connections))
            {
                return connections.ToList();
            }
            else
            {
                return new List<Connection>();
            }
        }

        /// <summary>
        /// Get the connections that goes out of this flow node.
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public List<Connection> GetOutgoingConnections(Node port)
        {
            if (_nodesOutgoingConnectionLookup.Count == 0 && !_alreadyTriedInit)
            {
                InitializeTree();
            }
            if (this._nodesOutgoingConnectionLookup.TryGetValue(port, out var connections))
            {
                return connections.ToList();
            }
            else
            {
                return new List<Connection>();
            }
        }

        /// <summary>
        /// Get the connection that goes out of this flow node.
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public Connection GetOutgoingConnection(Node port)
        {
            var connections = GetOutgoingConnections(port);
            if (connections.Count == 1)
            {
                return connections.First();
            }
            return connections?.FirstOrDefault(c => c.IsLoop != true);
        }

        /// <summary>
        /// Get the loop connections.
        /// </summary>
        /// <returns>Loop connections</returns>
        public List<Connection> GetLoopConnections()
        {
            return Connections.Where(c => c.IsLoop == true).ToList();
        }

        private void InitializeTree()
        {
            _alreadyTriedInit = true;
            foreach (var connection in Connections)
            {
                if (!this._nodesIncomingConnectionLookup.ContainsKey(connection.End))
                {
                    this._nodesIncomingConnectionLookup[connection.End] = new HashSet<Connection>();
                }
                this._nodesIncomingConnectionLookup[connection.End].Add(connection);

                if (!this._nodesOutgoingConnectionLookup.ContainsKey(connection.Start))
                {
                    this._nodesOutgoingConnectionLookup[connection.Start] = new HashSet<Connection>();
                }
                this._nodesOutgoingConnectionLookup[connection.Start].Add(connection);

                if (this._nodesOutgoingConnectionLookup.TryGetValue(connection.Start, out var outgoingConnections))
                {
                    if (outgoingConnections.Where(c => c.IsLoop != true).Count() > 1)
                    {
                        throw new InvalidOperationException("Bad tree.  There were too many connections found ending at a node.");
                    }
                }
            }
        }

        /// <summary>
        /// Make a list of given connections merge at a give point.  The first connection in the list
        /// is actually preserved and re-routed.  The rest are destroyed and new connections
        /// are made from the new node location to their end nodes.
        /// </summary>
        /// <param name="connectionsToMerge"></param>
        /// <param name="vector3"></param>
        public Node MergeConnectionsAtPoint(IEnumerable<Connection> connectionsToMerge, Vector3 vector3)
        {
            if (connectionsToMerge.Select(c => c.End).Distinct().Count() > 1)
            {
                throw new System.ArgumentException("You can only merge connections that share the same End Node, one of these does not.");
            }
            Node newInternal = this.SplitConnectionThroughPoint(connectionsToMerge.ElementAt(0), vector3, out _);
            foreach (var oldConnection in connectionsToMerge.Skip(1))
            {
                var oldStart = oldConnection.Start;
                if (oldStart.Position.IsAlmostEqualTo(newInternal.Position))
                {
                    // this merge location coincides with the already existing end of a tree pipe
                    // we should shift connections off of that other node and remove it.
                    GetIncomingConnections(oldStart).ForEach(conn => ShiftConnectionToNode(conn, newInternal));

                    this.Disconnect(oldConnection);
                    InternalNodes.Remove(oldStart);

                    continue;
                }
                this.Disconnect(oldConnection);
                this.Connect(oldStart, newInternal, isLoop: oldConnection.IsLoop);
            }
            return newInternal;
        }

        /// <summary>
        /// Pulls another Tree into this one.  The other one will be assigned to null when merging is done.
        /// All the other's connections that used to go to the outlet will be merged at the provided connection.
        /// If a point is provided then the provided connection is split, and new connections are made to the created internal node.
        /// If no point is provided, then the merging point is the provided connections current End node.
        /// </summary>
        /// <param name="other"></param>
        /// <param name="connection"></param>
        /// <param name="point"></param>
        public void MergeOtherCollectionInAtConnectionPoint(ref Tree other, Connection connection, Vector3? point = null)
        {
            // var incomingFromLookup = this._nodesIncomingConnectionLookup.SelectMany(kvp => kvp.Value);
            // var present = incomingFromLookup.Contains(connection);
            // var extraFromLookup = incomingFromLookup.ToList().Except(this.Fittings);
            // var extraFromConnections = this.Fittings.ToList().Except(incomingFromLookup);
            if (!this.Connections.Contains(connection))
            {
                throw new ArgumentException("The connection to merge into must be part of this tree, but the one provided is not.");
            }
            foreach (var node in other.InternalNodes)
            {
                this.InternalNodes.Add(node);
            }
            foreach (var inlet in other.Inlets)
            {
                this.Inlets.Add(inlet);
            }

            Node nodeThatReplacesOtherOutlet = null;
            if (point.HasValue)
            {
                nodeThatReplacesOtherOutlet = this.SplitConnectionThroughPoint(connection, point.Value, out _);
            }
            else
            {
                nodeThatReplacesOtherOutlet = connection.End;
            }
            foreach (var otherConnection in other.Connections)
            {
                if (otherConnection.End == other.Outlet)
                {
                    this.Connect(otherConnection.Start, nodeThatReplacesOtherOutlet, isLoop: otherConnection.IsLoop);
                }
                else
                {
                    this.Connect(otherConnection.Start, otherConnection.End, isLoop: otherConnection.IsLoop);
                }
            }
            other = null;
        }

        /// <summary>
        /// shifts the endpoint of a flowconnection from one node to another.
        /// If the node that is left behind is internal and has no connections it is deleted from the network.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="node"></param>
        public Connection ShiftConnectionToNode(Connection connection, Node node)
        {
            if (!this.HasNode(node, excludeInlets: true))
            {
                throw new ArgumentException("That node is not a valid connection End node in this network.");
            }
            if (!this.Connections.Contains(connection))
            {
                throw new ArgumentException("That connection is not in this network.");
            }
            if (connection.End == node)
            {
                throw new ArgumentException("That connection is already pointed to that node.");
            }
            var oldEnd = connection.End;
            var removed = this._nodesIncomingConnectionLookup[connection.End].Remove(connection);
            connection.End = node;
            if (!this._nodesIncomingConnectionLookup.ContainsKey(node))
            {
                this._nodesIncomingConnectionLookup[node] = new HashSet<Connection>();
            }
            this._nodesIncomingConnectionLookup[node].Add(connection);

            // check if old node is internal and now stranded, it can be deleted.
            var noIncome = GetIncomingConnections(oldEnd).Count == 0;
            var noOutgo = GetOutgoingConnections(oldEnd).Count == 0;
            var isInternal = InternalNodes.Contains(oldEnd);
            if (noIncome && noOutgo && isInternal)
            {
                _nodesIncomingConnectionLookup.Remove(oldEnd);
                _nodesOutgoingConnectionLookup.Remove(oldEnd);
                InternalNodes.Remove(oldEnd);
            }

            return connection;
        }


        /// <summary>
        /// Disconnects and removes a connection from this tree;
        /// </summary>
        /// <param name="connection"></param>
        public void Disconnect(Connection connection)
        {
            if (this._nodesIncomingConnectionLookup.TryGetValue(connection.End, out var setThatContains))
            {
                setThatContains.Remove(connection);
                if (!setThatContains.Any())
                {
                    this._nodesIncomingConnectionLookup.Remove(connection.End);
                }
            }
            if (this._nodesOutgoingConnectionLookup.TryGetValue(connection.Start, out var outgoingConnections))
            {
                outgoingConnections.Remove(connection);
                if (!outgoingConnections.Any())
                {
                    this._nodesOutgoingConnectionLookup.Remove(connection.Start);
                }
            }
            this.Connections.Remove(connection);
        }

        /// <summary>
        /// Split the given connection, adding a new InternalNode at the position provided.
        /// Returns the new created node.
        /// </summary>
        /// <param name="connection">The connection to split.</param>
        /// <param name="nodePosition">The desired position of the new internal node that will be created.</param>
        public Node SplitConnectionThroughPoint(Connection connection, Vector3 nodePosition)
        {
            return SplitConnectionThroughPoint(connection, nodePosition, out _);
        }

        /// <summary>
        /// Split the given connection, adding a new InternalNode at the position provided.
        /// Returns the new created node.
        /// </summary>
        /// <param name="connection">The connection to split.</param>
        /// <param name="nodePosition">The desired position of the new internal node that will be created.</param>
        /// <param name="newConnections">The 2 new connections that were made. The first ends at the new internal node, the second starts from the new internal node.</param>
        public Node SplitConnectionThroughPoint(Connection connection, Vector3 nodePosition, out Connection[] newConnections)
        {
            // TODO methods like this and connect vertically and others should be moved to the flow Section
            // the Section class should be what sits on top of the Fittings, and provides path information, or maybe it even merges with connection... ?

            if (connection.Start.Position.IsAlmostEqualTo(nodePosition) || connection.End.Position.IsAlmostEqualTo(nodePosition))
            {
                throw new InvalidOperationException("The new node position is the same as the existing Start or End position");
            }
            if (!this.Connections.Contains(connection))
            {
                throw new ArgumentException("That connection is not in this network.");
            }
            var newNode = new Node(nodePosition, Guid.NewGuid(), "");
            this.InternalNodes.Add(newNode);
            this.Disconnect(connection);
            var startConnection = this.Connect(connection.Start, newNode, isLoop: connection.IsLoop);
            var endConnection = this.Connect(newNode, connection.End, isLoop: connection.IsLoop);
            startConnection.Diameter = endConnection.Diameter = connection.Diameter;
            newConnections = new[] { startConnection, endConnection };

            return newNode;
        }

        /// <summary>
        /// Stitches together any connections that are connected and inline with each other.
        /// </summary>
        /// <param name="includeDiameterCheck">Only stitches together connections that are the same diameter.</param>
        /// <param name="excludedNodes">Excluded nodes from simplify.</param>
        /// <remarks>Both connections around the node should be included in the list to not be merged.</remarks>
        public void Simplify(bool includeDiameterCheck = false, Vector3[] excludedNodes = null)
        {
            var nodesToCheck = InternalNodes
                .Where(x => GetIncomingConnections(x).Count == 1 && GetOutgoingConnections(x).Count == 1)
                .ToList();

            foreach (var node in nodesToCheck)
            {
                var incomingConnection = GetIncomingConnections(node).Single();
                var outgoingConnection = GetOutgoingConnection(node);
                if (excludedNodes != null &&
                    excludedNodes.Any(n => n.IsAlmostEqualTo(node.Position)))
                {
                    continue;
                }
                if (includeDiameterCheck && incomingConnection.Diameter != outgoingConnection.Diameter)
                {
                    continue;
                }

                if (incomingConnection.Path().IsCollinear(outgoingConnection.Path()))
                {
                    ShiftConnectionToNode(incomingConnection, outgoingConnection.End);
                    Disconnect(outgoingConnection);
                }
            }
        }

        /// <summary>
        /// Heals any splits in the given connections. Healing is possible if there is
        /// exactly one incoming and one outgoing connection to the same node and if the
        /// two connections are aligned.
        /// </summary>
        /// <param name="connectionsToHeal"></param>
        public void HealSplits(IEnumerable<Connection> connectionsToHeal)
        {
            foreach (var connection in connectionsToHeal)
            {
                var outgoingConnections = GetOutgoingConnections(connection.End);
                if (outgoingConnections.Count != 1)
                {
                    continue;
                }
                var outgoing = outgoingConnections.First();
                if (!connectionsToHeal.Contains(outgoing))
                {
                    continue;
                }
                var angle = outgoing.Direction().AngleTo(connection.Direction());
                if (angle == 0)
                {
                    this.ShiftConnectionToNode(connection, outgoing.End);
                    this.Disconnect(outgoing);
                }
            }
            this.UpdateSections();
        }

        /// <summary>
        /// Force this connection to handle any Z change with a vertical connection by adding new connections as necessary.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="distanceFromStart"></param>
        /// <param name="flipOffset"></param>
        public void ConnectVertically(Connection connection, double distanceFromStart, bool flipOffset = false)
        {
            if ((connection.Start.Position.X.ApproximatelyEquals(connection.End.Position.X)
                  && connection.Start.Position.Y.ApproximatelyEquals(connection.End.Position.Y))
                 )
            {
                // The connection is already vertical.
                return;
            }
            if (connection.Start.Position.Z.ApproximatelyEquals(connection.End.Position.Z))
            {
                // The two points of the connection have no vertical offset.
                return;
            }
            var end = connection.End;

            var flatLine = new Line(connection.Start.Position, new Vector3(connection.End.Position.X,
                                                                           connection.End.Position.Y,
                                                                           connection.Start.Position.Z));

            var dropPoint = flipOffset ? flatLine.End + -distanceFromStart * flatLine.Direction()
                                       : flatLine.Start + distanceFromStart * flatLine.Direction();

            if (distanceFromStart.ApproximatelyEquals(0))
            {
                if (flipOffset)
                {
                    SplitConnectionThroughPoint(connection, new Vector3(dropPoint.X, dropPoint.Y, connection.Start.Position.Z), out _);
                }
                else
                {
                    SplitConnectionThroughPoint(connection, new Vector3(dropPoint.X, dropPoint.Y, connection.End.Position.Z), out _);
                }
            }
            else
            {
                var firstNew = SplitConnectionThroughPoint(connection, new Vector3(dropPoint.X, dropPoint.Y, connection.Start.Position.Z), out _);
                var nextConnToReplace = GetOutgoingConnection(firstNew);
                var secondNew = SplitConnectionThroughPoint(nextConnToReplace, new Vector3(dropPoint.X, dropPoint.Y, end.Position.Z), out _);
            }
        }

        /// <summary>
        /// Sets the position of the outlet, creating one if it does not exist.
        /// </summary>
        /// <param name="position">The desired position of the outlet.</param>
        public Trunk SetOutletPosition(Vector3 position)
        {
            if (this.Connections == null)
            {
                throw new Exception($"This tree has not been initialized yet, use the {nameof(Tree)} constructor that takes in regionReferences.");
            }
            if (this.Outlet == null)
            {
                this.Outlet = new Trunk(0, GetNetworkReference(), position, Guid.NewGuid(), "");
                return this.Outlet;
            }
            else
            {
                this.Outlet.Position = position;
                this.Outlet.NetworkReference = GetNetworkReference();
                return this.Outlet;
            }
        }

        /// <inheritdoc/>
        public override void UpdateRepresentations()
        {
            if (this.Representation == null)
            {
                this.Representation = new Representation(new List<SolidOperation>());
            }
            this.Representation.SolidOperations = new List<SolidOperation>();

            var nodeSize = 0.1;

            var nodeProfile = Polygon.Rectangle(nodeSize, nodeSize);

            var sectionIntersections = InternalNodes.Where(n => GetIncomingConnections(n).Count > 1 || GetOutgoingConnections(n).Count > 1);

            foreach (var n in sectionIntersections)
            {
                var node = new Extrude(nodeProfile.TransformedPolygon(new Transform(n.Position + new Vector3(0, 0, -nodeSize / 2))), nodeSize, Vector3.ZAxis, false);
                this.Representation.SolidOperations.Add(node);
            }
            if (!SkipConnectionsInRepresentation)
            {
                foreach (var c in Connections)
                {
                    // slightly smaller diameter so it will be rendered "underneath" sections or pipes downstream.
                    var circle = new Circle(c.Diameter > 0 ? (c.Diameter - Connection.DIAMETER_INSET * 4) / 2 : Connection.DEFAULT_CONNECTION_DIAMETER - Connection.DIAMETER_INSET).ToPolygon(FlowSystemConstants.CIRCLE_SEGMENTS);
                    var s = new Sweep(circle, c.Path(), 0, 0, 0, false);
                    this.Representation.SolidOperations.Add(s);
                }
            }
        }

        /// <summary>
        /// Split a connection that may be angled relative to some coordinate system into two connections, with the specified angle, with atleast one
        /// connection aligned with the primary axis. Which connection is aligned depends on the NormalizationType, the other connection is not guaranteed to be aligned,
        /// depending on the angle specified.
        /// </summary>
        /// <param name="flowConnection"></param>
        /// <param name="planeNormal"></param>
        /// <param name="primaryAxis"></param>
        /// <param name="angle"></param>
        /// <param name="normalizationType"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public Connection[] NormalizeConnectionPath(Connection flowConnection, Vector3 planeNormal, Vector3 primaryAxis, double angle, NormalizationType normalizationType = NormalizationType.Start)
        {
            // TODO consider doing some helpful normalization of the plane normal and primary axis to make sure they are orthogonal,
            // we currently require the use to get that right.  there is a normalized relationship between the primary axis, connection
            // direction, and the normal that is implicit here.
            if (angle > 90 || angle < 0)
            {
                throw new Exception("angle must be between 0 and 90");
            }
            if (flowConnection.Direction().Dot(primaryAxis).Equals(0) || flowConnection.Direction().ProjectOnto(primaryAxis).Length().ApproximatelyEquals(0))
            {
                return Array.Empty<Connection>();
            }
            var connectionAngleToAxis = flowConnection.Direction().AngleTo(primaryAxis);
            if (connectionAngleToAxis.ApproximatelyEquals(90 - angle, 0.1) || (connectionAngleToAxis - 90).ApproximatelyEquals(angle, 0.1)
            || connectionAngleToAxis.ApproximatelyEquals(180, 0.1) || connectionAngleToAxis.ApproximatelyEquals(0, 0.1))
            {
                return Array.Empty<Connection>();
            }
            var connectionVec = flowConnection.Direction() * (flowConnection.Start.Position.DistanceTo(flowConnection.End.Position));
            var secondaryAxis = primaryAxis.Cross(planeNormal).Unitized();
            var parallel = connectionVec.ProjectOnto(primaryAxis);
            var perp = connectionVec.ProjectOnto(secondaryAxis);

            var longAxis = parallel.Length() >= perp.Length() ? parallel : perp;
            var shortAxis = parallel.Length() >= perp.Length() ? perp : parallel;

            var longDiff = shortAxis.Length() / Math.Tan(angle * Math.PI / 180);

            var newConns = Array.Empty<Connection>();
            switch (normalizationType)
            {
                case NormalizationType.Start:
                    var hitPoint = flowConnection.Start.Position + longAxis.Unitized() * (longAxis.Length() - longDiff);
                    SplitConnectionThroughPoint(flowConnection, hitPoint, out newConns);
                    break;
                default:
                case NormalizationType.End:
                    hitPoint = flowConnection.End.Position - longAxis.Unitized() * (longAxis.Length() - longDiff);
                    SplitConnectionThroughPoint(flowConnection, hitPoint, out newConns);
                    break;
                case NormalizationType.Middle:
                    var hitPoint1 = flowConnection.Start.Position + longAxis.Unitized() * (longAxis.Length() - longDiff) / 2;
                    SplitConnectionThroughPoint(flowConnection, hitPoint1, out var firstConns);
                    var hitPoint2 = flowConnection.End.Position - longAxis.Unitized() * (longAxis.Length() - longDiff) / 2;
                    SplitConnectionThroughPoint(firstConns[1], hitPoint2, out var secondConns);
                    newConns = new[] { firstConns[0], secondConns[0], secondConns[1] };
                    break;
            }
            return newConns;
        }

        /// <summary>
        /// Run the specified function on each connection in the network recursing from the starting connection.
        /// </summary>
        /// <param name="startingConnection">First connection that will run the code.</param>
        /// <param name="action"></param>
        public void RecurseTrunkside(Connection startingConnection, Action<Connection> action)
        {
            action(startingConnection);
            var trunkSide = GetOutgoingConnections(startingConnection.End);
            if (!trunkSide.Any())
            {
                return;
            }
            foreach (var c in trunkSide)
            {
                RecurseTrunkside(c, action);
            }
        }

        /// <summary>
        /// Run the specified function on each connection in the network recursing from the starting connection.
        /// </summary>
        /// <param name="startingConnection">First connection that will run the code.</param>
        /// <param name="action"></param>
        public void RecurseBranchside(Connection startingConnection, Action<Connection> action)
        {
            action(startingConnection);
            var branchSide = GetIncomingConnections(startingConnection.Start);
            if (!branchSide.Any())
            {
                return;
            }
            foreach (var c in branchSide)
            {
                RecurseBranchside(c, action);
            }
        }

        /// <summary>
        /// Create a clone of this tree with new ids for all nodes and connections.
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            return Clone(out _);
        }

        /// <summary>
        /// Create a clone and return a lookup to map old connections to the new connections.
        /// </summary>
        /// <param name="newConnectionLookup">Mapping from the original connections to the new connections.</param>
        /// <returns></returns>
        public Tree Clone(out Dictionary<Connection, Connection> newConnectionLookup)
        {
            var nodeLookup = InternalNodes.ToDictionary(n => n, n => new Node(n.Position, Guid.NewGuid(), n.Name));
            var internalNodes = nodeLookup.Values.ToList();


            var outlet = new Trunk(Outlet.Flow,
                                   Outlet.NetworkReference,
                                   Outlet.Position,
                                   Guid.NewGuid(),
                                   Outlet.Name);
            nodeLookup.Add(Outlet, outlet);

            var inlets = new List<Leaf>();
            Inlets.ToList().ForEach(oldInlet =>
            {
                var newInlet = new Leaf(oldInlet.Flow,
                                        oldInlet.TerminalId,
                                        oldInlet.Position,
                                        Guid.NewGuid(),
                                        oldInlet.Name);
                nodeLookup.Add(oldInlet, newInlet);
                inlets.Add(newInlet);
            });

            newConnectionLookup = Connections.ToDictionary(c => c, c => new Connection(nodeLookup[c.Start], nodeLookup[c.End], Guid.NewGuid(), c.Name) { Diameter = c.Diameter, AdditionalProperties = new Dictionary<string, object>(c.AdditionalProperties), IsLoop = c.IsLoop });

            var connections = newConnectionLookup.Values.ToList();

            var tree = new Tree(internalNodes,
                                                outlet,
                                                inlets,
                                                connections,
                                                OutletFlow,
                                                RegionReferences,
                                                Purpose,
                                                Transform,
                                                Material,
                                                Representation,
                                                IsElementDefinition,
                                                Guid.NewGuid(),
                                                Name);

            tree.AdditionalProperties = this.AdditionalProperties;
            tree.UpdateSections();
            return tree;
        }

        /// <summary>
        /// Positional options for connection normalization.
        /// </summary>
        public enum NormalizationType
        {
            Start,
            End,
            Middle
        }
    }
}