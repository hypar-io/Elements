using System;
using System.Collections.Generic;
using System.Linq;
using Elements.Flow;
using Elements.Geometry;

namespace Elements.Fittings
{
    // TODO I'd like to rename to FittingTreeBuilder
    /// <summary>
    /// This class is responsible for creating a fitting tree from a Tree.
    /// It does this by choosing the correct fitting at each connection point in a Tree.
    /// A number of methods are overridable to change the behavior of the fitting tree routing.
    /// </summary>
    public class FittingTreeRouting
    {
        public static Material DefaultPipeMaterial = new Material("Pipe Segment",
                                                    new Color(0, 0.4, 0.4, 0.4),
                                                    specularFactor: 1,
                                                    glossinessFactor: 1,
                                                    unlit: false,
                                                    doubleSided: false,
                                                    repeatTexture: false,
                                                    interpolateTexture: false,
                                                    id: Guid.NewGuid());
        public static Material DefaultFittingMaterial = BuiltInMaterials.Black;
        private const double lengthMultiplier = 1.1;
        public Material PipeMaterial = DefaultPipeMaterial;

        /// <summary>
        /// Default value for PipeAngleTolerance and also in case a FittingTreeRouting instance is not available.
        /// </summary>
        public const double DefaultAngleTolerance = 1;

        /// <summary>
        /// The AngleTolerance tolerance that will be used internally to measure angles for Pipes and Ports
        /// </summary>
        public double AngleTolerance { get; protected set; } = DefaultAngleTolerance;
        
        /// <summary>
        /// The PortsDistanceTolerance tolerance that will be used internally to measure distances between ports
        /// </summary>
        public double PortsDistanceTolerance { get; protected set; } = 0.001;

        public static double DefaultDiameter = 0.04;

        /// <summary>
        /// An optional flow calculator that will be used to compute to compute
        /// either full flow or flow with some of the equipment excluded from the system
        /// </summary>
        public FlowCalculator FlowCalculator { get; set; } = new FullFlowCalculator();

        /// <summary>
        /// A Flow.Tree that is intended to be converted into a FittingTree.
        /// </summary>
        [Obsolete("Use Tree instead.")]
        public Tree Collection
        {
            get { return Tree; }
            private set { Tree = value; }
        }

        /// <summary>
        /// A Flow.Tree that is intended to be converted into a FittingTree.
        /// </summary>
        public Tree Tree { get; private set; }

        /// <summary>
        /// An optional pressure calculator that will be used to compute the pressures
        /// for all of the ports of the fittings and straight segments in the resulting FittingTree.
        /// </summary>
        public PressureCalculator PressureCalculator { get; set; }

        /// <summary>
        /// An optional fitting catalog that will be used to search for fitting of the suitable size from it.
        /// </summary>
        public FittingCatalog FittingCatalog { get; set; }

        /// <summary>
        /// What are the allowed angles to use for Wye fittings?
        /// </summary>
        public double[] AllowedWyeBranchAngles { get; set; } = new[] { 45.0, 90.0, 180.0 };

        /// <summary>
        /// Enforce the rule that the pipe size always matches the Flow.Connector size.
        /// </summary>
        public bool PipeSizeShouldMatchConnection { get; set; } = false;

        /// <summary>
        /// Build a FittingTree from the internally stored Tree.
        /// </summary>
        /// <param name="errors"></param>
        /// <returns></returns>
        public FittingTree BuildFittingTree(out List<FittingError> errors, FlowDirection flowDirection = FlowDirection.TowardTrunk)
        {
            return FittingTree.InternalBuildTree(Tree, out errors, this, flowDirection, AngleTolerance, PortsDistanceTolerance);
        }

        /// <summary>
        /// Build a FittingTree from the given tree.
        /// This method cannot be called if the FittingTreeRouting has a tree already stored.
        /// </summary>
        /// <param name="tree"></param>
        /// <param name="errors"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public FittingTree BuildFittingTree(Tree tree, out List<FittingError> errors)
        {
            if (Tree != null && tree != null)
            {
                throw new Exception("This FittingTreeRouting is already bound to a Tree. Create a new instance.");
            }

            return FittingTree.InternalBuildTree(tree, out errors, this, angleTolerance:AngleTolerance, portsDistanceTolerance:PortsDistanceTolerance);
        }
        
