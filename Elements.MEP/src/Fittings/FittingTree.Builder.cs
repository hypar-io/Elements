using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Elements.Flow;
using Elements.Geometry;
using Newtonsoft.Json;

[assembly: InternalsVisibleTo("Elements.MEP.Tests")]
namespace Elements.Fittings
{
    public partial class FittingTree
    {
        private double _angleTolerance = 0.1;
        private double _portsDistanceTolerance = 0.001;

        [JsonIgnore]
        private FittingTreeRouting _routing;
        [JsonIgnore]
        private readonly Dictionary<Port, Connection> trunkPortToConnectionLookup = new Dictionary<Port, Connection>();

        /// <summary>
        /// The routing used to build this FittingTree.
        /// </summary>
        [JsonIgnore]
        public FittingTreeRouting Routing { get { return _routing; } }

        [JsonIgnore]
        [Obsolete("Use the Tree property instead.")]
        public Tree Collection { get { return _routing.Collection; } }

        /// <summary>
        /// The Flow.Tree that this FittingTree was built from.
        /// </summary>
        [JsonIgnore]
        public Tree Tree { get { return _routing.Tree; } }

        /// <summary>
        /// Get all fittings of a given type.
        /// </summary>
        /// <typeparam name="T">The type of fittings you would like back.</typeparam>
        /// <returns></returns>
        public List<T> FittingsOfType<T>() where T : Fitting
        {
            return Fittings.Where(c => c is T).Cast<T>().ToList();
        }

        // TODO make the logic in the FittingTree.Builder file all live on the FittingTreeRouting class.
        // The routing now has the Collector, and so we should pass one in from the outside, the routing
        // should have the methods to build a FittingTree.
        internal static FittingTree InternalBuildTree(
            Tree tree,
            out List<FittingError> errors,
            FittingTreeRouting routing = null,
            FlowDirection flowDirection = FlowDirection.TowardTrunk,
            double angleTolerance = 0.5,
            double portsDistanceTolerance = 0.001)
        {
            if (routing != null && routing.Tree == null)
            {
                // TODO remove this when we refactor to always have a tree and routing together.
                routing = routing.CloneWithDifferentCollection(tree);
            }
            FittingTree fittings = FittingTree.Create(routing ?? new FittingTreeRouting(tree));
            fittings.FlowDirection = flowDirection;
            fittings._angleTolerance = angleTolerance;
            fittings._portsDistanceTolerance = portsDistanceTolerance;
            errors = new List<FittingError>();

            try
            {
                fittings.RecursiveBuildFittingTreeFromTrunk(tree.Outlet, null);
                fittings.RemoveAbsorbedFittings();
            }
            catch (CannotMakeConnectionException e) when (!Env.DisableCatchExceptions)
            {
                errors.Add(new FittingError(e.Message, e.Location));
                return fittings;
            }

            if (fittings == null)
            {
                errors.Add(new FittingError($"Failed to make the fittings for {tree.GetNetworkReference()}",
                                            tree.Outlet.Position));
                return null;
            }

            var failedPiping = fittings.CreateStraightSegmentsBetweenFittings();
            errors.AddRange(failedPiping);

            fittings.AssignGuidIds();
            var labelingErrors = fittings.AssignFlowIdsToComponents();
            errors.AddRange(labelingErrors);

            List<FittingError> flowErrors = null;
            if (routing.FlowCalculator != null)
            {
                // We have to assign pressures after we assign flow ids, because the pressure assignment depends on the component locators.
                // we fix this when we traverse via branch instead of by keys.
                if (routing.PressureCalculator != null)
                {
                    List<FittingError> pressureErrors = null;
                    const int maxIterations = 100;
                    routing.FlowCalculator.FlowUpdateStrategy?.Reset();

                    for (int iteration = 0; iteration < maxIterations; iteration++)
                    {
                        flowErrors = routing.FlowCalculator.AssignFlowCalcs(fittings);
                        pressureErrors = routing.PressureCalculator.UpdatePressureCalcs(fittings);

                        if (flowErrors.Any() || pressureErrors.Any())
                        {
                            break;
                        }

                        if (!routing.FlowCalculator.UpdateLeafFlow(fittings))
                        {
                            break;
                        }
                    }
                    tree.UpdateSections();
                    errors.AddRange(pressureErrors);
                }
                else
                {
                    flowErrors = routing.FlowCalculator.AssignFlowCalcs(fittings);
                }
                errors.AddRange(flowErrors);
            }

            return fittings;
        }

