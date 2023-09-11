using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Elements.Annotations;
using Elements.Flow;
using Elements.Geometry;

namespace Elements.Fittings
{
    public partial class FittingTree
    {
        public static FittingTree Create(FittingTreeRouting routing = null, double angleTolerance = 0.5, double portsDistanceTolerance = 0.001)
        {
            var fittings = new FittingTree(new List<StraightSegment>(),
                                   new List<Fitting>(),
                                   string.Empty,
                                   Guid.NewGuid(),
                                   string.Empty);
            
            fittings._angleTolerance = angleTolerance;
            fittings._portsDistanceTolerance = portsDistanceTolerance;
            fittings._routing = routing ?? new FittingTreeRouting(null);

            if (fittings._routing.Tree != null)
            {
                fittings.Name = routing.Tree.GetNetworkReference();
                fittings.Purpose = routing.Tree.Purpose;
            }

            return fittings;
        }

        /// <summary>
        /// Add a Fitting to the FittingTree.
        /// </summary>
        /// <param name="connection"></param>
        public void AddConnection(Fitting connection)
        {
            // TODO rename this to AddFitting
            this.Fittings.Add(connection);
        }

        /// <summary>
        /// Add multiple fittings to the FittingTree.
        /// </summary>
        /// <param name="connections"></param>
        public void AddConnections(IEnumerable<Fitting> connections)
        {
            // TODO rename this to AddFittings
            foreach (var connection in connections)
            {
                if (connection != null)
                {
                    AddConnection(connection);
                }
            }
        }
        
        /// <summary>
        /// Propagates flow
        /// </summary>
        /// <param name="leafFlow"></param>
        public void PropagateFlow(ComponentBase component, double leafFlow)
        {
            var all = component.GetAllTrunksideComponents();
            foreach (var f in all)
            {
                // Straight segments have connectors that exist on other fittings, we don't want to double add flows.
                if (!(f is StraightSegment))
                {
                    f.TrunkSidePort()?.AddFlow(leafFlow);
                }
                var next = f.TrunkSideComponent;
                if (!(next is StraightSegment))
                {
                    GetDownstreamPortOnTrunksideComponent(f)?.AddFlow(leafFlow);
                }
            }
        }

        private void AddStraightSegments(IEnumerable<StraightSegment> pipeSegments)
        {
            foreach (var pipe in pipeSegments)
            {
                if (pipe == null)
                {
                    continue;
                }
                this.StraightSegments.Add(pipe);
            }
        }

        /// <summary>
        /// Returns the components of  the section ordered by index in the section, which is expected to be trunkside to branchside.
        /// </summary>
        public IOrderedEnumerable<ComponentBase> GetComponentsOfSection(Section section)
        {
            return GetComponentsOfSectionKey(section.SectionKey);
        }

        public IOrderedEnumerable<ComponentBase> GetComponentsOfSectionKey(string sectionKey)
        {
            return ExpandAssemblies(AllComponents).Where(c => c.ComponentLocator.SectionKey == sectionKey).OrderBy(c => c.ComponentLocator.IndexInSection);
        }

        public void MoveFitting(Fitting connection, Geometry.Transform transform)
        {
            switch (connection)
            {
                case Reducer reducer:
                    foreach (var branch in reducer.BranchSideComponents.OfType<StraightSegment>())
                    {
                        var movementLine = new Line(branch.Start.Position, (reducer.TrunkSideComponent as StraightSegment).End.Position);
                        transform.Origin.DistanceTo(movementLine, out var modifiedOrigin);
                        var modifiedTransform = new Transform(modifiedOrigin - reducer.Transform.Origin);
                        reducer.Start.Position = modifiedTransform.OfPoint(reducer.Start.Position);
                        reducer.End.Position = modifiedTransform.OfPoint(reducer.End.Position);
                        reducer.Transform = reducer.Transform.Concatenated(modifiedTransform);
                        branch.SetPath();
                        (reducer.TrunkSideComponent as StraightSegment).SetPath();
                    }
                    break;
                default:
                    throw new Exception($"Moving the a connection of they type {connection.GetType()} is not supported yet.");
            }
        }

        /// <summary>
        /// Split the pipe at the given distance from the start of the pipe.
        /// <param name="pipe">The pipe to split.</param>
        /// <param name="distanceFromStart">The distance from the start of the pipe to insert the split.</param>
        /// <param name="flipEnd">Should the distance be relative to the End, rather than the Start of the StraightSegment</param>
        /// <param name="newEndPipe">The new pipe that was created.  Branch side if flipEnd is false.</param>
        /// <param name="error">Any error that occurred while making the split.</param>
        /// </summary>
        public Fitting SplitPipe(StraightSegment pipe, double distanceFromStart, bool flipEnd, out StraightSegment newEndPipe, out string error)
        {
            var reducer = _routing.ReduceOrJoin(pipe, flipEnd, pipe.Diameter, distanceFromStart) as Reducer;
            reducer.CanBeMoved = true;
            reducer.ComponentLocator.MatchNetworkSection(pipe.ComponentLocator);
            if (reducer.GetLength() > distanceFromStart
                || reducer.GetLength() > (pipe.Length() - distanceFromStart))
            {
                newEndPipe = null;
                var remainingDistance = reducer.GetLength() > distanceFromStart ? distanceFromStart : (pipe.Length() - distanceFromStart);
                error = $"{remainingDistance} is smaller than the size of the created reducer";
                return null;
            }

            var nextConn = pipe.End;
            var next = pipe.TrunkSideComponent;
            pipe.End = reducer.Start;
            pipe.TrunkSideComponent = reducer;
            reducer.BranchSideComponents.Add(pipe);
            // next.BranchSideComponents will be added below in MakePipe method
            next.BranchSideComponents.Remove(pipe);
            newEndPipe = MakePipe(next as Fitting, nextConn, reducer.End, reducer, out var createdConnectors);
            createdConnectors.ForEach(c => c.ComponentLocator.MatchNetworkSection(pipe.ComponentLocator));

            newEndPipe.ComponentLocator.MatchNetworkSection(pipe.ComponentLocator);

            pipe.SetPath();
            this.AddStraightSegments(new[] { newEndPipe });
            this.AddConnection(reducer);
            // TODO we might be able to just relabel the section where this pipe is.
            var errors = AssignFlowIdsToComponents();
            error = errors.Any() ? $"Labeling error on section: {errors.First().Section}: " + errors.First().Text : null;
            return reducer;
        }

