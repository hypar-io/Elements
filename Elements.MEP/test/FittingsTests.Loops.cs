using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Elements;
using Elements.Fittings;
using Elements.Flow;
using Elements.Geometry;
using Elements.Serialization.glTF;
using Xunit;

namespace Elements.MEP.Tests
{
    public partial class FittingsTests
    {
        private static string directoryName = "LoopTests";
        [Fact]
        public void LoopTest()
        {
            var tree = GetSampleTreeWithLoop(0.1);
            var sections = tree.GetSections();
            var routing = new FittingTreeRouting(tree);
            routing.PressureCalculator = new HazenWilliamsFullFlow();
            var fittings = routing.BuildFittingTree(out var errors);

            var model = new Model();
            model.AddElement(tree);
            model.AddElements(sections);
            model.AddElements(fittings);
            model.ToGlTF(TestUtils.GetTestPath(directoryName) + $"/{nameof(LoopTest)}.gltf", false);
            File.WriteAllText(TestUtils.GetTestPath(directoryName) + $"/{nameof(LoopTest)}.dot", fittings.ToDot());

            CheckTreeStaticPressures(fittings);
            CheckLoopFlowZero(tree);
            Assert.Empty(errors);
        }

        [Fact]
        public void AdjustLoopPathTest()
        {
            var tree = GetSampleTreeWithLoop(0.1);
            var path = new List<Vector3>
            {
                new Vector3(-1, 2, 4),
                new Vector3(-1, 1, 4),
                new Vector3(0, 1, 4),
                new Vector3(0, 0, 4),
                new Vector3(1, 0, 4)
            };
            Tree.AdjustPath(tree, "0,1", path);
            var sectionPathVertices = tree.GetSectionFromKey("0,1").Path.Vertices;
            for (int i = 0; i < sectionPathVertices.Count; i++)
            {
                Assert.True(sectionPathVertices[i].IsAlmostEqualTo(path[i]));
            }

            var sections = tree.GetSections();
            var routing = new FittingTreeRouting(tree);
            routing.PressureCalculator = new HazenWilliamsFullFlow();
            var fittings = routing.BuildFittingTree(out var errors);

            var model = new Model();
            model.AddElement(tree);
            model.AddElements(sections);
            model.AddElements(fittings);
            model.ToGlTF(TestUtils.GetTestPath(directoryName) + $"/{nameof(AdjustLoopPathTest)}.gltf", false);
            File.WriteAllText(TestUtils.GetTestPath(directoryName) + $"/{nameof(AdjustLoopPathTest)}.dot", fittings.ToDot());

            CheckTreeStaticPressures(fittings);
            CheckLoopFlowZero(tree);
            Assert.Empty(errors);
        }

        [Fact]
        public void TwoLoopsTest()
        {
            var tree = GetSampleTreeWithLoop(0.1, true);
            var sections = tree.GetSections();
            var routing = new FittingTreeRouting(tree) { AllowedWyeBranchAngles = new[] { 45.0, 90.0, 180.0 } };
            routing.PressureCalculator = new HazenWilliamsFullFlow();
            var fittings = routing.BuildFittingTree(out var errors);

            var model = new Model();
            model.AddElement(tree);
            model.AddElements(sections);
            model.AddElements(fittings);
            model.ToGlTF(TestUtils.GetTestPath(directoryName) + $"/{nameof(TwoLoopsTest)}.gltf", false);
            File.WriteAllText(TestUtils.GetTestPath(directoryName) + $"/{nameof(TwoLoopsTest)}.dot", fittings.ToDot());

            CheckTreeStaticPressures(fittings);
            CheckLoopFlowZero(tree);
            Assert.Empty(errors);
        }