        /// <summary>
        /// Create an instance of the routing with a set Tree.
        /// </summary>
        /// <param name="tree"></param>
        public FittingTreeRouting(Tree tree)
        {
            Tree = tree;
        }

        /// <summary>
        /// Create a new instance of the routing.
        /// </summary>
        /// <returns></returns>
        public FittingTreeRouting Clone()
        {
            return MemberwiseClone() as FittingTreeRouting;
        }

        /// <summary>
        /// Create a new instance of this routing with a different flow tree built in.
        /// </summary>
        /// <param name="tree"></param>
        /// <returns></returns>
        public virtual FittingTreeRouting CloneWithDifferentCollection(Tree tree)
        {
            var clone = Clone();
            clone.Tree = tree;
            clone.Tree.AdditionalProperties = tree.AdditionalProperties;
            return clone;
        }

        /// <summary>
        /// Create a fitting for a node with the ability to specify alternative connection diameters.
        /// This is useful for getting a fitting to estimate the impact of a change in diameter.
        /// </summary>
        /// <param name="node">The node to build a fitting for.</param>
        /// <param name="alternativeConnectionDiameters">A dictionary of alternative diameters for connections on the node.</param>
        /// <param name="errorDetail"></param>
        /// <returns>The fitting created at that node.</returns>
        /// <exception cref="ArgumentException"></exception>
        public Fitting FittingForNodeWithAlternativeDiameters(Node node, Dictionary<Connection, double> alternativeConnectionDiameters, out string errorDetail)
        {
            if (!Tree.HasNode(node))
            {
                throw new ArgumentException("Cannot make a connection, that node does not exist in the routing Tree.");
            }
            var allConnections = Tree.GetIncomingConnections(node);
            var outgoing = Tree.GetOutgoingConnection(node);
            if (outgoing != null)
            {
                allConnections.Add(outgoing);
            }
            var originalDiameterLookup = allConnections.ToDictionary(c => c, c => c.Diameter);

            // Set the diameter to the fixed diameter
            allConnections.ForEach(c =>
            {
                if (alternativeConnectionDiameters.TryGetValue(c, out var diameter))
                {
                    c.Diameter = diameter;
                }
            });

            var fitting = FittingForNode(node, out errorDetail, out _);
            if (fitting is Assembly assembly)
            {
                assembly.AssignReferenceBasedOnSection(Tree.GetSectionFromConnection(outgoing));
                assembly.AssignTrunkComponentsInternally(outgoing);
                foreach (var incoming in Tree.GetIncomingConnections(node))
                {
                    assembly.AssignNetworkLocatorAlongBranchComponents(incoming, new FittingLocator(Tree.GetNetworkReference(), Tree.GetSectionFromConnection(incoming).SectionKey, 0));
                }
            }

            // Restore the original diameter
            allConnections.ForEach(c => c.Diameter = originalDiameterLookup[c]);

            return fitting;
        }

