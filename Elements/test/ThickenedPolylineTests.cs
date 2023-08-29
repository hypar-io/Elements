using Elements.Tests;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Elements.Geometry;
using Xunit.Sdk;
using System;

namespace Elements.Geometry.Tests
{
    public class ThickenedPolylineTests : ModelTest
    {
        public ThickenedPolylineTests()
        {
            this.GenerateIfc = false;
        }

        [Fact, Trait("Category", "Examples")]
        public void ThickenedPolylineExample()
        {
            this.Name = "Elements_Geometry_ThickenedPolyline";

            // <example>
            // Construct a single polyline and thicken it on both sides.
            var a = new Vector3();
            var b = new Vector3(10, 10);
            var c = new Vector3(20, 5);
            var d = new Vector3(25, 10);

            var singleThickenedPolyline = new ThickenedPolyline(new[] { a, b, c, d }, 0.5, 0.5);
            var polygons = singleThickenedPolyline.GetPolygons();

            // Construct multiple thickened segments and compute their polygons.
            var segmentsWithDifferentThickness = new[] {
                new ThickenedPolyline(new Line((0, 15), (0, 25)), 0.5, 0),
                new ThickenedPolyline(new Line((0, 25), (10, 35)), 0.5, 0.25),
                new ThickenedPolyline(new Line((0, 25), (-10, 25)), 0.5, 0.5),
                new ThickenedPolyline(new Line((0, 15), (-10, 15)), 0, 1),
                new ThickenedPolyline(new Line((-10, 15), (-10, 25)), 1, 1),
            };
            var polygons2 = ThickenedPolyline.GetPolygons(segmentsWithDifferentThickness);

            // </example>

            Model.AddElement(new ModelCurve(singleThickenedPolyline.Polyline, BuiltInMaterials.XAxis, transform: new Transform(0, 0, 0.1)));
            foreach (var p in polygons)
            {
                Model.AddElement(new ModelCurve(p.offsetPolygon, BuiltInMaterials.YAxis));
            }

            foreach (var s in segmentsWithDifferentThickness)
            {
                Model.AddElement(new ModelCurve(s.Polyline, BuiltInMaterials.XAxis, transform: new Transform(0, 0, 0.1)));
            }

            foreach (var p in polygons2)
            {
                Model.AddElement(new ModelCurve(p.offsetPolygon, BuiltInMaterials.YAxis));
            }
        }

        [Fact]
        public void CorrectCornerConditions()
        {
            Name = nameof(CorrectCornerConditions);
            for (double thickness = 0.1; thickness < 2.5; thickness += 0.25)
            {
                var segmentA = new ThickenedPolyline(new Line((0, 0), (-10, 0)), thickness / 2, thickness / 2);
                for (double angle = 10; angle < 360; angle += 10)
                {
                    var rotation = new Transform().Rotated(Vector3.ZAxis, angle);
                    var segmentB = new ThickenedPolyline(new Line((0, 0), (-10, 0)).TransformedLine(rotation), thickness / 2, thickness / 2);
                    var polygons = ThickenedPolyline.GetPolygons(new[] { segmentA, segmentB });
                    var displayTransform = new Transform(0, angle * 1.5, thickness);
                    Model.AddElements(polygons.Select(p => new ModelCurve(p.offsetPolygon, BuiltInMaterials.YAxis, displayTransform)));
                }
            }
        }

        [Fact]
        public void DifferentAlignmentsAtCorners()
        {
            Name = nameof(DifferentAlignmentsAtCorners);
            var a = new ThickenedPolyline(new Line((0, 0), (10, 0)), 0.5, 0.5);
            var b = new ThickenedPolyline(new Line((10, 0), (9.9, 10)), 1, 0);
            var c = new ThickenedPolyline(new Line((9.9, 10), (0, 10)), 0.5, 0.5);
            var d = new ThickenedPolyline(new Line((0, 10), (0, 0)), 0.5, 0.5);
            var abcd = new[] { a, b, c, d };
            var polygons = ThickenedPolyline.GetPolygons(abcd);
            Model.AddElements(polygons.Select(p => new ModelCurve(p.offsetPolygon, BuiltInMaterials.YAxis)));
            Model.AddElements(abcd.Select(p => new ModelCurve(p.Polyline, BuiltInMaterials.XAxis, new Transform(0, 0, 0.1))));
        }

        [Fact]
        public void TestCorners()
        {
            var containsExpectedVertices = (Vector3[] expectedVertices, IEnumerable<Vector3> v) =>
            {
                // We want to make sure that all the vertices we expect are
                // present in v. We don't care about cases where there are
                // vertices in v that are not in expected vertices.
                return expectedVertices.All(ev => v.Any(vv => vv.IsAlmostEqualTo(ev)));
            };

            // Single Line
            var tp1 = new ThickenedPolyline(new Line((0, 0, 0), (10, 0, 0)), 0.5, 0.5);
            var pgon = tp1.GetPolygons();
            Assert.Equal(1, pgon.Count);
            var expectedVertices = new Vector3[] { (0, 0.5), (10, 0.5), (10, -0.5), (0, -0.5) };
            Assert.True(containsExpectedVertices(expectedVertices, pgon[0].offsetPolygon.Vertices));

            // 90 degree L
            var tp2 = new ThickenedPolyline(new Polyline((0, 0, 0), (10, 0, 0), (10, 10, 0)), 0.5, 0.5);
            pgon = tp2.GetPolygons();
            Assert.Equal(2, pgon.Count);
            var expectedVertices2 = new Vector3[] { (9.5, 10.0), (10.5, 10.0), (9.5, 0.5), (10.5, -0.5), (0.0, -0.5), (0.0, 0.5) };
            Assert.True(containsExpectedVertices(expectedVertices2, pgon.SelectMany(p => p.offsetPolygon.Vertices)));

            // 90 degree L with different thicknesses
            var tps3 = new[] { new ThickenedPolyline(new Line((0, 0), (10, 0)), 0.5, 0.5), new ThickenedPolyline(new Line((10, 0), (10, 10)), 1, 1) };
            pgon = ThickenedPolyline.GetPolygons(tps3);
            Assert.Equal(2, pgon.Count);
            var expectedVertices3 = new Vector3[] { (11.0, 10.0), (9.0, 10.0), (11.0, -0.5), (9.0, 0.5), (0.0, -0.5), (0.0, 0.5) };
            Assert.True(containsExpectedVertices(expectedVertices3, pgon.SelectMany(p => p.offsetPolygon.Vertices)));

            // Acute Angle with no cap
            var tp4 = new ThickenedPolyline(new Polyline((0, 0, 0), (10, 0, 0), (0, 10, 0)), 0.5, 0.5);
            pgon = tp4.GetPolygons();
            Assert.Equal(2, pgon.Count);
            var expectedVertices4 = new Vector3[] { (-0.35355339, 9.64644660), (0.35355339, 10.35355339), (11.20710678, -0.5), (8.7928932, 0.5), (0, -0.5), (0, 0.5) };

            // Acute Angle with cap
            var tp5 = new ThickenedPolyline(new Polyline((0, 0, 0), (10, 0, 0), (0, 3, 0)), 0.5, 0.5);
            pgon = tp5.GetPolygons();
            Assert.Equal(2, pgon.Count);
            var expectedVertices5 = new Vector3[] { (0, 0.5), (0, 0), (0, -0.5), (10.5, -0.5), (10.5613, -0.0824), (6.5933, 0.5), (6.5933, 0.5), (10.5613, -0.0824), (10.6226, 0.3352), (0.1437, 3.4789), (0, 3), (-0.1437, 2.5211) };
        }
    }
}