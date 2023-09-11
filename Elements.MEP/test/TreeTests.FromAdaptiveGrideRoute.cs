using System;
using System.Collections.Generic;
using System.Text;
using Elements.Geometry;
using Elements.Spatial.AdaptiveGrid;
using Elements.Flow;
using Xunit;

namespace Elements.MEP.Tests
{
    public class TreeTests
    {
        [Fact]
        public static void ConstructTreeFromAdaptiveGridRoute()
        {
            //1. Setup the AdaptiveGrid.
            Polygon region = new Polygon(new List<Vector3>()
            {
                new Vector3(0, 0),
                new Vector3(10, 0),
                new Vector3(10, 10),
                new Vector3(0, 10)
            });

            var configuration = new RoutingConfiguration(turnCost: 1);

            var inputPoints = new List<Vector3>()
            {
                new Vector3(5, 2),
                new Vector3(5, 5),
                new Vector3(4, 8)
            };

            var tailPoint = new Vector3(5, 10);
            var keyPoints = new List<Vector3>();
            keyPoints.AddRange(inputPoints);
            keyPoints.Add(tailPoint);

            //TODO: remove the first point and investigate strange path
            //(5;2) -> (4;2) -> (4;5) -> (3;5) -> (3;6)
            var hintPolyline = new Polyline(new List<Vector3>()
            {
                new Vector3(6, 6),
                new Vector3(3, 6),
                new Vector3(3, 7),
                new Vector3(6, 7),

            });
            keyPoints.AddRange(hintPolyline.Vertices);

            var offsetPolyline = new Polyline(new List<Vector3>()
            {
                new Vector3(6, 2),
                new Vector3(6, 10),
                new Vector3(5, 10),
            });
            keyPoints.AddRange(offsetPolyline.Vertices);

            var hints = new List<RoutingHintLine>();
            hints.Add(new RoutingHintLine(hintPolyline,
                factor: 0.1, influence: 0.2, userDefined: true, is2D: true));
            hints.Add(new RoutingHintLine(offsetPolyline,
                factor: 0.5, influence: 0.1, userDefined: false, is2D: true));

            var box = new BBox3(new Vector3(3, 6, 0), new Vector3(7, 7, 3));
            var obstacle = Obstacle.FromBBox(box);
            keyPoints.AddRange(box.Corners());

            AdaptiveGrid grid = new AdaptiveGrid();
            grid.AddFromPolygon(region, keyPoints);
            grid.SubtractObstacle(obstacle);

            //2. Route between inputs and outputs.
            var inputVertices = new List<RoutingVertex>();
            foreach (var input in inputPoints)
            {
                Assert.True(grid.TryGetVertexIndex(input, out ulong id));
                {
                    inputVertices.Add(new RoutingVertex(id, 0.5));
                }
            }

            Assert.True(grid.TryGetVertexIndex(tailPoint, out ulong tailVertex));

            AdaptiveGraphRouting alg = new AdaptiveGraphRouting(grid, configuration);
            var route = alg.BuildSpanningTree(
                inputVertices, tailVertex, hints,
                TreeOrder.ClosestToFurthest);

            //3. Create and check tree.
            Tree tree = new Tree(new List<string> { "1" });
            var leaf0 = tree.AddInlet(new Vector3(5, 2));
            var leaf1 = tree.AddInlet(new Vector3(5, 5));
            var leaf2 = tree.AddInlet(new Vector3(4, 8));
            var trunk = tree.SetOutletPosition(new Vector3(5, 10));
            Assert.True(tree.ConnectByAdaptiveGridRoute(grid, route, out var failedLeafs));
            Assert.Empty(failedLeafs);

            var c = tree.GetOutgoingConnection(leaf0);
            Assert.Equal(new Vector3(6, 2), c.End.Position);
            c = tree.GetOutgoingConnection(c.End);
            Assert.Equal(new Vector3(6, 6), c.End.Position);
            c = tree.GetOutgoingConnection(c.End);
            Assert.Equal(new Vector3(5, 6), c.End.Position);

            var other = tree.GetOutgoingConnection(leaf1);
            Assert.Equal(other.End, c.End);

            c = tree.GetOutgoingConnection(c.End);
            Assert.Equal(new Vector3(3, 6), c.End.Position);
            c = tree.GetOutgoingConnection(c.End);
            Assert.Equal(new Vector3(3, 7), c.End.Position);
            c = tree.GetOutgoingConnection(c.End);
            Assert.Equal(new Vector3(4, 7), c.End.Position);

            other = tree.GetOutgoingConnection(leaf2);
            Assert.Equal(other.End, c.End);

            c = tree.GetOutgoingConnection(c.End);
            Assert.Equal(new Vector3(6, 7), c.End.Position);
            c = tree.GetOutgoingConnection(c.End);
            Assert.Equal(new Vector3(6, 10), c.End.Position);
            c = tree.GetOutgoingConnection(c.End);
            Assert.Equal(new Vector3(5, 10), c.End.Position);
            Assert.Equal(c.End, trunk);
        }

