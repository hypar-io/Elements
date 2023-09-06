using System.Collections.Generic;
using Elements;
using Elements.Fittings;
using Elements.Flow;
using Elements.Geometry;
using Elements.Serialization.glTF;
using Xunit;

namespace Elements.MEP.Tests
{
    public class Samples
    {

        [Fact]
        public void MakeTree()
        {
            var tree = new Tree(new[] { "Room-101" });
            tree.SetOutletPosition((0, -1, 0));

            var inlet1 = tree.AddInlet((-1, 1, 0), 1);
            var inlet2 = tree.AddInlet((-1, 5, 0), 2);
            var inlet3 = tree.AddInlet((1, 2, 0), 3);

            var connection1 = tree.GetOutgoingConnection(inlet1);
            var connection2 = tree.GetOutgoingConnection(inlet2);
            var node = tree.MergeConnectionsAtPoint(new List<Connection> { connection1, connection2 }, (0, 1, 0));

            // connection1 is a new connection after the previous change
            connection2 = tree.GetOutgoingConnection(inlet2);
            var connection3 = tree.GetOutgoingConnection(inlet3);

            tree.ShiftConnectionToNode(connection3, node);
            tree.MergeConnectionsAtPoint(new List<Connection> { connection2, connection3 }, (0, 2, 0));

            connection2 = tree.GetOutgoingConnection(inlet2);
            tree.NormalizeConnectionPath(connection2, Vector3.ZAxis, Vector3.XAxis, 90, Tree.NormalizationType.End);

            var routing = new FittingTreeRouting(tree);
            var fittings = routing.BuildFittingTree(out var errors);

            var mdl = new Model();
            mdl.AddElement(fittings);
            mdl.ToGlTF(TestUtils.GetTestPath() + "tree.gltf", false);
        }
    }
}