        /// <summary>
        /// Resizes the given pipe, adding reducers as needed to maintain size continuity.
        /// If a coupler is found (reducer that has the same Start and End diameters) it will be left preserved.
        /// If a reducer can be removed after the resize, it will be.
        /// </summary>
        /// <param name="pipe">The pipe to resize.</param>
        /// <param name="newDiameter">The new diameter of the pipe in meters.</param>
        public void ResizePipe(StraightSegment pipe, double newDiameter)
        {
            if (pipe.BranchSideComponents.Count != 1)
            {
                throw new ArgumentException("Pipe cannot have more then one branch side component");
            }

            pipe.ClearAdditionalTransform();
            var toAdd = new Fitting[2];
            var oldDiameter = pipe.Diameter;
            pipe.Diameter = newDiameter;
            var nextReducer = GetWholeReducer(pipe.TrunkSideComponent as IReducer);
            var previousReducer = GetWholeReducer(pipe.BranchSideComponents.FirstOrDefault() as IReducer);
            var branchSideComponent = previousReducer == null ? pipe.BranchSideComponents.FirstOrDefault()
                : previousReducer.BranchSideComponents.FirstOrDefault();
            var trunkSideComponent = nextReducer == null ? pipe.TrunkSideComponent
                : nextReducer.TrunkSideComponent;

            // if the pipes TrunkSideComponent is reducer we need to remove it and create a new one with required diameter
            var preserverTrunkSideSplit = false;
            if (nextReducer != null)
            {
                preserverTrunkSideSplit = TrunkSideReducerShouldBePreserved(nextReducer, newDiameter);
                RemoveReducerOnTrunkSideFromPipe(pipe, nextReducer);
            }

            // if the pipes BranchSideComponent is reducer, we need to remove it and create a new one with required diameter
            var preserveBranchSideSplit = false;
            if (previousReducer != null)
            {
                preserveBranchSideSplit = BranchSideReducerShouldBePreserved(previousReducer, newDiameter);
                RemoveReducerOnBranchSideFromPipe(pipe, previousReducer);
            }

            var endDirection = pipe.End.Direction;
            var endPosition = pipe.End.Position;
            var startDirection = pipe.Start.Direction;
            var startPosition = pipe.Start.Position;
            var additionalTransform = new Transform();

            if (nextReducer != null)
            {
                additionalTransform = new Transform(nextReducer.BranchSideTransform.Inverted());
                endPosition = additionalTransform.OfPoint(endPosition);
                startPosition = additionalTransform.OfPoint(startPosition);
            }

            if (previousReducer != null)
            {
                additionalTransform.Concatenate(previousReducer.BranchSideTransform.Inverted());
            }

            if (branchSideComponent is StraightSegment branchPipeSegment)
            {
                var branchPipeTrunkSidePort = branchPipeSegment.TrunkSidePort();
                branchPipeTrunkSidePort.Position = additionalTransform.OfPoint(branchPipeTrunkSidePort.Position);
            }
            else
            {
                branchSideComponent.PropagateAdditionalTransform(additionalTransform, TransformDirection.TrunkToBranch);
            }
            branchSideComponent.ApplyAdditionalTransform();

            if (trunkSideComponent is Fitting trunksideFitting)
            {
                var newConn = GetBestComplementForPort(startPosition, startDirection, trunksideFitting);
                if (newConn == null)
                {
                    throw new Exception("Failed to get valid complimentary connector");
                }
                pipe.End = newConn;
            }
            else if (trunkSideComponent is StraightSegment trunksideSegment)
            {
                if (pipe.Diameter == trunksideSegment.Diameter && !preserverTrunkSideSplit)
                {
                    pipe = MergePipes(pipe, trunksideSegment, pipe.Diameter);
                }
                else
                {
                    pipe.End = trunksideSegment.Start;
                }
            }

            if (branchSideComponent is Fitting branchFitting)
            {
                var newConn = GetBestComplementForPort(endPosition, endDirection, branchFitting);
                if (newConn == null)
                {
                    throw new Exception("Failed to get valid complimentary connector");
                }
                pipe.Start = newConn;
            }
            else if (branchSideComponent is StraightSegment pipeSegment)
            {
                if (pipe.Diameter == pipeSegment.Diameter && !preserveBranchSideSplit)
                {
                    pipe = MergePipes(pipe, pipeSegment, pipe.Diameter);
                }
                else
                {
                    pipe.Start = pipeSegment.End;
                }
            }

            if (pipe.Start == null || pipe.End == null)
            {
                throw new Exception($"Invalid pipe created during resize operation of PipeName: {pipe.Name}");
            }

            // create reducer on trunk side of the pipe
            IReducer trunkSideReducer = null;
            if (pipe.End.Diameter != newDiameter || preserverTrunkSideSplit)
            {
                trunkSideReducer = _routing.ReduceOrJoin(pipe, false, pipe.End.Diameter);
                // Update pipe end before adding reducer in order to correctly assign branch components internally in reducer assembly
                AddReducerBetweenPipeAndTrunkConnection(pipe.TrunkSideComponent, pipe, trunkSideReducer);

                trunkSideReducer.ComponentLocator.MatchNetworkSection(pipe.ComponentLocator);

                if (trunkSideReducer is Assembly endReducerAssembly)
                {
                    endReducerAssembly.AssignSectionReferenceInternalToAssembly(pipe.ComponentLocator);
                }

                toAdd[1] = trunkSideReducer as Fitting;
            }

            // create reducer on branch side of the pipe
            IReducer branchSideReducer = null;
            if (pipe.Start.Diameter != newDiameter || preserveBranchSideSplit)
            {
                branchSideReducer = _routing.ReduceOrJoin(pipe, true, pipe.Start.Diameter);
                // Update pipe start before adding reducer in order to correctly assign trunk components internally in reducer assembly
                AddReducerBetweenPipeAndBranchConnection(pipe.BranchSideComponents.FirstOrDefault(), pipe, branchSideReducer);
                branchSideReducer.ComponentLocator.MatchNetworkSection(pipe.ComponentLocator);
                if (branchSideReducer is Assembly branchSideReducerAssembly)
                {
                    branchSideReducerAssembly.AssignSectionReferenceInternalToAssembly(pipe.ComponentLocator);
                }
                toAdd[0] = branchSideReducer as Fitting;
            }

            AddConnections(toAdd);

            if (trunkSideReducer != null)
            {
                // Since reducer is inserted into existing fittings - it doesn't receive any additional transformation.
                pipe.PropagateAdditionalTransform(trunkSideReducer.GetPropagatedTransform(TransformDirection.TrunkToBranch),
                                                  TransformDirection.TrunkToBranch);
                trunkSideReducer.ApplyAdditionalTransform();
            }

            // Copy transformation because its linked to component and will be overriden after ApplyAdditionalTransform
            var branchSideAdditionalTransform = new Transform(pipe.GetPropagatedTransform(TransformDirection.TrunkToBranch));
            pipe.ApplyAdditionalTransform();

            if (branchSideReducer != null)
            {
                branchSideReducer.PropagateAdditionalTransform(branchSideAdditionalTransform, TransformDirection.TrunkToBranch);
                branchSideAdditionalTransform = new Transform(branchSideReducer.GetPropagatedTransform(TransformDirection.TrunkToBranch));
                branchSideReducer.ApplyAdditionalTransform();
            }

            branchSideComponent.PropagateAdditionalTransform(branchSideAdditionalTransform, TransformDirection.TrunkToBranch);
            branchSideComponent.ApplyAdditionalTransform();

            additionalTransform.Concatenate(branchSideAdditionalTransform);
            ApplyAdditionalTransformToBranchComponents(additionalTransform, branchSideComponent);
            AssignFlowIdsToComponents();

            foreach (var item in toAdd)
            {
                item?.ApplyAdditionalTransform();
            }
        }

