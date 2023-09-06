using System;
using System.Collections.Generic;
using System.Linq;
using Elements.Flow;
using Elements.Geometry;
using Elements.Geometry.Solids;
using Newtonsoft.Json;

namespace Elements.Fittings
{
    public partial class Assembly
    {
        private PositionComparer _positionComparer;
        private double _positionTolerance;

        [JsonProperty]
        public double PositionTolerance
        {
            get => _positionTolerance;
            private set
            {
                _positionTolerance = value;
                _positionComparer = new PositionComparer(_positionTolerance);
            }
        }

        /// <summary>
        /// Create an assembly at the given position.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="material"></param>
        /// <param name="positionTolerance"></param>
        public Assembly(Vector3 position, Material material, double positionTolerance = 0.01) : base(false, FittingLocator.Empty(), new Transform(position),
                                                                            material == null ? FittingTreeRouting.DefaultFittingMaterial : material,
                                                                            new Representation(new List<SolidOperation>()),
                                                                            false,
                                                                            Guid.NewGuid(),
                                                                            "")
        {
            this.Transform = new Transform(position);
            this.ExternalPorts = new List<Port>();
            PositionTolerance = positionTolerance;
        }

        /// <inheritdoc/>
        public override Port[] GetPorts()
        {
            return this.ExternalPorts.ToArray();
        }

        /// <summary>
        ///
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public IEnumerable<ComponentBase> AllComponents
        {
            get
            {
                return InternalFittings.Cast<ComponentBase>().Union(InternalSegments);
            }
        }

        public void AssignTrunkComponentsInternally(Connection connection = null)
        {
            var allInternal = new HashSet<ComponentBase>(AllComponents);
            ComponentBase startingComponent = null;
            if (connection != null)
            {
                startingComponent = ComponentClosestToConnection(connection);
                // remove the starting component from the set of all remaining components if we are starting with an item from the assembly.
                allInternal.Remove(startingComponent);
            }
            else
            {
                startingComponent = TrunkSideComponent;
            }
            FindBranchSideAndAssignTrunk(startingComponent, allInternal);
        }

        /// <summary>
        /// Assigns all of the components in the assembly to the given FittingLocator.
        /// </summary>
        /// <param name="reference"></param>
        public void AssignSectionReferenceInternalToAssembly(FittingLocator reference)
        {
            foreach (var component in InternalFittings)
            {
                component.ComponentLocator.MatchNetworkSection(reference);
            }
            foreach (var pipe in InternalSegments)
            {
                pipe.ComponentLocator.MatchNetworkSection(reference);
            }
        }

        private class PositionComparer : IEqualityComparer<Vector3>
        {
            private readonly double _tolerance;

            public PositionComparer(double tolerance)
            {
                _tolerance = tolerance;
            }
            public bool Equals(Vector3 x, Vector3 y)
            {
                return x.IsAlmostEqualTo(y, _tolerance);
            }

            public int GetHashCode(Vector3 obj)
            {
                return base.GetHashCode();
            }
        }

        private bool ComponentsAreConnected(ComponentBase first, ComponentBase second)
        {
            HashSet<Vector3> connectorPositionSet1 = GetConnectorPositions(first);
            HashSet<Vector3> connectorPositionSet2 = GetConnectorPositions(second);

            return connectorPositionSet1.Intersect(connectorPositionSet2, _positionComparer).Any();
        }

        private HashSet<Vector3> GetConnectorPositions(ComponentBase component)
        {
            return new HashSet<Vector3>(component.GetPorts().Select(c => c.Position));
        }

        private ComponentBase FindFirstComponentConnectedToConnection(ComponentBase component, IEnumerable<ComponentBase> possibleComponents)
        {
            var fastFind = possibleComponents.FirstOrDefault(c => c.TrunkSideComponent == component || component.TrunkSideComponent == c);
            if (fastFind != null)
            {
                return fastFind;
            }
            foreach (var thisComponent in possibleComponents)
            {
                if (ComponentsAreConnected(thisComponent, component))
                {
                    return thisComponent;
                }
            }
            return null;
        }

        // TODO a method similar to this one could be used on an entire FittingTree to re-establish the
        // Next/Previous component relationships which will currently be lost on serialization.
        private void FindBranchSideAndAssignTrunk(ComponentBase trunkComponent, HashSet<ComponentBase> remaining)
        {
            var previous = FindFirstComponentConnectedToConnection(trunkComponent, remaining);
            // Use while-loop to try to find multiple connected "Branch" components that should all have this item assigned as their "next".
            while (previous != null)
            {
                if (trunkComponent is Assembly trunkComponentAssembly)
                {
                    previous.TrunkSideComponent = FindFirstComponentConnectedToConnection(previous, trunkComponentAssembly.AllComponents);
                }
                else
                {
                    previous.TrunkSideComponent = trunkComponent;
                }

                trunkComponent.BranchSideComponents.Remove(this);
                if (!trunkComponent.BranchSideComponents.Contains(previous))
                    trunkComponent.BranchSideComponents.Add(previous);


                // Recurse here on the found "Branch" on all remaining items.
                remaining.Remove(previous);
                FindBranchSideAndAssignTrunk(previous, remaining);

                // see if there is another "Branch" component that is connected to this trunk.
                var additionalBranch = FindFirstComponentConnectedToConnection(trunkComponent, remaining);
                if (additionalBranch != null)
                {
                    FindBranchSideAndAssignTrunk(trunkComponent, remaining);
                }

                trunkComponent = previous;
                previous = FindFirstComponentConnectedToConnection(previous, remaining);
            }
        }

