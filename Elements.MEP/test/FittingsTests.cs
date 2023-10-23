using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Elements;
using Elements.Flow;
using Elements.Geometry;
using Elements.Fittings;
using Elements.Serialization.glTF;
using Xunit;

namespace Elements.MEP.Tests
{
    public partial class FittingsTests
    {
        public static Material ClearPipe = new Material("Clear Pipe",
                                                 new Color(0, 0, 0.4, 0.4),
                                                 specularFactor: 0.5,
                                                 glossinessFactor: 0.5,
                                                 unlit: false,
                                                 doubleSided: false,
                                                 repeatTexture: false,
                                                 interpolateTexture: false,
                                                 id: Guid.NewGuid());
        [Fact]
        public void MakeWye()
        {
            var model = new Model();
            var branchDirection = new Vector3(Math.Sqrt(2) / 2, 1, Math.Sqrt(2) / 2);
            var mainDir = new Vector3(0, 1, 0);
            var connectionPoint = new Vector3(1, 0, 1);
            Port.ShowArrows = true;

            var wye = new Wye(connectionPoint,
                              mainDir,
                              branchDirection,
                              new WyeSettings() { BranchDiameter = FittingTreeRouting.DefaultDiameter / 2 },
                              FittingTreeRouting.DefaultFittingMaterial);
            var pipe1 = new StraightSegment(0,
                                        wye.MainBranch,
                                        new Port(wye.MainBranch.Position + mainDir * 2, mainDir, wye.MainBranch.Diameter));
            var pipe2 = new StraightSegment(0,
                                            wye.SideBranch,
                                            new Port(wye.SideBranch.Position + branchDirection, branchDirection, wye.SideBranch.Diameter));

            model.AddElements(new Element[] { pipe1, pipe2, wye });
            model.AddElement(new Mass(Polygon.Rectangle(0.1, 0.1), 0.1));
            model.ToGlTF(TestUtils.GetTestPath() + "wye.gltf", false);
        }

        [Fact]
        public void MakeCross()
        {
            var cs = new CrossSettings();

            var directionTrunk = Vector3.XAxis.Negate();
            var position = Vector3.Origin;

            var directionA = directionTrunk.Negate();
            var directionB = new Transform(position, 90).OfVector(directionA);
            var directionC = new Transform(position, -90).OfVector(directionA);

            var cross = new Cross(position, directionTrunk, directionA, directionB, directionC, cs);
            var model = new Model();
            model.AddElement(cross);
            model.ToGlTF(TestUtils.GetTestPath() + "cross.gltf", false);
        }

        [Fact]
        public void MakeReducer()
        {
            Port.ShowArrows = true;
            var reducer = new Reducer(new Vector3(0, 0, 0), new Vector3(0, 1, 0), 0.05, 0.1, 0.08, BuiltInMaterials.Wood);
            var model = new Model();
            model.AddElement(reducer);
            model.ToGlTF(TestUtils.GetTestPath() + "reducer.gltf", false);
        }

        [Fact]
        public void MakeElbow()
        {
            Port.ShowArrows = true;
            var position = new Vector3(1, 0, 1);
            var endDirection = new Vector3(1, 0, 0);
            var otherDirection = new Vector3(0, 1, 0);

            var elbow = new Elbow(position, endDirection, otherDirection, 0.2, 0.1, FittingTreeRouting.DefaultFittingMaterial);
            SaveToGltf(nameof(MakeElbow), elbow);
        }

        [Fact]
        public void MakeElbowWithZeroBendRadius()
        {
            Port.ShowArrows = true;
            var position = new Vector3(1, 0, 1);
            var endDirection = new Vector3(1, 0, 0);
            var otherDirection = new Vector3(0, 1, 0);

            var elbow = new Elbow(position, endDirection, otherDirection, 0.2, 0.1, FittingTreeRouting.DefaultFittingMaterial, 0);
            SaveToGltf(nameof(MakeElbowWithZeroBendRadius), elbow);
        }

        [Fact]
        public void MakeElbowWithZeroSideLength()
        {
            Port.ShowArrows = true;
            var position = new Vector3(1, 0, 1);
            var endDirection = new Vector3(1, 0, 0);
            var otherDirection = new Vector3(0, 1, 0);

            var elbow = new Elbow(position, endDirection, otherDirection, 0, 0.1, FittingTreeRouting.DefaultFittingMaterial, 0.4);
            SaveToGltf(nameof(MakeElbowWithZeroSideLength), elbow);
        }

