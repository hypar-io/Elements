using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Elements;
using Elements.Fittings;
using Elements.Flow;
using Elements.Geometry;
using Xunit;

namespace Elements.MEP.Tests
{
    public partial class FittingsTests
    {
        [Fact]
        public void HazenWilliamsPressureDrop()
        {
            var tree = GetSampleTreeWithTrunkBelow(.01);
            var routing = new FittingTreeRouting(tree);
            routing.PressureCalculator = new HazenWilliamsFullFlow();
            var fittings = routing.BuildFittingTree(out var errors);

            Assert.Empty(errors);
            CheckTreeStaticPressures(fittings);
        }

        [Fact]
        public void HazenWilliamsManifoldPressure()
        {
            var flowPerInlet = 0.01;
            Tree tree = GetSampleTreeWithManifold(flowPerInlet);

            var routing = new FittingTreeRouting(tree);
            routing.PressureCalculator = new HazenWilliamsFullFlow();
            var fittings = routing.BuildFittingTree(out var errors);

            Assert.Empty(errors);
            CheckTreeStaticPressures(fittings);
        }

        [Theory]
        [InlineData(130, .01, .076, 710)]
        [InlineData(100, .08, .2, 487.9)]
        public void HazenWilliamsCheck(double cCoefficient, double flowRate, double diameter, double pdPerM)
        {
            double actual = FluidFlow.HazenWilliamsPD(cCoefficient, flowRate, diameter);
            Assert.Equal(pdPerM, actual, 1);
        }

        [Theory]
        [InlineData(120, 1)]
        [InlineData(150, 1.51)]
        [InlineData(135, 1.245)]
        public void EquivalentLengthMultiplier(double cCoefficient, double expected)
        {
            double multiplier = EquivalentLength.GetMultiplierForCFactor(cCoefficient);
            Assert.Equal(expected, multiplier, 1);
        }

        [Theory]
        [InlineData(120, .032, 1.8)] // normal lookup
        [InlineData(150, .125, 7.6 * 1.51)] // with C multiplier
        [InlineData(135, .043, 3 * 1.245)] // interpolated C and round up diameter
        public void WyeEquivalentLengths(double C, double diameter, double expected)
        {
            var settings = new WyeSettings(diameter, diameter, diameter, 0.1, 0.1, 0.1);
            double actual = EquivalentLength.OfFitting(new Wye(Vector3.Origin, Vector3.XAxis, Vector3.XAxis.Negate(), Vector3.YAxis, settings, BuiltInMaterials.Default), C);
            Assert.Equal(expected, actual, 1);
        }


        [Theory]
        [InlineData(120, .032, 0.9)] // normal lookup
        [InlineData(150, .125, 3.7 * 1.51)] // with C multiplier
        [InlineData(135, .043, 1.5 * 1.245)] // interpolated C and round up diameter
        public void Elbow90EquivalentLengths(double C, double diameter, double expected)
        {
            double actual = EquivalentLength.OfFitting(new Elbow(Vector3.Origin, Vector3.XAxis, Vector3.YAxis, 0.1, diameter), C);
            Assert.Equal(expected, actual, 1);
        }

        [Fact]
        public void FlowRateOnPorts()
        {
            var tree = GetSampleTreeWithTrunkBelow();
            var routing = new FittingTreeRouting(tree);
            var fittings = routing.BuildFittingTree(out var errors);

            foreach (var f in fittings.ExpandedComponents)
            {
                foreach (var p in f.BranchSidePorts())
                {
                    Assert.NotNull(p.Flow);
                    Assert.True(p.Flow.FlowRate > 0);
                }
                if (f is Terminal t && t.FlowNode is Trunk)
                {
                    continue;
                }
                Assert.NotNull(f.TrunkSidePort().Flow);
                Assert.True(f.TrunkSidePort().Flow.FlowRate > 0);
            }
        }