        /// <summary>
        /// Get a fitting for a node.  Uses the overridable methods to determine the fitting to use.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="errorDetail"></param>
        /// <param name="absorbedNodes"></param>
        /// <returns>The fitting for a node.</returns>
        /// <exception cref="ArgumentException"></exception>
        public Fitting FittingForNode(Node node, out string errorDetail, out Node[] absorbedNodes)
        {
            if (!Tree.HasNode(node))
            {
                throw new ArgumentException("Cannot make a connection, that node does not exist in the routing Tree.");
            }
            Fitting newConnection = null;
            errorDetail = string.Empty;
            var outgoing = Tree.GetOutgoingConnections(node);
            var incoming = Tree.GetIncomingConnections(node);

            var mainOutgoing = Tree.GetOutgoingConnection(node);
            var loopOutgoing = outgoing.Where(c => c.IsLoop == true) ?? new List<Connection>();
            var invertedLoopOutgoing = loopOutgoing.Select(lc => new Connection(lc.End, lc.Start, lc.Diameter, lc.Flow));

            var allIncomingIncludingLoops = new List<Connection>(incoming);
            allIncomingIncludingLoops.AddRange(invertedLoopOutgoing);
            absorbedNodes = new Node[0];
            switch (incoming.Count)
            {
                case 0:
                    if (outgoing.Count == 0)
                    {
                        break;
                    }
                    else
                    {
                        if (outgoing.Count > 1)
                        {
                            // can't have 0 incoming and more than 1 outgoing
                            break;
                        }
                        newConnection = TerminatePipe(null, outgoing.First(), out absorbedNodes);
                        if (newConnection is Terminal terminal)
                        {
                            terminal.FlowNode = outgoing.First().Start;
                        }
                        break;
                    }
                case 1:
                    if (outgoing.Count == 0)
                    {
                        newConnection = TerminatePipe(incoming[0], null, out absorbedNodes);
                        newConnection.ComponentLocator.NetworkReference = Tree.GetNetworkReference();
                        newConnection.ComponentLocator.SectionKey = Tree.GetSectionFromConnection(incoming[0]).SectionKey;
                        if (newConnection is Terminal terminal)
                        {
                            terminal.FlowNode = incoming[0].End;
                        }
                        break;
                    }
                    if (outgoing.Count == 1)
                    {
                        var angle = incoming[0].Direction().AngleTo(outgoing.First().Direction());
                        var isStraight = angle.ApproximatelyEquals(0, AngleTolerance)
                                         || angle.ApproximatelyEquals(180, AngleTolerance);
                        if (isStraight)
                        {
                            newConnection = ChangePipe(incoming[0], outgoing.First());
                        }
                        else
                        {
                            newConnection = ChangeDirection(incoming[0], outgoing.First());
                        }
                    }
                    else if (outgoing.Count == 2)
                    {
                        newConnection = BranchPipe(incoming[0], invertedLoopOutgoing.First(), mainOutgoing);
                    }
                    else
                    {
                        newConnection = ManifoldPipe(allIncomingIncludingLoops, mainOutgoing);
                    }
                    break;
                case 2:
                    if (outgoing.Count == 0)
                    {
                        break;
                    }
                    if (outgoing.Count == 1)
                    {
                        newConnection = BranchPipe(incoming[0], incoming[1], outgoing.First());
                    }
                    else
                    {
                        newConnection = ManifoldPipe(allIncomingIncludingLoops, mainOutgoing);
                    }
                    break;
                case var _ when incoming.Count > 2:
                    if (outgoing.Count == 0)
                    {
                        break;
                    }
                    newConnection = ManifoldPipe(allIncomingIncludingLoops, mainOutgoing);
                    break;
                default:
                    break;
            }
            if (newConnection != null)
            {
                newConnection.ComponentLocator = new FittingLocator(Tree.GetNetworkReference(), Tree.GetSectionFromConnection(mainOutgoing ?? incoming[0]).SectionKey, 0);
            }

            return newConnection;
        }

        public virtual IReducer ReduceOrJoin(StraightSegment pipe, bool reducerAtEnd, double newDiameter, double additionalDistance = 0)
        {
            var length = 0.03;
            var extensionStart = 0d;
            var extensionEnd = 0d;
            if (FittingCatalog != null)
            {
                var largeDiameter = Math.Max(pipe.Diameter, newDiameter);
                var smallDiameter = Math.Min(pipe.Diameter, newDiameter);
                var reducerPart = FittingCatalog.GetBestReducerPart(largeDiameter, smallDiameter);
                if (reducerPart != null)
                {
                    length = reducerPart.Length;
                    extensionStart = reducerPart.ExtensionLarge;
                    extensionEnd = reducerPart.ExtensionSmall;
                }
                else
                {
                    return null;
                }
            }

            Reducer reducer = Reducer.ReducerForPipe(pipe, length, reducerAtEnd, newDiameter, additionalDistance);

            if (reducer.Start.Diameter < reducer.End.Diameter)
            {
                (extensionStart, extensionEnd) = (extensionEnd, extensionStart);
            }
            reducer.Start.Dimensions = new PortDimensions(extensionStart, 0, 0);
            reducer.End.Dimensions = new PortDimensions(extensionEnd, 0, 0);
            return reducer;
        }

