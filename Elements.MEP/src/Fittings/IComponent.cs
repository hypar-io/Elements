using System;
using System.Collections.Generic;
using System.Linq;
using Elements.Geometry;
using Newtonsoft.Json;

namespace Elements.Fittings
{
    // NOTE this interface is an integral part of component retrieval form a pipe network.
    // Some of the code in FittingTree could maybe be factored out and matched with this interface
    // in terms of the a standalone set of functionality.


    public abstract partial class ComponentBase : IComponent
    {
        public static bool UseRepresentationInstances = false;
        /// <summary>
        /// The component that is towards the trunk of the tree.
        /// </summary>
        public ComponentBase TrunkSideComponent { get; set; }

        /// <summary>
        /// The components that are towards the leaves of the tree.
        /// </summary>
        [JsonIgnore]
        public List<ComponentBase> BranchSideComponents { get; set; } = new List<ComponentBase>();

        /// <summary>
        /// Gets the AdditionalTransformation that should be applied to the connection
        /// Call ApplyAdditionalTransform() to apply it to connectors
        /// </summary>
        public Transform AdditionalTransform { get; set; } = new Transform();

        /// <summary>
        /// The pressure calculation data for this component.
        /// </summary>
        [Newtonsoft.Json.JsonProperty("Pressure Calculations", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public PressureCalculationBase PressureCalculations { get; set; }

        /// <summary>
        /// Apply additional transforms to this component.  Intended for internal use handling fitting eccentricity.
        /// </summary>
        public abstract void ApplyAdditionalTransform();

        public abstract void ClearAdditionalTransform();

        /// <summary>
        /// Checks if transformation should be applied and propagated to the next connections.
        /// It's intended to "pull" the supplied transform into the current component,
        /// and then return whether the transform needs to continue propagating.
        /// </summary>
        /// <param name="transform">The transform that will be concatenated to the existed</param>
        /// <param name="transformDirection">The direction from which the transformation was received</param>
        /// <returns>Returns if transform should be applied to the next connections</returns>
        public abstract bool PropagateAdditionalTransform(Transform transform, TransformDirection transformDirection);

        /// <summary>
        /// Returns the transform that this component induces on the next connection.
        /// </summary>
        /// <param name="transformDirection"></param>
        /// <returns></returns>
        public abstract Transform GetPropagatedTransform(TransformDirection transformDirection);

        /// <summary>
        /// The connector that is on the trunkside of the component.
        /// </summary>
        /// <returns></returns>
        [Obsolete("Use TrunkSidePort")]
        public Port TrunkSideConnector()
        {
            return TrunkSidePort();
        }

        /// <summary>
        /// The port that is on the trunkside of the component.
        /// </summary>
        /// <returns></returns>
        public abstract Port TrunkSidePort();

        /// <summary>
        /// The connectors that are on the branchside of the component.
        /// </summary>
        /// <returns></returns>
        [Obsolete("Use BranchSidePorts")]
        public List<Port> BranchSideConnectors()
        {
            return BranchSidePorts();
        }

        /// <summary>
        /// The ports that are on the branchside of the component.
        /// </summary>
        /// <returns></returns>
        public abstract List<Port> BranchSidePorts();

        /// <summary>
        /// String composed of name and ComponentLocator data.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{GetType().Name}({Name}, {ComponentLocator})";
        }
    }

    /// <summary>
    /// The FittingTree is a branching structure, with a trunk at the trunk of a Flow.Tree.
    /// Trunkside takes you towards the trunk.
    /// Branchside takes you towards the leaves.
    /// Trunkside will always correctly bring you to the trunk, but Branchside will not bring you to any particular leaf.
    /// This interface has mostly been replaced by using the ComponentBase class but continues to exist so that IReducer can inherit from it, and ensure access to
    /// to the require methods.
    /// </summary>
    public interface IComponent
    {
        /// <summary>
        /// The component that is towards the trunk of the tree.
        /// </summary>
        ComponentBase TrunkSideComponent { get; set; }

        /// <summary>
        /// The components that are towards the leaves of the tree.
        /// </summary>
        List<ComponentBase> BranchSideComponents { get; }