        [Theory]
        [InlineData(FlowDirection.TowardTrunk, new[] { 0.0, 3, 3, 3, 4 })]
        [InlineData(FlowDirection.TowardLeafs, new[] { 0.0, -3, -3, -3, -4 })]
        public void AssignPressureData(FlowDirection flowDirection, double[] expectedStaticPressure)
        {
            var tree = GetSampleTreeWithTrunkBelow();
            var fittings = new FittingTreeRouting(tree).BuildFittingTree(out var errors, flowDirection);
            Assert.Empty(errors);

            var data = GetSamplePressureCalculations(fittings);

            var errors2 = fittings.AssignPortPressuresFromPressureDiffs(data);
            Assert.Empty(errors2);
            var allConnectors = fittings.Fittings.SelectMany(c => c.GetPorts());
            Assert.Empty(allConnectors.Where(c => c.Flow == null));
            Assert.Empty(allConnectors.Where(c => c.Flow.FlowRate == 0));
            Assert.Equal(expectedStaticPressure, fittings.FittingsOfType<Terminal>().Select(t => t.Port.Flow.StaticPressure));
        }

        [Fact]
        public void AssignFlowRates()
        {
            var tree = GetSampleTreeWithTrunkBelow();
            var fittings = new FittingTreeRouting(tree).BuildFittingTree(out var errors);
            Assert.Empty(errors);
            foreach (var f in fittings.ExpandedComponents)
            {
                switch (f)
                {
                    case StraightSegment s:
                        Assert.Equal(s.Start.Flow.FlowRate, s.End.Flow.FlowRate);
                        break;
                    case Elbow e:
                        Assert.Equal(e.Start.Flow.FlowRate, e.End.Flow.FlowRate);
                        break;
                    case Coupler c:
                        Assert.Equal(c.Start.Flow.FlowRate, c.End.Flow.FlowRate);
                        break;
                    case Reducer r:
                        Assert.Equal(r.Start.Flow.FlowRate, r.End.Flow.FlowRate);
                        break;
                }
            }
        }

        private static void CheckTreeStaticPressures(FittingTree fittings)
        {
            foreach (var t in fittings.FittingsOfType<Terminal>().Where(t => t.FlowNode is Leaf))
            {
                var currentPressure = t.Port.Flow.StaticPressure;
                var current = (ComponentBase)t;
                StaticGainVsPressureDropCheck(fittings.Tree.Outlet.Position.Z, fittings.Routing.PressureCalculator, t);
            }
        }

        private static void StaticGainVsPressureDropCheck(double endOfTreeHeight, PressureCalculator pressureCalcs, Terminal terminal)
        {
            var heightForFullDelta = terminal.TrunkSidePort().Position.Z + terminal.GetLength();
            var totalAvailable = FluidFlow.StaticGainForHeightDelta(heightForFullDelta - endOfTreeHeight);
            var allDown = new List<double>();
            var trunkToLeafComponents = terminal.GetAllTrunksideComponents();
            trunkToLeafComponents.Reverse();
            for (int i = 0; i < trunkToLeafComponents.Count; i++)
            {
                var current = trunkToLeafComponents.ElementAt(i);
#if DEBUG
                // This more detailed check on every individual component is only necessary for debugging.
                if (current.TrunkSidePort() != null)
                {
                    var currentHeightDelta = TrunksideElevation(current)
                                            - endOfTreeHeight;
                    var currentAvailable = FluidFlow.StaticGainForHeightDelta(currentHeightDelta);
                    var currentPressureLoss = allDown.Sum();
                    var diffBaseStaticPressure = currentPressureLoss - currentAvailable;

                    var currentStaticPressure = current.TrunkSidePort().Flow.StaticPressure;
                    var diff = currentStaticPressure - diffBaseStaticPressure;
                    if (!diff.ApproximatelyEquals(0, 1))
                    {
                        throw new Exception("Pressure diff check was not 0");
                    }
                }
#endif
                var next = trunkToLeafComponents.ElementAtOrDefault(i + 1);
                allDown.Add(pressureCalcs.GetStaticPressureLossOfComponent(current, next));
            }
            var allDownstreamLosses = allDown.Sum();
            double foundStaticPressure = terminal.GetFinalStaticPressure().Value;
            var theoreticalStaticPressure = allDownstreamLosses - totalAvailable;
            var error = foundStaticPressure - theoreticalStaticPressure;
            if (!error.ApproximatelyEquals(0, 1))
            {
                throw new Exception("Pressure check error was not 0");
            }

        }