        private void RemoveAbsorbedFittings()
        {
            foreach (var (nodeAbsorbed, absorbingFitting) in _fittingsThatAbsorbNodes.Select(kvp => (kvp.Key, kvp.Value)))
            {
                var removedFitting = _fittingsCreatedForNodes[nodeAbsorbed];
                var trunkside = removedFitting.TrunkSideComponent;
                absorbingFitting.TrunkSideComponent = trunkside;
                trunkside.BranchSideComponents.Remove(removedFitting);
                trunkside.BranchSideComponents.Add(absorbingFitting);
                foreach (var branch in removedFitting.BranchSideComponents)
                {
                    if (!absorbingFitting.BranchSideComponents.Contains(branch) && branch != absorbingFitting)
                    {
                        absorbingFitting.BranchSideComponents.Add(branch);
                    }
                    Fittings.Remove(removedFitting);
                }
            }
        }

        private void AssignGuidIds()
        {
            foreach (var c in ExpandAssembliesInternal(AllComponents, false))
            {
                c.Name = c.GetAbbrev();
                if (c.TryGetGuid(out var id))
                {
                    c.Name += id.ToString().Substring(0, 3);
                }
            }
        }

        internal List<FailedStraightSegment> CreateStraightSegmentsBetweenFittings()
        {
            var componentsToPipe = new HashSet<Fitting>(Fittings);

            var connectorsStillNeedingPipes = new HashSet<Port>();
            var failedConnections = new List<FailedStraightSegment>();
            pipedPorts.Clear();
            foreach (var thisConnection in componentsToPipe)
            {
                try
                {
                    StraightSegment[] pipes = MakePossiblePipes(thisConnection, out List<Fitting> createdConnections, out var connectorsNotPiped);
                    AddConnections(createdConnections);
                    AddStraightSegments(pipes);
                    connectorsStillNeedingPipes.UnionWith(connectorsNotPiped.Select(f => f.StartPort));
                    failedConnections.AddRange(connectorsNotPiped);
                }
                catch when (!Env.DisableCatchExceptions)
                {
                    connectorsStillNeedingPipes.UnionWith(thisConnection.GetPorts());
                    failedConnections.AddRange(thisConnection.GetPorts().Select(p => new FailedStraightSegment(thisConnection, p)));
                }
            }
            var fittingsNeedingStraights = connectorsStillNeedingPipes.Except(pipedPorts);
            if (fittingsNeedingStraights.Count() > 0)
            {
                failedConnections = failedConnections.Where(
                    f => fittingsNeedingStraights.Any(c => f.StartPort == c)).ToList();
            }

            var filtered = new List<FailedStraightSegment>();
            foreach (var item in fittingsNeedingStraights)
            {
                if (filtered.Any(f => f.EndPort == item))
                {
                    continue;
                }

                var errors = failedConnections.Where(f => f.StartPort == item);
                var error = errors.FirstOrDefault(e => e.End != null);
                if (error == null)
                {
                    error = errors.First();
                }
                filtered.Add(error);
            }

            List<(Terminal, Transform)> leafTerminals = new List<(Terminal, Transform)>();
            foreach (var fitting in Fittings)
            {
                if (fitting is Terminal terminal && terminal.TrunkSideComponent != null)
                {
                    leafTerminals.Add((terminal, terminal.AdditionalTransform.Inverted()));
                }
                fitting.ApplyAdditionalTransform();
            }

            List<StraightSegment> emptySegments = new List<StraightSegment>();
            foreach (var segment in StraightSegments)
            {
                segment.ApplyAdditionalTransform();
            }

            foreach (var terminal in leafTerminals)
            {
                //TODO: handle false return when terminal can't be balanced.
                BalanceBranchTerminalAdditionalTransform(terminal.Item1, terminal.Item2);
            }

            foreach (var segment in StraightSegments)
            {
                if (segment.Length().ApproximatelyEquals(0, _portsDistanceTolerance))
                {
                    emptySegments.Add(segment);
                }
            }

            foreach (var segment in emptySegments)
            {
                RemoveEmptyStrainghtSegment(segment);
            }

            return filtered;
        }

