using Elements.Geometry;
using Elements.Spatial.AdaptiveGrid;
using Elements.Tests;
using System.Collections.Generic;
using Xunit;

namespace Elements
{
    public class ObstacleTests 
    {

        [Theory]
        [MemberData(nameof(GetIntersectsData))]
        public void IntersectsTest(Polyline polyline, bool expectedResult)
        {
            var rectangle = Polygon.Rectangle(10, 10);
            var obstacle = Obstacle.From2DPolygon(rectangle, 10);

            var result = obstacle.Intersects(polyline);

            Assert.Equal(expectedResult, result);
        }

        public static IEnumerable<object[]> GetIntersectsData()
        {
            var smallPolygon = Polygon.Rectangle(5, 5);
            //Polygon fully inside
            yield return new object[] { smallPolygon.TransformedPolygon(new Transform(0, 0, 2)), true };
            //Polygon fully outside
            yield return new object[] { smallPolygon.TransformedPolygon(new Transform(0, 0, -2)), false };
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
            //Polyline on top plane of obstacle
            yield return new object[] { new Polyline
            (
                new Vector3(-10, 0), 
                new Vector3(-10, 10), 
                new Vector3(10, 10), 
                new Vector3(10, 0)
            ), false };
        }

        [Fact]
        public void IntersectsRotatedPolygon()
        {
            var polygon = new Polygon(Vector3.Origin, new Vector3(5, 5), new Vector3(0, 10), new Vector3(-5, 5));
            var obstacle = Obstacle.From2DPolygon(polygon, 5);
            var polyline = new Polyline(new Vector3(-4, 6), new Vector3(-1, 9));

            var result = obstacle.Intersects(polyline);

            Assert.False(result);
        }

        [Fact]
        public void IsInsideTests()
        {
            var polygon = new Polygon(Vector3.Origin, new Vector3(5, 5), new Vector3(0, 10), new Vector3(-5, 5));
            var obstacle = Obstacle.From2DPolygon(polygon, 5);

            var vertexPoint = Vector3.Origin;
            Assert.False(obstacle.IsInside(vertexPoint));

            var verticalEdgePoint = new Vector3(0, 0, 3);
            Assert.False(obstacle.IsInside(verticalEdgePoint));

            var horizontalEdgePoint = new Vector3(-2, 2);
            Assert.False(obstacle.IsInside(horizontalEdgePoint));

            var bottomMiddlePoint = new Vector3(0, 5);
            Assert.True(obstacle.IsInside(bottomMiddlePoint));

            var pointBelow = new Vector3(0, 5, -2);
            Assert.False(obstacle.IsInside(pointBelow));

            var topMiddlePoint = new Vector3(0, 5, 5);
            Assert.True(obstacle.IsInside(topMiddlePoint));

            var pointAbove = new Vector3(0, 5, 7);
            Assert.False(obstacle.IsInside(pointAbove));

            var pointOnFace = new Vector3(2, 2, 2);
            Assert.False(obstacle.IsInside(pointOnFace));

            var pointInside = new Vector3(0, 2, 2);
            Assert.True(obstacle.IsInside(pointInside));

            var pointOutside = new Vector3(2, 0, 2);
            Assert.False(obstacle.IsInside(pointOutside));

            var flatObstacle = Obstacle.FromBBox(new BBox3(new Vector3(2, 2), new Vector3(4, 4)), perimeter: true);
            
            vertexPoint = new Vector3(2, 2);
            Assert.False(flatObstacle.IsInside(vertexPoint));

            var edgePoint = new Vector3(3, 4);
            Assert.False(flatObstacle.IsInside(edgePoint));

            pointInside = new Vector3(3, 3);
            Assert.True(flatObstacle.IsInside(pointInside));
        }

        [Fact]
        public void Test()
        {
            var flatObstacle = Obstacle.FromBBox(new BBox3(new Vector3(2, 2), new Vector3(4, 4)), perimeter: true);
            var pointInside = new Vector3(3, 3);

            Assert.True(flatObstacle.IsInside(pointInside));
        }
    }
}