        private static double TrunksideElevation(ComponentBase current)
        {
            var port = current.TrunkSidePort() ?? (current as Terminal).Port;
            if (port == null)
            {
                throw new InvalidDataException("Could not find port for component");
            }
            return port.Position.Z;
        }

        public static List<PressureCalculationBase> GetSamplePressureCalculations(FittingTree fittings)
        {
            var allCalculations = new List<PressureCalculationBase>();
            foreach (var conn in fittings.Fittings)
            {
                switch (conn)
                {
                    case Elbow elbow:
                        var elbowData = new PressureCalculationElbow(elbow.Id);
                        elbowData.ZLoss = 1;
                        elbowData.Flow = elbow.TrunkSidePort().Flow.FlowRate;
                        allCalculations.Add(elbowData);
                        break;
                    case Wye wye:
                        var wyeData = new PressureCalculationWye(wye.Id);
                        wyeData.Flow = wye.TrunkSidePort().Flow.FlowRate;
                        wyeData.FlowBranch = wye.SideBranch.Flow.FlowRate;
                        wyeData.FlowMain = wye.MainBranch.Flow.FlowRate;
                        allCalculations.Add(wyeData);
                        break;
                    case Reducer reducer:
                        var reducerData = new PressureCalculationReducer(reducer.Id);
                        reducerData.Flow = reducer.TrunkSidePort().Flow.FlowRate;
                        allCalculations.Add(reducerData);
                        break;
                    case Terminal terminal:
                        if (terminal.TrunkSideComponent == null)
                        {
                            var terminalData = new PressureCalculationTerminal(terminal.Id, default(Guid), 0);
                            terminalData.Flow = terminal.BranchSidePorts().First().Flow.FlowRate;
                            allCalculations.Add(terminalData);
                        }
                        else
                        {
                            if (terminal.TrunkSideComponent.TryGetGuid(out Guid trunkSideGuid))
                            {
                                var terminalData = new PressureCalculationTerminal(terminal.Id, trunkSideGuid, null);
                                terminalData.Flow = terminal.TrunkSidePort().Flow.FlowRate;
                                allCalculations.Add(terminalData);
                            }
                            else
                            {
                                throw new Exception("Could not get guid for terminal");
                            }
                        }
                        break;
                    default:
                        throw new Exception($"Bad type {conn.GetType().FullName}");
                }
            }
            allCalculations.AddRange(fittings.StraightSegments.Select(p =>
            {
                var straightData = new PressureCalculationSegment(p.Id);
                straightData.Flow = p.TrunkSidePort().Flow.FlowRate;
                return straightData;
            }));
            return allCalculations;
        }

        private static Tree GetSampleTreeWithManifold(double flowPerInlet)
        {
            var tree = new Tree(new List<string> { "Tree" });
            var inletPositions = new List<Vector3> {new Vector3(5, 1, 10),
                                                    new Vector3(5, 0, 10),
                                                    new Vector3(5, 2, 10) ,
                                                    new Vector3(6, 1, 10) };
            var inlets = new List<Leaf>();
            Node lastNode = null;

            var aboveManifoldInlet = tree.AddInlet(inletPositions[0], flowPerInlet, lastNode);
            var outgoing = tree.GetOutgoingConnection(aboveManifoldInlet);
            lastNode = tree.SplitConnectionThroughPoint(outgoing, inletPositions[0] + new Vector3(0, 0, -1), out var splitConns);
            tree.ConnectVertically(splitConns[0], 0);

            inlets.Add(aboveManifoldInlet);

            foreach (var position in inletPositions.Skip(1))
            {
                var newInlet = tree.AddInlet(position, flowPerInlet, lastNode);
                var newOutgoing = tree.GetOutgoingConnection(newInlet);
                tree.ConnectVertically(newOutgoing, 0);
                inlets.Add(newInlet);
            }

            var outlet = tree.SetOutletPosition(new Vector3(-1, 1, 0));
            var conn = tree.GetIncomingConnections(outlet).First();
            tree.ConnectVertically(conn, 0.5, true);

            foreach (var c in tree.Connections)
            {
                c.Diameter = 0.1;
            }

            tree.Material = ClearPipe;
            return tree;
        }
    }
}