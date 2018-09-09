using Hypar.Geometry;
using System;
using System.Linq;
using Xunit;

namespace Hypar.Tests
{
    public class PolylineTests
    {
        [Fact]
        public void Valid_Construct_Success()
        {
            var a = new Vector3();
            var b = new Vector3(10, 10);
            var c = new Vector3(3, 5);
            var d = new Vector3(2, 1);

            var plinew = new Polyline(new[]{a,b,c,d});
            Assert.Equal(4, plinew.Count());
            Assert.Equal(4, plinew.Segments().Count());
        }

        [Fact]
        public void Valid_Offset_Success()
        {
            var a = new Vector3();
            var b = new Vector3(2, 5);
            var c = new Vector3(-3, 5);

            var plinew = new Polyline(new[]{a,b,c});
            var offset = plinew.Offset(0.2);
            Assert.Equal(1, offset.Count());
            Console.WriteLine(offset.ElementAt(0).ToString());
        }

        [Fact]
        public void TwoPeaks__Offset_2Polylines()
        {
            var a = new Vector3();
            var b = new Vector3(5, 0);
            var c = new Vector3(5, 5);
            var d = new Vector3(0, 1);
            var e = new Vector3(-5, 5);
            var f = new Vector3(-5, 0);

            var plinew = new Polyline(new[]{a,b,c,d,e,f});
            var offset = plinew.Offset(-0.5);
            Assert.Equal(2, offset.Count());
            Console.WriteLine(offset.ElementAt(0).ToString());
        }
    }
}