        private readonly HashSet<Port> pipedPorts = new HashSet<Port>();
        private readonly HashSet<Port> loopPorts = new HashSet<Port>();

        private StraightSegment[] MakePossiblePipes(Fitting connection,
                                                    out List<Fitting> createdConnections,
                                                    out List<FailedStraightSegment> connectorsNotPiped)
        {
            createdConnections = new List<Fitting>();
            connectorsNotPiped = new List<FailedStraightSegment>();

            var allPipes = new List<StraightSegment>();
            foreach (var connector in connection.BranchSidePorts())
            {
                if (pipedPorts.Contains(connector) || loopPorts.Contains(connector))
                {
                    continue;
                }

                foreach (var branch in connection.BranchSideComponents.ToList())
                {
                    var branchFitting = branch as Fitting;
                    var branchSideMatch = OppositeSidePort(connector, branchFitting, out bool hasEnoughSpace);
                    if (branchSideMatch != null && !pipedPorts.Contains(branchSideMatch))
                    {
                        // Extending additional transform outside the assembly
                        if (connection is Assembly assembly)
                        {
                            if (assembly.GetBranchTransformForConnector(connector) is Transform transform)
                            {
                                branch.PropagateAdditionalTransform(transform, TransformDirection.TrunkToBranch);
                            }
                        }
                        if (!hasEnoughSpace)
                        {
                            connectorsNotPiped.Add(new FailedStraightSegment(connection,
                                                                             connector,
                                                                             branchFitting,
                                                                             branchSideMatch));
                        }
                        else
                        {
                            var pipe = MakePipe(connection,
                                                connector,
                                                branchSideMatch,
                                                branch as Fitting,
                                                out var createdConns);
                            if (pipe != null)
                            {
                                allPipes.Add(pipe);
                            }
                            createdConnections.AddRange(createdConns);
                            break;
                        }
                    }
                }
            }


            var notPiped = connection.BranchSidePorts().Except(pipedPorts).ToList();
            foreach (var port in notPiped)
            {
                if (!connectorsNotPiped.Any(f => f.StartPort == port || f.EndPort == port))
                {
                    connectorsNotPiped.Add(new FailedStraightSegment(connection, port));
                }
            }

            return allPipes.ToArray();
        }

