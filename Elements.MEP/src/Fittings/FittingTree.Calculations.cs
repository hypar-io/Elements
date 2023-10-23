using System;
using System.Collections.Generic;
using System.Linq;
using Elements.Flow;
using Elements.Geometry;

namespace Elements.Fittings
{
    public partial class FittingTree
    {
        public FlowDirection FlowDirection { get; set; } = FlowDirection.TowardTrunk;

        /// <summary>
        /// Given a list of pressure calculations for individual fittings, calculate and assign the pressure at each node.
        /// This method should only be used if you need behavior different from inheriting a PressureCalculator and using UpdatePressureCalcs.
        /// </summary>
        /// <param name="pdCalculations">A dictionary of pre-computed </param>
        /// <param name="errors"></param>
        internal List<FittingError> AssignPortPressuresFromPressureDiffs(
            List<PressureCalculationBase> pdCalculations)
        {
            var pdLookup = pdCalculations.ToDictionary(p => p.ElementId, p => p);
            var errors = new List<FittingError>();

            // TODO: This ordering by key length is a small hack to make sure that we only assign pressures to
            // fittings/pipes that already have a prior value applied.  could/should be replace by a recursive
            // strategy that starts at the trunk and goes to every branch.  Only trick is that
            // "BranchSideComponent" used to not actually walk us down every branch.  This is fixed now.
            var sections = ExpandedComponents.GroupBy(c => c.ComponentLocator.SectionKey).OrderBy(c => c.Key.Length);
            foreach (var section in sections)
            {
                var pressureAssigningErrors = AssignPressuresToSection(section, pdLookup);
                errors.AddRange(pressureAssigningErrors);
            }
            return errors;
        }

        private List<FittingError> AssignPressuresToSection(IGrouping<string, ComponentBase> section,
                                                            Dictionary<Guid, PressureCalculationBase> lookup)
        {
            var orderedSection = section.OrderBy(c => c.ComponentLocator.IndexInSection);
            var pressureAssigningErrors = new List<FittingError>();
            foreach (var component in orderedSection)
            {
                if (component is Assembly assembly)
                {
                    throw new Exception("Shouldn't be any assemblies.");
                }
                else if (component.TryGetGuid(out var id))
                {
                    if (lookup.TryGetValue(id, out var pd))
                    {
                        try
                        {
                            AssignPressuresToComponent(component, pd);
                        }
                        catch (Exception e) when (!Env.DisableCatchExceptions)
                        {
                            pressureAssigningErrors.Add(new FittingError(
                                $"Error while assigning pressure to: {component.ComponentLocator}: {e.Message}",
                                component.TrunkSidePort().Position));
                            break;
                        }
                    }
                }
                else
                {
                    pressureAssigningErrors.Add(new FittingError(
                        $"Could not find the a pressure drop for the item {component.ComponentLocator}",
                        component.TrunkSidePort().Position));
                }
            }
            return pressureAssigningErrors;
        }

        private void AssignPressuresToComponent(ComponentBase c, PressureCalculationBase pd)
        {
            c.PressureCalculations = pd;
            switch (c)
            {
                case StraightSegment pipe:
                    AssignPressureToPipe(pipe, pd as PressureCalculationSegment);
                    break;
                case Elbow elbow:
                    AssignPressureToElbow(elbow, pd as PressureCalculationElbow);
                    break;
                case Terminal terminal:
                    AssignPressureToTerminal(terminal, pd as PressureCalculationTerminal);
                    break;
                case Wye wye:
                    AssignPressureToWye(wye, pd as PressureCalculationWye);
                    break;
                case Reducer reducer:
                    AssignPressureToReducer(reducer, pd as PressureCalculationReducer);
                    break;
                case Coupler coupler:
                    AssignPressureToCoupler(coupler, pd as PressureCalculationCoupler);
                    break;
                case Cross cross:
                    AssignPressureToCross(cross, pd as PressureCalculationCross);
                    break;
                case Manifold manifold:
                    AssignPressureToManifoldBox(manifold, pd as PressureCalculationManifold);
                    break;
                default:
                    throw new Exception($"Unknown component, can't assign pressure to a {c.GetType()}.");
            }
        }