        /// <summary>
        /// Places the coupler at one of the ends of the pipe. The pipe length is adjusted then.
        /// </summary>
        /// <param name="pipe">Pipe to insert coupler into</param>
        /// <param name="connector">End of the pipe</param>
        /// <param name="coupler">Coupler to insert</param>
        public void PlaceCoupler(StraightSegment pipe, Port connector, Coupler coupler)
        {
            bool start = pipe.Start == connector;
            if (!start && pipe.End != connector)
            {
                throw new Exception("Connector doesn't belong to the pipe");
            }

            if (coupler.Length() > pipe.Length())
            {
                throw new Exception("Pipe is too small");
            }

            if (start)
            {
                coupler.AssignToStart(pipe);
            }
            else
            {
                coupler.AssignToEnd(pipe);
            }
            coupler.ComponentLocator.MatchNetworkSection(pipe.ComponentLocator);

            Fittings.Add(coupler);
            AssignFlowIdsToComponents();
        }

        /// <summary>
        /// Places the set of coupler into the pipe.
        /// Original pipe is resized to the first internal coupler.
        /// All spaces between couples are filled with new pipes.
        /// </summary>
        /// <param name="pipe">Pipe to insert couplers into</param>
        /// <param name="couplers">Couplers to insert.</param>
        public void PlaceCouplers(StraightSegment pipe, IList<Coupler> couplers)
        {
            if (!couplers.Any())
            {
                return;
            }

            var sortedCouplers = couplers.OrderBy(
                c => c.Start.Position.DistanceTo(pipe.Start.Position)).ToList();

            Coupler head = null;
            Port previousConnector = pipe.Start;
            Port nextConnector = pipe.End;
            Fitting nextComponent = pipe.TrunkSideComponent as Fitting;
            StraightSegment activePipe = pipe;

            foreach (var coupler in sortedCouplers)
            {
                if (head == null)
                {
                    if (!coupler.Start.Position.IsAlmostEqualTo(previousConnector.Position))
                    {
                        coupler.AssignToEnd(activePipe);
                        //AssignToEnd linked coupler with next connector.
                        //Since there are other elements ahead - unlink it.
                        //Next connector will be linked after all couplers are processed.
                        nextComponent.BranchSideComponents.Remove(coupler);
                        head = coupler;
                    }
                    else
                    {
                        coupler.AssignToStart(activePipe);
                    }
                }
                else
                {
                    activePipe = MakePipe(coupler, coupler.Start, head.End, head, out var createdConnectors);
                    StraightSegments.Add(activePipe);
                    activePipe.AdditionalTransform.Concatenate(pipe.AdditionalTransform);
                    createdConnectors.ForEach(c => c.ComponentLocator.MatchNetworkSection(head.ComponentLocator));
                    head = coupler;
                }
                pipe.SetPath();

                coupler.ComponentLocator.MatchNetworkSection(activePipe.ComponentLocator);
                Fittings.Add(coupler);
            }

            if (head != null)
            {
                if (!head.End.Position.IsAlmostEqualTo(nextConnector.Position))
                {
                    var endPipe = MakePipe(nextComponent, nextConnector, head.End, head, out var createdConnectors);
                    StraightSegments.Add(endPipe);
                    activePipe.AdditionalTransform.Concatenate(pipe.AdditionalTransform);
                    createdConnectors.ForEach(c => c.ComponentLocator.MatchNetworkSection(head.ComponentLocator));
                }
                else
                {
                    head.TrunkSideComponent = nextComponent;
                    nextComponent.BranchSideComponents.Remove(pipe);
                    nextComponent.BranchSideComponents.Add(head);
                }
            }
            AssignFlowIdsToComponents();
        }

        /// <summary>
        /// Merges tww pipes into one, re-establishing the connector and trunk/branch side relationships.
        /// </summary>
        /// <param name="pipe1"></param>
        /// <param name="pipe2"></param>
        /// <param name="newDiameter"></param>
        /// <returns>The pipe that was kept/created during merging</returns>
        /// <exception cref="Exception"></exception>
        private StraightSegment MergePipes(StraightSegment pipe1, StraightSegment pipe2, double newDiameter)
        {
            var branchSide = pipe1.TrunkSideComponent == pipe2 ? pipe1 : pipe2;
            var trunkSide = branchSide == pipe1 ? pipe2 : pipe1;
            if (branchSide.TrunkSideComponent != trunkSide)
            {
                throw new Exception("can't merge pipes that aren't branch/trunkside to eachother");
            }
            branchSide.End = trunkSide.End;
            StraightSegments.Remove(trunkSide);
            branchSide.TrunkSideComponent = trunkSide.TrunkSideComponent;
            trunkSide.TrunkSideComponent.BranchSideComponents.Remove(trunkSide);
            trunkSide.TrunkSideComponent.BranchSideComponents.Add(branchSide);
            return branchSide;
        }

