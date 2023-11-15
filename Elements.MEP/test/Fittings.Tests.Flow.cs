using Elements;
using Elements.Fittings;
using Elements.Flow;
using Elements.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Elements.MEP.Tests
{
    public partial class FittingsTests
    {
        [Fact]
        public void RemoteAreaFlow()
        {
            var tree = GetSampleTreeWithTrunkBelow(.01);
            var routing = new FittingTreeRouting(tree);
            routing.PressureCalculator = new HazenWilliamsFullFlow();
            var points = new List<Vector3>()
            {
                new Vector3(-1.0, 1.0, .0),
                new Vector3(5.0, 1.0, .0),
                new Vector3(5.0, -1.0, .0),
                new Vector3(-1.0, -1.0, .0),
            };
            routing.FlowCalculator = new RemoteAreaFlowCalculator(new Polygon(points));
            var fittings = routing.BuildFittingTree(out var errors);

            Assert.Empty(errors);
            var terminals = fittings.FittingsOfType<Terminal>().Where(t => t.FlowNode is Leaf).ToList();

            Assert.Equal(terminals[2].GetFinalStaticPressure(), terminals[3].GetFinalStaticPressure());
            Assert.NotEqual(terminals[0].GetFinalStaticPressure(), terminals[3].GetFinalStaticPressure());

            Assert.True(fittings.GetDownstreamPortOnTrunksideComponent(terminals[0]).Flow.FlowRate > 0);
            Assert.True(fittings.GetDownstreamPortOnTrunksideComponent(terminals[1]).Flow.FlowRate > 0);

            Assert.Equal(.0, fittings.GetDownstreamPortOnTrunksideComponent(terminals[2]).Flow.FlowRate);
            Assert.Equal(.0, fittings.GetDownstreamPortOnTrunksideComponent(terminals[3]).Flow.FlowRate);

            Assert.Equal(-84622.775363253357, terminals[0].GetFinalStaticPressure());
            Assert.Equal(-79002.876963800067, terminals[1].GetFinalStaticPressure());
            Assert.Equal(-80128.598604439816, terminals[2].GetFinalStaticPressure());
            Assert.Equal(-80128.598604439816, terminals[3].GetFinalStaticPressure());
        }

        [Theory]
        [InlineData(70, 119_400, 0.001275)]
        [InlineData(70, 128_300, 0.001322)]
        [InlineData(70, 159_700, 0.001475)]
        public void FlowFromPressureKFactor(double kFactor, double pressure, double expectedFlowRate)
        {
            var convertedKFactor = MathUtils.ConvertKFactorFromLitersMinutesBars(kFactor);
            var flowRate = MathUtils.CalculateKFactorFlowRate(pressure, convertedKFactor);
            Assert.True(expectedFlowRate.ApproximatelyEquals(flowRate, 1e-6));
        }

        [Theory]
        [InlineData(0.01)]
        [InlineData(0.023)]
        [InlineData(0.1)]
        public void IterativeFlowCalculation(double startingFlow)
        {
            var tree = GetSampleTreeWithTrunkAbove(startingFlow);
            var routing = new FittingTreeRouting(tree);
            routing.PressureCalculator = new HazenWilliamsFullFlow();
            routing.PressureCalculator.TrunkStaticPressure = 90_000;
            var kFactor = 80;
            var convertedKFactor = MathUtils.ConvertKFactorFromLitersMinutesBars(kFactor);
            Func<Terminal, double> kFactorFunc = port => convertedKFactor;
            var calc = new FullFlowCalculator();
            calc.FlowUpdateStrategy = new FlowUpdateFromPressureStrategy(kFactorFunc, 1e-6);
            routing.FlowCalculator = calc;
            var fittings = routing.BuildFittingTree(out var errors, FlowDirection.TowardLeafs);

            Assert.Empty(errors);
            var leafs = fittings.FittingsOfType<Terminal>().Select(t => t.FlowNode as Leaf).Where(l => l != null).ToList();
            Assert.True(leafs[0].Flow.ApproximatelyEquals(0.0013923, 1e-6));
            Assert.True(leafs[1].Flow.ApproximatelyEquals(0.0013893, 1e-6));
            Assert.True(leafs[2].Flow.ApproximatelyEquals(0.0013873, 1e-6));
            Assert.True(leafs[3].Flow.ApproximatelyEquals(0.0013867, 1e-6));
        }

        [Theory]
        [InlineData(30_000, 0.1)]
        [InlineData(100_000, 0.05)]
        [InlineData(500_000, 0.2)]
        public void IterativeFlowCalculationLargeGrid(double pressure, double diameter)
        {
            var tree = GetSampleGridTree(0.1, diameter);
            foreach (var con in tree.Connections)
            {
                con.Diameter = diameter;
            }

            var routing = new FittingTreeRouting(tree);
            routing.PressureCalculator = new HazenWilliamsFullFlow();
            routing.PressureCalculator.TrunkStaticPressure = pressure;
            var kFactor = 80;
            var convertedKFactor = MathUtils.ConvertKFactorFromLitersMinutesBars(kFactor);
            var calc = new FullFlowCalculator();
            Func<Terminal, double> kFactorFunc = port => convertedKFactor;
            calc.FlowUpdateStrategy = new FlowUpdateFromPressureStrategy(kFactorFunc, 1e-6);
            routing.FlowCalculator = calc;
            var fittings = routing.BuildFittingTree(out var errors, FlowDirection.TowardLeafs);

            Assert.Empty(errors);
            foreach (var t in fittings.FittingsOfType<Terminal>().Where(t => t.FlowNode is Leaf))
            {
                var leaf = t.FlowNode as Leaf;
                var terminalPressure = t.GetFinalStaticPressure().Value;
                var expected = MathUtils.CalculateKFactorFlowRate(terminalPressure, convertedKFactor);
                Assert.True(expected.ApproximatelyEquals(leaf.Flow, 1e-5));
            }
        }
    }
}