        private void AssignPressureToReducer(Reducer reducer, PressureCalculationReducer pressureCalcDataReducer)
        {
            reducer.PressureCalculations = pressureCalcDataReducer;
            if (reducer.End.Flow == null)
            {
                throw new InvalidOperationException($"No end flow data found for reducer, flow value calculation cannot continue. {nameof(reducer.TrunkSideComponent)} is a {reducer.TrunkSideComponent.GetType()}");
            }
            if (!(GetDownstreamPortOnTrunksideComponent(reducer) is Port port))
            {
                throw new InvalidOperationException($"No downstream port found for reducer, flow value calculation cannot continue. {nameof(reducer.TrunkSideComponent)} is a {reducer.TrunkSideComponent.GetType()}");
            }

            var otherStartVelocity = reducer.Start.Diameter > reducer.End.Diameter ? pressureCalcDataReducer.FluidVelocity : pressureCalcDataReducer.FluidVelocityStart;
            reducer.End.Flow.StaticPressure = port.Flow.StaticPressure;
            reducer.End.Flow.DynamicPressure = port.Flow.DynamicPressure;
            var pressureChange = pressureCalcDataReducer.StaticGain - pressureCalcDataReducer.ZLoss - pressureCalcDataReducer.PipeLoss - pressureCalcDataReducer.PipeLossStart;
            reducer.Start.Flow = new Flow(port.Flow.StaticPressure - pressureChange, port.Flow.FlowRate, otherStartVelocity, pressureCalcDataReducer.DynamicPressureStart);
        }

        private void AssignPressureToManifoldBox(Manifold manifoldBox, PressureCalculationManifold pressureCalcManifold)
        {
            for (var i = 0; i < manifoldBox.Branches.Count; i++)
            {
                var pressureLoss = pressureCalcManifold.PipeLosses[i] + pressureCalcManifold.ZLosses[i];
                var pressureChange = CalculatePressureChange(pressureCalcManifold.StaticGains[i], pressureLoss);
                var branchStaticPressure = manifoldBox.Trunk.Flow.StaticPressure - pressureChange;
                var b = manifoldBox.Branches[i];
                b.Flow = new Flow(branchStaticPressure, b.Flow.FlowRate, b.Flow.FluidVelocity, b.Flow.DynamicPressure);
            }
        }

        private void AssignPressureToWye(Wye wye, PressureCalculationWye pressureCalcDataWye)
        {
            wye.PressureCalculations = pressureCalcDataWye;
            Flow trunkFlow = wye.Trunk.Flow;
            if (wye.Trunk.Flow == null)
            {
                throw new InvalidOperationException($"No prior flow data found for wye, flow value calculation cannot continue. NextComponent is a {wye.TrunkSideComponent.GetType()}");
            }
            if (!(GetDownstreamPortOnTrunksideComponent(wye) is Port port))
            {
                throw new InvalidOperationException($"No downstream port found for wye, flow value calculation cannot continue. {nameof(wye.TrunkSideComponent)} is a {wye.TrunkSideComponent.GetType()}");
            }
            wye.Trunk.Flow.StaticPressure = port.Flow.StaticPressure;
            wye.Trunk.Flow.DynamicPressure = port.Flow.DynamicPressure;

            var pressureLoss = pressureCalcDataWye.ZLossBranchToTrunk + pressureCalcDataWye.PipeLossBranch + pressureCalcDataWye.PipeLoss;
            var pressureChange = CalculatePressureChange(pressureCalcDataWye.StaticGainBranchToTrunk, pressureLoss);

            var newBranchStaticPressure = trunkFlow.StaticPressure - pressureChange;
            wye.SideBranch.Flow = new Flow(newBranchStaticPressure, pressureCalcDataWye.FlowBranch, pressureCalcDataWye.FluidVelocityBranch, pressureCalcDataWye.DynamicPressureBranch);

            pressureLoss = pressureCalcDataWye.ZLoss + pressureCalcDataWye.PipeLossMain + pressureCalcDataWye.PipeLoss;
            pressureChange = CalculatePressureChange(pressureCalcDataWye.StaticGain, pressureLoss);
            var newMainStaticPressure = trunkFlow.StaticPressure - pressureChange;
            wye.MainBranch.Flow = new Flow(newMainStaticPressure, pressureCalcDataWye.FlowMain, pressureCalcDataWye.FluidVelocityMain, pressureCalcDataWye.DynamicPressureMain);
        }