        private StraightSegment MakePipe(Fitting trunkConnection, Port trunkConnector, Port branchConnector, Fitting branchConnection, out List<Fitting> createdConns)
        {
            if (trunkConnection == null || branchConnection == null)
            {
                createdConns = new List<Fitting>();
                return null;
            }
            if (trunkConnector.Position.IsAlmostEqualTo(branchConnector.Position, _portsDistanceTolerance))
            {
                if (trunkConnector.Diameter != branchConnector.Diameter)
                {
                    throw new CannotMakeConnectionException($"Connection is not acceptable because the diameters are different.", trunkConnector.Position);
                }
                // The connectors are already so close they are connected, no pipe necessary.
                pipedPorts.Add(branchConnector);
                pipedPorts.Add(trunkConnector);
                if (trunkConnection is Assembly trunkSideAssembly)
                {
                    // Whole assembly is initially set to the same section.
                    // When segment is created it calls this function to set section for every piece.
                    // If there are no segment - do it here based on branch fitting.
                    trunkSideAssembly.AssignBranchComponentInternallyAndBurrowSectionRef(branchConnection);
                }
                branchConnection.PropagateAdditionalTransform(trunkConnection.GetPropagatedTransform(TransformDirection.TrunkToBranch),
                                                              TransformDirection.TrunkToBranch);
                createdConns = new List<Fitting>();
                return null;
            }
            var angleOfPipe = new Line(trunkConnector.Position, branchConnector.Position).Direction().AngleTo(branchConnector.Direction);
            if (!angleOfPipe.ApproximatelyEquals(180, _angleTolerance))
            {
                throw new Exception("Pipe connectors are not aligned.");
            }

            // create pipe
            var newPipe = new StraightSegment(0, trunkConnector, branchConnector, _routing.PipeMaterial, true);
            AssignPreviousAndNextComponents(branchConnection, trunkConnection, newPipe);
            // after assigning all components:
            // Branch direction: TrunkConnection => PS (newPipe) => BranchConnection
            // Trunk direction: TrunkConnection <= PS (newPipe) <= BranchConnection

            createdConns = new List<Fitting>();

            // if resizing is required
            if (Routing.PipeSizeShouldMatchConnection &&
             trunkPortToConnectionLookup.TryGetValue(branchConnector, out var pipeConnection) &&
             (trunkConnector.Diameter != pipeConnection.Diameter || branchConnector.Diameter != pipeConnection.Diameter))
            {
                // This is the new path and should become the default rather than the following if else below.
                var availableLength = newPipe.Length();
                var needsTwoReducers = trunkConnector.Diameter != pipeConnection.Diameter && branchConnector.Diameter != pipeConnection.Diameter;
                // First create reducer on trunkside
                if (!trunkConnector.Diameter.ApproximatelyEquals(pipeConnection.Diameter))
                {
                    var reducerOnBranchSide = false;
                    newPipe.Diameter = pipeConnection.Diameter;
                    var reducer = _routing.ReduceOrJoin(newPipe, reducerOnBranchSide, trunkConnector.Diameter);
                    reducer.ComponentLocator.MatchNetworkSection(newPipe.ComponentLocator);
                    if (reducer is Assembly reducerAssembly)
                    {
                        reducerAssembly.AssignSectionReferenceInternalToAssembly(newPipe.ComponentLocator);
                    }

                    AddReducerBetweenPipeAndTrunkConnection(trunkConnection, newPipe, reducer);
                    // If there are 2 reducers, branch component of pipe will be the branch reducer, which will be created later.
                    var currentBranchConnection = needsTwoReducers ? null : branchConnection;
                    PropagateTransformationReducerPipeBranch(trunkConnection, reducer, newPipe, currentBranchConnection);

                    if (trunkConnection is Assembly assembly)
                    {
                        assembly.AssignTrunkComponentsInternally();
                        assembly.AssignBranchComponentInternallyAndBurrowSectionRef(reducer as ComponentBase);
                    }
                    availableLength -= reducer.Length();
                    createdConns.Add(reducer as Fitting);
                }
                // reducer on branchside
                if (!branchConnector.Diameter.ApproximatelyEquals(pipeConnection.Diameter))
                {
                    var reducerOnBranchSide = true;
                    newPipe.Diameter = pipeConnection.Diameter;
                    var reducer = _routing.ReduceOrJoin(newPipe, reducerOnBranchSide, branchConnector.Diameter);
                    reducer.ComponentLocator.MatchNetworkSection(newPipe.ComponentLocator);
                    if (reducer is Assembly reducerAssembly)
                    {
                        reducerAssembly.AssignSectionReferenceInternalToAssembly(newPipe.ComponentLocator);
                    }

                    AddReducerBetweenPipeAndBranchConnection(branchConnection, newPipe, reducer);

                    if (!needsTwoReducers)
                    {
                        PropagateTransformationPipeReducerBranch(trunkConnection, newPipe, reducer, branchConnection);
                    }
                    else
                    {
                        // Trunk transform was propagated to pipe when creating trunk side reducer. We don't want to do it a second time
                        reducer.PropagateAdditionalTransform(newPipe.GetPropagatedTransform(TransformDirection.TrunkToBranch),
                                                                TransformDirection.TrunkToBranch);


                        branchConnection.PropagateAdditionalTransform(reducer.GetPropagatedTransform(TransformDirection.TrunkToBranch),
                                                            TransformDirection.BranchToTrunk);
                    }

                    if (branchConnection is Assembly assembly)
                    {
                        assembly.AssignTrunkComponentsInternally();
                    }
                    availableLength -= reducer.Length();
                    createdConns.Add(reducer as Fitting);
                }

                if (availableLength < -Vector3.EPSILON)
                {
                    pipedPorts.Remove(trunkConnector);
                    pipedPorts.Remove(branchConnector);
                    // do I need to do this?
                    createdConns.Clear();
                    return null;
                }

            }
            else if (!trunkConnector.Diameter.ApproximatelyEquals(branchConnector.Diameter))
            {
                // TODO: this is the legacy path and should be removed
                bool reducerOnBranchSide;
                double newDiameter, oldDiameter;
                GetLegacyReducerSettings(trunkConnector, branchConnector, out reducerOnBranchSide, out newDiameter, out oldDiameter);
                if (Routing.PipeSizeShouldMatchConnection && trunkPortToConnectionLookup.TryGetValue(branchConnector, out var pipesConnection))
                {
                    GetNewReducerSettings(trunkConnector, branchConnector, pipesConnection, out reducerOnBranchSide, out newDiameter, out oldDiameter);
                }
                newPipe.Diameter = newDiameter;
                var reducer = _routing.ReduceOrJoin(newPipe, reducerOnBranchSide, oldDiameter);
                reducer.ComponentLocator.MatchNetworkSection(newPipe.ComponentLocator);
                if (reducer is Assembly reducerAssembly)
                {
                    reducerAssembly.AssignSectionReferenceInternalToAssembly(newPipe.ComponentLocator);
                }

                if (newPipe.Length() < reducer.Start.Position.DistanceTo(reducer.End.Position))
                {
                    pipedPorts.Remove(trunkConnector);
                    pipedPorts.Remove(branchConnector);
                    return null;
                }

                // if reducer is added to the branch side of the pipe
                if (reducerOnBranchSide)
                {
                    AddReducerBetweenPipeAndBranchConnection(branchConnection, newPipe, reducer);
                    PropagateTransformationPipeReducerBranch(trunkConnection, newPipe, reducer, branchConnection);

                    if (branchConnection is Assembly assembly)
                    {
                        assembly.AssignTrunkComponentsInternally();
                    }
                }
                // if reducer is added to the trunk side of the pipe
                else
                {
                    AddReducerBetweenPipeAndTrunkConnection(trunkConnection, newPipe, reducer);
                    PropagateTransformationReducerPipeBranch(trunkConnection, reducer, newPipe, branchConnection);

                    if (trunkConnection is Assembly assembly)
                    {
                        assembly.AssignTrunkComponentsInternally();
                        assembly.AssignBranchComponentInternallyAndBurrowSectionRef(reducer as ComponentBase);
                    }
                }

                createdConns.Add(reducer as Fitting);
            }
            // if reducer was not required, we still need to set additional transform to branchConnection
            else
            {
                if (newPipe.PropagateAdditionalTransform(trunkConnection.GetPropagatedTransform(TransformDirection.TrunkToBranch),
                                                         TransformDirection.TrunkToBranch))
                {
                    branchConnection.PropagateAdditionalTransform(newPipe.GetPropagatedTransform(TransformDirection.TrunkToBranch),
                                                                  TransformDirection.TrunkToBranch);
                }
                else if (!newPipe.IsValidConnection(out _))
                {
                    return null;
                    // TODO: Use code below to move other fittings in order to make pipe valid
                    // branchConnection.PropagateAdditionalTransform(transformToFixConnection, TransformDirection.TrunkToBranch);
                }
            }

            pipedPorts.Add(trunkConnector);
            pipedPorts.Add(branchConnector);
            if (trunkPortToConnectionLookup.TryGetValue(branchConnector, out var connection))
            {
                newPipe.Connection = connection.Id;
            }
            newPipe?.SetPath();
            return newPipe;
        }

