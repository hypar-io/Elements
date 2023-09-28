using System;
using Elements.Flow;
using System.Linq;

namespace Elements.Fittings
{
    /// <summary>
    /// Compute the pressure drops of each component in a fitting tree.
    /// Uses HazenWilliams and assumes full flow.
    /// </summary>
    public class HazenWilliamsFullFlow : PressureCalculator
    {
        private double C = 130;
        /// <summary>
        /// Initialize a HazenWilliamsFullFlow calculator with a roughness coefficient
        /// </summary>
        /// <param name="cCoefficient">The roughness coefficient.</param>
        public HazenWilliamsFullFlow(double cCoefficient = 130)
        {
            C = cCoefficient;
        }

        /// <summary>
        /// Equivalent length Hazen-Williams pressure drop of a coupler
        /// </summary>
        /// <param name="coupler"></param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override PressureCalculationCoupler PressureCalcDataForCoupler(Coupler coupler)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Equivalent length Hazen-Williams pressure drop of a coupler
        /// </summary>
        /// <param name="cross"></param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override PressureCalculationCross PressureCalcDataForCross(Cross cross)
        {
            var data = new PressureCalculationCross(cross);
            var equivalentLength = 2;
            data.ZLossAToTrunk = FluidFlow.HazenWilliamsPD(C, data.FlowA, cross.BranchA.Diameter) * equivalentLength;
            data.ZLossBToTrunk = FluidFlow.HazenWilliamsPD(C, data.FlowB, cross.BranchB.Diameter) * equivalentLength;
            data.ZLossCToTrunk = FluidFlow.HazenWilliamsPD(C, data.FlowC, cross.BranchC.Diameter) * equivalentLength;
            return data;
        }

        /// <summary>
        /// Equivalent length Hazen-Williams pressure drop of a elbow.
        /// </summary>
        /// <param name="elbow"></param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override PressureCalculationElbow PressureCalcDataForElbow(Elbow elbow)
        {
            var data = new PressureCalculationElbow(elbow);
            var equivalentLength = 2d;
            if (elbow.Angle.ApproximatelyEquals(90, FittingTreeRouting.DefaultAngleTolerance))
            {
                equivalentLength = EquivalentLength.OfFitting(elbow, C);
            }
            data.ZLoss = FluidFlow.HazenWilliamsPD(C, data.Flow, elbow.End.Diameter) * equivalentLength;
            return data;
        }

        /// <summary>
        /// HazenWilliams pressure calculation for a pipe.
        /// </summary>
        /// <param name="ps"></param>
        /// <returns></returns>
        public override PressureCalculationSegment PressureCalcDataForPipe(StraightSegment ps)
        {
            var data = new PressureCalculationSegment(ps);
            data.PipeLoss = FluidFlow.HazenWilliamsPD(C, data.Flow, ps.Diameter) * data.Length;
            return data;
        }

        /// <summary>
        /// Equivalent length Hazen-Williams pressure drop of a reducer.
        /// </summary>
        /// <param name="reducer"></param>
        /// <returns></returns>
        public override PressureCalculationReducer PressureCalcDataForReducer(Reducer reducer)
        {
            var data = new PressureCalculationReducer(reducer);
            var equivalentLength = 2;
            data.ZLoss = FluidFlow.HazenWilliamsPD(C, data.Flow, reducer.End.Diameter) * equivalentLength;
            return data;
        }

        /// <summary>
        /// Equivalent length Hazen-Williams pressure drop of a terminal.
        /// </summary>
        /// <param name="terminal"></param>
        /// <returns></returns>
        public override PressureCalculationTerminal PressureCalcDataForTerminal(Terminal terminal)
        {
            var data = new PressureCalculationTerminal(terminal,
                                                       terminal.TrunkSideComponent == null ? (double?)0 : null);
            if (terminal.FlowNode is Trunk trunk)
            {
                data.ZLoss = 0;
                if (data.FixedPressure.HasValue)
                {
                    data.FixedPressure = trunk.FixedPressure;
                }
            }
            else if (terminal.FlowNode is Leaf)
            {
                var equivalentLength = 0.5;
                data.ZLoss = FluidFlow.HazenWilliamsPD(C, data.Flow, terminal.Port.Diameter) * equivalentLength;
            }
            else
            {
                throw new InvalidOperationException("Terminal node is neither Trunk or Leaf");
            }
            return data;
        }

        /// <summary>
        /// HazenWilliams pressure calculation for a tee with equivalent length.
        /// </summary>
        /// <param name="wye"></param>
        /// <param name="mainFlow"></param>
        /// <returns></returns>
        public override PressureCalculationWye PressureCalcDataForWye(Wye wye, double? mainFlow)
        {
            var data = new PressureCalculationWye(wye);
            var equivalentLengthMain = EquivalentLength.OfFitting(wye, C);
            var equivalentLengthBranch = EquivalentLength.OfFitting(wye, C);

            data.ZLoss = FluidFlow.HazenWilliamsPD(C, data.Flow, wye.Trunk.Diameter) * equivalentLengthMain;
            data.ZLossBranchToTrunk = FluidFlow.HazenWilliamsPD(C, data.FlowBranch, wye.SideBranch.Diameter) * equivalentLengthBranch;
            return data;
        }

        /// <summary>
        /// HazenWilliams pressure calculation for a manifold box.
        /// </summary>
        /// <param name="manifold"></param>
        /// <returns></returns>
        public override PressureCalculationManifold PressureCalcDataForManifold(Manifold manifold)
        {
            var data = new PressureCalculationManifold(manifold);
            var trunkLength = manifold.Trunk.Position.DistanceTo(manifold.Transform.Origin);
            data.Lengths = manifold.Branches.Select(p => p.Position.DistanceTo(manifold.Transform.Origin) + trunkLength).ToList();
            data.PipeLosses = manifold.Branches.Zip(data.Lengths, (port, length) => FluidFlow.HazenWilliamsPD(C, port.Flow.FlowRate, port.Diameter) * length).ToList();
            return data;
        }
    }
}