        public virtual Fitting BranchPipe(Connection incoming1, Connection incoming2, Connection outgoing)
        {
            (Connection mainConnection, Connection branchConnection) = Wye.GetMainAndBranch(new[] { incoming1, incoming2 }, outgoing);

            var trunkDiameter = outgoing.Diameter.ApproximatelyEquals(0) ? DefaultDiameter : outgoing.Diameter;
            var branchDiameter = branchConnection.Diameter.ApproximatelyEquals(0) ? DefaultDiameter : branchConnection.Diameter;
            var mainDiameter = trunkDiameter;

            var isTeeConnection = false;
            // If branch and main point at one another, we default to making main and branch both equal trunk diameter.
            if (mainConnection.Direction().AngleTo(branchConnection.Direction()).ApproximatelyEquals(180, 1))
            {
                branchDiameter = trunkDiameter;
                mainDiameter = trunkDiameter;
                isTeeConnection = true;
            }

            WyeSettings wyes = new WyeSettings()
            {
                TrunkDistance = trunkDiameter * lengthMultiplier,
                BranchDiameter = branchDiameter,
                Diameter = trunkDiameter,
                BranchDistance = trunkDiameter * lengthMultiplier,
                MainDiameter = mainDiameter,
                MainDistance = mainDiameter * lengthMultiplier,
                AllowedBranchAngles = AllowedWyeBranchAngles,
                AngleTolerance = AngleTolerance
            };

            var mainExtension = 0d;
            var branchExtension = 0d;
            if (FittingCatalog != null)
            {
                var angle = RoundAngle(branchConnection.Direction().AngleTo(mainConnection.Direction()));
                var teePart = FittingCatalog.GetBestTeePart(trunkDiameter, branchDiameter, angle);
                if (teePart != null)
                {
                    var sideLength = teePart.BranchLength;
                    wyes.MainDistance = teePart.MainLength;
                    if (isTeeConnection)
                    {
                        wyes.TrunkDistance = sideLength;
                        wyes.BranchDistance = teePart.TrunkLength;
                    }
                    else
                    {
                        wyes.TrunkDistance = teePart.TrunkLength;
                        wyes.BranchDistance = sideLength;
                    }
                    wyes.MainDiameter = teePart.Diameter;
                    wyes.BranchDiameter = teePart.BranchDiameter;
                    wyes.Diameter = teePart.Diameter;

                    mainExtension = teePart.Extension;
                    branchExtension = teePart.BranchExtension;
                }
                else
                {
                    return null;
                }
            }

            // TODO We should get rid of the need for WyeSettings. Tee part should be able to replace the WyeSetting entirely.
            var wye = new Wye(incoming1.End.Position, outgoing.Direction(), mainConnection.Direction().Negate(), branchConnection.Direction().Negate(), wyes, DefaultFittingMaterial);
            wye.Trunk.Dimensions = new PortDimensions(mainExtension, 0, 0);
            wye.MainBranch.Dimensions = new PortDimensions(mainExtension, 0, 0);
            wye.SideBranch.Dimensions = new PortDimensions(branchExtension, 0, 0);
            return wye;
        }

