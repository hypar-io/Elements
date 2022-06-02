using Elements.Geometry;
using Elements.Tests;
using Xunit;

namespace Elements
{
    public class DimensionTests : ModelTest
    {
        [Fact]
        public void Dimension()
        {
            this.Name = "Elements_Dimension";

            var l = Polygon.L(5, 5, 1);
            this.Model.AddElement(new ModelCurve(l));
            var offset = l.Offset(0.25)[0];

            var segs = l.Segments();
            var offsetSegs = offset.Segments();

            for (var i = 0; i < segs.Length; i++)
            {
                var a = segs[i];
                var b = offsetSegs[i];
                var d = new LinearDimension(new Plane(Vector3.Origin, Vector3.ZAxis), a.Start, a.End, b);
                var draw = d.ToModelArrowsAndText();
                this.Model.AddElements(draw);
            }
        }
    }
}