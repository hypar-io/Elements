using System;
using System.Collections.Generic;
using System.Linq;
using Elements.Flow;
using Elements.Geometry;

namespace Elements.Fittings
{
    /// <summary>
    /// An abstract class for calculating pressure.
    /// </summary>
    public abstract class PressureCalculator
    {
        /// <summary>
        /// Updates all of the pressure values on ports in the network.
        /// </summary>
        /// <param name="n">The network to modify.</param>
        /// <returns>Returns errors that occurred while setting pressures.</returns>
        public List<FittingError> UpdatePressureCalcs(FittingTree n)
        {
            try
            {
                var pdCalculations = ComputeAllFlowProperties(n);
                AssignTrunkStaticPressure(n);
                return n.AssignPortPressuresFromPressureDiffs(pdCalculations);
            }
            catch (Exception ex) when (!Env.DisableCatchExceptions)
            {
                return new List<FittingError>() { new FittingError(ex.Message) };
            }
        }

        /// <summary>
        /// Assign static pressure to the trunk terminal.
        /// </summary>
        public double TrunkStaticPressure { get; set; } = 0;

        /// <summary>
        /// Compute the pressure data for a Tee.
        /// </summary>
        /// <param name="ps"></param>
        /// <returns></returns>
        public abstract PressureCalculationSegment PressureCalcDataForPipe(StraightSegment ps);

        /// <summary>
        /// Compute pressure data for a terminal.
        /// </summary>
        /// <param name="terminal">The terminal of interest.</param>
        /// <returns></returns>
        public abstract PressureCalculationTerminal PressureCalcDataForTerminal(Terminal terminal);

        /// <summary>
        /// Compute the pressure data for a Coupler.
        /// </summary>
        /// <param name="coupler"></param>
        /// <returns></returns>
        public abstract PressureCalculationCoupler PressureCalcDataForCoupler(Coupler coupler);

        /// <summary>
        /// Compute the pressure data for a Reducer.
        /// </summary>
        /// <param name="reducer"></param>
        /// <returns></returns>
        public abstract PressureCalculationReducer PressureCalcDataForReducer(Reducer reducer);

        /// <summary>
        /// Compute the pressure data for an Elbow.
        /// </summary>
        /// <param name="elbow"></param>
        /// <returns></returns>
        public abstract PressureCalculationElbow PressureCalcDataForElbow(Elbow elbow);

        /// <summary>
        /// Compute the pressure data for a Wye.  You can optionally input how much flow there
        /// is from the main branch if that is known.
        /// </summary>
        /// <param name="wye"></param>
        /// <param name="mainFlow">How much flow is in the main branch.</param>
        /// <returns></returns>
        public abstract PressureCalculationWye PressureCalcDataForWye(Wye wye, double? mainFlow);

        /// <summary>
        ///
        /// </summary>
        /// <param name="cross"></param>
        /// <returns></returns>
        public abstract PressureCalculationCross PressureCalcDataForCross(Cross cross);

        /// <summary>
        /// Compute the pressure data for a manifold.
        /// </summary>
        /// <param name="manifold"></param>
        /// <returns></returns>
        public abstract PressureCalculationManifold PressureCalcDataForManifold(Manifold manifold);

        /// <summary>
        /// Get the pressure data for a fitting.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public PressureCalculationBase GetPressureCalcData(IComponent c)
        {
            switch (c)
            {
                case StraightSegment ps:
                    return PressureCalcDataForPipe(ps);
                case Terminal terminal:
                    return PressureCalcDataForTerminal(terminal);
                case Coupler coupler:
                    return PressureCalcDataForCoupler(coupler);
                case Reducer reducer:
                    return PressureCalcDataForReducer(reducer);
                case Elbow elbow:
                    return PressureCalcDataForElbow(elbow);
                case Wye wye:
                    return PressureCalcDataForWye(wye, null);
                case Cross cross:
                    return PressureCalcDataForCross(cross);
                case Manifold manifold:
                    return PressureCalcDataForManifold(manifold);
                default:
                    return null;
            }
        }

        private List<PressureCalculationBase> ComputeAllFlowProperties(FittingTree fittings, IEnumerable<Section> sections = null)
        {
            if (fittings == null)
            {
                throw new ArgumentNullException("The FittingTree may not be null");
            }
            if (fittings.Tree == null)
            {
                throw new ArgumentNullException("The Tree may not be null");
            }
            var allComponents = fittings.AllComponents;
            if (sections != null && sections.Count() > 0)
            {
                allComponents = sections.SelectMany(s => fittings.GetComponentsOfSection(s));
            }

            var computedPressureDrops = ComputePressureDeltas(allComponents.ToList());
            return computedPressureDrops;
        }

