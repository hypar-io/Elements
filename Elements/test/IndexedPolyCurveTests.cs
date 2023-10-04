using System.Collections.Generic;
using System.Linq;
using Elements.Geometry;
using Newtonsoft.Json;
using Xunit;

namespace Elements.Tests
{
    public class IndexedPolycurveTests : ModelTest
    {
        [Fact]
        public void IndexedPolycurve()
        {
            Name = nameof(IndexedPolycurve);

            // <example>
            var arc = new Arc(new Vector3(2.5, 5), 2.5, 0, 180);

            var a = new Vector3(5, 0, 0);
            var b = new Vector3(5, 5, 0);
            var c = arc.Mid();
            var d = new Vector3(0, 5, 0);
            var e = Vector3.Origin;
            var vertices = new[] { a, b, c, d, e };
            var indices = new[]{
                new[]{0,1},
                new[]{1,2,3},
                new[]{3,4}
            };

            var pc = new IndexedPolycurve(vertices, indices);
            // </example>

            Model.AddElement(new ModelCurve(pc));

            var t = pc.TransformAt(1.5);
            Model.AddElements(t.ToModelCurves());

            var t1 = new Transform(Vector3.Origin, Vector3.XAxis);
            var pc1 = pc.TransformedPolycurve(t1);
            Model.AddElement(new ModelCurve(pc1));

            var t2 = pc1.TransformAt(1.5);
            Model.AddElements(t2.ToModelCurves());
        }

        [Fact]
        public void PolyCurveFromFillet()
        {
            Name = nameof(PolyCurveFromFillet);

            var shape3 = Polygon.Star(5, 3, 5);
            var contour3 = shape3.Fillet(0.5);
            Model.AddElement(new ModelCurve(contour3));

            foreach (var curve in contour3)
            {
                if (curve is Arc)
                {
                    var arc = (Arc)curve;
                    Model.AddElements(arc.BasisCurve.Transform.ToModelCurves());
                }
            }
        }

        [Fact]
        public void PolyCurveWithBackwardsArc()
        {
            Name = nameof(PolyCurveWithBackwardsArc);
            var line1 = new Line((0, 0), (10, 0));
            var line2 = new Line((10, 0), (10, 8));
            var arc3 = new Arc((10, 10), 2, 270, 180);
            var line4 = new Line((8, 10), (0, 10));
            var line5 = new Line((0, 10), (0, 0));
            var polycurve = new IndexedPolycurve(new List<BoundedCurve> { line1, line2, arc3, line4, line5 });
            Model.AddElement(new ModelCurve(polycurve, BuiltInMaterials.XAxis));
            var vectors = new List<(Vector3 location, Vector3 direction, double magnitude, Color? color)>();
            for (int i = 0; i < 100; i++)
            {
                var transform = polycurve.TransformAtNormalized(i / 100.0);
                var normal = transform.ZAxis.Negate();
                vectors.Add((transform.Origin, normal, 0.1, Colors.Magenta));
            }
            var ma = new ModelArrows(vectors);
            Model.AddElement(ma);
        }

        private IndexedPolycurve CreateTestPolycurve()
        {
            var arc = new Arc(new Vector3(2.5, 5), 2.5, 0, 180);
            var a = new Vector3(5, 0, 0);
            var b = new Vector3(5, 5, 0);
            var c = arc.Mid();
            var d = new Vector3(0, 5, 0);
            var e = Vector3.Origin;
            var vertices = new[] { a, b, c, d, e };
            var indices = new[]{
                new[]{0,1},
                new[]{1,2,3},
                new[]{3,4}
            };

            return new IndexedPolycurve(vertices, indices);
        }

        [Fact]
        public void ArcLength()
        {
            // An arc that looks like the middle of the test polycurve.
            var middleArc = new Arc(new Vector3(2.5, 5), 2.5, 0, 180);

            // Measure the arc length half way along one
            // leg, around the arc, and half way down the
            // other leg.
            var pc = CreateTestPolycurve();
            Assert.Equal(5 + middleArc.Length(), pc.ArcLength(0.5, 2.5));
        }

        [Fact]
        public void CurveCounts()
        {
            var pc = CreateTestPolycurve();
            Assert.Equal(3, pc.Count());

            var pline = new Polyline(pc.Vertices);
            Assert.Equal(pline.Vertices.Count - 1, pline.Count());

            var pgon = new Polygon(pc.Vertices);
            Assert.Equal(pgon.Vertices.Count, pgon.Count());
        }

        [Fact]
        public void EndPoints()
        {
            // Polycurve is not closed
            var pc = CreateTestPolycurve();
            Assert.NotEqual(pc.End, pc.Start);

            // Polyline is not closed
            var pline = new Polyline(pc.Vertices);
            Assert.NotEqual(pline.Last().End, pline.First().Start);

            // Polygon is closed
            var pgon = new Polygon(pc.Vertices);
            Assert.Equal(pgon.Last().End, pgon.First().Start);
        }

        [Fact]
        public void RenderVertices()
        {
            var pgon = Polygon.Ngon(5);
            var pline = new Polyline(pgon.Vertices);
            var pgonRVerts = pgon.RenderVertices();
            var plineRVerts = pline.RenderVertices();
            Assert.Equal(6, pgonRVerts.Count);
            Assert.Equal(5, plineRVerts.Count);
        }

        [Fact]
        public void Serialization()
        {
            var pc = CreateTestPolycurve();
            var json = JsonConvert.SerializeObject(pc);
            var pc2 = JsonConvert.DeserializeObject<IndexedPolycurve>(json);
            var mc1 = new ModelCurve(pc, BuiltInMaterials.XAxis);
            var mc2 = new ModelCurve(pc2, BuiltInMaterials.YAxis);
            Assert.Equal(pc.Vertices, pc2.Vertices);
            Assert.Equal(pc._bounds, pc2._bounds);
        }

        [Fact]
        public void Intersects()
        {
            var pc = CreateTestPolycurve();
            var polygon = new Polygon(new Vector3[]
            {
                (0, 5),
                (4, 9),
                (8, 5),
                (4, 1)
            });

            Assert.True(pc.Intersects(polygon, out var results));
            Assert.Equal(3, results.Count);
            Assert.Contains(new Vector3(0, 5), results);
            Assert.Contains(new Vector3(5, 2), results);
            Assert.Contains(new Vector3(2.5, 7.5), results);
        }

        [Fact]
        public void PreservesIndicesTransformed()
        {
            var pc = CreateTestPolycurve();
            var indices = pc.CurveIndices;
            var copy = pc.TransformedPolycurve(new Transform(10, 0, 0));
            var newIndicies = copy.CurveIndices;
            Assert.Equal(indices.Count, newIndicies.Count);
            for (int i = 0; i < indices.Count; i++)
            {
                Assert.Equal(indices[i].Count, newIndicies[i].Count);
                for (int j = 0; j < indices[i].Count; j++)
                {
                    Assert.Equal(indices[i][j], newIndicies[i][j]);
                }
            }
        }
    }
}