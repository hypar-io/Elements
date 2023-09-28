using System;
using System.Collections.Generic;
using System.Linq;
using Elements.Flow;
using Elements.Geometry;

namespace Elements.Fittings
{
    /// <summary>
    /// A collection of fluid and system properties globally availble for pressure calculations.
    /// </summary>
    public static class FluidFlowGlobals
    {
        /// <summary>
        /// ρ = density of water in kg/m^3
        /// </summary>
        public const double Rho = 1000;
        /// <summary>
        /// dynamic viscosity of water in Pa*s
        /// </summary>
        public const double Mu = 0.001;
        /// <summary>
        /// acceleration due to gravity in m/s^2
        /// </summary>
        public const double Gravity = 9.81;
        /// <summary>
        /// The roughness parameter of the interior of the pipe.
        /// </summary>
        public static double WallRoughness = 0.025;
    }

    public partial class PressureCalculationBase
    {
        /// <summary>
        /// The element id of the Trunk side component.
        /// </summary>
        public Guid NextComponentId; // Next is the trunk side component
    }

    public partial class PressureCalculationReducer
    {
        /// <summary>
        /// Initialize a PressureCalculationReducer with an element id.
        /// </summary>
        /// <param name="elementId"></param>
        public PressureCalculationReducer(Guid elementId) : base(elementId) { }

        /// <summary>
        /// Initialize the pressure calculation data for a reducer.
        /// Assigns the flow rate from the End port.
        /// </summary>
        /// <param name="reducer"></param>
        public PressureCalculationReducer(Reducer reducer) : base(reducer.Id)
        {
            Flow = reducer.End.Flow.FlowRate;
            HeightDelta = reducer.Start.Position.Z - reducer.End.Position.Z;
            StaticGain = FluidFlow.StaticGainForHeightDelta(HeightDelta);
            Diameter = reducer.End.Diameter;
            StartDiameter = reducer.Start.Diameter;
        }
    }

    public partial class PressureCalculationWye
    {
        /// <summary>
        /// Initialize a PressureCalculationWye with an element id.
        /// </summary>
        /// <param name="elementId"></param>
        public PressureCalculationWye(Guid elementId) : base(elementId) { }

        /// <summary>
        /// Initialize pressure calculation data for a wye.
        /// Assigns the flow rates from the wye ports.
        /// </summary>
        /// <param name="wye"></param>
        public PressureCalculationWye(Wye wye) : base(wye.Id)
        {
            Flow = wye.Trunk.Flow.FlowRate;
            FlowBranch = wye.SideBranch.Flow.FlowRate;
            FlowMain = wye.MainBranch.Flow.FlowRate;
            HeightDelta = wye.MainBranch.Position.Z - wye.Trunk.Position.Z;
            HeightDeltaBranchToTrunk = wye.SideBranch.Position.Z - wye.Trunk.Position.Z;
            StaticGain = FluidFlow.StaticGainForHeightDelta(HeightDelta);
            StaticGainBranchToTrunk = FluidFlow.StaticGainForHeightDelta(HeightDeltaBranchToTrunk);
            Diameter = wye.Trunk.Diameter;
            BranchDiameter = wye.SideBranch.Diameter;
            MainDiameter = wye.MainBranch.Diameter;
        }
    }
    public partial class PressureCalculationSegment
    {
        /// <summary>
        /// Initialize a PressureCalculationSegment with an element id.
        /// </summary>
        /// <param name="elementId"></param>
        public PressureCalculationSegment(Guid elementId) : base(elementId) { }

        /// <summary>
        /// Initialize pressure calculation data for a straight segment.
        /// Assigns the Flow and Length.
        /// </summary>
        /// <param name="segment"></param>
        public PressureCalculationSegment(StraightSegment segment) : base(segment.Id)
        {
            Flow = segment.End.Flow.FlowRate;
            Length = segment.Length();
            HeightDelta = segment.Start.Position.Z - segment.End.Position.Z;
            StaticGain = FluidFlow.StaticGainForHeightDelta(HeightDelta);
            Diameter = segment.End.Diameter;
        }
    }

    public partial class PressureCalculationElbow
    {
        /// <summary>
        /// Initialize a PressureCalculationElbow with an element id.
        /// </summary>
        /// <param name="elementId"></param>
        public PressureCalculationElbow(Guid elementId) : base(elementId) { }