        [Fact]
        public void MakeSweptBendElbow()
        {
            Port.ShowArrows = true;
            var position = new Vector3(1, 0, 1);
            var endDirection = new Vector3(1, 0, 0);
            var otherDirection = new Vector3(0, 1, 0);

            var elbow = new Elbow(position, endDirection, otherDirection, 0.5, 0.1, FittingTreeRouting.DefaultFittingMaterial, 0.4);
            SaveToGltf(nameof(MakeSweptBendElbow), elbow);
        }

        [Fact]
        public void PipeWithReducer()
        {
            Port.ShowArrows = true;

            // Small diameter trunk side.
            var trunk1 = new Terminal(new Vector3(0, 0, 0), new Vector3(1, 1, 0), 0.2, 0.05, BuiltInMaterials.XAxis);
            var branch1 = new Terminal(new Vector3(3, 3, 0), new Vector3(-1, -1, 0), 0.2, 0.08, BuiltInMaterials.Glass);
            trunk1.BranchSideComponents.Add(branch1);
            var fittings1 = FittingTree.Create();
            fittings1.AddConnections(new[] { trunk1, branch1 });
            var errors = fittings1.CreateStraightSegmentsBetweenFittings();
            Assert.Empty(errors);

            Assert.True(1 == fittings1.StraightSegments.Count);
            var pipe1 = fittings1.StraightSegments[0];
            Assert.True(pipe1.TrunkSideComponent == trunk1);
            Assert.True(pipe1.BranchSideComponents.Count == 1);
            Assert.IsType<Reducer>(pipe1.BranchSideComponents[0]);
            Reducer reducer1 = pipe1.BranchSideComponents[0] as Reducer;
            Assert.True(reducer1.End.Diameter < reducer1.Start.Diameter);
            Assert.True(pipe1.Start == reducer1.End);
            Assert.True(reducer1.End.Position.DistanceTo(pipe1.End.Position) < reducer1.Start.Position.DistanceTo(pipe1.End.Position));

            // Large diameter trunk side.
            var trunk2 = new Terminal(new Vector3(0, 0, 2), new Vector3(1, 1, 0), 0.2, 0.15, BuiltInMaterials.XAxis);
            var branch2 = new Terminal(new Vector3(3, 3, 2), new Vector3(-1, -1, 0), 0.2, 0.08, BuiltInMaterials.Glass);
            trunk2.BranchSideComponents.Add(branch2);
            var fittings2 = FittingTree.Create();
            fittings2.AddConnections(new[] { trunk2, branch2 });
            var errors2 = fittings2.CreateStraightSegmentsBetweenFittings();
            Assert.Empty(errors2);

            Assert.True(1 == fittings2.StraightSegments.Count);
            var pipe2 = fittings2.StraightSegments[0];
            Assert.True(pipe2.BranchSideComponents.Count == 1);
            Assert.True(pipe2.BranchSideComponents[0] == branch2);
            Assert.IsType<Reducer>(pipe2.TrunkSideComponent);
            Reducer reducer2 = pipe2.TrunkSideComponent as Reducer;
            Assert.True(reducer2.End.Diameter > reducer2.Start.Diameter);
            Assert.True(pipe2.End == reducer2.Start);
            Assert.True(reducer2.Start.Position.DistanceTo(pipe2.Start.Position) < reducer2.End.Position.DistanceTo(pipe2.Start.Position));

            var model = new Model();
            model.AddElement(fittings1);
            model.AddElement(fittings2);
            model.ToGlTF(TestUtils.GetTestPath() + "reduced_pipe.gltf", false);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void PipeNetworkOperations(bool resizeNewPipe)
        {
            Port.ShowArrows = true;
            var tree = new Tree(new List<string> { "Test" });
            var inlet = tree.AddInlet(new Vector3(0, 3), 10);
            tree.SetOutletPosition(new Vector3(1, 1));
            var s = tree.SplitConnectionThroughPoint(tree.GetOutgoingConnection(inlet), new Vector3(1, 3), out var conns);
            Assert.True(conns.Count() == 2);

            var fittings = new FittingTreeRouting(tree).BuildFittingTree(out var errors);
            Assert.Empty(errors);

            Assert.Equal(2, fittings.StraightSegments.Count);
            Assert.Equal(3, fittings.Fittings.Count);
            Assert.All(fittings.StraightSegments, (pipe) => Assert.Equal("Test:0", pipe.ComponentLocator.NetworkReference + ":" + pipe.ComponentLocator.SectionKey));
            fittings.CheckComponentLabeling();

            var pipe = fittings.StraightSegments[1];
            var elbow = fittings.FittingsOfType<Elbow>().First();
            Assert.True(pipe.End == elbow.Start);
            var reducer1 = fittings.SplitPipe(pipe, 0.2, false, out var newEndPipe, out var error) as Reducer;
            Assert.Null(error);
            Assert.True(newEndPipe.Start == reducer1.End);
            Assert.True(newEndPipe.End == elbow.Start);
            Assert.True(pipe.End == reducer1.Start);
            Assert.True(pipe.BranchSideComponents.Count == 1);
            var inletTerminal = pipe.BranchSideComponents[0] as Terminal;
            Assert.True(pipe.Start == inletTerminal.Port);

            fittings.CheckComponentLabeling();

            SaveToGltf("Operation resize before", fittings, "Resizing", true);
            var pipeToResize = resizeNewPipe ? newEndPipe : pipe;
            fittings.ResizePipe(pipeToResize, 0.06);
            SaveToGltf("Operation resize after", fittings, "Resizing", true);
            File.WriteAllText(TestUtils.GetTestPath() + "conns.dot", fittings.ToDotConnectors());

            fittings.CheckComponentLabeling();

            var model = new Model();
            model.AddElements(fittings);
            model.ToGlTF(TestUtils.GetTestPath() + "pipe_operations.gltf", false);
        }

        [Fact]
        public void MakeFittingTree()
        {
            Port.ShowArrows = true;
            var tree = GetSampleTreeWithTrunkBelow();
            var fittings = new FittingTreeRouting(tree).BuildFittingTree(out var errors);
            Assert.Empty(errors);
            fittings.CheckComponentLabeling();

            var model = new Model();
            model.AddElement(fittings);
            model.ToGlTF(TestUtils.GetTestPath() + "fittings.gltf", false);
        }

        [Fact]
        public void Serialize()
        {
            var tree = GetSampleTreeWithTrunkBelow();
            var fittings = new AssemblyRouting().BuildFittingTree(tree, out var errors);
            Assert.Empty(errors);
            fittings.CheckComponentLabeling();

            var originalModel = new Model();
            originalModel.AddElement(fittings);
            var checkTrunkSide = originalModel.AllElementsAssignableFromType<ComponentBase>().Where(f => f.TrunkSideComponent == null);
            Assert.Equal(new[] { fittings.Fittings.First() }, checkTrunkSide);

            var originalAllFittings = FittingTree.ExpandAssemblies(fittings.AllComponents, false);
            var originalModelFittings = originalModel.AllElementsAssignableFromType<ComponentBase>().ToList();
            Assert.Equal(originalAllFittings.OrderBy(e => e.Id), originalModelFittings.OrderBy(e => e.Id));

            var newModel = Model.FromJson(originalModel.ToJson());
            var newFittings = newModel.AllElementsOfType<FittingTree>().Single();

            var newAllFittings = FittingTree.ExpandAssemblies(newFittings.AllComponents, false);
            var newModelFittings = newModel.AllElementsAssignableFromType<ComponentBase>().ToList();

            var leftBehindByModel = originalModelFittings.Where(f => !newModelFittings.Any(e => e.Id == f.Id)).ToList();
            Assert.Empty(leftBehindByModel);

            var leftBehindByFittings = originalAllFittings.Where(f => !newAllFittings.Any(e => e.Id == f.Id)).ToList();
            Assert.Empty(leftBehindByFittings);

            var checkTrunkSide2 = newModel.AllElementsAssignableFromType<ComponentBase>().Where(f => f.TrunkSideComponent == null);
            Assert.Equal(new[] { newFittings.Fittings.First() }, checkTrunkSide2);

            Assert.Throws<Exception>(() => newFittings.CheckComponentLabeling());
            newFittings.RestoreBranchReferences();
            newFittings.CheckComponentLabeling();

            SaveToGltf(nameof(Serialize), newFittings);
        }


        [Fact]
        public void ResizeEntireSection()
        {
            var tree = new Tree(new List<string> { "Test" });
            var inlet1 = tree.AddInlet(new Vector3(3, 6), 10);
            tree.SetOutletPosition(new Vector3(0, 1));
            var s = tree.SplitConnectionThroughPoint(tree.GetOutgoingConnection(inlet1), new Vector3(0, 3), out var conns);
            Assert.True(conns.Count() == 2);
            var s2 = tree.SplitConnectionThroughPoint(tree.GetOutgoingConnection(inlet1), new Vector3(0, 6), out var conns2);
            var fittings = new EccentricRouting().BuildFittingTree(tree, out var errors);

            var section = tree.GetSections().Single();
            SaveToGltf(nameof(ResizeEntireSection) + "_preResize", fittings, "Resizing");

            fittings.ChangeSizeOfSection(section, 0.07, double.MaxValue, false);
            fittings.ChangeSizeOfSection(section, 0.05, section.Path.Length() / 2, true);
            fittings.ChangeSizeOfSection(section, 0.09, section.Path.Length() / 3, false);

            SaveToGltf(nameof(ResizeEntireSection), fittings, "Resizing");
            fittings.CheckComponentLabeling();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ResizeSplitPipeRemoveExtraPipe(bool resizeOriginalPipe)
        {
            // TODO re-activate and complete this test
            // Setup horizontal piped fittings for testing.
            var trunk = new Terminal(new Vector3(0, 0, 0), new Vector3(1, 1, 0), 0.2, 0.05, terminalMaterial);
            var branch = new Terminal(new Vector3(3, 3, 0), new Vector3(-1, -1, 0), 0.2, 0.05, terminalMaterial);
            trunk.BranchSideComponents.Add(branch);
            var fittings = FittingTree.Create(new EccentricRouting());

            fittings.AddConnections(new[] { trunk, branch });
            var errors = fittings.CreateStraightSegmentsBetweenFittings();
            Assert.Empty(errors);

            var pipe = fittings.StraightSegments.First();
            fittings.SplitPipe(pipe, 0.5, false, out var newPipe, out var error);
            Assert.Null(error);

            var pipeToResize = resizeOriginalPipe ? pipe : newPipe;
            // 5 components to start with.  2 terminal, 2 pipe, 1 reducer
            Assert.Equal(5, fittings.AllComponents.Count());

            // resize to current size should leave split alone..
            fittings.ResizePipe(pipeToResize, 0.05);
            fittings.CheckComponentLabeling();
            Assert.Equal(5, fittings.AllComponents.Count());

            // // size up, then size back down should trigger reducer removal and pipe joining.
            fittings.ResizePipe(pipeToResize, 0.08);
            Assert.Equal(6, fittings.AllComponents.Count()); // keep split reducer and add 1 more.

            fittings.ResizePipe(pipeToResize, 0.05);
            Assert.Equal(3, fittings.AllComponents.Count()); // even original reducer should be gone now and the two pipes joined to one.
            SaveToGltf("RemoveSplit", fittings, "Resizing", true);
        }

        [Fact]
        public void ResizePipeRemoveReducers()
        {
            // Setup horizontal piping tree for testing.
            var trunk = new Terminal(new Vector3(0, 0, 0), new Vector3(1, 1, 0), 0.2, 0.05, terminalMaterial);
            var branch = new Terminal(new Vector3(3, 3, 0), new Vector3(-1, -1, 0), 0.2, 0.05, terminalMaterial);
            trunk.BranchSideComponents.Add(branch);
            var fittings = FittingTree.Create(new EccentricRouting());

            fittings.AddConnections(new[] { trunk, branch });
            var errors = fittings.CreateStraightSegmentsBetweenFittings();
            Assert.Empty(errors);

            // 3 components in simple 2 terminal, 1 pipe fitting tree.
            Assert.Equal(3, fittings.AllComponents.Count());

            // Resize pipe to new size, which should create 2 reducers.
            var newSize = 0.1;
            var pipe = fittings.StraightSegments.FirstOrDefault();
            fittings.ResizePipe(pipe, newSize);
            SaveToGltf("RemoveReducerBefore", fittings, "Resizing");

            // 5 components exist after resizing adds 2 reducers.
            Assert.Equal(5, fittings.AllComponents.Count());

            // Resize again to new terminal diameter.
            trunk.Port.Diameter = newSize;
            branch.Port.Diameter = newSize;
            fittings.ResizePipe(pipe, newSize);
            SaveToGltf("RemoveReducerAfter", fittings, "Resizing");

            // 3 components after resize smartly removes the 2 unnecessary reducers.
            Assert.Equal(3, fittings.AllComponents.Count());
            fittings.CheckComponentLabeling();
        }

        [Fact]
        public void ChangeSizeOfWholeSection()
        {
            Port.ShowArrows = true;
            var tree = GetSampleTreeWithTrunkBelow();
            var fittings = new FittingTreeRouting(tree).BuildFittingTree(out var errors);
            Assert.Empty(errors);

            // change at 1.5 from collector to test changing close to an elbow
            var section = tree.GetSectionFromKey("0,1");
            fittings.ChangeSizeOfSection(section, 0.1, 1.5);
            Assert.Equal(6, fittings.GetComponentsOfSection(section).Count());
            fittings.CheckComponentLabeling();

            // change all of section to new size
            var section2 = tree.GetSectionFromKey("0,0,1");
            fittings.ChangeSizeOfSection(section2, 0.07, double.MaxValue);

            // change 2 meters from collector
            section2 = tree.GetSectionFromKey(section2.SectionKey);
            fittings.ChangeSizeOfSection(section2, 0.15, 2);
            Assert.Equal(7, fittings.GetComponentsOfSection(section2).Count());
            fittings.CheckComponentLabeling();

            // change section again resetting all to new size
            section2 = tree.GetSectionFromKey(section2.SectionKey);
            fittings.ChangeSizeOfSection(section2, 0.2, double.MaxValue);
            fittings.CheckComponentLabeling();

            // change 2 meters starting from terminal
            var section3 = tree.GetSectionFromKey("0,0,0,1");
            fittings.ChangeSizeOfSection(section3, 0.15, 2, true);
            Assert.Equal(6, fittings.GetComponentsOfSection(section3).Count());

            SaveToGltf(nameof(ChangeSizeOfWholeSection), fittings, "Resizing", true);
            fittings.CheckComponentLabeling();
        }

        [Fact]
        public void ChangeBranchSizeWithReducers()
        {
            Port.ShowArrows = true;
            var tree = GetSampleTreeWithTrunkBelow();
            var fittings = new FittingTreeRouting(tree).BuildFittingTree(out var errors);
            Assert.Empty(errors);

            var branchSize = 0.05;
            fittings.Fittings.OfType<Wye>().ToList().ForEach(w => w.SideBranch.Diameter = branchSize);

            var section = tree.GetSectionFromKey("0,1");
            fittings.ChangeSizeOfSection(section, 0.1, double.MaxValue);
            SaveToGltf("ChangeSectionSizeReduced", fittings, "Resizing", true);
            Assert.Equal(5, fittings.GetComponentsOfSection(section).Count());

            var trunkSideReducer = fittings.GetComponentsOfSection(section).First() as Reducer;
            Assert.Equal(trunkSideReducer.End.Diameter, branchSize);

            // TODO there maybe additional reducer sizes we need to check here.

            fittings.CheckComponentLabeling();
        }

        [Fact]
        public void InsertInspectionOpening()
        {
            Port.ShowArrows = true;
            var tree = GetSampleTreeWithTrunkBelow();
            var fittings = new FittingTreeRouting(tree).BuildFittingTree(out var errors);
            Assert.Empty(errors);

            var gravityTerminals = fittings.AllComponents.OfType<Terminal>().OrderBy(
                pt => pt.Port.Position.DistanceTo(tree.Outlet.Position));

            var endTerminal = gravityTerminals.First();
            var startTerminal = gravityTerminals.Last();
            var endComponent = endTerminal.BranchSideComponents.First();
            Assert.IsType<StraightSegment>(endComponent);
            var startComponent = startTerminal.TrunkSideComponent;
            Assert.IsType<StraightSegment>(startComponent);
            var endPipe = (StraightSegment)endComponent;
            var startPipe = (StraightSegment)startComponent;

            var length = startPipe.Diameter * 2;
            var topLength = startPipe.Diameter;
            var io = new InspectionOpening(startPipe.Start.Position, startPipe.Start.Direction,
                new Vector3(1, 0, 0), length, topLength, startPipe.Diameter);
            fittings.PlaceCoupler(startPipe, startPipe.Start, io);
            fittings.CheckComponentLabeling();

            length = endPipe.Diameter * 2;
            topLength = endPipe.Diameter;
            io = new InspectionOpening(endPipe.End.Position + endPipe.End.Direction * length, endPipe.End.Direction.Negate(),
                new Vector3(0, 1, 0), length, topLength, endPipe.Diameter);
            fittings.PlaceCoupler(endPipe, endPipe.End, io);
            fittings.CheckComponentLabeling();
        }

        [Fact]
        public void SortNetworkReferences()
        {
            var keys = new List<string> { "RS-8", "RA-8", "RS-1" };
            var c0 = new Tree(keys);
            Assert.Equal(3, c0.RegionReferences.Count);
            Assert.Equal(c0.RegionReferences[0], keys[1]);
            Assert.Equal(c0.RegionReferences[1], keys[2]);
            Assert.Equal(c0.RegionReferences[2], keys[0]);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void InsertExpansionSockets(bool reverseCouplers)
        {
            Port.ShowArrows = true;
            var tree = GetSampleTreeWithTrunkBelow();
            var fittings = new FittingTreeRouting(tree).BuildFittingTree(out var errors);
            Assert.Empty(errors);

            var downpipe = fittings.StraightSegments.Where(
                p => p.End.Position.Z != p.Start.Position.Z).OrderBy(
                p => p.End.Position.DistanceTo(tree.Outlet.Position)).First();
            List<Coupler> sockets = new List<Coupler>();
            for (double i = 0; i < downpipe.Length(); i += 5)
            {
                var diameter = downpipe.Diameter;
                var length = diameter * 2;
                if (length + i < downpipe.Length())
                {
                    Vector3 direction = downpipe.Start.Direction;
                    Vector3 position = downpipe.Start.Position + direction * i;
                    var depth = 0.05;
                    sockets.Add(new ExpansionSocket(position, direction, length, diameter, depth));
                }
            }

            if (reverseCouplers)
            {
                sockets.Reverse();
            }
            fittings.PlaceCouplers(downpipe, sockets);
            fittings.CheckComponentLabeling();
            Assert.All(sockets, s => Assert.True(s.Start.Direction == s.End.Direction.Negate()));
            SaveToGltf(nameof(InsertExpansionSockets) + $"-Reverse is {reverseCouplers}", fittings, "Couplers", true);
        }

        [Fact]
        public void InsertSingleCoupler()
        {
            Port.ShowArrows = true;
            var tree = GetSampleTreeWithTrunkBelow();
            var fittings = new FittingTreeRouting(tree).BuildFittingTree(out var errors);
            Assert.Empty(errors);

            var downpipe = fittings.StraightSegments.Where(
                p => p.End.Position.Z != p.Start.Position.Z).OrderBy(
                p => p.End.Position.DistanceTo(tree.Outlet.Position)).First();

            var length = downpipe.Diameter * 2;
            Vector3 direction = downpipe.Start.Direction;
            var pipeLine = new Line(downpipe.Start.Position, downpipe.End.Position);
            pipeLine.Intersects(new Plane(new Vector3(0, 0, 5), Vector3.ZAxis), out Vector3 position);
            var offset = (position - downpipe.Start.Position).Length();
            Assert.False(offset < length || offset + length > downpipe.Length() - length);

            var coupler = new Coupler("Coupler", position, direction, length, downpipe.Diameter);
            fittings.PlaceCouplers(downpipe, new List<Coupler> { coupler });
            fittings.CheckComponentLabeling();

            Assert.True(coupler.Start.Direction == coupler.End.Direction.Negate());
            Assert.True(downpipe.End == coupler.Start);
            Assert.Contains(coupler.TrunkSideComponent.BranchSidePorts(), c => c == coupler.End);

            SaveToGltf(nameof(InsertSingleCoupler), fittings, "Couplers", true);
        }

        [Fact]
        public void HealConnectionSplits()
        {
            var tree = GetSampleTreeWithTrunkBelow();
            var firstSection = tree.GetSectionFromKey("0");

            var connections = tree.GetConnectionsForSection(firstSection);
            Assert.Equal(3, connections.Length);

            tree.SplitConnectionThroughPoint(connections[0], connections[0].Path().Mid(), out _);

            var connectionsAfterSplit = tree.GetConnectionsForSection(firstSection);
            Assert.Equal(4, connectionsAfterSplit.Length);

            tree.HealSplits(connectionsAfterSplit);

            var connectionsAfterHeal = tree.GetConnectionsForSection(firstSection);
            Assert.Equal(3, connectionsAfterHeal.Length);
        }

        [Fact]
        public void Wye_GetBranchSideComponent()
        {
            Port.ShowArrows = true;
            var tree = GetSampleTreeWithTrunkBelow();
            var fittings = new FittingTreeRouting(tree).BuildFittingTree(out var errors);
            Assert.Empty(errors);

            var wyes = fittings.FittingsOfType<Wye>();

            Assert.All(wyes, x =>
            {
                Assert.NotNull(x.GetBranchSideComponent(x.MainBranch));
                Assert.NotNull(x.GetBranchSideComponent(x.SideBranch));

                Assert.Null(x.GetBranchSideComponent(null));
                Assert.Null(x.GetBranchSideComponent(x.TrunkSidePort()));
            });
        }

        [Fact]
        public void FittingTreeWithConnectivity()
        {
            Port.ShowArrows = true;
            var tree = GetSampleTreeWithTrunkAbove(0.01, treeAdjustmentVector: new Vector3(0, 1, 1));
            foreach (var connection in tree.Connections)
            {
                connection.Diameter = 0.1524;
            }
            tree.Connections[5].Diameter = 0.127;

            var routing = new FittingTreeRouting(tree);
            routing.FittingCatalog = LoadFittingCatalog();
            routing.PipeSizeShouldMatchConnection = true;
            var fittingTree = routing.BuildFittingTree(out var errors, FlowDirection.TowardLeafs);
            Assert.Empty(errors);

            var connectivityErrors = fittingTree.CheckConnectivities();
            Assert.Empty(connectivityErrors);

            foreach (var fitting in fittingTree.Fittings)
            {
                //There is a rule that only one of two connected fittings can have extension.
                double extension = fitting is IReducer ? 0 : 0.04;
                foreach (var port in fitting.GetPorts())
                {
                    port.ConnectionType = new PortConnectionType(PortConnectionTypeConnectivty.Welded, PortConnectionTypeEndType.None);
                    port.Dimensions = new PortDimensions(extension, port.Diameter * 1.5, 0);
                }
            }

            connectivityErrors = fittingTree.CheckConnectivities();
            Assert.Empty(connectivityErrors);

            fittingTree.Fittings[0].GetPorts()[0].ConnectionType = null;
            fittingTree.Fittings[0].GetPorts()[0].Dimensions = null;

            fittingTree.Fittings[2].GetPorts()[0].Dimensions.Extension = 0.65;
            fittingTree.Fittings[3].GetPorts()[0].Dimensions.Extension = 0.65;

            fittingTree.Fittings[4].GetPorts()[1].ConnectionType.Connectivty = PortConnectionTypeConnectivty.Flange;

            var directConnection = fittingTree.Fittings.First(f => f.TrunkSideComponent is Fitting);
            directConnection.TrunkSidePort().ConnectionType.EndType = PortConnectionTypeEndType.Female;

            connectivityErrors = fittingTree.CheckConnectivities();
            Assert.Equal(4, connectivityErrors.Count);

            SaveToGltf(nameof(FittingTreeWithConnectivity), new Element[] { fittingTree });
        }

        public static Tree GetSampleTreeWithTrunkBelow(double flowPerInlet = 5, Vector3? treeAdjustmentVector = null)
        {
            var inletPositions = new List<Vector3> {new Vector3(1, 0, 10),
                                                    new Vector3(4, 0, 10),
                                                    new Vector3(7, 0, 10) ,
                                                    new Vector3(10, 0, 10) };
            var treeAdjustment = treeAdjustmentVector ?? new Vector3(-1, 1, -1);
            var outlet = new Vector3(-1, 1, 0);
            return GetSampleTree(inletPositions, treeAdjustment, outlet, flowPerInlet);
        }

        public static Tree GetSampleTreeWithTrunkAbove(double flowPerInlet = 5, Vector3? treeAdjustmentVector = null)
        {
            var inletPositions = new List<Vector3> {new Vector3(1, 0, 0),
                                                    new Vector3(4, 0, 0),
                                                    new Vector3(7, 0, 0) ,
                                                    new Vector3(10, 0, 0) };
            var treeAdjustment = treeAdjustmentVector ?? new Vector3(-1, 1, 1);
            var outlet = new Vector3(-1, 1, 2);
            return GetSampleTree(inletPositions, treeAdjustment, outlet, flowPerInlet);
        }

        private static Tree GetSampleTree(List<Vector3> inletPositions, Vector3 leafOffset, Vector3 trunkPosition, double flowPerInlet)
        {
            var tree = new Tree(new List<string> { "Tree" });
            var inlets = new List<Leaf>();
            Node lastNode = null;
            int i = 0;
            foreach (var position in inletPositions)
            {
                i++;
                var newInlet = tree.AddInlet(position, flowPerInlet, lastNode);
                var outgoing = tree.GetOutgoingConnection(newInlet);
                lastNode = tree.SplitConnectionThroughPoint(outgoing, position + leafOffset, out var splitConns);
                tree.ConnectVertically(splitConns[0], 0);
                if (i % 2 == 0 && position != inletPositions.First())
                {
                    var splitPOint = splitConns[1].Start.Position.Average(splitConns[1].End.Position);
                    tree.SplitConnectionThroughPoint(splitConns[1], splitPOint, out _);
                }
                inlets.Add(newInlet);
            }

            var outlet = tree.SetOutletPosition(trunkPosition);
            var conn = tree.GetIncomingConnections(outlet).First();
            tree.ConnectVertically(conn, 0.5, true);

            foreach (var c in tree.Connections)
            {
                c.Diameter = 0.1;
            }

            tree.Material = ClearPipe;

            return tree;
        }

        private static Tree GetSampleTreeWithCross(double flowPerInlet = 5)
        {
            var tree = new Tree(new List<string> { "Tree" });
            var inletPositions = new List<Vector3> {new Vector3(5, 0, 10),
                                                    new Vector3(5, 2, 10) ,
                                                    new Vector3(6, 1, 10) };
            var inlets = new List<Leaf>();
            Node lastNode = null;

            var aboveManifoldInlet = tree.AddInlet(inletPositions[0], flowPerInlet, lastNode);
            var outgoing = tree.GetOutgoingConnection(aboveManifoldInlet);
            lastNode = tree.SplitConnectionThroughPoint(outgoing, new Vector3(5, 1, 9), out var splitConns);
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

        private static Tree GetSampleGridTree(double flowPerInlet = 5, double diameter = 0.1)
        {
            var tree = new Tree(new List<string> { "Tree" });
            Vector3 betweenRows = new Vector3(5, 0, 0);
            Vector3 betweenColumns = new Vector3(0, 5, 0);
            int numRows = 5;
            int numColumns = 6;
            Vector3 inletHeight = new Vector3(0, 0, 3);

            tree.SetOutletPosition(new Vector3(-5, 0, 0));
            Node lastColumn = null;

            for (int i = 0; i < numRows; i++)
            {
                Node lastNode = null;
                for (int j = 0; j < numColumns; j++)
                {
                    var cornerPoint = betweenRows * i + betweenColumns * j;
                    var connectNode = j == 0 ? lastColumn : lastNode;
                    var newInlet = tree.AddInlet(cornerPoint + inletHeight, flowPerInlet, connectNode);
                    var newOutgoing = tree.GetOutgoingConnection(newInlet);
                    lastNode = tree.SplitConnectionThroughPoint(newOutgoing, cornerPoint);

                    if (j == 0)
                    {
                        lastColumn = lastNode;
                    }
                }
            }

            foreach (var c in tree.Connections)
            {
                c.Diameter = diameter;
            }

            tree.Material = ClearPipe;

            return tree;
        }
    }
}
