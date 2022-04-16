using System;
using System.Collections.Generic;
using System.Linq;
using Elements.Geometry;
using Elements.Geometry.Profiles;
using Elements.Geometry.Solids;
using System.Text.Json.Serialization;
using Xunit;

namespace Elements.Tests
{
    public class BoxTests : ModelTest
    {
        [Fact, Trait("Category", "Examples")]
        public void Box()
        {
            Name = "Elements_Geometry_Box";
            // <example>
            var worldOrigin = new Transform().ToModelCurves();
            var rotation = new Transform().Rotated(Vector3.ZAxis, 45);
            var box = new Box((0, 0, 0), (6, 0, 2), rotation);
            var modelCurves = box.ToModelCurves();
            Model.AddElements(modelCurves);
            Model.AddElements(worldOrigin);
            // </example>
        }

        [Fact]
        public void BoxFromPoints()
        {
            Name = nameof(BoxFromPoints);
            var points = new List<Vector3>();
            var random = new Random(10);
            for (int i = 0; i < 100; i++)
            {
                points.Add(random.NextVector(new BBox3((4, 4, 4), (9, 9, 9))));
            }
            var modelPoints = new ModelPoints(points);
            Model.AddElement(modelPoints);
            for (int i = 0; i < 10; i++)
            {
                var transform = new Transform(random.NextVector(), random.NextVector());
                var box = new Box(points, transform);
                var mat = random.NextMaterial();
                var modelCurves = box.ToModelCurves(mat);
                Model.AddElements(modelCurves);
            }
        }

        [Fact, Trait("Category", "Examples")]
        public void BoxMapping()
        {
            Name = nameof(BoxMapping);
            // <example>
            var box1 = new Box((-1, -1, -1), (1, 1, 1));
            var box2 = new Box((6, 7, 8), (9, 12, 15), new Transform().Rotated(new Vector3(1, 1, 1).Unitized(), 30));

            var circle = new Circle((0, 0, 0), 1);
            var polygon = circle.ToPolygon(20);
            var mappedPolygon = polygon.TransformedPolygon(Elements.Geometry.Box.TransformBetween(box1, box2));
            // </example>
            Model.AddElements(box1.ToModelCurves(BuiltInMaterials.ZAxis));
            Model.AddElements(box2.ToModelCurves(BuiltInMaterials.ZAxis));
            Model.AddElements(polygon);
            Model.AddElements(mappedPolygon);

            var mappedBack = mappedPolygon.TransformedPolygon(Elements.Geometry.Box.TransformBetween(box2, box1));
            Assert.Equal(polygon.Area(), mappedBack.Area(), 5);
        }

        [Fact]
        public void PointMapping()
        {
            Name = nameof(PointMapping);
            var box = new Box((0, 0), (0, 10, 10), new Transform().Rotated(new Vector3(1, 1, 1).Unitized(), 30));
            Model.AddElements(box.ToModelCurves());
            Model.AddElements(new Transform().ToModelCurves());
            Model.AddElements(new Line((0, 0), (0, 10, 10)));
            var coordLines = new Polyline((0, 0), (0.5, 0), (0.5, 0.5), (0.5, 0.5, 0.5));
            Model.AddElement(coordLines.TransformedPolyline(box.UVWToBox()));
            Assert.Equal(new Vector3(0.5, 0.5, 0.5), box.UVWAtPoint((0, 5, 5)));
        }
    }
}