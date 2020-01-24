using Xunit;

namespace Elements.Geometry.Tests
{
    public class PolylineTests
    {
        [Fact]
        public void Polyline_Construct()
        {
            var a = new Vector3();
            var b = new Vector3(10, 10);
            var c = new Vector3(3, 5);
            var d = new Vector3(2, 1);

            var pline = new Polyline(new[] { a, b, c, d });
            Assert.Equal(4, pline.Vertices.Count);
            Assert.Equal(3, pline.Segments().Length);
        }

        [Fact]
        public void Polyline_ClosedOffset()
        {
            var length = 10;
            var offsetAmt = 1;
            var a = new Vector3();
            var b = new Vector3(length, 0);
            var pline = new Polyline(new[] { a, b });
            var offsetResults = pline.ClosedOffset(offsetAmt);
            Assert.Equal(1, offsetResults.Length);
            var offsetResult = offsetResults[0];
            Assert.Equal(4, offsetResult.Vertices.Count);
            // offsets to a rectangle that's offsetAmt longer than the segment in
            // each direction, and 2x offsetAmt in width, so the long sides are 
            // each length + 2x offsetAmt, and the short sides are each 2x offsetAmt.
            var targetLength = 2 * length + 8 * offsetAmt;
            var tolerance = 0.01;
            Assert.InRange(offsetResult.Length(), targetLength - tolerance, targetLength + tolerance);
        }
    }
}