        [Fact]
        public void FindCyclesInTwoLoopsTreeTest()
        {
            var tree = GetSampleTreeWithLoop(0.1, true);
            var connectionsCycles = tree.FindAllCyclesOfConnections();
            var nodesCycles = tree.FindAllNodesCycles().Select(c => c.Select(n => n.Position));

            var model = new Model();
            model.AddElement(tree);

            var random = new Random();
            var height = 3;
            foreach (var cycle in connectionsCycles)
            {
                model.AddElement(new ModelLines(cycle.Select(c => c.Path()).ToList(), random.NextMaterial(), new Transform(new Vector3(0, 0, height++))));
            }

            model.ToGlTF(TestUtils.GetTestPath(directoryName) + $"/{nameof(FindCyclesInTwoLoopsTreeTest)}.gltf", false);

            Assert.Equal(2, nodesCycles.Count());
            Assert.True(nodesCycles.First().SequenceEqual(new List<Vector3>
            {
                new Vector3(1, 5, 4),
                new Vector3(-1, 5, 4),
                new Vector3(-1, 3, 4),
                new Vector3(0, 3, 4),
                new Vector3(1, 3, 4)
            }));
        }

        private static Tree GetSampleTreeWithLoop(double flowPerInlet, bool addSecondLoop = false)
        {
            var tree = new Tree(new List<string> { "Tree" });
            var inletPositions = new List<(Vector3 position, List<Vector3> additionalSplitPoints)> {
                (new Vector3(2, 2, 5), new List<Vector3>(){ new Vector3(1, 2, 4)}),
                (new Vector3(0, 4, 5), new List<Vector3>(){ new Vector3(1, 3, 4), new Vector3(0, 3, 4)}),
                (new Vector3(-2, 2, 5), new List<Vector3>(){ new Vector3(-1, 3, 4), new Vector3(-1, 2, 4)}),
            };
            var inlets = new List<Leaf>();
            Node lastNode = null;
            int i = 0;
            foreach (var (position, additionalSplitPoints) in inletPositions)
            {
                i++;
                var newInlet = tree.AddInlet(position, flowPerInlet, lastNode);
                var outgoing = tree.GetOutgoingConnection(newInlet);
                var connectionToSplit = outgoing;
                var j = 0;
                foreach (var point in additionalSplitPoints)
                {
                    lastNode = tree.SplitConnectionThroughPoint(connectionToSplit, point, out var splitConns);
                    if (j == additionalSplitPoints.Count - 1)
                    {
                        tree.ConnectVertically(splitConns[0], 0);
                    }
                    connectionToSplit = tree.GetOutgoingConnection(newInlet);
                    j++;
                }

                inlets.Add(newInlet);
            }

            var outlet = tree.SetOutletPosition(new Vector3(1, -1, 0));
            var conn = tree.GetIncomingConnections(outlet).First();
            tree.ConnectVertically(conn, 0.5, true);

            var node = tree.InternalNodes.First(n => n.Position.Equals(new Vector3(1, 2, 4)));
            var connection = tree.GetOutgoingConnection(node);
            var loopEndNode = tree.SplitConnectionThroughPoint(connection, new Vector3(1, 0, 4));
            var loopStartNode = tree.InternalNodes.First(n => n.Position.Equals(new Vector3(-1, 2, 4)));
            var loopConnection = tree.AddLoopConnection(loopStartNode, loopEndNode);
            var splitNode = tree.SplitConnectionThroughPoint(loopConnection, new Vector3(-1, 0, 4), out var newConns);

            if (addSecondLoop)
            {
                var loopStartNode2 = tree.InternalNodes.First(n => n.Position.Equals(new Vector3(-1, 3, 4)));
                var loopEndNode2 = tree.InternalNodes.First(n => n.Position.Equals(new Vector3(1, 3, 4)));
                var loopConnection2 = tree.AddLoopConnection(loopStartNode2, loopEndNode2);
                tree.SplitConnectionThroughPoint(loopConnection2, new Vector3(-1, 5, 4), out var newConns2);
                tree.SplitConnectionThroughPoint(newConns2[1], new Vector3(1, 5, 4), out _);
            }

            foreach (var c in tree.Connections)
            {
                c.Diameter = 0.1;
            }
            newConns[0].Diameter = 0.05;
            newConns[1].Diameter = 0.05;

            tree.Material = ClearPipe;
            return tree;
        }

        private static void CheckLoopFlowZero(Tree tree)
        {
            foreach (var section in tree.GetLoopSections())
            {
                Assert.True(section.Flow.ApproximatelyEquals(0));
            }
            foreach (var connection in tree.GetLoopConnections())
            {
                Assert.True(connection.Flow.ApproximatelyEquals(0));
            }
        }
    }
}