        public virtual Fitting ManifoldPipe(IEnumerable<Connection> incoming, Connection outgoing)
        {
            switch (incoming.Count())
            {
                case 3:
                    var main = incoming.FirstOrDefault(c => c.Direction().AngleTo(outgoing.Direction()).ApproximatelyEquals(0, 1));
                    if (main == null)
                    {
                        if (FittingCatalog != null)
                        {
                            return null;
                        }
                        // only support cross fittings where BranchA goes straight through. For other cases use Manifold
                        return new Manifold(outgoing.Start.Position,
                                            outgoing.Direction(),
                                            NonZeroDiameter(outgoing),
                                            incoming.Select(c => (c.Direction().Negate(), NonZeroDiameter(c))).ToList());
                    }
                    var angles = incoming.Select(c => c.Direction().AngleTo(outgoing.Direction()));
                    var others = incoming.Where(c => !c.Direction().AngleTo(outgoing.Direction()).ApproximatelyEquals(0, 1));
                    var branchB = others.ElementAt(0);
                    var branchC = others.ElementAt(1);

                    var settings = new CrossSettings()
                    {
                        AllowedBranchAngles = AllowedWyeBranchAngles,
                        Diameter_A = NonZeroDiameter(main),
                        Diameter_B = NonZeroDiameter(branchB),
                        Diameter_C = NonZeroDiameter(branchC),
                        Diameter = NonZeroDiameter(outgoing),
                    };

                    var angleB = RoundAngle(branchB.Direction().AngleTo(main.Direction()));
                    var angleC = RoundAngle(branchC.Direction().AngleTo(main.Direction()));
                    var extension = 0d;
                    var extensionB = 0d;
                    var extensionC = 0d;

                    if (FittingCatalog != null)
                    {
                        var crossPart = FittingCatalog.GetBestCrossPart(outgoing.Diameter, branchB.Diameter, branchC.Diameter, angleB, angleC);
                        if (crossPart != null)
                        {
                            settings.Distance_Trunk = crossPart.PipeLength / 2;
                            settings.Distance_A = crossPart.PipeLength / 2;
                            settings.Distance_B = crossPart.BranchLength1;
                            settings.Distance_C = crossPart.BranchLength2;

                            settings.Diameter = crossPart.PipeDiameter;
                            settings.Diameter_A = crossPart.PipeDiameter;
                            settings.Diameter_B = crossPart.BranchDiameter1;
                            settings.Diameter_C = crossPart.BranchDiameter2;

                            extension = crossPart.PipeExtension;
                            extensionB = crossPart.BranchExtension1;
                            extensionC = crossPart.BranchExtension2;
                        }
                        else
                        {
                            return null;
                        }
                    }

                    // TODO We should get rid of the need for CrossSettings. Cross part should be able to replace the CrossSettings entirely.
                    var cross = new Cross(outgoing.Start.Position,
                                          outgoing.Direction(),
                                          main.Direction().Negate(),
                                          branchB.Direction().Negate(),
                                          branchC.Direction().Negate(), settings);
                    cross.Trunk.Dimensions = new PortDimensions(extension, 0, 0);
                    cross.BranchA.Dimensions = new PortDimensions(extension, 0, 0);
                    cross.BranchB.Dimensions = new PortDimensions(extensionB, 0, 0);
                    cross.BranchC.Dimensions = new PortDimensions(extensionC, 0, 0);
                    return cross;
                default:
                    if (FittingCatalog != null)
                    {
                        return null;
                    }
                    return new Manifold(outgoing.Start.Position,
                                        outgoing.Direction(),
                                        NonZeroDiameter(outgoing),
                                        incoming.Select(c => (c.Direction().Negate(), NonZeroDiameter(c))).ToList());
            }
        }

        /// <summary>
        /// Base implementation for terminating a pipe.  The termination can be either at the start or end of the system, Leaf or Trunk nodes.
        /// </summary>
        /// <param name="incoming"></param>
        /// <param name="outgoing"></param>
        /// <param name="absorbedNodes"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual Fitting TerminatePipe(Connection incoming, Connection outgoing, out Node[] absorbedNodes)
        {
            absorbedNodes = new Node[0];
            if (incoming != null && outgoing != null)
            {
                throw new Exception("Shouldn't create terminal for both incoming and outgoing");
            }
            if (outgoing != null)
            {
                var diameter = !outgoing.Diameter.ApproximatelyEquals(0) ? outgoing.Diameter : DefaultDiameter;
                var terminal = new Terminal(outgoing.Start.Position, outgoing.Direction(), 0.03, diameter, DefaultFittingMaterial);
                terminal.Port.Dimensions = new PortDimensions(0, 0, 0);
                return terminal;
            }
            else if (incoming != null)
            {
                var diameter = !incoming.Diameter.ApproximatelyEquals(0) ? incoming.Diameter : DefaultDiameter;
                var terminal = new Terminal(incoming.End.Position, incoming.Direction().Negate(), 0.03, diameter, DefaultFittingMaterial);
                terminal.Port.Dimensions = new PortDimensions(0, 0, 0);
                return terminal;
            }
            else
            {
                throw new ArgumentNullException("Both connections to terminate were null");
            }
        }

