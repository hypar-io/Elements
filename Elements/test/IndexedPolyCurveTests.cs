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
    }
}