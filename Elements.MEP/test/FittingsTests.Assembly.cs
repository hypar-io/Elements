
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
        [Fact]
        public void PipeAssemblyNetwork()
        {
            var tree = new Tree(new List<string> { "Test" });
            var inlet1 = tree.AddInlet(new Vector3(3, 6), 10);
            tree.SetOutletPosition(new Vector3(0, 1));
            var s = tree.SplitConnectionThroughPoint(tree.GetOutgoingConnection(inlet1), new Vector3(0, 3), out var conns);
            Assert.True(conns.Count() == 2);

            var s2 = tree.SplitConnectionThroughPoint(tree.GetOutgoingConnection(inlet1), new Vector3(0, 6), out var conns2);

            var topPlane = new Plane(new Vector3(0, 0, 0.5), Vector3.ZAxis);
            FittingTreeRouting alignedRouting = new AssemblyRouting();
            var fittings = alignedRouting.BuildFittingTree(tree, out var errors);
            Assert.Empty(errors);
            fittings.CheckComponentLabeling();
            SaveToGltf(nameof(PipeAssemblyNetwork), fittings);
        }

        [Fact]
        public void PipeAssemblyNetworkFailedPipe()
        {

            var tree = new Tree(new List<string> { "Test" });
            var inlet1 = tree.AddInlet(new Vector3(3, 6), 10);
            tree.SetOutletPosition(new Vector3(0, 1));
            var s = tree.SplitConnectionThroughPoint(tree.GetOutgoingConnection(inlet1), new Vector3(0, 3), out var conns);
            Assert.True(conns.Count() == 2);

            var s2 = tree.SplitConnectionThroughPoint(tree.GetOutgoingConnection(inlet1), new Vector3(0, 3.5), out var conns2);

            var topPlane = new Plane(new Vector3(0, 0, 0.5), Vector3.ZAxis);

            var fittings = new AssemblyRouting().BuildFittingTree(tree, out var errors);
            var connectionError = errors.OfType<FailedStraightSegment>().FirstOrDefault();
            Assert.True(errors.Count == 2);
            Assert.NotNull(connectionError);
            Assert.True(connectionError.Start is Assembly || connectionError.End is Assembly);
        }


        private class AssemblyRouting : FittingTreeRouting
        {
            public AssemblyRouting() : base(null)
            {
            }
            public override Fitting ChangePipe(Connection incoming, Connection outgoing)
            {
                return new DodgeAssembly(incoming, outgoing);
            }
        }
    }
    public class DodgeAssembly : Assembly
    {
        [Newtonsoft.Json.JsonConstructor]
        public DodgeAssembly(IList<Port> externalConnectors,
                             IList<Fitting> internalConnections,
                             IList<StraightSegment> internalPipes,
                             FittingLocator locator,
                             bool canBeMoved,
                             Transform transform,
                             Material material,
                             Representation representation,
                             bool isElementDefinition,
                             Guid id,
                             string name) : base(externalConnectors, internalConnections, internalPipes, canBeMoved, locator, transform, material, representation, isElementDefinition, id, name)
        {
        }

        public DodgeAssembly(Connection incoming, Connection outgoing) : base(incoming.End.Position, BuiltInMaterials.Default)
        {
            var boxDistance = 1.0;
            var diam = 0.2;
            var side = 0.05;
            var dodgeDirection = incoming.Direction().Cross(Vector3.ZAxis).Unitized();
            var corner1 = incoming.End.Position - (incoming.Direction() * boxDistance / 2);
            var corner2 = corner1 + dodgeDirection * boxDistance;
            var corner3 = corner2 + (incoming.Direction() * boxDistance);
            var corner4 = incoming.End.Position + (incoming.Direction() * boxDistance / 2);
            var elbow1 = new Elbow(corner1, incoming.Direction().Negate(), dodgeDirection, side, diam);
            var elbow2 = new Elbow(corner2, dodgeDirection.Negate(), incoming.Direction(), side, diam);
            var elbow3 = new Elbow(corner3, incoming.Direction().Negate(), dodgeDirection.Negate(), side, diam);
            var elbow4 = new Elbow(corner4, dodgeDirection, incoming.Direction(), side, diam);

            this.InternalFittings = new List<Fitting>();
            this.InternalSegments = new List<StraightSegment>();

            this.InternalFittings.Add(elbow1);
            this.InternalFittings.Add(elbow2);
            this.InternalFittings.Add(elbow3);
            this.InternalFittings.Add(elbow4);

            this.InternalSegments.Add(new StraightSegment(0, elbow2.Start, elbow1.End));
            this.InternalSegments.Add(new StraightSegment(0, elbow3.Start, elbow2.End));
            this.InternalSegments.Add(new StraightSegment(0, elbow4.Start, elbow3.End));

            this.ExternalPorts.Add(elbow1.Start);
            this.ExternalPorts.Add(elbow4.End);
        }
    }
}
