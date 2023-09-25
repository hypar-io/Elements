using System.IO.Pipes;
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
        private const string EccentricDirectory = "EccentricReducers";
        private static Material terminalMaterial = new Material("Terminal", new Color(1.0, 0f, 0f, 0.3f),
            specularFactor: 1.0f, glossinessFactor: 1.0f, unlit: true, doubleSided: false, repeatTexture: false,
            interpolateTexture: false, id: Guid.Parse("31c8c2b1-d61c-ac46-8666-d651f50f0313"));

        [Fact]
        public void LshapedFittingTreeWithAssemblyAndReducersAroundIt()
        {
            FittingTreeRouting alignedRouting = new EccentricRouting();

            var tree = new Tree(new List<string> { "Test" });
            var inlet1 = tree.AddInlet(new Vector3(3, 6), 10);
            tree.SetOutletPosition(new Vector3(0, 1));
            var s = tree.SplitConnectionThroughPoint(tree.GetOutgoingConnection(inlet1), new Vector3(0, 3), out var conns);
            Assert.True(conns.Count() == 2);
            var s2 = tree.SplitConnectionThroughPoint(tree.GetOutgoingConnection(inlet1), new Vector3(0, 6), out var conns2);
            var fittings = alignedRouting.BuildFittingTree(tree, out var errors);
            Assert.Empty(errors);
            fittings.CheckComponentLabeling();

            SaveToGltf(nameof(LshapedFittingTreeWithAssemblyAndReducersAroundIt), fittings, EccentricDirectory, true);

            double diameter = 0.16;
            var reducers = fittings.Fittings.OfType<Reducer>().ToList();
            Assert.True(reducers.Count() == 2);
            var reducer = reducers[0];
            Assert.True(reducer.BranchSideTransform.Origin.IsAlmostEqualTo(new Vector3(0, 0, -diameter / 2)));
            Assert.True(reducer.Transform.Origin.IsAlmostEqualTo(new Vector3(0, 2.41, 0)));
            Assert.True(reducer.Start.Position.IsAlmostEqualTo(new Vector3(0, 2.45, -diameter / 2)));
            Assert.True(reducer.End.Position.IsAlmostEqualTo(new Vector3(0, 2.37, 0)));

            Assert.IsType<StraightSegment>(reducer.TrunkSideComponent);
            var pipe = (StraightSegment)reducer.TrunkSideComponent;
            Assert.True(pipe.Start.Position.IsAlmostEqualTo(new Vector3(0, 2.37, 0)));
            Assert.True(pipe.End.Position.IsAlmostEqualTo(new Vector3(0, 1.1, 0)));

            reducer = reducers[1];
            Assert.True(reducer.BranchSideTransform.Origin.IsAlmostEqualTo(new Vector3(0, 0, diameter / 2)));
            Assert.True(reducer.Transform.Origin.IsAlmostEqualTo(new Vector3(0, 3.59, -diameter / 2)));
            Assert.True(reducer.Start.Position.IsAlmostEqualTo(new Vector3(0, 3.63, 0)));
            Assert.True(reducer.End.Position.IsAlmostEqualTo(new Vector3(0, 3.55, -diameter / 2)));

            Assert.True(reducer.BranchSideComponents.Count == 1);
            Assert.IsType<StraightSegment>(reducer.BranchSideComponents[0]);
            pipe = (StraightSegment)reducer.BranchSideComponents[0];
            // Pipe should both be horizontal and at 0 since our reducer strategy should not elevate the entire branch side.
            // The branch side and trunk side should be parallel.
            Assert.True(pipe.Start.Position.IsAlmostEqualTo(new Vector3(0, 5.956, 0)));
            Assert.True(pipe.End.Position.IsAlmostEqualTo(new Vector3(0, 3.63, 0)));
        }

        [Fact]
        public void HorizontalFittingTreeWithOneReducer()
        {
            Port.ShowArrows = true;

            // Small diameter trunk side.
            var trunk = new Terminal(new Vector3(0, 0, 0), new Vector3(1, 1, 0), 0.2, 0.05, terminalMaterial);
            var branch = new Terminal(new Vector3(3, 3, 0), new Vector3(-1, -1, 0), 0.2, 0.08, terminalMaterial);
            trunk.BranchSideComponents.Add(branch);
            var fittings = FittingTree.Create(new EccentricRouting());

            fittings.AddConnections(new[] { trunk, branch });
            var errors = fittings.CreateStraightSegmentsBetweenFittings();
            Assert.Empty(errors);
            SaveToGltf(nameof(HorizontalFittingTreeWithOneReducer), fittings, EccentricDirectory);

            double diameter = 0.03;
            var reducers = fittings.Fittings.OfType<Reducer>().ToList();
            Assert.True(reducers.Count() == 1);
            var reducer = reducers[0];
            Assert.True(reducer.BranchSideTransform.Origin.IsAlmostEqualTo(new Vector3(0, 0, -diameter / 2)));
            Assert.True(reducer.Transform.Origin.Z.ApproximatelyEquals(0));
            Assert.True(reducer.Start.Position.Z.ApproximatelyEquals(-diameter / 2));
            Assert.True(reducer.End.Position.Z.ApproximatelyEquals(0));

            Assert.IsType<StraightSegment>(reducer.TrunkSideComponent);
            var pipe = (StraightSegment)reducer.TrunkSideComponent;
            Assert.True(pipe.Start.Position.Z.ApproximatelyEquals(0));
            Assert.True(pipe.End.Position.Z.ApproximatelyEquals(0));

            Assert.True(1 == fittings.StraightSegments.Count);
            Assert.True(pipe.TrunkSideComponent == trunk);
            Assert.True(pipe.BranchSideComponents.Count == 1);
            Assert.IsType<Reducer>(pipe.BranchSideComponents[0]);
            Assert.True(reducer.End.Diameter < reducer.Start.Diameter);
            Assert.True(pipe.Start == reducer.End);
            Assert.True(reducer.End.Position.DistanceTo(pipe.End.Position) < reducer.Start.Position.DistanceTo(pipe.End.Position));

            // Large diameter trunk side.
            trunk = new Terminal(new Vector3(0, 0, 2), new Vector3(1, 1, 0), 0.2, 0.15, terminalMaterial);
            branch = new Terminal(new Vector3(3, 3, 2), new Vector3(-1, -1, 0), 0.2, 0.08, terminalMaterial);
            trunk.BranchSideComponents.Add(branch);
            fittings = FittingTree.Create(new EccentricRouting());
            fittings.AddConnections(new[] { trunk, branch });
            var errors2 = fittings.CreateStraightSegmentsBetweenFittings();
            Assert.Empty(errors2);

            diameter = 0.07;
            reducers = fittings.Fittings.OfType<Reducer>().ToList();
            Assert.True(reducers.Count() == 1);
            reducer = reducers[0];
            Assert.True(reducer.BranchSideTransform.Origin.IsAlmostEqualTo(new Vector3(0, 0, diameter / 2)));
            Assert.True(reducer.Transform.Origin.Z.ApproximatelyEquals(2));
            Assert.True(reducer.Start.Position.Z.ApproximatelyEquals(2 + diameter / 2));
            Assert.True(reducer.End.Position.Z.ApproximatelyEquals(2));

            Assert.True(reducer.BranchSideComponents.Count() == 1);
            Assert.IsType<StraightSegment>(reducer.BranchSideComponents[0]);
            pipe = (StraightSegment)reducer.BranchSideComponents[0];
            Assert.True(pipe.Start.Position.Z.ApproximatelyEquals(2 + diameter / 2));
            Assert.True(pipe.End.Position.Z.ApproximatelyEquals(2 + diameter / 2));

            Assert.True(1 == fittings.StraightSegments.Count);
            Assert.True(pipe.BranchSideComponents.Count == 1);
            Assert.True(pipe.BranchSideComponents[0] == branch);
            Assert.IsType<Reducer>(pipe.TrunkSideComponent);
            Reducer reducer2 = pipe.TrunkSideComponent as Reducer;
            Assert.True(reducer2.End.Diameter > reducer2.Start.Diameter);
            Assert.True(pipe.End == reducer2.Start);
            Assert.True(reducer2.Start.Position.DistanceTo(pipe.Start.Position) < reducer2.End.Position.DistanceTo(pipe.Start.Position));

        }

        [Fact]
        public void LShapedFittingTreeWithReducer()
        {
            Port.ShowArrows = true;

            var trunk = new Terminal(new Vector3(0, 0, 0), new Vector3(1, 1, 0), 0.2, 0.15, terminalMaterial);
            var elbow = new Elbow(new Vector3(3, 3, 0), Vector3.ZAxis, new Vector3(-1, -1, 0), 0.2, 0.08);
            var branch = new Terminal(new Vector3(3, 3, 3), new Vector3(0, 0, -1), 0.2, 0.08, terminalMaterial);
            trunk.BranchSideComponents.Add(elbow);
            elbow.BranchSideComponents.Add(branch);
            var fittings = FittingTree.Create(new EccentricRouting());

            fittings.AddConnection(trunk);
            fittings.AddConnection(elbow);
            fittings.AddConnection(branch);
            var errors = fittings.CreateStraightSegmentsBetweenFittings();
            Assert.Empty(errors);

            Assert.True(fittings.StraightSegments.Count == 2);

            Assert.True(trunk.BranchSideComponents.Count == 1);
            Assert.IsType<Reducer>(trunk.BranchSideComponents.First());
            var reducer = (Reducer)trunk.BranchSideComponents.First();

            double diameter = 0.07;
            Assert.True(reducer.BranchSideTransform.Origin.IsAlmostEqualTo(new Vector3(0, 0, diameter / 2)));
            Assert.True(reducer.Transform.Origin.Z.ApproximatelyEquals(0));
            Assert.True(reducer.Start.Position.Z.ApproximatelyEquals(diameter / 2));
            Assert.True(reducer.End.Position.Z.ApproximatelyEquals(0));

            Assert.True(reducer.BranchSideComponents.Count() == 1);
            Assert.IsType<StraightSegment>(reducer.BranchSideComponents[0]);
            var pipe = (StraightSegment)reducer.BranchSideComponents[0];
            Assert.True(pipe.Start.Position.Z.ApproximatelyEquals(diameter / 2));
            Assert.True(pipe.End.Position.Z.ApproximatelyEquals(diameter / 2));

            Assert.True(branch.Transform.Origin.IsAlmostEqualTo(new Vector3(3, 3, 3)));

            SaveToGltf(nameof(LShapedFittingTreeWithReducer), fittings, EccentricDirectory);
        }

        [Fact]
        public void LshapedFittingTreeWithReducerAndNegativePipeLength()
        {
            Port.ShowArrows = true;

            var trunk = new Terminal(new Vector3(0, 0, 0), new Vector3(1, 1, 0), 0.2, 0.15, terminalMaterial);
            var elbow = new Elbow(new Vector3(0.5, 0.5, 0), Vector3.ZAxis, new Vector3(-1, -1, 0), 0.2, 0.08);
            var branchHeigh = StraightSegment.MinLength + 0.2 /*terminal length*/
                + 0.2 /*elbow height*/ + 0.035 /*additional offset (0.15 - 0.8) / 2*/ - 0.02; /*to make terminal jump*/
            var branch = new Terminal(new Vector3(0.5, 0.5, branchHeigh),
                new Vector3(0, 0, -1), 0.2, 0.08, terminalMaterial);
            trunk.BranchSideComponents.Add(elbow);
            elbow.BranchSideComponents.Add(branch);
            var fittings = FittingTree.Create(new EccentricRouting());

            fittings.AddConnection(trunk);
            fittings.AddConnection(elbow);
            fittings.AddConnection(branch);
            var errors = fittings.CreateStraightSegmentsBetweenFittings();

            // Result fittings have no errors but branch terminal position is modified.
            // This is not throwing errors at the moment as too much other test are fine with it but this may change.
            Assert.True(errors.Count == 0);
            Assert.True(fittings.StraightSegments.Count == 1);
            Assert.Equal(branch.Port.Position.Z, branchHeigh - 0.2 /*terminal length*/ + 0.02);

            SaveToGltf(nameof(LshapedFittingTreeWithReducerAndNegativePipeLength), fittings, EccentricDirectory);
        }

        [Fact]
        public void BuildLFittingTreeFromTree()
        {
            var tree = new Tree(new[] { "test" });
            tree.SetOutletPosition((0, 0, 0));
            var i = tree.AddInlet((0, 4, 3), 0.1);
            var fromInlet = tree.GetOutgoingConnection(i);
            tree.SplitConnectionThroughPoint(fromInlet, (0, 2, 3), out var createdConns);
            var toOutlet = createdConns[1];
            tree.SplitConnectionThroughPoint(toOutlet, (0, 2, 0), out var createdConns2);

            tree.Connections.ToList().ForEach(c => c.Diameter = 0.09);
            // perform split in middle of downpipe
            tree.SplitConnectionThroughPoint(createdConns2[0], (0, 2, 2), out var createdConns3);
            createdConns3[1].Diameter = 0.07;

            var routing = new EccentricRouting() { UseDodgeAssembly = false };
            var fittings = routing.BuildFittingTree(tree, out var errors);
            SaveToGltf(nameof(BuildLFittingTreeFromTree), new Element[] { fittings }, EccentricDirectory);
            Assert.Empty(errors);

            CheckAllPipesVerticalOrHorizontal(fittings);
        }

        [Fact]
        public void LshapedFittingTreeTwoReducersAndReducerInVertical()
        {
            Port.ShowArrows = true;
            const double terminalLength = 0.2;
            var topTerminalHeight = 1;
            var elbowDiameter = 0.08;

            var eccentricRouting = new EccentricRouting();

            var trunk = new Terminal(new Vector3(0, 0, 0), new Vector3(1, 1, 0), terminalLength, 0.15, terminalMaterial);
            var elbow = new Elbow(new Vector3(3, 3, 0), Vector3.ZAxis, new Vector3(-1, -1, 0), 0.2, elbowDiameter);
            var branch = new Terminal(new Vector3(3, 3, topTerminalHeight), new Vector3(0, 0, -1), terminalLength, 0.16, terminalMaterial);

            var reducer = eccentricRouting.ReduceOrJoin(new StraightSegment(0, new Port(branch.Port.Position, branch.Port.Direction, elbowDiameter), elbow.Start),
                                                        false,
                                                        0.1,
                                                        topTerminalHeight - terminalLength - 0.5) as Reducer;
            reducer.Material = terminalMaterial;

            trunk.BranchSideComponents.Add(elbow);
            elbow.BranchSideComponents.Add(reducer);
            reducer.BranchSideComponents.Add(branch);

            var fittings = FittingTree.Create(eccentricRouting);
            fittings.AddConnection(trunk);
            fittings.AddConnection(elbow);
            fittings.AddConnection(reducer);
            fittings.AddConnection(branch);
            Assert.Equal(topTerminalHeight - terminalLength, branch.Port.Position.Z);

            var errors = fittings.CreateStraightSegmentsBetweenFittings();
            Assert.Empty(errors);

            Assert.Equal(topTerminalHeight - terminalLength, branch.Port.Position.Z);

            CheckAllPipesVerticalOrHorizontal(fittings);

            SaveToGltf(nameof(LshapedFittingTreeTwoReducersAndReducerInVertical), fittings, EccentricDirectory);
        }

        private static void CheckAllPipesVerticalOrHorizontal(FittingTree fittings)
        {
            foreach (var p in fittings.StraightSegments)
            {
                var zComponent = p.Path.Segments()[0]
                                         .Direction()
                                         .Cross(Vector3.ZAxis)
                                         .Length();
                var isHorizontal = zComponent.ApproximatelyEquals(0);
                var isVertical = zComponent.ApproximatelyEquals(1);
                Assert.True(isHorizontal || isVertical);
            }
        }

        [Fact]
        public void LshapedFittingTreeWithTwoReducersAndNegativePipeLength()
        {
            Port.ShowArrows = true;

            var trunk = new Terminal(new Vector3(0, 0, 0), new Vector3(1, 1, 0), 0.2, 0.15, terminalMaterial);
            var elbow = new Elbow(new Vector3(3, 3, 0), Vector3.ZAxis, new Vector3(-1, -1, 0), 0.2, 0.08);
            var branch = new Terminal(new Vector3(3, 3, 0.5), new Vector3(0, 0, -1), 0.2, 0.16, terminalMaterial);
            trunk.BranchSideComponents.Add(elbow);
            elbow.BranchSideComponents.Add(branch);
            var fittings = FittingTree.Create(new EccentricRouting());

            fittings.AddConnection(trunk);
            fittings.AddConnection(elbow);
            fittings.AddConnection(branch);
            var errors = fittings.CreateStraightSegmentsBetweenFittings();
            Assert.Empty(errors);

            SaveToGltf(nameof(LshapedFittingTreeWithTwoReducersAndNegativePipeLength), fittings, EccentricDirectory);
            Assert.True(trunk.BranchSideComponents.Count == 1);
            Assert.IsType<Reducer>(trunk.BranchSideComponents.First());
            var reducer = (Reducer)trunk.BranchSideComponents.First();

            double diameter = 0.07;
            Assert.True(reducer.BranchSideTransform.Origin.IsAlmostEqualTo(new Vector3(0, 0, diameter / 2)));
            Assert.True(reducer.Transform.Origin.Z.ApproximatelyEquals(0));
            Assert.True(reducer.Start.Position.Z.ApproximatelyEquals(diameter / 2));
            Assert.True(reducer.End.Position.Z.ApproximatelyEquals(0));

            Assert.True(reducer.BranchSideComponents.Count() == 1);
            Assert.IsType<StraightSegment>(reducer.BranchSideComponents[0]);
            var pipe = (StraightSegment)reducer.BranchSideComponents[0];
            Assert.True(pipe.Start.Position.Z.ApproximatelyEquals(diameter / 2));
            Assert.True(pipe.End.Position.Z.ApproximatelyEquals(diameter / 2));

            Assert.IsType<Reducer>(branch.TrunkSideComponent);
            reducer = (Reducer)branch.TrunkSideComponent;

            diameter = 0.08;
            Assert.True(reducer.BranchSideTransform.Origin.IsAlmostEqualTo(new Vector3(0, -diameter / 2, 0)));
            Assert.True(reducer.Transform.Origin.Y.ApproximatelyEquals(3));
            Assert.True(reducer.Start.Position.Y.ApproximatelyEquals(3 - diameter / 2));
            Assert.True(reducer.End.Position.Y.ApproximatelyEquals(3));

            Assert.IsType<Elbow>(reducer.TrunkSideComponent);
            elbow = (Elbow)reducer.TrunkSideComponent;
            Assert.True(elbow.Start.Position.Y.ApproximatelyEquals(3));
            Assert.True(reducer.End.Position.IsAlmostEqualTo(elbow.Start.Position));

            Assert.True(branch.Port.Position.Y.ApproximatelyEquals(3 - diameter / 2));
            Assert.True(branch.Port.Position.IsAlmostEqualTo(reducer.Start.Position));
            var eccentricRaise = (0.15 - 0.08) / 2;
            var reducerOverflow = reducer.Start.Position.Z - reducer.End.Position.Z - 0.1 + eccentricRaise; /*space left*/;
            Assert.True(branch.Port.Position.Z.ApproximatelyEquals(0.5 - 0.2/*terminal length*/ + reducerOverflow));
        }

        [Fact]
        public void FittingTreeWithTwoWyeConnectionsAndTwoReducers()
        {
            Port.ShowArrows = true;

            var trunk = new Terminal(new Vector3(0, 0, 0), new Vector3(0, 1, 0), 0.2, 0.15, terminalMaterial);
            var wye1 = new Wye(new Vector3(0, 1, 0), new Vector3(0, 1, 0), new Vector3(1, 1, 0), new WyeSettings(0.15, 0.1, 0.1, 0.2, 0.2, 0.2), BuiltInMaterials.Steel);
            trunk.BranchSideComponents.Add(wye1);
            var branch1 = new Terminal(new Vector3(1, 2, 0), new Vector3(-1, -1, 0), 0.2, 0.3, terminalMaterial);
            wye1.BranchSideComponents.Add(branch1);
            var wye2 = new Wye(new Vector3(0, 2, 0), new Vector3(0, 1, 0), new Vector3(1, 1, 0), new WyeSettings(0.1, 0.08, 0.1, 0.2, 0.2, 0.2), BuiltInMaterials.Steel);
            wye1.BranchSideComponents.Add(wye2);
            var branch2 = new Terminal(new Vector3(0, 3, 0), new Vector3(0, -1, 0), 0.2, 0.08, terminalMaterial);
            wye2.BranchSideComponents.Add(branch2);
            var branch3 = new Terminal(new Vector3(1, 3, 0), new Vector3(-1, -1, 0), 0.2, 0.16, terminalMaterial);
            wye2.BranchSideComponents.Add(branch3);

            var fittings = FittingTree.Create(new EccentricRouting());

            fittings.AddConnection(trunk);
            fittings.AddConnection(wye1);
            fittings.AddConnection(branch1);
            fittings.AddConnection(wye2);
            fittings.AddConnection(branch2);
            fittings.AddConnection(branch3);

            var errors = fittings.CreateStraightSegmentsBetweenFittings();
            Assert.Empty(errors);
            SaveToGltf(nameof(FittingTreeWithTwoWyeConnectionsAndTwoReducers), fittings, EccentricDirectory);

            double diameter = 0.2;
            var reducers = fittings.Fittings.OfType<Reducer>().ToList();
            Assert.True(reducers.Count() == 2);
            Assert.IsType<Reducer>(branch1.TrunkSideComponent);
            var reducer = (Reducer)branch1.TrunkSideComponent;
            Assert.True(reducer.BranchSideTransform.Origin.IsAlmostEqualTo(new Vector3(0, 0, -diameter / 2)));
            Assert.True(reducer.Transform.Origin.Z.ApproximatelyEquals(0));
            Assert.True(reducer.Start.Position.Z.ApproximatelyEquals(-diameter / 2));
            Assert.True(reducer.End.Position.Z.ApproximatelyEquals(0));

            Assert.IsType<StraightSegment>(reducer.TrunkSideComponent);
            var pipe = (StraightSegment)reducer.TrunkSideComponent;
            Assert.True(pipe.Start.Position.Z.ApproximatelyEquals(0));
            Assert.True(pipe.End.Position.Z.ApproximatelyEquals(0));

            diameter = 0.06;
            Assert.IsType<Reducer>(branch3.TrunkSideComponent);
            reducer = (Reducer)branch3.TrunkSideComponent;
            Assert.True(reducer.BranchSideTransform.Origin.IsAlmostEqualTo(new Vector3(0, 0, -diameter / 2)));
            Assert.True(reducer.Transform.Origin.Z.ApproximatelyEquals(0));
            Assert.True(reducer.Start.Position.Z.ApproximatelyEquals(-diameter / 2));
            Assert.True(reducer.End.Position.Z.ApproximatelyEquals(0));

            Assert.IsType<StraightSegment>(reducer.TrunkSideComponent);
            pipe = (StraightSegment)reducer.TrunkSideComponent;
            Assert.True(pipe.Start.Position.Z.ApproximatelyEquals(0));
            Assert.True(pipe.End.Position.Z.ApproximatelyEquals(0));

        }

        [Fact]
        public void HorizontalFittingTreeResize()
        {
            Port.ShowArrows = true;

            var trunk = new Terminal(new Vector3(0, 0, 0), new Vector3(0, 1, 0), 0.2, 0.15, terminalMaterial);
            var branch = new Terminal(new Vector3(0, 3, 0), new Vector3(0, -1, 0), 0.2, 0.15, terminalMaterial);
            trunk.BranchSideComponents.Add(branch);
            branch.TrunkSideComponent = trunk;

            var fittings = FittingTree.Create(new EccentricRouting());

            fittings.AddConnection(trunk);
            fittings.AddConnection(branch);

            var errors = fittings.CreateStraightSegmentsBetweenFittings();

            var pipe = branch.TrunkSideComponent as StraightSegment;
            Assert.Equal(pipe.Start.Position.Z, pipe.End.Position.Z);
            fittings.ResizePipe(pipe, 0.08);
            Assert.Equal(pipe.Start.Position.Z, pipe.End.Position.Z);
            fittings.CheckComponentLabeling();

            SaveToGltf(nameof(HorizontalFittingTreeResize), fittings, EccentricDirectory);
        }

        [Fact]
        public void HorizontalFittingTreeSplitTwiceAndResizePipeParts()
        {
            Port.ShowArrows = true;

            var trunk = new Terminal(new Vector3(0, 0, 0), new Vector3(0, 1, 0), 0.2, 0.15, terminalMaterial);
            var branch = new Terminal(new Vector3(0, 3, 0), new Vector3(0, -1, 0), 0.2, 0.10, terminalMaterial);
            trunk.BranchSideComponents.Add(branch);
            branch.TrunkSideComponent = trunk;

            var fittings = FittingTree.Create(new EccentricRouting());
            fittings.AddConnection(trunk);
            fittings.AddConnection(branch);
            var errors = fittings.CreateStraightSegmentsBetweenFittings();

            var pipe = branch.TrunkSideComponent as StraightSegment;

            SaveToGltf(nameof(HorizontalFittingTreeSplitTwiceAndResizePipeParts) + "_pre", fittings, EccentricDirectory);
            Assert.Equal(pipe.Start.Position.Z, pipe.End.Position.Z, 5);

            fittings.SplitPipe(pipe, 1, true, out var newPipe, out var error);
            fittings.SplitPipe(newPipe, 1, false, out var newPipe2, out var error2);
            SaveToGltf(nameof(HorizontalFittingTreeSplitTwiceAndResizePipeParts) + "_split", fittings, EccentricDirectory);
            Assert.Null(error);
            Assert.Null(error2);

            Assert.Equal(pipe.Start.Position.Z, pipe.End.Position.Z, 5);
            Assert.Equal(newPipe.Start.Position.Z, newPipe.End.Position.Z, 5);
            Assert.Equal(newPipe2.Start.Position.Z, newPipe2.End.Position.Z, 5);

            fittings.ResizePipe(newPipe2, 0.09);
            SaveToGltf(nameof(HorizontalFittingTreeSplitTwiceAndResizePipeParts) + "_first", fittings, EccentricDirectory);
            Assert.Equal(newPipe.Start.Position.Z, newPipe.BranchSideComponents.First().TrunkSidePort().Position.Z);
            Assert.Equal(pipe.Start.Position.Z, pipe.End.Position.Z, 5);
            Assert.Equal(newPipe.Start.Position.Z, newPipe.End.Position.Z, 5);
            Assert.Equal(newPipe2.Start.Position.Z, newPipe2.End.Position.Z, 5);

            fittings.ResizePipe(newPipe, 0.08);
            SaveToGltf(nameof(HorizontalFittingTreeSplitTwiceAndResizePipeParts) + "_second", fittings, EccentricDirectory);
            Assert.Equal(pipe.Start.Position.Z, pipe.End.Position.Z, 5);
            Assert.Equal(newPipe.Start.Position.Z, newPipe.End.Position.Z, 5);
            Assert.Equal(newPipe2.Start.Position.Z, newPipe2.End.Position.Z, 5);

            fittings.CheckComponentLabeling();
        }

        [Fact]
        public void HorizontalLshapedFittingTreeResizeTwice()
        {
            Port.ShowArrows = true;

            var trunk = new Terminal(new Vector3(0, 0, 0), new Vector3(1, 0, 0), 0.2, 0.15, terminalMaterial);
            var elbow = new Elbow(new Vector3(1.5, 0, 0), new Vector3(0, 1, 0), new Vector3(-1, 0, 0), 0.2, 0.08);
            var branch = new Terminal(new Vector3(1.5, 1, 0), new Vector3(0, -1, 0), 0.2, 0.16, terminalMaterial);

            trunk.BranchSideComponents.Add(elbow);
            elbow.BranchSideComponents.Add(branch);
            branch.TrunkSideComponent = elbow;
            var fittings = FittingTree.Create(new EccentricRouting());

            fittings.AddConnection(trunk);
            fittings.AddConnection(elbow);
            fittings.AddConnection(branch);
            var errors = fittings.CreateStraightSegmentsBetweenFittings();

            var trunkReducer = trunk.BranchSideComponents.First() as IReducer;
            Assert.NotNull(trunkReducer);
            var branchReducer = branch.TrunkSideComponent as IReducer;
            Assert.NotNull(branchReducer);
            Assert.Equal(trunk.Port.Position.Z, trunkReducer.End.Position.Z);
            Assert.Equal(elbow.End.Position.Z, trunkReducer.Start.Position.Z);
            Assert.Equal(branch.Port.Position.Z, branchReducer.Start.Position.Z);
            Assert.Equal(elbow.Start.Position.Z, branchReducer.End.Position.Z);

            var pipe = elbow.TrunkSideComponent as StraightSegment;
            fittings.ResizePipe(pipe, 0.05);
            fittings.ResizePipe(pipe, 0.075);

            SaveToGltf(nameof(HorizontalLshapedFittingTreeResizeTwice), fittings, EccentricDirectory);
            Assert.All(fittings.StraightSegments, s => s.Start.Position.Z.Equals(s.End.Position.Z));
        }

        [Theory]
        [InlineData(0.05)]
        [InlineData(0.2)]
        [InlineData(0.25)]
        public void VerticalFittingTreeCreation(double topDiameter)
        {
            var tree = new Tree(new[] { "Test" });
            var inlet = tree.AddInlet(new Vector3(0, 0, 3));
            tree.SetOutletPosition(new Vector3(0, 0, 0));

            var connection = tree.GetOutgoingConnection(inlet);
            tree.SplitConnectionThroughPoint(connection, new Vector3(0, 0, 1), out var newConnections);
            newConnections[0].Diameter = 0.2;
            newConnections[1].Diameter = 0.1;

            var routing = new EccentricRouting() { UseDodgeAssembly = false, FixedOutlet = topDiameter };
            var fittings = routing.BuildFittingTree(tree, out var errors);
            Assert.Empty(errors);
            SaveToGltf(nameof(VerticalFittingTreeCreation), fittings, EccentricDirectory);

            // pipe is vertical
            var pipe1 = fittings.StraightSegments.First();
            Assert.Equal(pipe1.Start.Position.X, pipe1.End.Position.X);
            Assert.Equal(pipe1.Start.Position.Y, pipe1.End.Position.Y);

            // pipe is vertical
            var pipe2 = fittings.StraightSegments.Last();
            Assert.Equal(pipe2.Start.Position.X, pipe2.End.Position.X);
            Assert.Equal(pipe2.Start.Position.Y, pipe2.End.Position.Y);

            // pipe1 and pipe2 should be offset
            Assert.NotEqual(pipe2.Start.Position.Y, pipe1.End.Position.Y);
            Assert.Equal(pipe2.Start.Position.X, pipe1.End.Position.X);
        }

        [Fact]
        public void VerticalFittingTreeResize()
        {
            Port.ShowArrows = true;

            var trunk = new Terminal(new Vector3(0, 0, 0), new Vector3(0, 0, 1), 0.2, 0.15, terminalMaterial);
            var branch = new Terminal(new Vector3(0, 0, 3), new Vector3(0, 0, -1), 0.2, 0.15, terminalMaterial);
            trunk.BranchSideComponents.Add(branch);
            branch.TrunkSideComponent = trunk;

            var fittings = FittingTree.Create(new EccentricRouting());

            fittings.AddConnection(trunk);
            fittings.AddConnection(branch);

            var errors = fittings.CreateStraightSegmentsBetweenFittings();
            fittings.ResizePipe(branch.TrunkSideComponent as StraightSegment, 0.08);
            fittings.CheckComponentLabeling();

            SaveToGltf(nameof(VerticalFittingTreeResize), fittings, EccentricDirectory);
        }

        [Fact]
        public void VerticalFittingTreeSplit()
        {
            Port.ShowArrows = true;

            var trunk = new Terminal(new Vector3(0, 0, 0), new Vector3(0, 0, 1), 0.2, 0.15, terminalMaterial);
            var branch = new Terminal(new Vector3(0, 0, 3), new Vector3(0, 0, -1), 0.2, 0.15, terminalMaterial);
            trunk.BranchSideComponents.Add(branch);
            branch.TrunkSideComponent = trunk;

            var fittings = FittingTree.Create(new EccentricRouting());

            fittings.AddConnection(trunk);
            fittings.AddConnection(branch);

            var errors = fittings.CreateStraightSegmentsBetweenFittings();
            var pipe = branch.TrunkSideComponent as StraightSegment;
            fittings.SplitPipe(pipe, pipe.Path.Length() * 0.6, false, out var newPipe, out var error);

            SaveToGltf(nameof(VerticalFittingTreeSplit), fittings, EccentricDirectory);
        }

        [Fact]
        public void VerticalFittingTreeSplitAndResize()
        {
            Port.ShowArrows = true;

            var trunk = new Terminal(new Vector3(0, 0, 0), new Vector3(0, 0, 1), 0.2, 0.15, terminalMaterial);
            var branch = new Terminal(new Vector3(0, 0, 3), new Vector3(0, 0, -1), 0.2, 0.15, terminalMaterial);
            trunk.BranchSideComponents.Add(branch);
            branch.TrunkSideComponent = trunk;

            var fittings = FittingTree.Create(new EccentricRouting());

            fittings.AddConnection(trunk);
            fittings.AddConnection(branch);

            var errors = fittings.CreateStraightSegmentsBetweenFittings();
            var pipe = branch.TrunkSideComponent as StraightSegment;
            fittings.SplitPipe(pipe, pipe.Path.Length() * 0.6, false, out var newPipe, out var error);
            fittings.ResizePipe(newPipe, 0.08);

            SaveToGltf(nameof(VerticalFittingTreeSplitAndResize), fittings, EccentricDirectory);
        }

        private class EccentricRouting : FittingTreeRouting
        {
            public EccentricRouting() : base(null)
            {
            }
            public bool UseDodgeAssembly = true;

            public double? FixedOutlet { get; internal set; }
            public override Fitting TerminatePipe(Connection incoming, Connection outgoing, out Node[] absorbedNodes)
            {
                absorbedNodes = new Node[0];
                if (incoming != null && outgoing != null)
                {
                    throw new Exception("Shouldn't create terminal for both incoming and outgoing");
                }
                if (outgoing != null)
                {
                    var diameter = FixedOutlet ?? (!outgoing.Diameter.ApproximatelyEquals(0) ? outgoing.Diameter : DefaultDiameter);
                    var terminal = new Terminal(outgoing.Start.Position, outgoing.Direction(), 0.1, diameter, DefaultFittingMaterial);
                    return terminal;
                }
                else if (incoming != null)
                {
                    var diameter = (!incoming.Diameter.ApproximatelyEquals(0) ? incoming.Diameter : DefaultDiameter);
                    var terminal = new Terminal(incoming.End.Position, incoming.Direction().Negate(), 0.1, diameter, DefaultFittingMaterial);
                    return terminal;
                }
                else
                {
                    throw new ArgumentNullException("Both connections to terminate were null");
                }
            }

            public override Fitting ChangePipe(Connection incoming, Connection outgoing)
            {
                if (UseDodgeAssembly)
                {
                    return new DodgeAssembly(incoming, outgoing);
                }
                else
                {
                    var start = new Port(incoming.Start.Position, incoming.Direction(), incoming.Diameter);
                    var end = new Port(outgoing.End.Position, incoming.Direction(), incoming.Diameter); // use incoming diameter for both while making pipe
                    var tempPipe = new StraightSegment(0, end, start);
                    var reducer = ReduceOrJoin(tempPipe, false, outgoing.Diameter, outgoing.Length());
                    return reducer as Fitting;
                }
            }


            public override IReducer ReduceOrJoin(StraightSegment pipe, bool invert, double newDiameter, double additionalDistance)
            {
                var length = 0.08;
                var reducer = Reducer.ReducerForPipe(pipe, length, invert, newDiameter, additionalDistance);
                var pipeDirection = (pipe.Start.Position - pipe.End.Position).Unitized();

                invert = false;
                double offset = invert ? reducer.Start.Diameter - reducer.End.Diameter : reducer.End.Diameter - reducer.Start.Diameter;
                offset /= 2.0;

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

        private static void SaveToGltf(string testName, Element[] elements, string directory = "./", bool saveDot = false)
        {
            var dir = TestUtils.GetTestPath(directory);
            var path = Path.Join(dir, $"{testName}.gltf");
            var model = new Model();
            var i = 0;
            foreach (var element in elements)
            {
                model.AddElement(element);
                if (saveDot && element is FittingTree net)
                {
                    var p = Path.Join(dir, $"{testName}_{i}.dot");
                    File.WriteAllText(p, net.ToDot());
                }
            }
            model.ToGlTF(path, false);
        }
        private static void SaveToGltf(string testName, Element element, string directory = "./", bool saveDot = false)
        {
            SaveToGltf(testName, new[] { element }, directory, saveDot);
        }
    }
}