        /// <summary>
        /// Gets the AdditionalTransformation that should be applied to the connection
        /// Call ApplyAdditionalTransform() to apply it to connectors
        /// </summary>
        Transform AdditionalTransform { get; }

        /// <summary>
        /// The data needed to find a component in a network without static ids.
        /// </summary>
        FittingLocator ComponentLocator { get; set; }

        /// <summary>
        /// Checks if transformation should be applied and propagated to the next connections.
        /// It's intended to "pull" the supplied transform into the current component,
        /// and then return wether the transform needs to continue propagating.
        /// </summary>
        /// <param name="transform">The transform that will be concatenated to the existed</param>
        /// <param name="transformDirection">The direction from which the transformation was received</param>
        /// <returns>Returns if transform should be applied to the next connections</returns>
        bool PropagateAdditionalTransform(Transform transform, TransformDirection transformDirection);

        /// <summary>
        /// Returns the transform that this component induces on the next connection.
        /// </summary>
        /// <param name="transformDirection"></param>
        /// <returns></returns>
        Transform GetPropagatedTransform(TransformDirection transformDirection);

        /// <summary>
        /// Apply additional transforms to this component.  Intended for internal use handling fitting eccentricity.
        /// </summary>
        void ApplyAdditionalTransform();
    }

    public static class NetworkExtensions
    {
        /// <summary>
        /// Returns the connector from this INetworkComponent that is closest to a connector on the other component.
        /// </summary>
        public static Port GetMatchConnectorByDistance(this ComponentBase component, ComponentBase otherComponent, out double distance, out Port otherComponentConnector)
        {
            Port closestMatch = null;
            otherComponentConnector = null;
            distance = double.MaxValue;
            foreach (var localConn in component.GetPorts())
            {
                foreach (var otherConn in otherComponent.GetPorts())
                {
                    var currentDistance = localConn.Position.DistanceTo(otherConn.Position);
                    if (currentDistance < distance)
                    {
                        closestMatch = localConn;
                        otherComponentConnector = otherConn;
                        distance = currentDistance;
                    }
                }
            }
            return closestMatch;
        }

        /// <summary>
        /// Gets all of the ports of the component, including trunkside and branchside.
        /// </summary>
        /// <param name="component"></param>
        /// <returns></returns>
        public static Port[] GetPorts(this ComponentBase component)
        {
            if (component is StraightSegment pipe)
            {
                return new[] { pipe.Start, pipe.End };
            }

            if (component is Fitting connection)
            {
                return connection.GetPorts();
            }

            else
            {
                throw new ArgumentException($"That component is not of a valid type. Component type is : {component.GetType()}");
            }
        }

        /// <summary>
        /// Gets all of the ports of this component, trunkside and branchside.
        /// </summary>
        /// <param name="component"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        [Obsolete("Use GetPorts() instead")]
        public static Port[] GetConnectors(this ComponentBase component)
        {
            return component.GetPorts();
        }

        /// <summary>
        /// Try to get the Guid of this component.
        /// </summary>
        /// <param name="component"></param>
        /// <param name="guid"></param>
        /// <returns></returns>
        public static bool TryGetGuid(this ComponentBase component, out Guid guid)
        {
            switch (component)
            {
                case Fitting connection:
                    guid = connection.Id;
                    return true;
                case StraightSegment pipe:
                    guid = pipe.Id;
                    return true;
                default:
                    guid = default(Guid);
                    return false;
            }
        }

        /// <summary>
        /// Finds the next branchside component of a given type, only from within a current section.
        /// Because sections are by definition one stretch of components with no internal branching we
        /// inspect only the first Branchside component.
        /// </summary>
        public static T FindBranchsideOfTypeInSection<T>(this ComponentBase component) where T : ComponentBase
        {
            while (true)
            {
                component = component.BranchSideComponents.FirstOrDefault(c => c.IsInSameSection(component));
                if (component == null)
                {
                    return default(T);
                }
                if (component.GetType() == typeof(T))
                {
                    return (T)component;
                }
            }
        }

