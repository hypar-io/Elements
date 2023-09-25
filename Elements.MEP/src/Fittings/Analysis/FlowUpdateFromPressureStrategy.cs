using Elements.Flow;
using Elements.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Elements.Fittings
{
    /// <summary>
    /// Flow update strategy that uses pressure and k-factor.
    /// </summary>
    public class FlowUpdateFromPressureStrategy : IFlowUpdateStrategy
    {
        private Func<Terminal, double> _kFactorFunc;
        private double _tolerance;

        private List<(Terminal Terminal, double Flow, double ExpectedFlow)> _flows = new();
        private double? _lastTotalFlow = null;
        private double _conversionFactor = 1;
        int _iterationsInSameDirection = 0;

        /// <summary>
        /// Create new flow update strategy from  pressure.
        /// </summary>
        /// <param name="kFactorFunc">K factor function for specific port.</param>
        /// <param name="tolerance">Average tolerance between leaf flow rates.</param>
        public FlowUpdateFromPressureStrategy(Func<Terminal, double> kFactorFunc,
                                              double tolerance = Vector3.EPSILON)
        {
            _kFactorFunc = kFactorFunc;
            _tolerance = tolerance;
        }

        /// <summary>
        /// List of last applied flow and expected flow for each leaf terminal.
        /// </summary>
        public List<(Terminal Terminal, double Flow, double ExpectedFlow)> Flows { get { return _flows; } }

        /// <summary>
        /// Reset internal values stored from previous iterations.
        /// </summary>
        public void Reset()
        {
            _lastTotalFlow = null;
            _conversionFactor = 1;
            _iterationsInSameDirection = 0;
    }

        /// <summary>
        /// Check if flow rates calculated from pressure are close to flow rates assigned to leaf terminals.
        /// Flow rates only on terminals are updated, AssignFlowCalcs still need to be called to propagate them.
        /// They are updated only if difference between average assigned and average calculated flow rates differs by more than tolerance. 
        /// </summary>
        /// <param name="tree">Tree to update flow rates.</param>
        /// <returns>True in flow rates on any leaf terminals were updated.</returns>
        public bool UpdateLeafFlow(FittingTree tree)
        {
            var terminals = tree.FittingsOfType<Terminal>();
            var groupedTerminals = terminals.ToLookup(t => t.TrunkSidePort() != null);
            var leafTerminals = groupedTerminals[true];
            double oldTotalFlow = groupedTerminals[false].First().Port.Flow.FlowRate;
            double newTotalFlow = 0;
            double totalExpectedFlow = 0;
            _flows.Clear();

            foreach (var t in leafTerminals)
            {
                if (t.FlowNode is Leaf leaf)
                {
                    var port = t.Port;
                    var kFactor = _kFactorFunc(t);
                    var pressure = t.GetFinalStaticPressure();
                    if (!pressure.HasValue)
                    {
                        continue;
                    }

                    var newFlow = 0.0;
                    var expectedFlow = 0.0;

                    if (pressure > 0)
                    {
                        expectedFlow = MathUtils.CalculateKFactorFlowRate(pressure.Value, kFactor);
                        totalExpectedFlow += expectedFlow;
                        newFlow = leaf.Flow + (expectedFlow - leaf.Flow) * _conversionFactor;
                    }
                    else
                    {
                        // If pressure on the leaf is negative - water can't reach it and flow is 0.
                        // By lovering the flow, pressure will rise and leaf will get non zero flow.
                        // However, if factor < 1 then, although expected flow is 0, we try the one in between.
                        // Setting 0 flow right away can lead to iterations stuck between too low and too high flow:
                        // setting 0 often lead to next flow being too high and this, again, makes next flow 0.
                        newFlow = leaf.Flow - leaf.Flow * _conversionFactor;
                    }

                    newTotalFlow += newFlow;
                    _flows.Add((t, newFlow, expectedFlow));
                }
            }
 
            if (_lastTotalFlow.HasValue)
            {
                var d0 = newTotalFlow - _lastTotalFlow.Value;
                var d1 = oldTotalFlow - _lastTotalFlow.Value;
                var delta = d0 / d1;

                // If iteration B > iteration A, but C < A, values are changing to rapid,
                // and go away from each other instead of converging, speed should be decreased.
                // The same is true for B < A, but C > A.
                // But this leads to slow and iterations sometimes.
                // To speed process up increase speed if values move in the same direction for more than 2 iterations.
                if (delta < 0.1)
                {
                    _conversionFactor /= 2;
                }
                else if (delta > 1)
                {
                    _iterationsInSameDirection++;
                }

                if (delta <= 1)
                {
                    _iterationsInSameDirection = 0;
                }
                else if (_iterationsInSameDirection > 2)
                {
                    _conversionFactor *= 2;
                    _iterationsInSameDirection = 0;
                }
            }

            _lastTotalFlow = oldTotalFlow;

            double maxIterationDifference = 0;
            foreach (var (t, flow, _) in _flows)
            {
                var leaf = ((Leaf)t.FlowNode);
                var diff = Math.Abs(flow - leaf.Flow);
                leaf.Flow = flow;
                if (diff > maxIterationDifference)
                {
                    maxIterationDifference = diff;
                }
            }

            var maxExpectedDiffernece = _flows.Max(f => Math.Abs(f.ExpectedFlow - f.Flow));
            var averageDifference = Math.Abs(totalExpectedFlow - oldTotalFlow) / leafTerminals.Count();
            // Flow is correct if sum of differences between assumed and calculated flow is less than tolerance on average.
            // Each individual flow must be no more than 10 times tolerance away from expected.
            bool needsUpdate = averageDifference > _tolerance ||
                maxExpectedDiffernece > _tolerance * 10 || maxIterationDifference > _tolerance * 10;
            return needsUpdate;
        }
    }
}