        private IReducer GetWholeReducer(IReducer reducer)
        {
            var reducerAssembly = AllComponents
                            .OfType<IReducer>()
                            .OfType<Assembly>()
                            .Where(assembly => assembly.AllComponents.Contains(reducer as ComponentBase))
                            .FirstOrDefault();
            return reducerAssembly as IReducer ?? reducer;
        }

        private void ApplyAdditionalTransformToBranchComponents(Transform transform, ComponentBase connection)
        {
            if (!connection.BranchSideComponents.Any() && connection is Terminal terminal)
            {
                BalanceBranchTerminalAdditionalTransform(terminal, transform.Inverted());
            }

            foreach (var branch in connection.BranchSideComponents)
            {
                if (branch.PropagateAdditionalTransform(transform, TransformDirection.TrunkToBranch))
                {
                    transform = new Transform(branch.GetPropagatedTransform(TransformDirection.TrunkToBranch));
                    branch.ApplyAdditionalTransform();
                    ApplyAdditionalTransformToBranchComponents(transform, branch);
                }
            }
        }

        public void RestoreBranchReferences()
        {
            var fittings = ExpandedComponents;

            var inAllNoTFitting = AllComponents.Except(fittings);
            var inFittingNotAll = fittings.Except(AllComponents);

            foreach (var fitting in fittings)
            {
                var trunkComponent = fittings.SingleOrDefault(x => x.Id == fitting.TrunkSideComponent?.Id);
                if (trunkComponent == null)
                {
                    continue;
                }
                if (trunkComponent.BranchSideComponents == null)
                {
                    trunkComponent.BranchSideComponents = new List<ComponentBase>();
                }

                if (trunkComponent.BranchSideComponents.All(x => x.Id != fitting.Id))
                {
                    trunkComponent.BranchSideComponents.Add(fitting);
                }
            }
        }

        public void CheckComponentLabeling()
        {
            var sections = ExpandedComponents.GroupBy(c => c.ComponentLocator.SectionKey);
            foreach (var section in sections)
            {
                var current = section.Single(c => c.ComponentLocator.IndexInSection == 0);
                var totalComponents = section.Count();
                for (int i = 0; i < totalComponents; i++)
                {
                    if (i != current.ComponentLocator.IndexInSection)
                    {
                        throw new DataMisalignedException("Expected index " + i + " but got " + current.ComponentLocator.IndexInSection);
                    }
                    if (current is StraightSegment segment)
                    {
                        if (segment.BranchSideComponents.SingleOrDefault() != null && segment.BranchSideComponents.Single().GetType() == typeof(StraightSegment))
                        {
                            throw new Exception("StraightSegments should not be connected to pipes.");
                        }
                        var branchSide = segment.BranchSideComponents.Single();
                        var closestToStart = segment.Start.GetClosestPort(branchSide as Fitting, out var dStart);
                        const double tolerance = 0.001;
                        if (dStart > tolerance)
                        {
                            throw new Exception($"A StraightSegment.Start is too far from the likely best connector: {dStart}");
                        }
                        var closestToEnd = segment.End.GetClosestPort(segment.TrunkSideComponent as Fitting, out var dEnd);
                        if (dEnd > tolerance)
                        {
                            throw new Exception($"A StraightSegment.End is too far from the likely best connector: {dEnd}");
                        }
                    }

                    if (current.BranchSideComponents.Count > 1)
                    {
                        // Items that branch are the end of the section.
                        break;
                    }

                    var branch = current.BranchSideComponents.FirstOrDefault();
                    if (branch == null || branch.ComponentLocator.SectionKey != current.ComponentLocator.SectionKey)
                    {
                        if (i != totalComponents - 1)
                        {
                            throw new Exception("Did not find the correct number of components");
                        }
                        break;
                    }
                    current = branch;
                }
            }
        }

        private bool BalanceBranchTerminalAdditionalTransform(Terminal terminal, Transform transform)
        {
            if (transform.Equals(new Transform()))
            {
                return true;
            }

            // Balancing goes trunkside first until transformation is distributed among straight segments.
            // Then it goes branchside into every branch and move all component, by transformation, not distributed at that point.
            // Cache visited components so when algorithm goes backwards it doesn't visit the same branch twice.
            HashSet<ComponentBase> visited = new HashSet<ComponentBase>();

            if (BalanceAdditionalTransformToTrunkComponents(transform, terminal, visited))
            {
                foreach (var component in visited)
                {
                    component.ApplyAdditionalTransform();
                }
                return true;
            }
            else
            {
                foreach (var component in visited)
                {
                    component.ClearAdditionalTransform();
                }
                return false;
            }
        }

        private bool BalanceAdditionalTransformToBranchComponents(
            ComponentBase component, HashSet<ComponentBase> visited)
        {
            if (!component.BranchSideComponents.Any())
            {
                return visited.Contains(component);
            }

            bool transformationDissolved = true;
            var transform = component.GetPropagatedTransform(TransformDirection.TrunkToBranch);
            foreach (var branch in component.BranchSideComponents)
            {
                if (visited.Contains(branch))
                {
                    continue;
                }

                // TODO: Assembly is not always correctly replaced by internal components in next/previous links.
                // This leads to situation when one way there is Segment -> Elbow -> Elbow -> Reducer, but
                // Reducer -> Assembly -> Segment if traverse back.
                // This may be due to the fact that replacement code looks the best Port pair by comparing
                // positions and directions that may be yet fully setup and still have unresolved transformation.
                var branchAssembly = branch as Assembly;
                if (branchAssembly != null)
                {
                    if (branchAssembly.InternalFittings.Any(f => visited.Contains(f)) ||
                        branchAssembly.InternalSegments.Any(s => visited.Contains(s)))
                    {
                        continue;
                    }
                }

                if (branch.PropagateAdditionalTransform(transform, TransformDirection.TrunkToBranch))
                {
                    transformationDissolved &= BalanceAdditionalTransformToBranchComponents(branch, visited);
                    visited.Add(branch);
                    if (branchAssembly != null)
                    {
                        foreach (var item in branchAssembly.InternalFittings)
                        {
                            visited.Add(item);
                        }
                        foreach (var item in branchAssembly.InternalSegments)
                        {
                            visited.Add(item);
                        }
                    }
                }
            }
            return transformationDissolved;
        }

