using Hypar.Elements;
using Hypar.Geometry;
using System;
using System.Linq;
using Xunit;

namespace Hypar.Tests
{
    public class PolygonTests
    {
        [Fact]
        public void Polygon_Offset()
        {
            var a = new Vector3();
            var b = new Vector3(2, 5);
            var c = new Vector3(-3, 5);

            var plinew = new Polygon(new[]{a,b,c});
            var offset = plinew.Offset(0.2);
            Assert.Equal(1, offset.Count());
        }

        [Fact]
        public void Polygon_TwoPeaks__Offset_2Polylines()
        {
            var a = new Vector3();
            var b = new Vector3(5, 0);
            var c = new Vector3(5, 5);
            var d = new Vector3(0, 1);
            var e = new Vector3(-5, 5);
            var f = new Vector3(-5, 0);

            var plinew = new Polygon(new[]{a,b,c,d,e,f});
            var offset = plinew.Offset(-0.5);
            Assert.Equal(2, offset.Count());
        }

        [Fact]
        public void Polygon_Construct()
        {
            var a = new Vector3();
            var b = new Vector3(1,0);
            var c = new Vector3(1,1);
            var d = new Vector3(0,1);
            var p = new Polygon(new[]{a,b,c,d});
            Assert.Equal(4, p.Segments().Count());
        }

        [Fact]
        public void Polygon_Area()
        {
            var a = Profiles.Rectangular();
            Assert.Equal(1.0, a.Area);

            var b = Profiles.Rectangular(Vector3.Origin, 2.0,2.0);
            Assert.Equal(4.0, b.Area);

            var p1 = Vector3.Origin;
            var p2 = Vector3.XAxis;
            var p3 = new Vector3(1.0, 1.0);
            var p4 = new Vector3(0.0, 1.0);
            var pp = new Polygon(new[]{p1,p2,p3,p4});
            Assert.Equal(1.0, pp.Area);
        }

        public void Polygon_Length()
        {
            var a = new Vector3();
            var b = new Vector3(1,0);
            var c = new Vector3(1,1);
            var d = new Vector3(0,1);
            var p = new Polygon(new[]{a,b,c,d});
            Assert.Equal(4, p.Length);
        }

        public void Polygon_PointAt()
        {
            var a = new Vector3();
            var b = new Vector3(1,0);
            var c = new Vector3(1,1);
            var d = new Vector3(0,1);
            var p = new Polygon(new[]{a,b,c,d});
            Assert.Equal(new Vector3(0.5, 1), p.PointAt(0.5));
        }
    }
}