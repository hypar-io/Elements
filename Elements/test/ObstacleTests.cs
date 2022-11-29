using Elements.Geometry;
using Elements.Serialization.glTF;
using Elements.Spatial.AdaptiveGrid;
using Elements.Tests;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Elements
{

    public class ObstacleTests : ModelTest
    {
        [Theory]
        [MemberData(nameof(GetIntersectsData))]
        public void IntersectsTestDefaultConstructor(Polyline polyline, bool expectedResult)
        {
            var rectangle = Polygon.Rectangle(10, 10);
            var obstacle = new Obstacle(rectangle, 10, 0, false, false, null);

            var result = obstacle.Intersects(polyline);

            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [MemberData(nameof(GetIntersectsData))]
        public void IntersectsTestDefaultConstructorWithOffset(Polyline polyline, bool expectedResult)
        {
            var rectangle = Polygon.Rectangle(8, 8).TransformedPolygon(new Transform(0, 0, 1));
            var obstacle = new Obstacle(rectangle, 8, 1, false, false, null);

            var result = obstacle.Intersects(polyline);

            Model.AddElement(obstacle);
            Model.AddElements(polyline.Segments().Select(x => new ModelCurve(x, BuiltInMaterials.XAxis)));

            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [MemberData(nameof(GetIntersectsData))]
        public void IntersectsTestFromBBox(Polyline polyline, bool expectedResult)
        {
            var bbox = new BBox3(new Vector3(-4, -4, 1), new Vector3(4, 4, 9));
            var obstacle = Obstacle.FromBBox(bbox, 1);

            var result = obstacle.Intersects(polyline);

            Model.AddElement(obstacle);
            Model.AddElements(polyline.Segments().Select(x => new ModelCurve(x, BuiltInMaterials.XAxis)));

            Assert.Equal(expectedResult, result);
        }

        public static IEnumerable<object[]> GetIntersectsData()
        {
            var smallPolygon = Polygon.Rectangle(5, 5);
            //Polygon fully inside
            yield return new object[] { smallPolygon.TransformedPolygon(new Transform(0, 0, 2)), true };
            //Polygon fully outside below
            yield return new object[] { smallPolygon.TransformedPolygon(new Transform(0, 0, -2)), false };
            //Polygon fully outside on side
            yield return new object[] { smallPolygon.TransformedPolygon(new Transform().Rotated(Vector3.YAxis, 90).Moved(7, 0, 5)), false };
            //Only one vertex inside
            yield return new object[] { smallPolygon.TransformedPolygon(new Transform(5, 5, 2)), true };
            //Vertex on perimeter
            yield return new object[] { smallPolygon.TransformedPolygon(new Transform(10, 10, 2)), false };

            var bigPolygon = Polygon.Rectangle(20, 20);
            //Obstacle inside polygon
            yield return new object[] { bigPolygon.TransformedPolygon(new Transform(0, 0, 2)), false };
            //One segment intersecting with obstacle
            yield return new object[] { bigPolygon.TransformedPolygon(new Transform(10, 0, 2)), true };

            //Polyline on bottom plane of obstacle
            yield return new object[] { new Polyline(new Vector3(-10, 0), new Vector3(10, 0)), true };
            //Polyline on top plane of obstacle
            yield return new object[] { new Polyline(new Vector3(-10, 0, 10), new Vector3(10, 0, 10)), true };
            //Polyline on bottom plane of obstacle, but not intersecting
            yield return new object[] { new Polyline
            (
                new Vector3(-10, 0),
                new Vector3(-10, 10),
                new Vector3(10, 10),
                new Vector3(10, 0)
            ), false};
            //Polyline on bottom plane of obstacle, but not intersecting
            yield return new object[] { new Polyline
            (
                new Vector3(-10, 0, -2),
                new Vector3(0, 0, -2),
                new Vector3(0, 0, 12),
                new Vector3(10, 0, 12)
            ), true};
            //Polyline on bottom plane of obstacle, but not intersecting
            yield return new object[] { new Polyline
            (
                new Vector3(-10, 0, -2),
                new Vector3(-6, 0, -2),
                new Vector3(-6, 0, 12),
                new Vector3(10, 0, 12)
            ), false};
        }

        [Fact]
        public void IntersectsRotatedPolygon()
        {
            var polygon = new Polygon(Vector3.Origin, new Vector3(5, 5), new Vector3(0, 10), new Vector3(-5, 5));
            var obstacle = new Obstacle(polygon, 5, 0, false, false, null);
            var polyline = new Polyline(new Vector3(-4, 6), new Vector3(-1, 9));

            var result = obstacle.Intersects(polyline);

            Assert.True(result);
        }

        [Fact]
        public void ObstacleFromWall()
        {
            Line centerLine = new Line(Vector3.Origin, new Vector3(10, 0));
            var wall = new StandardWall(centerLine, 0.5, 3);
            var obstacle = Obstacle.FromWall(wall, 0.5);

            // intersects wall line
            Assert.True(obstacle.Intersects(new Line(Vector3.Origin, new Vector3(10, 0))));
            // intersects line that crosses wall
            Assert.True(obstacle.Intersects(new Line(new Vector3(3, -1, 2), new Vector3(3, 1, 3))));
            // intersects line that crosses wall at the top
            Assert.True(obstacle.Intersects(new Line(new Vector3(3, -1, 3.5), new Vector3(3, 1, 3.5))));
            // does not intersect line that is above wall
            Assert.False(obstacle.Intersects(new Line(new Vector3(0, 0, 4), new Vector3(10, 0, 4))));

            // ensure that line direction does not matter
            centerLine = centerLine.Reversed();
            var wall2 = new StandardWall(centerLine, 0.5, 3);
            var obstacle2 = Obstacle.FromWall(wall2, 0.5);
        }

        [Fact]
        public void ObstacleFromWallHasCorrectOrientationInGrid()
        {
            var grid = new AdaptiveGrid();
            grid.AddFromPolygon(Polygon.Rectangle(new Vector3(0, 0), new Vector3(10, 10)),
                                new List<Vector3> { });
            Line centerLine = new Line(Vector3.Origin, new Vector3(10, 0));
            var horizontalWall = new StandardWall(centerLine, 1, 3);
            var horizontalObstacle = Obstacle.FromWall(horizontalWall, addPerimeterEdges: true);
            var expectedPoints = new List<Vector3>()
            {
                new Vector3(0, -0.5, 0),
                new Vector3(0, 0.5, 0),
                new Vector3(10, -0.5, 0),
                new Vector3(10, 0.5, 0),
                new Vector3(0, -0.5, 3),
                new Vector3(0, 0.5, 3),
                new Vector3(10, -0.5, 3),
                new Vector3(10, 0.5, 3),
            };
            Assert.Equal(horizontalObstacle.Points.Count, expectedPoints.Count);
            Assert.True(horizontalObstacle.Points.All(p => expectedPoints.Any(e => e.IsAlmostEqualTo(p))));
            grid.SubtractObstacle(horizontalObstacle);
            Assert.True(grid.TryGetVertexIndex(new Vector3(0, 0.5, 0), out var id));
            var vertex = grid.GetVertex(id);
            Assert.Contains(vertex.Edges, e => grid.GetVertex(e.OtherVertexId(id)).Point.IsAlmostEqualTo(new Vector3(new Vector3(10, 0.5, 0))));

            grid = new AdaptiveGrid();
            grid.AddFromPolygon(Polygon.Rectangle(new Vector3(0, 0), new Vector3(10, 10)),
                                new List<Vector3> { });
            centerLine = new Line(Vector3.Origin, new Vector3(0, 10));
            var verticalWall = new StandardWall(centerLine, 1, 3);
            var verticalObstacle = Obstacle.FromWall(verticalWall, addPerimeterEdges: true);
            expectedPoints = new List<Vector3>()
            {
                new Vector3(-0.5, 0, 0),
                new Vector3(0.5, 0, 0),
                new Vector3(-0.5, 10, 0),
                new Vector3(0.5, 10, 0),
                new Vector3(-0.5, 0, 3),
                new Vector3(0.5, 0, 3),
                new Vector3(-0.5, 10, 3),
                new Vector3(0.5, 10, 3),
            };
            Assert.Equal(verticalObstacle.Points.Count, expectedPoints.Count);
            Assert.True(verticalObstacle.Points.All(p => expectedPoints.Any(e => e.IsAlmostEqualTo(p))));
            grid.SubtractObstacle(verticalObstacle);
            Assert.True(grid.TryGetVertexIndex(new Vector3(0.5, 0, 0), out id));
            vertex = grid.GetVertex(id);
            Assert.Contains(vertex.Edges, e => grid.GetVertex(e.OtherVertexId(id)).Point.IsAlmostEqualTo(new Vector3(new Vector3(0.5, 10, 0))));

            grid = new AdaptiveGrid();
            grid.AddFromPolygon(Polygon.Rectangle(new Vector3(0, 0), new Vector3(10, 10)),
                                new List<Vector3> { });
            centerLine = new Line(new Vector3(0.3535534, 0.3535534), new Vector3(9.64645, 9.64645));
            var diagonalWall = new StandardWall(centerLine, 1, 3);
            var diagonalObstacle = Obstacle.FromWall(diagonalWall, addPerimeterEdges: true);
            expectedPoints = new List<Vector3>()
            {
                new Vector3(0, 0.70711, 0),
                new Vector3(0.70711, 0, 0),
                new Vector3(10, 9.292891, 0),
                new Vector3(9.292891, 10, 0),
                new Vector3(0, 0.70711, 3),
                new Vector3(0.70711, 0, 3),
                new Vector3(10, 9.292891, 3),
                new Vector3(9.292891, 10, 3),
            };
            Assert.Equal(diagonalObstacle.Points.Count, expectedPoints.Count);
            Assert.True(diagonalObstacle.Points.All(p => expectedPoints.Any(e => e.IsAlmostEqualTo(p))));
            grid.SubtractObstacle(diagonalObstacle);
            Assert.True(grid.TryGetVertexIndex(new Vector3(0, 0.70711, 0), out id, grid.Tolerance));
            vertex = grid.GetVertex(id);
            Assert.Equal(4, vertex.Edges.Count());
            Assert.Contains(vertex.Edges, e => grid.GetVertex(e.OtherVertexId(id)).Point.IsAlmostEqualTo(new Vector3(9.292891, 10, 0)));
            Assert.True(grid.TryGetVertexIndex(new Vector3(0.70711, 0, 0), out id, grid.Tolerance));
            vertex = grid.GetVertex(id);
            Assert.Equal(4, vertex.Edges.Count());
            Assert.Contains(vertex.Edges, e => grid.GetVertex(e.OtherVertexId(id)).Point.IsAlmostEqualTo(new Vector3(10, 9.292891, 0)));
        }

        [Fact]
        public void IntersectsObstacleFromLine()
        {
            var offset = 0.1;
            var horizontalLine = new Line(Vector3.Origin, new Vector3(0, 10));
            var horizontalObstacle = Obstacle.FromLine(horizontalLine, offset);

            Assert.True(horizontalObstacle.Intersects(horizontalLine));

            var horizontalLineOnTop = horizontalLine.TransformedLine(new Transform(0, 0, offset));
            Assert.True(horizontalObstacle.Intersects(horizontalLineOnTop));

            var horizontalLineOnBottom = horizontalLine.TransformedLine(new Transform(0, 0, -offset));
            Assert.True(horizontalObstacle.Intersects(horizontalLineOnBottom));

            var horizontalLineOnSide = horizontalLine.TransformedLine(new Transform(offset, 0, 0));
            Assert.True(horizontalObstacle.Intersects(horizontalLineOnSide));

            var horizontalLineIntersecting = horizontalLine.TransformedLine(new Transform(0, offset, 0));
            Assert.True(horizontalObstacle.Intersects(horizontalLineIntersecting));

            var verticalLine = new Line(Vector3.Origin, new Vector3(0, 0, 10));
            var verticalObstacle = Obstacle.FromLine(verticalLine);

            Assert.True(verticalObstacle.Intersects(verticalLine));

            var verticalLineOnMainTop = verticalLine.TransformedLine(new Transform(0, offset, 0));
            Assert.True(verticalObstacle.Intersects(verticalLineOnMainTop));

            var verticalLineOnMainBottom = verticalLine.TransformedLine(new Transform(0, -offset, 0));
            Assert.True(verticalObstacle.Intersects(verticalLineOnMainBottom));

            var lineIntersectingVerticalObstacle = verticalLine.TransformedLine(new Transform().RotatedAboutPoint(new Vector3(0, 0, 5), Vector3.YAxis, 30));
            Assert.True(verticalObstacle.Intersects(lineIntersectingVerticalObstacle));

            var angledLine = new Line(Vector3.Origin, new Vector3(10, 10, 10));
            var angledObstacle = Obstacle.FromLine(angledLine);

            Assert.True(angledObstacle.Intersects(angledLine));

            var lineIntersectingAngleObstacle = new Line(new Vector3(5, 5), new Vector3(5, 5, 10));
            Assert.True(angledObstacle.Intersects(lineIntersectingAngleObstacle));

            var offsettedLine = angledLine.TransformedLine(new Transform(offset, 0, 0));
            Assert.True(angledObstacle.Intersects(offsettedLine));
        }

        [Fact]
        public void ObstacleFromLineHasCorrectOrientationInGrid()
        {
            var grid = new AdaptiveGrid();
            grid.AddFromPolygon(Polygon.Rectangle(new Vector3(0, 0), new Vector3(10, 10)),
                                new List<Vector3> { });
            Line centerLine = new Line(new Vector3(9, 0), new Vector3(1, 0));
            var horizontalObstacle = Obstacle.FromLine(centerLine, 0.5, addPerimeterEdges: true);
            var expectedPoints = new List<Vector3>()
            {
                new Vector3(0.5, -0.5, -0.5),
                new Vector3(0.5, 0.5, -0.5),
                new Vector3(9.5, -0.5, -0.5),
                new Vector3(9.5, 0.5, -0.5),
                new Vector3(0.5, -0.5, 0.5),
                new Vector3(0.5, 0.5, 0.5),
                new Vector3(9.5, -0.5, 0.5),
                new Vector3(9.5, 0.5, 0.5)
            };
            Assert.Equal(horizontalObstacle.Points.Count, expectedPoints.Count);
            Assert.True(horizontalObstacle.Points.All(p => expectedPoints.Any(e => e.IsAlmostEqualTo(p))));
            grid.SubtractObstacle(horizontalObstacle);
            Assert.True(grid.TryGetVertexIndex(new Vector3(0.5, 0.5, 0), out var id));
            var vertex = grid.GetVertex(id);
            Assert.Contains(vertex.Edges, e => grid.GetVertex(e.OtherVertexId(id)).Point.IsAlmostEqualTo(new Vector3(new Vector3(9.5, 0.5, 0))));
            Assert.Contains(vertex.Edges, e => grid.GetVertex(e.OtherVertexId(id)).Point.IsAlmostEqualTo(new Vector3(new Vector3(0.5, 0, 0))));

            grid = new AdaptiveGrid();
            grid.AddFromPolygon(Polygon.Rectangle(new Vector3(0, 0), new Vector3(10, 10)),
                                new List<Vector3> { });
            centerLine = new Line(new Vector3(0, 1), new Vector3(0, 9));
            var perpendicularObstacle = Obstacle.FromLine(centerLine, 0.5, addPerimeterEdges: true);
            expectedPoints = new List<Vector3>()
            {
                new Vector3(-0.5, 0.5, -0.5),
                new Vector3(0.5, 0.5, -0.5),
                new Vector3(-0.5, 9.5, -0.5),
                new Vector3(0.5, 9.5, -0.5),
                new Vector3(-0.5, 0.5, 0.5),
                new Vector3(0.5, 0.5, 0.5),
                new Vector3(-0.5, 9.5, 0.5),
                new Vector3(0.5, 9.5, 0.5),
            };
            Assert.Equal(perpendicularObstacle.Points.Count, expectedPoints.Count);
            Assert.True(perpendicularObstacle.Points.All(p => expectedPoints.Any(e => e.IsAlmostEqualTo(p))));
            grid.SubtractObstacle(perpendicularObstacle);
            Assert.True(grid.TryGetVertexIndex(new Vector3(0.5, 0.5, 0), out id));
            vertex = grid.GetVertex(id);
            Assert.Contains(vertex.Edges, e => grid.GetVertex(e.OtherVertexId(id)).Point.IsAlmostEqualTo(new Vector3(new Vector3(0.5, 9.5, 0))));
            Assert.Contains(vertex.Edges, e => grid.GetVertex(e.OtherVertexId(id)).Point.IsAlmostEqualTo(new Vector3(new Vector3(0, 0.5, 0))));

            grid = new AdaptiveGrid();
            var boundary = Polygon.Rectangle(new Vector3(0, 0), new Vector3(10, 10));
            grid.AddFromPolygon(boundary, new List<Vector3> { });
            grid.Boundaries = boundary;
            centerLine = new Line(new Vector3(0, 0), new Vector3(10, 10));
            var diagonalObstacle = Obstacle.FromLine(centerLine, 0.5, addPerimeterEdges: true, allowOutsideBoundary: false);
            expectedPoints = new List<Vector3>()
            {
                new Vector3(0, -0.70711, -0.5),
                new Vector3(-0.70711, 0, -0.5),
                new Vector3(10, 10.70711, -0.5),
                new Vector3(10.70711, 10, -0.5),
                new Vector3(0, -0.70711, 0.5),
                new Vector3(-0.70711, 0, 0.5),
                new Vector3(10, 10.70711, 0.5),
                new Vector3(10.70711, 10, 0.5),
            };
            Assert.Equal(diagonalObstacle.Points.Count, expectedPoints.Count);
            Assert.True(diagonalObstacle.Points.All(p => expectedPoints.Any(e => e.IsAlmostEqualTo(p))));
            grid.SubtractObstacle(diagonalObstacle);
            Assert.True(grid.TryGetVertexIndex(new Vector3(0, 0.70711, 0), out id, grid.Tolerance));
            vertex = grid.GetVertex(id);
            Assert.Contains(vertex.Edges, e => grid.GetVertex(e.OtherVertexId(id)).Point.IsAlmostEqualTo(new Vector3(9.292891, 10, 0)));
            Assert.True(grid.TryGetVertexIndex(new Vector3(0.70711, 0, 0), out id, grid.Tolerance));
            vertex = grid.GetVertex(id);
            Assert.Contains(vertex.Edges, e => grid.GetVertex(e.OtherVertexId(id)).Point.IsAlmostEqualTo(new Vector3(10, 9.292891, 0)));

            grid = new AdaptiveGrid(new Transform().Rotated(Vector3.ZAxis, 45));
            boundary = new Polygon(new List<Vector3> { new Vector3(0, 0), new Vector3(-5, 5), new Vector3(0, 10), new Vector3(5, 5) });
            grid.AddFromExtrude(boundary, Vector3.ZAxis, 1, new List<Vector3> { new Vector3(0, 0) });
            centerLine = new Line(new Vector3(0, 0, 2), new Vector3(0, 0));
            var verticalObstacle = Obstacle.FromLine(centerLine, 0.5, addPerimeterEdges: true);
            expectedPoints = new List<Vector3>()
            {
                new Vector3(0.5, 0.5, -0.5),
                new Vector3(-0.5, 0.5, -0.5),
                new Vector3(-0.5, -0.5, -0.5),
                new Vector3(0.5, -0.5, -0.5),
                new Vector3(0.5, 0.5, 2.5),
                new Vector3(-0.5, 0.5, 2.5),
                new Vector3(-0.5, -0.5, 2.5),
                new Vector3(0.5, -0.5, 2.5),
            };
            Assert.Equal(verticalObstacle.Points.Count, expectedPoints.Count);
            Assert.True(verticalObstacle.Points.All(p => expectedPoints.Any(e => e.IsAlmostEqualTo(p))));
            grid.SubtractObstacle(verticalObstacle);
            Assert.True(grid.TryGetVertexIndex(new Vector3(0.5, 0.5, 0), out id, grid.Tolerance));
            vertex = grid.GetVertex(id);
            Assert.Contains(vertex.Edges, e => grid.GetVertex(e.OtherVertexId(id)).Point.IsAlmostEqualTo(new Vector3(1, 0, 0)));
            Assert.Contains(vertex.Edges, e => grid.GetVertex(e.OtherVertexId(id)).Point.IsAlmostEqualTo(new Vector3(0, 1, 0)));
        }

        [Fact]
        public void ObstacleFromColumnHasCorrectOrientationInGrid()
        {
            var grid = new AdaptiveGrid();
            grid.AddFromPolygon(Polygon.Rectangle(new Vector3(0, 0), new Vector3(10, 10)),
                                new List<Vector3> { new Vector3(5, 5) });
            var center = new Vector3(5, 5);
            var profile = Polygon.Rectangle(1, 1);
            Column column = new Column(center, 1, null, profile);
            var obstacle = Obstacle.FromColumn(column, 0.1, addPerimeterEdges: true);
            var expectedPoints = new List<Vector3>()
            {
                new Vector3(5.6, 5.6, -0.1),
                new Vector3(5.6, 4.4, -0.1),
                new Vector3(4.4, 4.4, -0.1),
                new Vector3(4.4, 5.6, -0.1),
                new Vector3(5.6, 5.6, 1.1),
                new Vector3(5.6, 4.4, 1.1),
                new Vector3(4.4, 4.4, 1.1),
                new Vector3(4.4, 5.6, 1.1),
            };

            Assert.Equal(obstacle.Points.Count, expectedPoints.Count);
            Assert.True(obstacle.Points.All(p => expectedPoints.Any(e => e.IsAlmostEqualTo(p))));
            grid.SubtractObstacle(obstacle);
            Assert.True(grid.TryGetVertexIndex(new Vector3(5.6, 5, 0), out var id));
            var vertex = grid.GetVertex(id);
            Assert.Equal(3, vertex.Edges.Count());
            Assert.Contains(vertex.Edges, e => grid.GetVertex(e.OtherVertexId(id)).Point.IsAlmostEqualTo(new Vector3(new Vector3(5.6, 5.6, 0))));
            Assert.Contains(vertex.Edges, e => grid.GetVertex(e.OtherVertexId(id)).Point.IsAlmostEqualTo(new Vector3(new Vector3(5.6, 4.4, 0))));
        }
    }
}