        private bool BalanceAdditionalTransformToTrunkComponents(
            Transform transform, ComponentBase component, HashSet<ComponentBase> visited)
        {
            visited.Add(component);
            if (component is Assembly assembly)
            {
                foreach (var item in assembly.InternalFittings)
                {
                    visited.Add(item);
                }
                foreach (var item in assembly.InternalSegments)
                {
                    visited.Add(item);
                }
            }

            if (component.PropagateAdditionalTransform(transform, TransformDirection.BranchToTrunk))
            {
                var trunk = component.TrunkSideComponent;
                if (trunk == null)
                {
                    return false;
                }

                if (BalanceAdditionalTransformToTrunkComponents(
                    component.GetPropagatedTransform(TransformDirection.BranchToTrunk), trunk, visited))
                {
                    return BalanceAdditionalTransformToBranchComponents(component, visited);
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        private void RemoveReducerOnBranchSideFromPipe(StraightSegment pipe, IReducer previousReducer)
        {
            // Branch direction: TrunkConnection => PS (newPipe) ? Reducer
            // Trunk direction: TrunkConnection <= PS (newPipe) <= Reducer
            if (previousReducer is Assembly reducerAssembly)
            {
                pipe.BranchSideComponents.RemoveAll(c => reducerAssembly.AllComponents.Contains(c));
            }
            else
            {
                pipe.BranchSideComponents.Remove(previousReducer as ComponentBase); // Currently reducers are always NetworkComponentBases
            }
            // Remove Reducer from Fittings list
            Fittings.Remove(previousReducer as Fitting);
            if (previousReducer.BranchSideComponents.Count() > 1)
            {
                throw new InvalidOperationException("Reducers should never have more than one branch side component.");
            }
            var branch = previousReducer.BranchSideComponents.FirstOrDefault();

            // Branch direction: TrunkConnection => PS (newPipe) => BranchConnection
            // Trunk direction: TrunkConnection <= PS (newPipe) <= Reducer <= BranchConnection
            pipe.BranchSideComponents.Add(branch);
            if (!(branch is Fitting) && !(branch is StraightSegment))
            {
                throw new Exception($"Unexpected type {branch.GetType().FullName} found on branch side of reducer.");
            }

            // Branch direction: TrunkConnection => PS (newPipe) => BranchConnection
            // Trunk direction: TrunkConnection <= PS (newPipe) <= BranchConnection
            branch.TrunkSideComponent = pipe;
        }

        private bool BranchSideReducerShouldBePreserved(IReducer previousReducer, double newDiameter)
        {
            // if a reducer exists even though it has no difference in its diameters, we assume it is intentional.
            if (previousReducer.BranchSideComponents.Single() is StraightSegment pipeSegment
                && previousReducer.Start.Diameter == previousReducer.End.Diameter
                && newDiameter == previousReducer.Start.Diameter)
            {
                return true;
            }
            return false;
        }

        private void RemoveReducerOnTrunkSideFromPipe(StraightSegment pipe, IReducer nextReducer)
        {
            // Branch direction: Reducer => PS (newPipe) => BranchConnection
            // Trunk direction: Reducer <= PS (newPipe) <= BranchConnection
            if (nextReducer.TrunkSideComponent != null)
            {
                // if reducer TrunkSideComponent is not null, we need to reassign components:
                // Branch direction: TrunkConnection => Reducer => PS (pipe) => BranchConnection
                // Trunk direction: TrunkConnection <= Reducer <= PS (pipe) <= BranchConnection
                // lets remove reducer:
                // Branch direction: TrunkConnection ? Reducer => PS (pipe) => BranchConnection
                // Trunk direction: TrunkConnection <= Reducer <= PS (pipe) <= BranchConnection
                if (nextReducer is Assembly reducerAssembly)
                {
                    nextReducer.TrunkSideComponent.BranchSideComponents.RemoveAll(c => reducerAssembly.AllComponents.Contains(c));
                }
                else
                {
                    nextReducer.TrunkSideComponent.BranchSideComponents.Remove(nextReducer as ComponentBase);
                }

                if (nextReducer.TrunkSideComponent is StraightSegment trunksidePipe)
                {
                    trunksidePipe.BranchSideComponents.Add(pipe);
                    pipe.TrunkSideComponent = trunksidePipe;
                }
                else if (nextReducer.TrunkSideComponent is Fitting trunksideFitting)
                {
                    trunksideFitting.BranchSideComponents.Add(pipe);
                    pipe.TrunkSideComponent = trunksideFitting;
                }
            }
            else
            {
                // if reducer TrunkSideComponent is null, we need to remove links on it:
                pipe.TrunkSideComponent = null;
            }

            // Remove reducer from Fittings list
            Fittings.Remove(nextReducer as Fitting);
        }

        private void RemoveEmptyStrainghtSegment(StraightSegment segment)
        {
            var trunk = segment.TrunkSideComponent;
            if (trunk != null)
            {
                trunk.BranchSideComponents.Remove(segment);
                foreach (var branch in segment.BranchSideComponents)
                {
                    trunk.BranchSideComponents.Add(branch);
                }
            }

            foreach (var branch in segment.BranchSideComponents)
            {
                branch.TrunkSideComponent = trunk;
            }

            StraightSegments.Remove(segment);
        }

        private bool TrunkSideReducerShouldBePreserved(IReducer nextReducer, double newDiameter)
        {
            // if a reducer exists even though it has no difference in its diameters, we assume it is intentional.
            if (nextReducer.TrunkSideComponent is StraightSegment pipeSegment
                && nextReducer.Start.Diameter == nextReducer.End.Diameter
                && nextReducer.Start.Diameter == newDiameter)
            {
                return true;
            }
            return false;
        }

        [Newtonsoft.Json.JsonIgnore]
        public IEnumerable<ComponentBase> AllComponents
        {
            get
            {
                return Fittings.Cast<ComponentBase>().Union(StraightSegments.Cast<ComponentBase>());
            }
        }

        /// <summary>
        /// The components of the system with the assemblies expanded.
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public IEnumerable<ComponentBase> ExpandedComponents
        {
            get
            {
                return ExpandAssembliesInternal(AllComponents);
            }
        }

        private Dictionary<string, ComponentBase[]> _componentLookup = null;
        internal List<SectionLabelingError> AssignFlowIdsToComponents(string sectionReference = null)
        {
            var labelingErrors = new List<SectionLabelingError>();
            //TODO I think there's a weakness here where the ComponentBase will only have their
            // information filled in when they are built by this FittingTree.Builder.  To access this
            // information in subsequent functions we are going to need an Initialize method perhaps.
            _componentLookup = new Dictionary<string, ComponentBase[]>();
            var componentsGroupedBySection = ExpandedComponents.GroupBy(c => c.ComponentLocator.SectionKey);
            if (sectionReference != null)
            {
                componentsGroupedBySection = componentsGroupedBySection.Where(g => g.Key == sectionReference);
            }
            int overallNetworkItemNumber = 0;

            foreach (var componentGroup in componentsGroupedBySection)
            {
                if (!TryGetOrderedComponents(componentGroup, out var orderedComponentList, out var error))
                {
                    if (error != null)
                    {
                        labelingErrors.Add(new SectionLabelingError(componentGroup.Key, Name, error));
                    }
                    continue;
                }
                if (componentGroup.Count() != orderedComponentList.Count())
                {
                    labelingErrors.Add(new SectionLabelingError(componentGroup.Key, 
                        Name,
                        "Lost or gained items while ordering the components, " +
                        "previous and next are probably set incorrectly, or a SectionKey is not assigned correctly."));
                    continue;
                }

                int i = 0;
                var reversed = orderedComponentList.Reverse().ToArray();
                foreach (var component in reversed)
                {
                    overallNetworkItemNumber++;
                    component.Name = component.GetAbbrev() + overallNetworkItemNumber;
                    component.ComponentLocator.IndexInSection = i;
                    switch (component)
                    {
                        case Fitting connection:
                            connection.ComponentLocator.IndexInSection = i;
                            break;
                        case StraightSegment segment:
                            segment.ComponentLocator.IndexInSection = i;
                            break;
                        default:
                            throw new Exception("Component was neither a Fitting, nor a StraightSegment, this is unexpected.");
                    }
                    i++;
                }
                _componentLookup[componentGroup.Key] = reversed;
            }
            return labelingErrors;
        }

        /// <summary>
        /// Expands assemblies from the list into a flat list of components.  Deprecated in favor of the ExpandedComponents property.
        /// </summary>
        /// <param name="components"></param>
        /// <param name="removeAssemblies"></param>
        /// <returns></returns>
        [Obsolete("Use the ExpandedComponents property which always removed the expanded assemblies.")]
        public static IEnumerable<ComponentBase> ExpandAssemblies(IEnumerable<ComponentBase> components, bool removeAssemblies = true)
        {
            return ExpandAssembliesInternal(components, removeAssemblies);
        }

        private static IEnumerable<ComponentBase> ExpandAssembliesInternal(IEnumerable<ComponentBase> components, bool removeAssemblies = true)
        {
            foreach (var c in components)
            {
                if (c is Assembly assembly)
                {
                    foreach (var component in assembly.AllComponents)
                    {
                        yield return component;
                    }
                    if (!removeAssemblies)
                    {
                        yield return assembly;
                    }
                }
                else
                {
                    yield return c;
                }
            }
        }

        public bool TryGetComponent(string sectionRef, int index, out ComponentBase component, bool forceUpdate = false)
        {
            if (_componentLookup == null || forceUpdate)
            {
                AssignFlowIdsToComponents();
            }
            if (_componentLookup.TryGetValue(sectionRef, out var components))
            {
                if (index < components.Length)
                {
                    component = _componentLookup[sectionRef][index];
                    return true;
                }
                else
                {
                    component = null;
                    return false;
                }
            }
            else
            {
                component = null;
                return false;
            }
        }

        /// <summary>
        /// Change a flow section to a given size from one side for a given length.
        /// </summary>
        /// <param name="s">The flow section to change</param>
        /// <param name="newDiameter">The desired diameter of the section.</param>
        /// <param name="lengthToChange">How much of the section should be changed.</param>
        /// <param name="travelTowardsTrunk">Start the size change at the branch side, default is to start at the trunk side.</param>
        public void ChangeSizeOfSection(Section s, double newDiameter, double lengthToChange, bool travelTowardsTrunk = false)
        {
            var components = GetComponentsOfSection(s).ToList();

            var current = travelTowardsTrunk ? components.Last() : components.First();
            RecursiveResize(current, newDiameter, travelTowardsTrunk, lengthToChange);

            var labelingErrors = AssignFlowIdsToComponents(s.SectionKey);
            if (labelingErrors.Count() > 0)
            {
                throw new Exception("Failed to resize.");
            }
        }

        /// <summary>
        /// Check if all fittings in the tree use compatible connectivity parameters.
        /// </summary>
        /// <returns>List of warnings messages for any incompatible ports.</returns>
        public List<Message> CheckConnectivities()
        {
            List<Message> warnings = new List<Message>();
            foreach (var fitting in Fittings)
            {
                var port = fitting.TrunkSidePort();
                var trunkside = fitting.TrunkSideComponent;
                if (trunkside == null)
                {
                    continue;
                }

                if (trunkside is StraightSegment trunksideSegment)
                {
                    var warning = CheckPortConnection(port, trunksideSegment.End, false);
                    if (warning != null)
                    {
                        warnings.Add(warning);
                    }
                }
                else
                {
                    var otherPort = GetBestComplementForPort(port, trunkside);
                    var warning = CheckPortConnection(port, otherPort, true);
                    if (warning != null)
                    {
                        warnings.Add(warning);
                    }
                }
            }
            return warnings;
        }

        /// <summary>
        /// Check if two neighbor ports, connected by straight segment or directly,
        /// are compatible with each other.
        /// </summary>
        /// <param name="left">First port.</param>
        /// <param name="right">Second port.</param>
        /// <param name="directConnection">Are two ports connected without other component in between.</param>
        /// <returns>Message if two ports are incompatible. Null otherwise.</returns>
        public Message? CheckPortConnection(Port left, Port right, bool directConnection)
        {
            StringBuilder message = new StringBuilder();

            // Connectivity data should be either set on neither of ports or both of them.
            if (left.ConnectionType != null ^ right.ConnectionType != null)
            {
                message.Append("One of the port has connectivity data missing. ");
            }

            // Dimensions data should be either set on neither of ports or both of them.
            if (left.Dimensions != null ^ right.Dimensions != null)
            {
                message.Append("One of the port has dimensions data missing. ");
            }

            if (left.ConnectionType != null && right.ConnectionType != null)
            {
                var leftConnectivity = left.ConnectionType.Connectivty;
                var rightConnectivity = right.ConnectionType.Connectivty;
                if (leftConnectivity != rightConnectivity)
                {
                    message.Append("Connectivity types don't match. ");
                }

                // If two fittings connected directly - they should either both
                // have None type or both be not None and have different end type.
                if (directConnection)
                {
                    var leftEndType = left.ConnectionType.EndType;
                    var rightEndType = right.ConnectionType.EndType;
                    if (leftEndType == PortConnectionTypeEndType.None || 
                        rightEndType == PortConnectionTypeEndType.None)
                    {
                        if (leftEndType != rightEndType)
                        {
                            message.Append("End types don't match. ");
                        }
                    }
                    else if (leftEndType != rightEndType)
                    {
                        message.Append("End types don't match. ");
                    }
                }
            }

            if (left.Dimensions != null && right.Dimensions != null)
            {
                var leftExtension = left.Dimensions.Extension;
                var rightExtension = right.Dimensions.Extension;
                if (!directConnection)
                {
                    var dist = left.Position.DistanceTo(right.Position);
                    if (leftExtension + rightExtension > dist)
                    {
                        message.Append("Port extensions don't fit. ");
                    }
                }
                else
                {
                    // Since two ports meet at the same point two extensions will collide.
                    // One fitting can go into other but without extension.
                    if (leftExtension > Vector3.EPSILON && rightExtension > Vector3.EPSILON)
                    {
                        message.Append("Both ports have extension. ");
                    }
                }
            }

            if (message.Length > 0)
            {
                return PortConnectivityMessage(left, right,  message.ToString());
            }
            return null;
        }

        private Message PortConnectivityMessage(Port left, Port right, string text)
        {
            var dist = left.Position.DistanceTo(right.Position);
            if (dist < 0.3)
            {
                return Message.FromPoint(text, left.Position.Average(right.Position));
            }
            else
            {
                var line = new Line(left.Position, right.Position);
                return Message.FromCurve(text, line);
            }
        }

        /// <summary>
        /// Recursively resize the fittings from a given component staying within the same section as the original component.
        /// </summary>
        /// <param name="current"></param>
        /// <param name="newDiameter"></param>
        /// <param name="startAtBranchSide"></param>
        /// <param name="remainingLength"></param>
        /// <exception cref="Exception"></exception>
        [Obsolete("This method is no longer being developed or supported. Resizing workflows should focus on resizing connections in the Flow.Tree.")]
        public void RecursiveResize(ComponentBase current, double newDiameter, bool startAtBranchSide, double remainingLength)
        {
            switch (current)
            {
                case Reducer reducer:
                    if (startAtBranchSide)
                    {
                        var connector = reducer.Start;
                        var component = reducer.BranchSideComponents.Single();
                        var reducerStartMatch = GetBestComplementForPort(connector, component);
                        if (reducer.Start.Diameter != reducerStartMatch.Diameter)
                        {
                            reducer.Start.Diameter = reducerStartMatch.Diameter;
                        }
                    }
                    else
                    {
                        var connector = reducer.End;
                        var component = reducer.TrunkSideComponent;
                        var reducerEndMatch = GetBestComplementForPort(connector, component);
                        if (reducer.End.Diameter != reducerEndMatch.Diameter)
                        {
                            reducer.End.Diameter = reducerEndMatch.Diameter;
                        }
                    }

                    break;
                case StraightSegment pipe:
                    if (pipe.Length() < remainingLength)
                    {
                        ResizePipe(pipe, newDiameter);
                        var labelingErrors2 = AssignFlowIdsToComponents(pipe.ComponentLocator.SectionKey);
                        if (labelingErrors2.Count() > 0)
                        {
                            throw new Exception("Failed to resize.");
                        }
                    }
                    else
                    {
                        var newFitting = SplitPipe(pipe, remainingLength, startAtBranchSide, out var newPipe, out var errors);
                        if (newFitting == null)
                        {
                            break;
                        }
                        if (errors?.Count() > 0)
                        {
                            throw new Exception($"Failed to split. {errors}");
                        }
                        ResizePipe(startAtBranchSide ? pipe : newPipe, newDiameter);
                        var labelingErrors3 = AssignFlowIdsToComponents(pipe.ComponentLocator.SectionKey);
                        return; // if we have split and resize we are done.
                    }
                    break;
                case Elbow elbow:
                    var newElbow = _routing.CreateElbow(newDiameter, elbow.Transform.Origin, elbow.Start.Direction, elbow.End.Direction);
                    ReplaceElbow(elbow, newElbow, startAtBranchSide);
                    current = newElbow;
                    break;
                case Terminal terminal:
                    terminal.Port.Diameter = newDiameter;
                    ResetNearbyPipeSegmentSizing(startAtBranchSide, terminal);
                    ResetNearbyPipeSegmentSizing(!startAtBranchSide, terminal);
                    break;
            }
            remainingLength -= current.GetLength();
            current = startAtBranchSide ? current.TrunkSideComponent : current.BranchSideComponents.FirstOrDefault(c => c.ComponentLocator.IsInSameSection(current.ComponentLocator));
            if (remainingLength > 0 && current != null)
            {
                RecursiveResize(current, newDiameter, startAtBranchSide, remainingLength);
            }
        }

        private void ReplaceElbow(Elbow oldElbow, Elbow newElbow, bool startAtBranchSide)
        {
            newElbow.TrunkSideComponent = oldElbow.TrunkSideComponent;
            newElbow.BranchSideComponents.Clear();
            newElbow.BranchSideComponents.AddRange(oldElbow.BranchSideComponents);
            newElbow.Name = oldElbow.Name;
            newElbow.ComponentLocator = oldElbow.ComponentLocator;
            newElbow.TrunkSideComponent.BranchSideComponents.Remove(oldElbow);
            newElbow.TrunkSideComponent.BranchSideComponents.Add(newElbow);
            newElbow.BranchSideComponents.ForEach(branch => branch.TrunkSideComponent = newElbow);

            var assembly = AllComponents
                           .OfType<Assembly>()
                           .Where(a => a.AllComponents.Contains(oldElbow))
                           .FirstOrDefault();
            if (assembly != null)
            {
                assembly.InternalFittings.Remove(oldElbow);
                assembly.InternalFittings.Add(newElbow);
                if (assembly.ExternalPorts.Contains(oldElbow.Start))
                {
                    assembly.ExternalPorts.Remove(oldElbow.Start);
                    assembly.ExternalPorts.Add(newElbow.Start);
                }
                else if (assembly.ExternalPorts.Contains(oldElbow.End))
                {
                    assembly.ExternalPorts.Remove(oldElbow.End);
                    assembly.ExternalPorts.Add(newElbow.End);
                }
            }
            else
            {
                Fittings.Remove(oldElbow);
                AddConnection(newElbow);
            }

            if (newElbow.TrunkSideComponent is StraightSegment pipe)
            {
                pipe.Start = newElbow.End;
            }
            if (newElbow.BranchSideComponents.FirstOrDefault() is StraightSegment branchPipe)
            {
                branchPipe.End = newElbow.Start;
            }

            ResetNearbyPipeSegmentSizing(startAtBranchSide, newElbow);
            ResetNearbyPipeSegmentSizing(!startAtBranchSide, newElbow);

            try
            {
                // check that elbow and trunk component are connected
                if (newElbow.TrunkSideComponent is Fitting trunkFitting)
                {
                    var trunkSidePort = GetBestComplementForPort(oldElbow.End.Position, oldElbow.End.Direction, trunkFitting);
                    TryConnectElbow(newElbow, assembly, trunkFitting, trunkSidePort, newElbow, newElbow.End);
                }

                // check that elbow and branch component are connected
                if (newElbow.BranchSideComponents.FirstOrDefault() is Fitting branchFitting)
                {
                    var branchSidePort = GetBestComplementForPort(oldElbow.Start.Position, oldElbow.Start.Direction, branchFitting);
                    TryConnectElbow(newElbow, assembly, newElbow, newElbow.Start, branchFitting, branchSidePort);
                }
            }
            catch (Exception)
            {
                ReplaceElbow(newElbow, oldElbow, startAtBranchSide);
                throw;
            }
        }

        private void TryConnectElbow(Elbow elbow, Assembly assembly, Fitting trunkConnection, Port trunkSidePort, Fitting branchConnection, Port branchSideConnector)
        {
            if (trunkSidePort != null && branchSideConnector != null
                && !trunkSidePort.Position.IsAlmostEqualTo(branchSideConnector.Position, _portsDistanceTolerance))
            {
                if (!CanCreatePipe(trunkSidePort, branchSideConnector))
                {
                    throw new Exception($"Can't resize elbow {elbow.Name}. Pipe connections are clashing.");
                }
                var newPipe = MakePipe(trunkConnection, trunkSidePort, branchSideConnector, branchConnection, out var createdConnectors);
                createdConnectors.ForEach(c => c.ComponentLocator.MatchNetworkSection(elbow.ComponentLocator));
                newPipe.ComponentLocator.MatchNetworkSection(elbow.ComponentLocator);
                if (assembly != null && assembly.AllComponents.Contains(branchConnection))
                {
                    assembly.InternalSegments.Add(newPipe);
                }
                else
                {
                    AddStraightSegments(new[] { newPipe });
                }
            }
        }

        private static bool CanCreatePipe(Port trunkConnector, Port branchConnector)
        {
            var angleOfPipe = (branchConnector.Position - trunkConnector.Position).AngleTo(branchConnector.Direction);
            return angleOfPipe.ApproximatelyEquals(180, 1);
        }

        private void ResetNearbyPipeSegmentSizing(bool startAtBranchSide, ComponentBase elbow)
        {
            if (startAtBranchSide)
            {
                var pipeToResize = elbow.FindBranchsideOfTypeInSection<StraightSegment>();
                if (pipeToResize != null)
                {
                    // resize to same diameter to cleanup reducers
                    ResizePipe(pipeToResize, pipeToResize.Diameter);
                }
            }
            else
            {
                var pipeToResize2 = elbow.FindTrunksideOfTypeInSection<StraightSegment>();
                if (pipeToResize2 != null)
                {
                    // resize to same diameter to cleanup reducers
                    ResizePipe(pipeToResize2, pipeToResize2.Diameter);
                }
            }
        }

        private static bool TryGetOrderedComponents(IEnumerable<ComponentBase> componentGroup, out LinkedList<ComponentBase> orderedComponentList, out string error)
        {
            error = null;

            orderedComponentList = new LinkedList<ComponentBase>();
            orderedComponentList.AddFirst(componentGroup.First());
            
            var visited = new HashSet<ComponentBase>();
            while (true)
            {
                var t = orderedComponentList.Last.Value.TrunkSideComponent;
                if (t == null || t.ComponentLocator.SectionKey != orderedComponentList.Last.Value.ComponentLocator.SectionKey)
                {
                    break;
                }
                if (t == orderedComponentList.Last.Value || visited.Contains(t))
                {
                    error = "There is a loop in the section.  Cannot label the components.  Loop found while navigating towards the \"Trunk\".";
                    return false;
                }
                orderedComponentList.AddLast(t);
                visited.Add(t);
            }

            bool isProceed = true;
            while (isProceed)
            {
                if (!orderedComponentList.First.Value.BranchSideComponents.Any())
                {
                    break;
                }

                if (orderedComponentList.First.Value.BranchSideComponents.Count > 1)
                {
                    break;
                }

                foreach (var branch in orderedComponentList.First.Value.BranchSideComponents)
                {
                    if (branch == null || branch.ComponentLocator.SectionKey != orderedComponentList.First.Value.ComponentLocator.SectionKey)
                    {
                        isProceed = false;
                        break;
                    }

                    if (branch == orderedComponentList.First.Value || visited.Contains(branch))
                    {
                        error = "There is a loop in the section.  Cannot label the components.  Loop found while navigating towards the \"Branches\".";
                        return false;
                    }
                    orderedComponentList.AddFirst(branch);
                    visited.Add(branch);
                }
            }

            return true;
        }
    }
}