        /// <summary>
        /// Base implementation for a change of direction.  This is often used to create an elbow.
        /// </summary>
        /// <param name="incoming"></param>
        /// <param name="outgoing"></param>
        /// <returns></returns>
        public virtual Fitting ChangeDirection(Connection incoming, Connection outgoing)
        {
            var larger = incoming.Diameter > outgoing.Diameter ? incoming.Diameter : outgoing.Diameter;
            var diameter = !larger.ApproximatelyEquals(0) ? larger : DefaultDiameter;
            return CreateElbow(diameter, incoming.End.Position, incoming.Direction().Negate(), outgoing.Direction());
        }

        public virtual Elbow CreateElbow(double diameter, Vector3 position, Vector3 startDirection, Vector3 endDirection)
        {
            var diameterInInches = Units.MetersToInches(diameter);

            double sideLength;
            double extension = 0d;
            if (FittingCatalog == null)
            {
                sideLength = diameter * lengthMultiplier;
            }
            else
            {
                var angle = RoundAngle(startDirection.AngleTo(endDirection));
                var elbowPart = FittingCatalog.GetBestElbowPart(diameter, angle);
                if (elbowPart != null)
                {
                    sideLength = elbowPart.SideLength;
                    diameter = elbowPart.Diameter;
                    extension = elbowPart.Extension;
                }
                else
                {
                    return null;
                }
            }

            var elbow = new Elbow(position, startDirection, endDirection, sideLength, diameter, DefaultFittingMaterial);
            elbow.Start.Dimensions = new PortDimensions(extension, 0, 0);
            elbow.End.Dimensions = new PortDimensions(extension, 0, 0);
            return elbow;
        }

        public virtual Fitting ChangePipe(Connection incoming, Connection outgoing)
        {
            var length = 0.2;
            var extensionStart = 0d;
            var extensionEnd = 0d;
            var largeDiameter = Math.Max(incoming.Diameter, outgoing.Diameter);
            var smallDiameter = Math.Min(incoming.Diameter, outgoing.Diameter);

            if (FittingCatalog != null)
            {
                var reducerPart = FittingCatalog.GetBestReducerPart(largeDiameter, smallDiameter);
                if (reducerPart != null)
                {
                    length = reducerPart.Length;
                    extensionStart = reducerPart.ExtensionLarge;
                    extensionEnd = reducerPart.ExtensionSmall;
                }
                else
                {
                    return null;
                }
            }

            var reducer = new Reducer(incoming.End.Position,
                                      incoming.Direction().Negate(),
                                      incoming.Diameter,
                                      outgoing.Diameter,
                                      length,
                                      DefaultFittingMaterial);

            if (reducer.Start.Diameter < reducer.End.Diameter)
            {
                (extensionStart, extensionEnd) = (extensionEnd, extensionStart);
            }
            reducer.Start.Dimensions = new PortDimensions(extensionStart, 0, 0);
            reducer.End.Dimensions = new PortDimensions(extensionEnd, 0, 0);

            return reducer;
        }

        private double NonZeroDiameter(Connection connection)
        {
            return connection.Diameter.ApproximatelyEquals(0) ? DefaultDiameter : connection.Diameter;
        }

        private double RoundAngle(double angle)
        {
            return Math.Round(angle / AngleTolerance, 0) * AngleTolerance;
        }
    }
}