        [Fact]
        public static void ConstructTreeFromAdaptiveGridRouteFails()
        {
            //1. Setup the AdaptiveGrid.
            Polygon region = new Polygon(new List<Vector3>()
            {
                new Vector3(0, 0),
                new Vector3(10, 0),
                new Vector3(10, 10),
                new Vector3(0, 10)
            });

            var configuration = new RoutingConfiguration(turnCost: 1);

            var inputPoints = new List<Vector3>()
            {
                new Vector3(5, 2),
                new Vector3(5, 5),
                new Vector3(4, 8)
            };

            var tailPoint = new Vector3(5, 10);
            var keyPoints = new List<Vector3>();
            keyPoints.AddRange(inputPoints);
            keyPoints.Add(tailPoint);

            AdaptiveGrid grid = new AdaptiveGrid();
            grid.AddFromPolygon(region, keyPoints);

            //2. Route between inputs and outputs.
            var inputVertices = new List<RoutingVertex>();
            foreach (var input in inputPoints)
            {
                Assert.True(grid.TryGetVertexIndex(input, out ulong id));
                {
                    inputVertices.Add(new RoutingVertex(id, 0.5));
                }
            }

            var outletPoint = new Vector3(6, 10);
            Assert.True(grid.TryGetVertexIndex(tailPoint, out ulong tailVertex));
            Assert.False(grid.TryGetVertexIndex(outletPoint, out _));

            var hints = new List<RoutingHintLine>();
            AdaptiveGraphRouting alg = new AdaptiveGraphRouting(grid, configuration);
            var route = alg.BuildSpanningTree(
                inputVertices, tailVertex, hints,
                TreeOrder.ClosestToFurthest);

            //3. Create and check tree.
            Tree tree = new Tree(new List<string> { "1" });
            tree.AddInlet(new Vector3(5, 2));
            tree.AddInlet(new Vector3(5, 5));
            tree.AddInlet(new Vector3(4, 8));
            // Outlet is not on the grid, ConnectByAdaptiveGridRoute fails for every inlet. 
            var trunk = tree.SetOutletPosition(outletPoint);
            Assert.False(tree.ConnectByAdaptiveGridRoute(grid, route, out var failedLeafs));
            Assert.Equal(3, failedLeafs.Count);
        }

        [Fact]
        public static void ConstructTreeFromAdaptiveGridRoutePartiallyFails()
        {
            //1. Setup the AdaptiveGrid.
            var inputPoints = new List<Vector3>()
            {
                new Vector3(5, 2),
                new Vector3(5, 5),
                new Vector3(4, 8)
            };
            var tailPoint = new Vector3(5, 10);

            AdaptiveGrid grid = new AdaptiveGrid();
            grid.AddVertices(new List<Vector3>
            {
                inputPoints[1],
                new Vector3(6, 5),
                new Vector3(6, 10),
                tailPoint
            }, AdaptiveGrid.VerticesInsertionMethod.Connect);

            grid.AddVertices(new List<Vector3>
            {
                inputPoints[0],
                new Vector3(6, 2),
                new Vector3(6, 5)
            }, AdaptiveGrid.VerticesInsertionMethod.Connect);

            //2. Route between inputs and outputs.
            // Only two inputs are routed.
            var inputVertices = new List<RoutingVertex>();
            Assert.True(grid.TryGetVertexIndex(inputPoints[0], out ulong id));
            inputVertices.Add(new RoutingVertex(id, 0.5));
            Assert.True(grid.TryGetVertexIndex(inputPoints[1], out id));
            inputVertices.Add(new RoutingVertex(id, 0.5));
            Assert.False(grid.TryGetVertexIndex(inputPoints[2], out _));

            Assert.True(grid.TryGetVertexIndex(tailPoint, out ulong tailVertex));

            var hints = new List<RoutingHintLine>();
            var configuration = new RoutingConfiguration(turnCost: 1);
            AdaptiveGraphRouting alg = new AdaptiveGraphRouting(grid, configuration);
            var route = alg.BuildSpanningTree(
                inputVertices, tailVertex, hints,
                TreeOrder.ClosestToFurthest);

            //3. Create and check tree.
            Tree tree = new Tree(new List<string> { "1" });
            var leaf0 = tree.AddInlet(new Vector3(5, 2));
            var leaf1 = tree.AddInlet(new Vector3(5, 5));
            var leaf2 = tree.AddInlet(new Vector3(4, 8));
            var trunk = tree.SetOutletPosition(new Vector3(5, 10));
            Assert.False(tree.ConnectByAdaptiveGridRoute(grid, route, out var failedLeafs));
            Assert.Single(failedLeafs);

            var c = tree.GetOutgoingConnection(leaf0);
            Assert.Equal(new Vector3(6, 2), c.End.Position);
            c = tree.GetOutgoingConnection(c.End);
            Assert.Equal(new Vector3(6, 5), c.End.Position);
            c = tree.GetOutgoingConnection(c.End);
            Assert.Equal(new Vector3(6, 10), c.End.Position);
            c = tree.GetOutgoingConnection(c.End);
            Assert.Equal(new Vector3(5, 10), c.End.Position);
            Assert.Equal(c.End, trunk);

            c = tree.GetOutgoingConnection(leaf1);
            Assert.Equal(new Vector3(6, 5), c.End.Position);
            c = tree.GetOutgoingConnection(c.End);
            Assert.Equal(new Vector3(6, 10), c.End.Position);
            c = tree.GetOutgoingConnection(c.End);
            Assert.Equal(new Vector3(5, 10), c.End.Position);
            Assert.Equal(c.End, trunk);

            c = tree.GetOutgoingConnection(leaf2);
            Assert.Null(c);
        }
    }
}
