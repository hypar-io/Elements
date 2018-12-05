using Elements.Geometry;
using System;
using System.Linq;
using Xunit;

namespace Hypar.Tests
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

            var pline = new Polyline(new[]{a,b,c,d});
            Assert.Equal(4, pline.Vertices.Count);
            Assert.Equal(3, pline.Segments().Count());
        }
    }
}