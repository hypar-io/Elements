using System;
using System.Collections.Generic;
using Elements;
using Elements.Fittings;
using Elements.Flow;
using System.Linq;
using Elements.Geometry;
using Xunit;

namespace Elements.MEP.Tests
{
    public partial class FittingsTests
    {
        [Fact]
        public void TerminalsBeyondConnectionNode()
        {
            var tree = new Tree(new[] { "Test" });
            tree.SetOutletPosition(new Vector3());
            var inlet = tree.AddInlet(new Vector3(0, 5, 0.5));
            tree.Connections.ToList().ForEach(c => c.Diameter = 0.1);
            tree.SplitConnectionThroughPoint(tree.GetOutgoingConnection(inlet), new Vector3(0, 5, 0));

            var fittings = new LargeFittingRouting(tree).BuildFittingTree(out var errors);
            SaveToGltf(nameof(TerminalsBeyondConnectionNode), fittings);
            Assert.Equal(4, fittings.ExpandedComponents.Count()); // 2 terminals, 1 elbow, 1 pipe
            Assert.Equal(3, fittings.AllComponents.Count()); // 2 terminals, 1 pipe
            Assert.Empty(errors);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void PipeWithTwoReducers(bool isEccentric)
        {
            var tree = new Tree(new[] { "Test" });
            tree.SetOutletPosition(new Vector3());
            var inlet = tree.AddInlet(new Vector3(10, 5, 0));
            tree.SplitConnectionThroughPoint(tree.GetOutgoingConnection(inlet), new Vector3(5, 0, 0));
            var newNode = tree.SplitConnectionThroughPoint(tree.GetOutgoingConnection(inlet), new Vector3(5, 5, 0));
            tree.Connections.ToList().ForEach(c => c.Diameter = 0.1);
            tree.GetOutgoingConnection(newNode).Diameter = 0.05;

            var routing = new SizeAlwaysFromLeafOrTrunk(tree, isEccentric);
            routing.PipeSizeShouldMatchConnection = true;
            routing.PressureCalculator = new HazenWilliamsFullFlow();

            var fittings = routing.BuildFittingTree(out var errors);
            SaveToGltf(nameof(PipeWithTwoReducers), fittings);
            Assert.Empty(errors);
            Assert.Equal(2, fittings.FittingsOfType<Reducer>().Count());
        }

        [Fact]
        public void FittingCatalog()
        {
            var tree = GetSampleTreeWithTrunkBelow(treeAdjustmentVector: new Vector3(0, 1, -1));
            foreach (var connection in tree.Connections)
            {
                connection.Diameter = Units.InchesToMeters(4);
            }

            var branchConnections = tree.Connections.Where(c => tree.GetIncomingConnections(c.End).Count > 1 && !tree.GetOutgoingConnection(c.End).Direction().IsParallelTo(c.Direction()));
            foreach (var connection in branchConnections)
            {
                connection.Diameter = Units.InchesToMeters(1);
            }
            var routing = new FittingTreeRouting(tree);
            FittingCatalog fittingCatalog = LoadFittingCatalog();
            routing.FittingCatalog = fittingCatalog;
            routing.PipeSizeShouldMatchConnection = true;
            Assert.Equal(24, routing.FittingCatalog.Elbows.Count);
            Assert.Equal(77, routing.FittingCatalog.Reducers.Count);
            Assert.Equal(12, routing.FittingCatalog.Tees.Count);
            Assert.Equal(12, routing.FittingCatalog.Crosses.Count);

            var fittings = routing.BuildFittingTree(out var errors);
            SaveToGltf(nameof(FittingCatalog), new Element[] { fittings });
            Assert.Empty(errors);
        }

        [Fact]
        public void FittingCatalogTreeWith45Tees()
        {
            // tree with 45 degree tees connections should fail, because catalog doesn't have such fittings.
            var tree = GetSampleTreeWithTrunkBelow();
            foreach (var connection in tree.Connections)
            {
                connection.Diameter = Units.InchesToMeters(4);
            }
            var routing = new FittingTreeRouting(tree);
            routing.FittingCatalog = LoadFittingCatalog();
            routing.PipeSizeShouldMatchConnection = true;
            var fittings = routing.BuildFittingTree(out var errors);
            Assert.NotEmpty(errors);
        }

        [Fact]
        public void FittingCatalogTreeWithCross()
        {
            Tree tree = GetSampleTreeWithCross(0.01);
            foreach (var connection in tree.Connections)
            {
                connection.Diameter = Units.InchesToMeters(4);
            }
            var crossNode = tree.InternalNodes.FirstOrDefault(n => tree.GetIncomingConnections(n).Count == 3);
            var crossIncomingConnections = tree.GetIncomingConnections(crossNode);
            crossIncomingConnections[0].Diameter = Units.InchesToMeters(2);
            crossIncomingConnections[1].Diameter = Units.InchesToMeters(2);

            tree.GetIncomingConnections(tree.Outlet).First().Diameter = Units.InchesToMeters(4);
            foreach (var inlet in tree.Inlets)
            {
                tree.GetOutgoingConnection(inlet).Diameter = Units.InchesToMeters(4);
            }
            var routing = new FittingTreeRouting(tree);
            routing.FittingCatalog = LoadFittingCatalog();
            routing.PipeSizeShouldMatchConnection = true;
            var fittings = routing.BuildFittingTree(out var errors);
            SaveToGltf(nameof(FittingCatalogTreeWithCross), new Element[] { fittings });
            Assert.Empty(errors);
        }

        [Fact]
        public void FittingCatalogBadCsv()
        {
            var elbows = ElbowPart.LoadFromCSV("test part catalogs/elbowPartsBad.csv", Units.LengthUnit.Inch);
            // csv has 12 rows, but 3 of them should fail parsing
            Assert.Equal(9, elbows.Count);
        }

        private static FittingCatalog LoadFittingCatalog()
        {
            return new FittingCatalog()
            {
                Elbows = ElbowPart.LoadFromCSV("test part catalogs/elbowParts.csv", Units.LengthUnit.Inch),
                Reducers = ReducerPart.LoadFromCSV("test part catalogs/reducerParts.csv", Units.LengthUnit.Inch),
                Tees = TeePart.LoadFromCSV("test part catalogs/teeParts.csv", Units.LengthUnit.Inch),
                Crosses = CrossPart.LoadFromCSV("test part catalogs/crossParts.csv", Units.LengthUnit.Inch)
            };
        }

        private class SizeAlwaysFromLeafOrTrunk : FittingTreeRouting
        {
            public SizeAlwaysFromLeafOrTrunk(Tree tree, bool eccentric = false) : base(tree)
            {
                Eccentric = eccentric;
            }

            private bool Eccentric = false;

            public override Fitting ChangeDirection(Connection incoming, Connection outgoing)
            {
                var diameter = outgoing.Diameter;

                if (incoming.Start is Leaf)
                {
                    diameter = incoming.Diameter;
                }
                return CreateElbow(diameter, incoming.End.Position, incoming.Direction().Negate(), outgoing.Direction());
            }

            public override IReducer ReduceOrJoin(StraightSegment pipe, bool invert, double newDiameter, double additionalDistance)
            {
                var length = 0.08;
                var reducer = Reducer.ReducerForPipe(pipe, length, invert, newDiameter, additionalDistance);
                var pipeDirection = (pipe.Start.Position - pipe.End.Position).Unitized();

                invert = false;
                double offset = invert ? reducer.Start.Diameter - reducer.End.Diameter : reducer.End.Diameter - reducer.Start.Diameter;
                offset = Eccentric ? offset / 2.0 : 0;

                if (pipeDirection.Z.ApproximatelyEquals(0, 0.05))
                {
                    reducer.BranchSideTransform.Move(Vector3.ZAxis * offset);
                }
                else if (pipeDirection.Y.ApproximatelyEquals(0) && pipeDirection.X.ApproximatelyEquals(0))
                {
                    reducer.BranchSideTransform.Move(Vector3.YAxis * offset);
                }
                return reducer;
            }
        }

        private class LargeFittingRouting : FittingTreeRouting
        {
            public LargeFittingRouting(Tree tree) : base(tree)
            {
            }
            public override Fitting TerminatePipe(Connection incoming, Connection outgoing, out Node[] absorbedNodes)
            {
                absorbedNodes = new Node[0];
                if (outgoing?.Length() < 1)
                {
                    var nextConn = Tree.GetOutgoingConnection(outgoing.End);
                    absorbedNodes = new[] { outgoing.End };
                    return new TerminalWithElbow(outgoing, nextConn);
                }
                else
                {
                    return base.TerminatePipe(incoming, outgoing, out _);
                }
            }
        }

        public class TerminalWithElbow : Assembly
        {
            public TerminalWithElbow(Connection outgoing, Connection nextConn)
            {
                if (outgoing.Length() < 0.3)
                {
                    throw new Exception("No space for Terminal too short");
                }
                double elbowSize = 0.1;
                var inlet = outgoing.Start as Leaf;
                var elbow = new Elbow(outgoing.End.Position, outgoing.Direction().Negate(), nextConn.Direction(), elbowSize, outgoing.Diameter);
                var terminal = new Terminal(inlet.Position, Vector3.ZAxis.Negate(), outgoing.Length() - elbowSize, outgoing.Diameter, BuiltInMaterials.Default);
                terminal.Material = BuiltInMaterials.Glass;
                terminal.TrunkSideComponent = elbow;
                InternalSegments = new List<StraightSegment>();
                InternalFittings = new List<Fitting> { elbow, terminal };
                ExternalPorts = new List<Port> { elbow.End };
            }
        }
    }
}