        private void GetNewReducerSettings(Port trunkConnector, Port branchConnector, Connection pipesConnection, out bool reducerOnBranchSide, out double newDiameter, out double oldDiameter)
        {
            //if connection diameter doesn't match either diameter then throw exception.
            if (!pipesConnection.Diameter.ApproximatelyEquals(trunkConnector.Diameter) && !pipesConnection.Diameter.ApproximatelyEquals(branchConnector.Diameter))
            {
                throw new Exception("Connection diameter doesn't match either diameter.");
            }
            //if connection diameter matches trunk diameter then reducer is on branch side.
            if (pipesConnection.Diameter.ApproximatelyEquals(trunkConnector.Diameter))
            {
                reducerOnBranchSide = true;
                newDiameter = trunkConnector.Diameter;
                oldDiameter = branchConnector.Diameter;
            }
            //if connection diameter matches branch diameter then reducer is on trunk side.
            else
            {
                reducerOnBranchSide = false;
                newDiameter = branchConnector.Diameter;
                oldDiameter = trunkConnector.Diameter;
            }
        }

        private static void GetLegacyReducerSettings(Port trunkConnector, Port branchConnector, out bool reducerOnBranchSide, out double newDiameter, out double oldDiameter)
        {
            var trunksideSmaller = trunkConnector.Diameter < branchConnector.Diameter;

            bool branchsideStrongerPreferReducer = branchConnector.PreferReducer && !trunkConnector.PreferReducer;
            reducerOnBranchSide = trunksideSmaller || branchsideStrongerPreferReducer;
            newDiameter = reducerOnBranchSide ? trunkConnector.Diameter : branchConnector.Diameter;
            oldDiameter = !reducerOnBranchSide ? trunkConnector.Diameter : branchConnector.Diameter;
        }