        private void AssignPressureToCross(Cross cross, PressureCalculationCross pressureCalcDataCross)
        {
            cross.PressureCalculations = pressureCalcDataCross;
            Flow trunkFlow = cross.Trunk.Flow;
            if (cross.Trunk.Flow == null)
            {
                throw new InvalidOperationException($"No prior flow data found for cross, flow value calculation cannot continue. NextComponent is a {cross.TrunkSideComponent.GetType()}");
            }
            if (!(GetDownstreamPortOnTrunksideComponent(cross) is Port port))
            {
                throw new InvalidOperationException($"No downstream port found for cross, flow value calculation cannot continue. {nameof(cross.TrunkSideComponent)} is a {cross.TrunkSideComponent.GetType()}");
            }
            cross.Trunk.Flow.StaticPressure = port.Flow.StaticPressure;
            cross.Trunk.Flow.DynamicPressure = port.Flow.DynamicPressure;

            var pressureLoss = pressureCalcDataCross.ZLossAToTrunk + pressureCalcDataCross.PipeLossA + pressureCalcDataCross.PipeLoss;
            var pressureChange = CalculatePressureChange(pressureCalcDataCross.StaticGainAToTrunk, pressureLoss);
            var aBranchStaticPressure = trunkFlow.StaticPressure - pressureChange;
            cross.BranchA.Flow = new Flow(aBranchStaticPressure, pressureCalcDataCross.FlowA, pressureCalcDataCross.FluidVelocityA, pressureCalcDataCross.DynamicPressureA);

            pressureLoss = pressureCalcDataCross.ZLossBToTrunk + pressureCalcDataCross.PipeLossB + pressureCalcDataCross.PipeLoss;
            pressureChange = CalculatePressureChange(pressureCalcDataCross.StaticGainBToTrunk, pressureLoss);
            var bBranchStaticPressure = trunkFlow.StaticPressure - pressureChange;
            cross.BranchB.Flow = new Flow(bBranchStaticPressure, pressureCalcDataCross.FlowB, pressureCalcDataCross.FluidVelocityB, pressureCalcDataCross.DynamicPressureB);

            pressureLoss = pressureCalcDataCross.ZLossCToTrunk + pressureCalcDataCross.PipeLossC + pressureCalcDataCross.PipeLoss;
            pressureChange = CalculatePressureChange(pressureCalcDataCross.StaticGainCToTrunk, pressureLoss);
            var cBranchStaticPressure = trunkFlow.StaticPressure - pressureChange;
            cross.BranchC.Flow = new Flow(cBranchStaticPressure, pressureCalcDataCross.FlowC, pressureCalcDataCross.FluidVelocityC, pressureCalcDataCross.DynamicPressureC);
        }

        private void AssignPressureToElbow(Elbow elbow, PressureCalculationElbow pressureCalcDataElbow)
        {
            elbow.PressureCalculations = pressureCalcDataElbow;
            if (elbow.End.Flow == null)
            {
                throw new InvalidOperationException($"No prior flow data found for elbow, flow value calculation cannot continue. TrunkSideComponent is a {elbow.TrunkSideComponent.GetType()}");
            }
            if (!(GetDownstreamPortOnTrunksideComponent(elbow) is Port port))
            {
                throw new InvalidOperationException($"No downstream port found for elbow, flow value calculation cannot continue. {nameof(elbow.TrunkSideComponent)} is a {elbow.TrunkSideComponent.GetType()}");
            }
            elbow.End.Flow.StaticPressure = port.Flow.StaticPressure;
            elbow.End.Flow.DynamicPressure = port.Flow.DynamicPressure;

            var pressureLoss = pressureCalcDataElbow.ZLoss + pressureCalcDataElbow.PipeLossStart + pressureCalcDataElbow.PipeLoss;
            var pressureChange = CalculatePressureChange(pressureCalcDataElbow.StaticGain, pressureLoss);
            elbow.Start.Flow = new Flow(elbow.End.Flow.StaticPressure - pressureChange, pressureCalcDataElbow.Flow, pressureCalcDataElbow.FluidVelocity, pressureCalcDataElbow.DynamicPressure);
        }