        /// <summary>
        /// Initialize pressure calculation data for an elbow.
        /// Assigns the flow rate from the End port.
        /// </summary>
        /// <param name="elbow"></param>
        public PressureCalculationElbow(Elbow elbow) : base(elbow.Id)
        {
            Flow = elbow.End.Flow.FlowRate;
            HeightDelta = elbow.Start.Position.Z - elbow.End.Position.Z;
            StaticGain = FluidFlow.StaticGainForHeightDelta(HeightDelta);
            Diameter = elbow.End.Diameter;
        }
    }
    public partial class PressureCalculationTerminal
    {
        /// <summary>
        /// Initialize a PressureCalculationTerminal with an element id.
        /// </summary>
        /// <param name="elementId"></param>
        /// <param name="trunkSideComponent"></param>
        /// <param name="fixedPressure"></param>
        /// <exception cref="Exception"></exception>
        public PressureCalculationTerminal(Guid elementId, Guid? trunkSideComponent, double? fixedPressure) : base(elementId)
        {
            if ((trunkSideComponent == default(Guid) || trunkSideComponent == null) && !fixedPressure.HasValue)
            {
                throw new Exception("Terminals must have either a fixed flow or a \"trunkSideComponent\" in order for pressure data to be assigned correctly");
            }
            NextComponentId = trunkSideComponent ?? default(Guid);
            FixedPressure = fixedPressure;
        }

        /// <summary>
        /// Initialize pressure calculation data for a terminal.
        /// </summary>
        /// <param name="terminal"></param>
        /// <param name="fixedPressure"></param>
        public PressureCalculationTerminal(Terminal terminal, double? fixedPressure) : this(terminal.Id,
                                                                                          terminal.TrunkSideComponent?.Id,
                                                                                          fixedPressure)
        {
            Flow = terminal.Port.Flow.FlowRate;
            HeightDelta = terminal.Port.Position.Z - terminal.Transform.Origin.Z;
            if (terminal.FlowNode is Leaf)
            {
                HeightDelta = -HeightDelta;
            }
            StaticGain = FluidFlow.StaticGainForHeightDelta(HeightDelta);
            Diameter = terminal.Port.Diameter;
        }
    }

    public partial class PressureCalculationCoupler
    {
        /// <summary>
        /// Initialize a PressureCalculationCross with an element id.
        /// </summary>
        /// <param name="elementId"></param>
        public PressureCalculationCoupler(Guid elementId) : base(elementId) { }

        /// <summary>
        /// Initialize pressure calculation data for a coupler.
        /// Assigns the flow rate from the coupler End port.
        /// </summary>
        /// <param name="coupler"></param>
        public PressureCalculationCoupler(Coupler coupler) : base(coupler.Id)
        {
            Flow = coupler.End.Flow.FlowRate;
            HeightDelta = coupler.Start.Position.Z - coupler.End.Position.Z;
            StaticGain = FluidFlow.StaticGainForHeightDelta(HeightDelta);
            Diameter = coupler.End.Diameter;
        }
    }

    public partial class PressureCalculationCross
    {
        /// <summary>
        /// Initialize a PressureCalculationCross with an element id.
        /// </summary>
        /// <param name="elementId"></param>
        public PressureCalculationCross(Guid elementId) : base(elementId) { }

        /// <summary>
        /// Initialize pressure calculation data for a cross.
        /// Assigns the flow rates from the cross ports.
        /// </summary>
        /// <param name="cross"></param>
        public PressureCalculationCross(Cross cross) : base(cross.Id)
        {
            Flow = cross.Trunk.Flow.FlowRate;
            FlowA = cross.BranchA.Flow.FlowRate;
            FlowB = cross.BranchB.Flow.FlowRate;
            FlowC = cross.BranchC.Flow.FlowRate;
            StaticGainAToTrunk = FluidFlow.StaticGainForHeightDelta(cross.BranchA.Position.Z - cross.Trunk.Position.Z);
            StaticGainBToTrunk = FluidFlow.StaticGainForHeightDelta(cross.BranchB.Position.Z - cross.Trunk.Position.Z);
            StaticGainCToTrunk = FluidFlow.StaticGainForHeightDelta(cross.BranchC.Position.Z - cross.Trunk.Position.Z);
            Diameter = cross.Trunk.Diameter;
            DiameterA = cross.BranchA.Diameter;
            DiameterB = cross.BranchB.Diameter;
            DiameterC = cross.BranchC.Diameter;
        }
    }

    public partial class PressureCalculationManifold
    {
        /// <summary>
        /// Initialize a PressureCalculationManifold with an element id.
        /// </summary>
        /// <param name="elementId"></param>
        public PressureCalculationManifold(Guid elementId) : base(elementId) { }

        /// <summary>
        /// Initialize pressure calculation data for a manifold.
        /// Assigns the flow rates from the manifold ports.
        /// </summary>
        /// <param name="manifold"></param>
        public PressureCalculationManifold(Manifold manifold) : base(manifold.Id)
        {
            Flow = manifold.Trunk.Flow.FlowRate;
            Diameter = manifold.Trunk.Diameter;
            Flows = manifold.Branches.Select(p => p.Flow.FlowRate).ToList();
            Diameters = manifold.Branches.Select(p => p.Diameter).ToList();
            HeightDeltas = manifold.Branches.Select(p => p.Position.Z - manifold.Trunk.Position.Z).ToList();
            StaticGains = HeightDeltas.Select(d => FluidFlow.StaticGainForHeightDelta(d)).ToList();
            ZLosses = manifold.Branches.Select(p => 0.0).ToList();
        }
    }
}