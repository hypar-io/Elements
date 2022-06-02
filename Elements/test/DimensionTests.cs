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

            var segs = l.Segments();

            for (var i = 0; i < segs.Length; i++)
            {
                var a = segs[i];
                var b = segs[i].Offset(0.25, false);
                var d = new LinearDimension(new Plane(Vector3.Origin, Vector3.ZAxis), a.Start, a.End, b);
                var draw = d.ToModelArrowsAndText();
                this.Model.AddElements(draw);
            }
        }
    }
}