        // Before:
        // Branch direction: TrunkConnection => PS (newPipe) => BranchConnection
        // Trunk direction: TrunkConnection <= PS (newPipe) <= BranchConnection
        // After:
        // Branch direction: TrunkConnection => Reducer => PS (newPipe) => BranchConnection
        // Trunk direction: TrunkConnection <= Reducer <= PS (newPipe) <= BranchConnection
        private static void AddReducerBetweenPipeAndTrunkConnection(ComponentBase trunkConnection, StraightSegment newPipe, IReducer reducer)
        {
            reducer.BranchSideComponents.Add(newPipe);
            newPipe.TrunkSideComponent = reducer as ComponentBase;
            reducer.TrunkSideComponent = trunkConnection;
            trunkConnection.BranchSideComponents.Remove(newPipe);
            trunkConnection.BranchSideComponents.Add(reducer as ComponentBase);

            newPipe.End = reducer.Start;
            if (trunkConnection is StraightSegment pipeSegment)
            {
                pipeSegment.Start = reducer.End;
            }

            if (reducer is Assembly reducerAssembly)
            {
                reducerAssembly.AssignTrunkComponentsInternally();
                reducerAssembly.AssignBranchComponentInternallyAndBurrowSectionRef(newPipe);
            }
        }

        private static void PropagateTransformationReducerPipeBranch(
            IComponent trunk, IReducer reducer, StraightSegment pipe, IComponent branch)
        {
            reducer.PropagateAdditionalTransform(trunk.GetPropagatedTransform(TransformDirection.TrunkToBranch),
                                                 TransformDirection.TrunkToBranch);
            if (pipe.PropagateAdditionalTransform(reducer.GetPropagatedTransform(TransformDirection.TrunkToBranch),
                                                  TransformDirection.TrunkToBranch))
            {
                branch?.PropagateAdditionalTransform(pipe.GetPropagatedTransform(TransformDirection.TrunkToBranch),
                                                    TransformDirection.TrunkToBranch);
            }
        }

        // Before:
        // Branch direction: TrunkConnection => PS (newPipe) => BranchConnection
        // Trunk direction: TrunkConnection <= PS (newPipe) <= BranchConnection
        // After:
        // Branch direction: TrunkConnection => PS (newPipe) => Reducer => BranchConnection
        // Trunk direction: TrunkConnection <= PS (newPipe) <= Reducer <= BranchConnection
        private static void AddReducerBetweenPipeAndBranchConnection(ComponentBase branchComponent, StraightSegment newPipe, IReducer reducer)
        {
            reducer.TrunkSideComponent = newPipe;
            newPipe.BranchSideComponents.Clear();
            newPipe.BranchSideComponents.Add(reducer as ComponentBase);
            reducer.BranchSideComponents.Add(branchComponent);

            if (branchComponent.BranchSideComponents.Contains(newPipe))
            {
                branchComponent.BranchSideComponents.Remove(newPipe);
                branchComponent.BranchSideComponents.Add(reducer as ComponentBase);
            }
            else
            {
                branchComponent.TrunkSideComponent = reducer as ComponentBase;
            }

            newPipe.Start = reducer.End;
            if (branchComponent is StraightSegment pipeSegment)
            {
                pipeSegment.End = reducer.Start;
            }

            if (reducer is Assembly reducerAssembly)
            {
                reducerAssembly.AssignTrunkComponentsInternally();
                reducerAssembly.AssignBranchComponentInternallyAndBurrowSectionRef(branchComponent);
            }
        }