        private List<PressureCalculationBase> ComputePressureDeltas(List<ComponentBase> allComponents)
        {
            List<PressureCalculationBase> allPressureCalcData = new List<PressureCalculationBase>();
            foreach (var c in allComponents)
            {
                if (IsValidSingleComponent(c))
                {
                    PressureCalculationBase data = GetPressureCalcData(c);
                    allPressureCalcData.Add(data);
                }
                else if (c is Assembly assembly)
                {
                    foreach (var connection in assembly.AllComponents)
                    {
                        if (IsValidSingleComponent(connection))
                        {
                            PressureCalculationBase data = GetPressureCalcData(connection);
                            allPressureCalcData.Add(data);
                        }
                        else
                        {
                            throw new ArgumentException($"Network components of the type {c.GetType()} are not yet supported.");
                        }
                    }
                }
                else
                {

                    throw new ArgumentException($"Network components of the type {c.GetType()} are not yet supported.");
                }
            }
            return allPressureCalcData;
        }

        private static bool IsValidSingleComponent(IComponent c)
        {
            var t = c.GetType();
            return t == typeof(StraightSegment)
                   || t == typeof(Elbow)
                   || t == typeof(Terminal)
                   || t == typeof(Wye)
                   || t == typeof(Reducer)
                   || t == typeof(Cross)
                   || t == typeof(Manifold)
                   || c is Coupler;
        }

        internal double GetStaticPressureLossOfComponent(ComponentBase current, ComponentBase branchside)
        {
            var pressureData = GetPressureCalcData(current);
            switch (pressureData)
            {
                case PressureCalculationTerminal t:
                    return t.ZLoss + t.PipeLoss;
                case PressureCalculationSegment p:
                    return p.PipeLoss;
                case PressureCalculationReducer r:
                    return r.ZLoss + r.PipeLossStart + r.PipeLoss;
                case PressureCalculationElbow e:
                    return e.ZLoss + e.PipeLossStart + e.PipeLoss;
                case PressureCalculationCoupler c:
                    return c.PipeLoss;
                case PressureCalculationWye y:
                    if (branchside.TrunkSideComponent != current)
                    {
                        throw new ArgumentException("The branchside component must be a branch of the wye");
                    }
                    if (!(current is Wye wye))
                    {
                        throw new ArgumentException($"The current component should be a wye but is a ${current.GetType()}");
                    }
                    var closestPort = branchside.TrunkSidePort().GetClosestPort(current, out var distance);
                    if (closestPort == wye.SideBranch)
                    {
                        return y.ZLossBranchToTrunk + y.PipeLossBranch + y.PipeLoss;
                    }
                    else if (closestPort == wye.MainBranch)
                    {
                        return y.ZLoss + y.PipeLossMain + y.PipeLoss;
                    }
                    else
                    {
                        throw new ArgumentException("The found port wasn't a branch of the wye.");
                    }
                case PressureCalculationCross x:
                    if (branchside.TrunkSideComponent != current)
                    {
                        throw new ArgumentException("The branchside component must be a branch of the cross");
                    }
                    if (!(current is Cross cross))
                    {
                        throw new ArgumentException($"The current component should be a cross but is a ${current.GetType()}");
                    }
                    var closestPortCross = branchside.TrunkSidePort().GetClosestPort(current, out var distanceCross);
                    if (closestPortCross == cross.BranchA)
                    {
                        return x.ZLossAToTrunk + x.PipeLossA + x.PipeLoss;
                    }
                    else if (closestPortCross == cross.BranchB)
                    {
                        return x.ZLossBToTrunk + x.PipeLossB + x.PipeLoss;
                    }
                    else if (closestPortCross == cross.BranchC)
                    {
                        return x.ZLossCToTrunk + x.PipeLossC + x.PipeLoss;
                    }
                    else
                    {
                        throw new ArgumentException("The found port wasn't a branch of the cross.");
                    }
                case PressureCalculationManifold m:
                    if (branchside.TrunkSideComponent != current)
                    {
                        throw new ArgumentException("The branchside component must be a branch of the manifold");
                    }
                    if (!(current is Manifold manifold))
                    {
                        throw new ArgumentException($"The current component should be a manifold but is a ${current.GetType()}");
                    }
                    var closestPortManifold = branchside.TrunkSidePort().GetClosestPort(current, out _);
                    for (var i = 0; i < manifold.Branches.Count; i++)
                    {
                        if (closestPortManifold == manifold.Branches[i])
                        {
                            return m.ZLosses[i] + m.PipeLosses[i];
                        }
                    }

                    throw new ArgumentException("The found port wasn't a branch of the manifold.");
                default:
                    throw new Exception($"Unknown component type {pressureData.GetType()}");
            }
        }

        private void AssignTrunkStaticPressure(FittingTree tree)
        {
            if (TrunkStaticPressure.ApproximatelyEquals(0))
            {
                return;
            }

            var terminals = tree.FittingsOfType<Terminal>();
            var trunkTerminal = terminals.Single(t => t.TrunkSidePort() == null);
            trunkTerminal.Port.Flow.StaticPressure = TrunkStaticPressure;
        }  
    }
}