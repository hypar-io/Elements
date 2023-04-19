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

            var line = new Line(Vector3.Origin, new Vector3(0, 5, 0));
            var line1 = new Line(new Vector3(5, 5, 0), new Vector3(5, 0, 0));
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
        }
    }
}