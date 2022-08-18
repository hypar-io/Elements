using Elements.Geometry;
using Elements.Spatial.AdaptiveGrid;
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
            yield return new object[] { new Polyline(new Vector3(-10, 0), new Vector3(10, 0)), true};
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
    }
}