        private static void PropagateTransformationPipeReducerBranch(
            IComponent trunk, StraightSegment pipe, IReducer reducer, IComponent branch)
        {
            if (pipe.PropagateAdditionalTransform(trunk.GetPropagatedTransform(TransformDirection.TrunkToBranch),
                                                  TransformDirection.TrunkToBranch))
            {
                reducer.PropagateAdditionalTransform(pipe.GetPropagatedTransform(TransformDirection.TrunkToBranch),
                                                    TransformDirection.TrunkToBranch);
            }

            branch.PropagateAdditionalTransform(reducer.GetPropagatedTransform(TransformDirection.TrunkToBranch),
                                                TransformDirection.BranchToTrunk);
        }

        internal Port GetDownstreamPortOnTrunksideComponent(ComponentBase component)
        {
            var trunkside = component.TrunkSideComponent;
            if (trunkside == null)
            {
                return null;
            }
            var trunksideBranchPorts = trunkside.BranchSidePorts();
            if (component is StraightSegment segment)
            {
                // TODO when we stop having StraightSegments contain a direct reference to the ports of other components we can remove this special case.
                return trunksideBranchPorts.SingleOrDefault(p => p.IsIdenticalConnector(segment.End, _portsDistanceTolerance, _angleTolerance));
            }
            if (trunkside is StraightSegment trunksideSegment)
            {
                if (trunksideSegment.Start.IsIdenticalConnector(component.TrunkSidePort(), _portsDistanceTolerance, _angleTolerance))
                {
                    return trunksideSegment.Start;
                }
            }

            return GetBestComplementForPort(component.TrunkSidePort(), component.TrunkSideComponent);
        }

        internal Port GetBestComplementForPort(Port connector, ComponentBase otherConnection)
        {
            return GetBestComplementForPort(connector.Position, connector.Direction, otherConnection);
        }

        internal Port GetBestComplementForPort(Vector3 portPosition,
                                                      Vector3 portDirection,
                                                      ComponentBase otherComponent)
        {
            var port = OppositeSidePort(portPosition, portDirection, otherComponent, out bool hasEnoughSpace);
            return (port != null && hasEnoughSpace) ? port : null;
        }

        private Port OppositeSidePort(Port port,
                                             ComponentBase otherComponent,
                                             out bool hasEnoughSpace)
        {
            return OppositeSidePort(port.Position, port.Direction, otherComponent, out hasEnoughSpace);
        }

        private Port OppositeSidePort(Vector3 portPosition,
                                             Vector3 portDirection,
                                             ComponentBase otherComponent,
                                             out bool hasEnoughSpace)
        {
            hasEnoughSpace = false;

            if (otherComponent == null)
            {
                return null;
            }

            foreach (var otherPort in otherComponent.GetPorts())
            {
                var angleBetweenDirections = portDirection.AngleTo(otherPort.Direction);
                if (angleBetweenDirections.ApproximatelyEquals(180, _angleTolerance))
                {
                    var delta = portPosition - otherPort.Position;
                    if (delta.Length() < _portsDistanceTolerance)
                    {
                        hasEnoughSpace = true;
                        return otherPort;
                    }

                    var angleToDelta = delta.AngleTo(portDirection);
                    if (angleToDelta.ApproximatelyEquals(0, _angleTolerance) ||
                        angleToDelta.ApproximatelyEquals(180, _angleTolerance))
                    {
                        hasEnoughSpace = angleToDelta.ApproximatelyEquals(180, _angleTolerance);
                        return otherPort;
                    }
                }
            }
            return null;
        }

        private static void AssignPreviousAndNextComponents(Fitting branchSideConnection, Fitting trunkSideConnection, StraightSegment newPipe)
        {
            if (branchSideConnection.BranchSideComponents.Contains(trunkSideConnection))
            {
                branchSideConnection.BranchSideComponents.Remove(trunkSideConnection);
                branchSideConnection.BranchSideComponents.Add(newPipe);
                newPipe.ComponentLocator.MatchNetworkSection(trunkSideConnection.ComponentLocator);
            }
            else
            {
                branchSideConnection.TrunkSideComponent = newPipe;
                newPipe.ComponentLocator.MatchNetworkSection(branchSideConnection.ComponentLocator);
            }

            newPipe.BranchSideComponents.Add(branchSideConnection);
            trunkSideConnection.BranchSideComponents.Remove(branchSideConnection);
            trunkSideConnection.BranchSideComponents.Add(newPipe);
            newPipe.TrunkSideComponent = trunkSideConnection;

            if (trunkSideConnection is Assembly trunkSideAssembly)
            {
                trunkSideAssembly.AssignTrunkComponentsInternally();
                trunkSideAssembly.AssignBranchComponentInternallyAndBurrowSectionRef(newPipe);
            }
            if (branchSideConnection is Assembly branchSideAssembly)
            {
                var last = branchSideAssembly.GetInternalTrunkComponent();
            }
        }