        internal void AssignNetworkLocatorAlongBranchComponents(Connection branchConnection, FittingLocator locator)
        {
            var component = ComponentClosestToConnection(branchConnection);
            var allInternal = new HashSet<ComponentBase>(AllComponents);
            while (component != null)
            {
                // don't apply branchside component locator to a wye component, those are located on the trunk
                if (component is Wye)
                {
                    break;
                }
                allInternal.Remove(component);
                component.ComponentLocator = locator;
                component = FindFirstComponentConnectedToConnection(component, allInternal);
            }
        }

        public ComponentBase ComponentClosestToConnection(Connection incoming)
        {
            ComponentBase foundComponent = null;
            var bestDistance = double.MaxValue;
            Port bestConnector = null;
            foreach (var connector in ExternalPorts)
            {
                var distance = connector.Position.DistanceTo(incoming.Path());
                if (distance < bestDistance)
                {
                    foreach (var component in AllComponents)
                    {
                        if (component.GetPorts().Contains(connector))
                        {
                            foundComponent = component;
                            break;
                        }
                    }
                    bestConnector = connector;
                    bestDistance = distance;
                }
            }
            return foundComponent;
        }

        internal void AssignBranchComponentInternallyAndBurrowSectionRef(ComponentBase branchSideConnection)
        {
            var internalComponentThatIsConnected = FindFirstComponentConnectedToConnection(branchSideConnection, this.AllComponents);
            internalComponentThatIsConnected.BranchSideComponents.RemoveAll(b => branchSideConnection.BranchSideComponents.Contains(b));
            if (branchSideConnection is Assembly branchSideAssembly)
            {
                internalComponentThatIsConnected.BranchSideComponents.Add(branchSideAssembly.GetInternalTrunkComponent());
            }
            else
            {
                internalComponentThatIsConnected.BranchSideComponents.Add(branchSideConnection);
            }

            branchSideConnection.TrunkSideComponent = internalComponentThatIsConnected;

            var current = internalComponentThatIsConnected;
            while (current != null
                   && !(current is Wye)
                   && this.AllComponents.Contains(current))
            {
                SetComponentNetworkAndSection(branchSideConnection.ComponentLocator, current);
                current = current.TrunkSideComponent;
            }
            if (TrunkSideComponent != null)
            {

                var last = GetInternalTrunkComponent();
            }
        }

        private static void SetComponentNetworkAndSection(FittingLocator componentLocator, ComponentBase internalComponentThatIsConnected)
        {
            if (internalComponentThatIsConnected.ComponentLocator == null)
            {
                internalComponentThatIsConnected.ComponentLocator = new FittingLocator(componentLocator.NetworkReference, componentLocator.SectionKey, 0);
            }
            else
            {
                internalComponentThatIsConnected.ComponentLocator.NetworkReference = componentLocator.NetworkReference;
                internalComponentThatIsConnected.ComponentLocator.SectionKey = componentLocator.SectionKey;
            }
        }

        public override void ApplyAdditionalTransform()
        {
            foreach (var connection in InternalFittings)
            {
                connection.AdditionalTransform.Concatenate(AdditionalTransform);
                connection.ApplyAdditionalTransform();
            }

            Transform.Concatenate(AdditionalTransform);
            ClearAdditionalTransform();
        }

        internal ComponentBase GetInternalTrunkComponent()
        {
            AssignTrunkComponentsInternally();
            ComponentBase last = AllComponents.LastOrDefault();

            while (true)
            {
                if (last.TrunkSideComponent == null || !AllComponents.Contains(last.TrunkSideComponent))
                {
                    return last;
                }
                last = last.TrunkSideComponent;
            }
            throw new InvalidOperationException("No valid last component found");
        }

        /// <inheritdoc/>
        public override List<Port> BranchSidePorts()
        {
            if (BranchSideComponents.Any())
            {
                var ports = new List<Port>();
                foreach (var component in BranchSideComponents)
                {
                    component.GetMatchConnectorByDistance(this, out _, out var connector);
                    if (connector != null)
                    {
                        ports.Add(connector);
                    }
                }
                if (ports.Distinct().Count() != BranchSideComponents.Count())
                {
                    throw new InvalidOperationException($"Incorrect number of ports found on branch side of an Assembly.\nConsider overriding the default {nameof(BranchSidePorts)} method, or check the {nameof(Assembly)} implementation of this.");
                }
                return ports;
            }
            return null;
        }

        /// <inheritdoc/>
        public override Port TrunkSidePort()
        {
            if (TrunkSideComponent == null)
            {
                return null;
            }
            TrunkSideComponent.GetMatchConnectorByDistance(this, out _, out var connector);
            return connector;
        }

        public override Transform GetRotatedTransform()
        {
            throw new NotImplementedException();
        }
    }
}