        /// <summary>
        /// Finds the next trunkside component of a given type, only from within a current section.
        /// </summary>
        public static T FindTrunksideOfTypeInSection<T>(this ComponentBase component) where T : ComponentBase
        {
            while (true)
            {
                if (component == null)
                {
                    return default(T);
                }
                if (component.TrunkSideComponent == null || !component.TrunkSideComponent.IsInSameSection(component))
                {
                    return default(T);
                }
                component = component.TrunkSideComponent;
                if (component.GetType() == typeof(T))
                {
                    return (T)component;
                }
            }
        }

        /// <summary>
        /// Find all of trunkside components from this component to the end of the network.
        /// </summary>
        /// <param name="component"></param>
        /// <param name="stayInSection">Should we only return trunkside components that are in the same section?</param>
        /// <returns></returns>
        public static List<ComponentBase> GetAllTrunksideComponents(this ComponentBase component, bool stayInSection = false)
        {
            var components = new List<ComponentBase>();
            while (true)
            {
                if (component == null)
                {
                    return components;
                }
                components.Add(component);
                if (component.TrunkSideComponent == null || (stayInSection && !component.TrunkSideComponent.IsInSameSection(component)))
                {
                    return components;
                }
                component = component.TrunkSideComponent;
            }
        }

        /// <summary>
        /// Check if the given components is in the same section as this one.
        /// </summary>
        public static bool IsInSameSection(this ComponentBase component, ComponentBase other)
        {
            return component.ComponentLocator.IsInSameSection(other.ComponentLocator);
        }

        /// <summary>
        /// Get the length of a component.  Won't work for components
        /// with more than one branch port.
        /// </summary>
        /// <param name="component"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static double GetLength(this ComponentBase component)
        {
            switch (component)
            {
                case Elbow elbow:
                    if (elbow.PressureCalculations != null)
                    {
                        return elbow.PressureCalculations.Length + elbow.PressureCalculations.LengthStart;
                    }
                    else
                    {
                        return elbow.End.Position.DistanceTo(elbow.Transform.Origin) + elbow.Start.Position.DistanceTo(elbow.Transform.Origin);
                    }
                case Reducer reducer:
                    if (reducer.PressureCalculations != null)
                    {
                        return reducer.PressureCalculations.Length + reducer.PressureCalculations.LengthStart;
                    }
                    else
                    {
                        return reducer.Length();
                    }
                case StraightSegment ps:
                    return ps.Length();
                case Terminal t:
                    var heightDelta = Math.Abs(t.Transform.Origin.Z - t.Port.Position.Z);

                    var terminalTransformOrigin = t.Transform.Origin.Project(Plane.XY);
                    var terminalPortPosition = t.Port.Position.Project(Plane.XY);

                    return terminalTransformOrigin.IsAlmostEqualTo(terminalPortPosition)
                        ? heightDelta
                        : heightDelta + terminalTransformOrigin.DistanceTo(terminalPortPosition);
                case Coupler c:
                    return c.Start.Position.DistanceTo(c.End.Position);
                case Wye _:
                case Assembly _:
                    throw new ArgumentException($"${component.GetType().Name} doesn't have length calculated");
                default:
                    throw new ArgumentException($"${component.GetType().Name} doesn't have length calculated");
            }
        }

        /// <summary>
        /// Get the standard prefix for a component.
        /// </summary>
        /// <param name="component"></param>
        /// <returns></returns>
        public static string GetAbbrev(this ComponentBase component)
        {
            var prefix = "?-";
            switch (component)
            {
                case Elbow _:
                    prefix = "E-";
                    break;
                case Terminal _:
                    prefix = "T-";
                    break;
                case Wye _:
                    prefix = "Y-";
                    break;
                case Reducer _:
                    prefix = "R-";
                    break;
                case Assembly _:
                    prefix = "A-";
                    break;
                case StraightSegment _:
                    prefix = "PS-";
                    break;
                case Cross _:
                    prefix = "X-";
                    break;
                case Coupler coupler:
                    {
                        if (coupler is ExpansionSocket)
                        {
                            prefix = "ES-";
                        }
                        else if (coupler is InspectionOpening)
                        {
                            prefix = "IO-";
                        }
                        else
                        {
                            prefix = "EC-";
                        }
                    }
                    break;
                case Manifold _:
                    prefix = "M-";
                    break;
                default:
                    prefix = "?-";
                    break;
            }
            return prefix;
        }
    }
}