        Dictionary<Node, Fitting> _fittingsThatAbsorbNodes = new Dictionary<Node, Fitting>();
        Dictionary<Node, Fitting> _fittingsCreatedForNodes = new Dictionary<Node, Fitting>();

        private void RecursiveBuildFittingTreeFromTrunk(Node node, Fitting previous)
        {
            Fitting newFitting = null;
            Node[] absorbedNodes = null;
            try
            {
                if (_fittingsCreatedForNodes.TryGetValue(node, out newFitting))
                {
                    // loop starts at this node. The BranchsideComponents will point a each other at the Loop location.
                    var fittingLoopPorts = newFitting.GetPorts().Where(c => loopPorts.Contains(c));
                    var isTrunkSideLoop = fittingLoopPorts.Any(c => GetBestComplementForPort(c, newFitting.TrunkSideComponent as Fitting) != null);
                    if (isTrunkSideLoop)
                    {
                        var oldTrunk = newFitting.TrunkSideComponent;
                        newFitting.TrunkSideComponent = previous;
                        newFitting.BranchSideComponents.Add(oldTrunk);
                    }
                    else
                    {
                        newFitting.BranchSideComponents.Add(previous);
                    }
                    previous.BranchSideComponents.Add(newFitting);

                    return;
                }
                newFitting = _routing.FittingForNode(node, out var errorDetail, out absorbedNodes);
                if (newFitting == null)
                {
                    throw new CannotMakeConnectionException($"No fitting could be made. {errorDetail}", node.Position);
                }

                _fittingsCreatedForNodes.Add(node, newFitting);
                if (absorbedNodes?.Count() > 0)
                {
                    foreach (var absorbed in absorbedNodes)
                    {
                        _fittingsThatAbsorbNodes.Add(absorbed, newFitting);
                    }
                }

                var outgoing = _routing.Tree.GetOutgoingConnection(node);
                newFitting.AssignReferenceBasedOnSection(_routing.Tree.GetSectionFromConnection(outgoing));
                AddConnection(newFitting);
                newFitting.TrunkSideComponent = previous;

                var connectionTrunkPort = newFitting.TrunkSidePort();
                if (connectionTrunkPort != null)
                {
                    trunkPortToConnectionLookup.Add(newFitting.TrunkSidePort(), outgoing);
                }

                if (previous != null)
                {
                    previous.BranchSideComponents.Add(newFitting);
                }
                if (newFitting is Assembly assembly)
                {
                    assembly.AssignTrunkComponentsInternally();
                }

                var loopConnections = Tree.GetOutgoingConnections(node).Where(c => c.IsLoop == true);
                foreach (var loopConnection in loopConnections)
                {
                    var loopPort = newFitting.BranchSidePorts().FirstOrDefault(port => port.Direction.AngleTo(loopConnection.Direction()).ApproximatelyEquals(0, _angleTolerance));
                    if (loopPort != null)
                    {
                        loopPorts.Add(loopPort);
                        trunkPortToConnectionLookup.Add(loopPort, loopConnection);
                    }
                }
            }
            catch (Exception e) when (!Env.DisableCatchExceptions)
            {
                if (e.InnerException is CannotMakeConnectionException innerException)
                {
                    throw new CannotMakeConnectionException($"Error creating fitting. {e.Message}. {innerException.Message}", innerException.Location);
                }
                throw new CannotMakeConnectionException($"Error creating fitting. {e.Message}", node.Position);
            }

            var incoming = _routing.Tree.GetIncomingConnections(node);
            foreach (var edge in incoming)
            {
                // TODO handle absorbed nodes that are closer to the leaves than the current node.
                if (absorbedNodes != null && absorbedNodes.Contains(edge.Start))
                {
                    throw new Exception("Absorbed nodes that are branchside are not supported.");
                }
                RecursiveBuildFittingTreeFromTrunk(edge.Start, newFitting);
            }
        }
    }
}