        private void AssignPressureToTerminal(Terminal terminal, PressureCalculationTerminal pressureCalcDataTerminal)
        {
            terminal.PressureCalculations = pressureCalcDataTerminal;
            if (terminal.TrunkSideComponent == null && pressureCalcDataTerminal.FixedPressure.HasValue)
            {
                var pressureLoss = pressureCalcDataTerminal.ZLoss + pressureCalcDataTerminal.PipeLoss;
                var pressureChange = CalculatePressureChange(pressureCalcDataTerminal.StaticGain, pressureLoss);
                terminal.Port.Flow = new Flow(pressureCalcDataTerminal.FixedPressure.Value - pressureChange, pressureCalcDataTerminal.Flow, pressureCalcDataTerminal.FluidVelocity, 0);
            }
            else if (terminal.TrunkSideComponent is ComponentBase trunksideComponent)
            {
                var connHandoff = GetDownstreamPortOnTrunksideComponent(terminal);
                terminal.Port.Flow = connHandoff.Flow;
            }

            if (terminal.Port.Flow != null)
            {
                terminal.StaticPressure = terminal.Port.Flow.StaticPressure;
            }
        }

        private void AssignPressureToPipe(StraightSegment pipe, PressureCalculationSegment pressureCalcDataPipe)
        {
            pipe.PressureCalculations = pressureCalcDataPipe;

            if (pipe.End.Flow == null)
            {
                throw new Exception("Pipe End has no flow.  While assigning flow from trunk to branch we expect the pipe end to have flow when we get to it");
            }
            if (!(GetDownstreamPortOnTrunksideComponent(pipe) is Port port))
            {
                throw new InvalidOperationException($"No downstream port found for pipe, flow value calculation cannot continue. {nameof(pipe.TrunkSideComponent)} is a {pipe.TrunkSideComponent.GetType()}");
            }
            pipe.End.Flow.StaticPressure = port.Flow.StaticPressure;
            pipe.End.Flow.DynamicPressure = port.Flow.DynamicPressure;
            var pressureChange = CalculatePressureChange(pressureCalcDataPipe.StaticGain, pressureCalcDataPipe.PipeLoss);
            var newPressure = pipe.End.Flow.StaticPressure - pressureChange;
            pipe.PressureCalculations.TotalPressure = newPressure + pressureCalcDataPipe.DynamicPressure;

            pipe.Start.Flow = new Flow(newPressure, pressureCalcDataPipe.Flow, pressureCalcDataPipe.FluidVelocity, pressureCalcDataPipe.DynamicPressure);
        }

        private void AssignPressureToCoupler(Coupler coupler, PressureCalculationCoupler pressureCalcDataCoupler)
        {
            coupler.PressureCalculations = pressureCalcDataCoupler;
            if (coupler.End.Flow == null)
            {
                throw new InvalidOperationException(
                    $"No end flow data found for coupler, flow value calculation cannot continue. " +
                    $"{nameof(coupler.TrunkSideComponent)} is a {coupler.TrunkSideComponent.GetType()}");
            }
            if (!(GetDownstreamPortOnTrunksideComponent(coupler) is Port port))
            {
                throw new InvalidOperationException($"Coupler {coupler.Id} has no port on the trunk side component {coupler.TrunkSideComponent.Id}");
            }
            coupler.End.Flow.StaticPressure = port.Flow.StaticPressure;
            coupler.End.Flow.DynamicPressure = port.Flow.DynamicPressure;
            var pressureChange = CalculatePressureChange(pressureCalcDataCoupler.StaticGain, pressureCalcDataCoupler.PipeLoss);
            var newPressure = coupler.End.Flow.StaticPressure - pressureChange;
            coupler.PressureCalculations.TotalPressure = newPressure + pressureCalcDataCoupler.DynamicPressure;

            coupler.Start.Flow = new Flow(newPressure,
                pressureCalcDataCoupler.Flow, pressureCalcDataCoupler.FluidVelocity, pressureCalcDataCoupler.DynamicPressure);
        }

        private double CalculatePressureChange(double staticGain, double pressureLoss)
        {
            switch (FlowDirection)
            {
                case FlowDirection.TowardLeafs:
                    return staticGain + pressureLoss;
                case FlowDirection.TowardTrunk:
                default:
                    return staticGain - pressureLoss;
            }
        }
    }
}