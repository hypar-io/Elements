using System.Collections.Generic;
using System.Linq;
using Elements.Geometry;
using Xunit;

namespace Elements.Tests
{
    public class SetOperationsTests
    {
        [Fact]
        public void OverlappingSets()
        {
            var a = Polygon.Rectangle(5, 5);
            var b = Polygon.Rectangle(5, 5).TransformedPolygon(new Transform((2.5, 2.5)));

            if (a.Intersects2d(b, out List<(Vector3 result, int, int)> results))
            {
                var locations = results.Select(r => r.result).ToList();
                a.Split(locations);
                b.Split(locations);
            }

            Assert.Equal(6, a.Vertices.Count);
            Assert.Equal(6, b.Vertices.Count);

            var set = SetOperations.ClassifySegments2d(a, b);

            Assert.Equal(4, set.Where(c => c.classification == SetClassification.AOutsideB).Count());
            Assert.Equal(4, set.Where(c => c.classification == SetClassification.BOutsideA).Count());
            Assert.Equal(2, set.Where(c => c.classification == SetClassification.AInsideB).Count());
            Assert.Equal(2, set.Where(c => c.classification == SetClassification.BInsideA).Count());
        }

        [Fact]
        public void DisjointSets()
        {
            var a = Polygon.Rectangle(5, 5);
            var b = Polygon.Rectangle(5, 5).TransformedPolygon(new Transform((10, 10)));

            var set = SetOperations.ClassifySegments2d(a, b);

            Assert.Equal(4, set.Where(c => c.classification == SetClassification.AOutsideB).Count());
            Assert.Equal(4, set.Where(c => c.classification == SetClassification.BOutsideA).Count());

            Assert.Empty(set.Where(c => c.classification == SetClassification.AInsideB));
            Assert.Empty(set.Where(c => c.classification == SetClassification.BInsideA));
        }

        [Fact]
        public void TouchingSets()
        {
            var a = Polygon.Rectangle(5, 5);
            var b = Polygon.Rectangle(5, 5).TransformedPolygon(new Transform((5, 5)));

            var set = SetOperations.ClassifySegments2d(a, b);

            Assert.Equal(4, set.Where(c => c.classification == SetClassification.AOutsideB).Count());
            Assert.Equal(4, set.Where(c => c.classification == SetClassification.BOutsideA).Count());

            Assert.Empty(set.Where(c => c.classification == SetClassification.AInsideB));
            Assert.Empty(set.Where(c => c.classification == SetClassification.BInsideA));
        }

        [Fact]
        public void AdjacentSets()
        {
            var a = Polygon.Rectangle(5, 5);
            var b = Polygon.Rectangle(5, 2.5).TransformedPolygon(new Transform((5, 5)));

            var set = SetOperations.ClassifySegments2d(a, b);

            Assert.Equal(4, set.Where(c => c.classification == SetClassification.AOutsideB).Count());
            Assert.Equal(4, set.Where(c => c.classification == SetClassification.BOutsideA).Count());

            Assert.Empty(set.Where(c => c.classification == SetClassification.AInsideB));
            Assert.Empty(set.Where(c => c.classification == SetClassification.BInsideA));
        }

        [Fact]
        public void OverlappingSetsFilterAndGraphCorrectly()
        {
            var a = Polygon.Rectangle(5, 5);
            var b = Polygon.Rectangle(5, 5).TransformedPolygon(new Transform((2.5, 2.5)));

            if (a.Intersects2d(b, out List<(Vector3 result, int, int)> results))
            {
                var locations = results.Select(r => r.result).ToList();
                a.Split(locations);
                b.Split(locations);
            }
            var set = SetOperations.ClassifySegments2d(a, b, ((Vector3, Vector3, SetClassification classification) e) =>
            {
                return e.classification == SetClassification.AOutsideB || e.classification == SetClassification.BInsideA || e.classification == SetClassification.AInsideB;
            });

            Assert.Empty(set.Where(c => c.classification == SetClassification.BOutsideA));

            var graph = SetOperations.BuildGraph(set, new[] { SetClassification.BInsideA });

            Assert.Equal(7, graph.Vertices.Count);

            var polys = graph.Polygonize();

            Assert.Equal(2, polys.Count);
        }

    }
}