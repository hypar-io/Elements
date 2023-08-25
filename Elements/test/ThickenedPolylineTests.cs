using Elements.Tests;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Elements.Geometry;

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
                    Model.AddElements(polygons.Select(p => new ModelCurve(p.offsetPolygon, BuiltInMaterials.XAxis, displayTransform)));
                }
            }
        }
    }
}