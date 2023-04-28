using System.Linq;
using Elements.Geometry;
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
    }
}