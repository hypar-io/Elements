using System;
using System.Collections.Generic;
using Elements;
using Elements.Flow;
using Elements.Geometry;
using Elements.Serialization.glTF;
using Xunit;

namespace Elements.MEP.Tests
{
    public partial class FlowTests
    {
        [Fact]
        public void NormalizeConnection()
        {
            var primaryAxis = Vector3.XAxis;
            var normal = Vector3.ZAxis;
            var origin = Vector3.Origin;

            var tree = new Tree(new List<string> { "sample" });
            tree.SetOutletPosition(new Vector3(0, 0, 0));

            // Case: The connection angle to primary is less than the fitting angle
            var angle = 70;
            var inlet = tree.AddInlet(new Vector3(2, 1, 0));
            var newConns = tree.NormalizeConnectionPath(tree.GetOutgoingConnection(inlet), normal, primaryAxis, angle);
            var resultAngle = newConns[0].Direction().AngleTo(newConns[1].Direction());
            Assert.Equal(angle, resultAngle, 4);

            // Case: connection is parallel to primary axis
            angle = 45;
            inlet = tree.AddInlet(new Vector3(1, 0, 0));
            newConns = tree.NormalizeConnectionPath(tree.GetOutgoingConnection(inlet), normal, primaryAxis, angle);
            Assert.Empty(newConns);

            // Case: where the connection is already at that angle relative to the primary axis.
            primaryAxis = new Vector3(1, 1, 0);
            angle = 45;
            inlet = tree.AddInlet(new Vector3(1, 1, 0));
            newConns = tree.NormalizeConnectionPath(tree.GetOutgoingConnection(inlet), normal, primaryAxis, angle);
            Assert.Empty(newConns);

            // Case:  The connection angle to primary is greater than fitting angle
            angle = 70;
            inlet = tree.AddInlet(new Vector3(1, 2, 0));
            newConns = tree.NormalizeConnectionPath(tree.GetOutgoingConnection(inlet), normal, primaryAxis, angle);
            resultAngle = newConns[0].Direction().AngleTo(newConns[1].Direction());
            Assert.Equal(angle, resultAngle, 4);

            // Case: connection in negative Y + X quadrant
            angle = 45;
            inlet = tree.AddInlet(new Vector3(1, -2, 0));
            newConns = tree.NormalizeConnectionPath(tree.GetOutgoingConnection(inlet), normal, primaryAxis, angle);
            resultAngle = newConns[0].Direction().AngleTo(newConns[1].Direction());
            Assert.Equal(angle, resultAngle, 4);

            // Case: connection is in Z,X plane
            angle = 70;
            primaryAxis = Vector3.ZAxis;
            normal = Vector3.YAxis;
            inlet = tree.AddInlet(new Vector3(1, 0, -2));
            newConns = tree.NormalizeConnectionPath(tree.GetOutgoingConnection(inlet), normal, primaryAxis, angle);
            resultAngle = newConns[0].Direction().AngleTo(newConns[1].Direction());
            Assert.Equal(angle, resultAngle, 4);

            var model = new Model();
            model.AddElement(tree);
            model.AddElements(tree.GetSections());

            model.AddElement(new ModelCurve(new Line(origin, primaryAxis, 10)));
            model.AddElement(new ModelCurve(new Line(origin, normal.Cross(primaryAxis), 10)));
            model.AddElement(new ModelCurve(new Line(origin, Vector3.XAxis, 10), BuiltInMaterials.XAxis));
            model.ToGlTF(TestUtils.GetTestPath() + "normalizedConnections.gltf", false);
        }

        [Fact]
        public void NormalizeConnectionStart()
        {
            var tree = new Tree(new List<string> { "sample" });
            tree.SetOutletPosition(new Vector3(0, 0, 0));
            var inlet = tree.AddInlet(new Vector3(0, 10, 2));
            var connection = tree.GetOutgoingConnection(inlet);
            tree.SplitConnectionThroughPoint(connection, new Vector3(0, 0, 2), out var newConnections);

            var path = new List<Vector3>
            {
                new Vector3(0, 10, 2),
                new Vector3(0, 8, 2),
                new Vector3(4, 5, 2),
                new Vector3(4, 3, 2),
                new Vector3(1, 2, 2),
                new Vector3(1, 0, 2),
                new Vector3(0, 0, 1.1),
                new Vector3(0, 0, 0)
            };
            Tree.AdjustPath(tree, "0", path);

            var c1 = tree.GetOutgoingConnection(inlet);
            var c2 = tree.GetOutgoingConnection(c1.End);
            var c3 = tree.GetOutgoingConnection(c2.End);
            var c4 = tree.GetOutgoingConnection(c3.End);
            var c5 = tree.GetOutgoingConnection(c4.End);
            var c6 = tree.GetOutgoingConnection(c5.End);
            var c7 = tree.GetOutgoingConnection(c6.End);


            //1 Get 45 degree between (0, 10 2), (0, 8, 2) and (4, 5, 2) shifting both connection.
            Assert.True(c2.Direction().AngleTo(c1.Direction()).ApproximatelyEquals(45));
            Assert.True(c3.Direction().AngleTo(c2.Direction()).ApproximatelyEquals(45));
            Assert.Equal(new Vector3(0, 8.5, 2), c2.Start.Position);
            Assert.Equal(new Vector3(4, 4.5, 2), c2.End.Position);

            //2 Get 90 degree between (4, 5, 2), (4, 3, 2) and (0, 2, 2) shifting both connections.
            Assert.True(c4.Direction().AngleTo(c3.Direction()).ApproximatelyEquals(90));
            Assert.True(c5.Direction().AngleTo(c4.Direction()).ApproximatelyEquals(90));
            Assert.Equal(new Vector3(4, 2.5, 2), c4.Start.Position);
            Assert.Equal(new Vector3(1, 2.5, 2), c4.End.Position);

            //3 Get 45 degree between (1, 2, 2), (1, 0, 2) and (0, 0, 1.1) shifting only outgoing connections.
            // Incoming connection is the same since change is not in its plane.
            Assert.True(c5.Direction().AngleTo(c6.Direction()).ApproximatelyEquals(90));
            Assert.True(c6.Direction().AngleTo(c7.Direction()).ApproximatelyEquals(45));
            Assert.Equal(new Vector3(1, 0, 2), c6.Start.Position);
            Assert.Equal(new Vector3(0, 0, 1), c6.End.Position);

            var model = new Model();
            model.AddElement(tree);
            model.AddElements(tree.GetSections());
            model.ToGlTF(TestUtils.GetTestPath() + "normalizedConnectionStart.gltf", false);
        }
    }
}