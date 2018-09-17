using Hypar.Geometry;
using System;
using Xunit;

namespace Hypar.Tests
{
    public class LineTests
    {
        [Fact]
        public void Example()
        {
            var a = new Vector3();
            var b = new Vector3(5,5,5);
            var l = new Line(a,b);
        }

        [Fact]
        public void Construct()
        {
            var a = new Vector3();
            var b = new Vector3(1, 0);
            var l = new Line(a,b);
            Assert.Equal(1.0, l.Length);
            Assert.Equal(new Vector3(0.5,0), l.PointAt(0.5));
        }

        [Fact]
        public void ByPoints_ZeroLength_ThrowsException()
        {
            var a = new Vector3();
            Assert.Throws<ArgumentException>(()=>new Line(a,a));
        }

        [Fact]
        public void ByDirectionLength_ZeroLength_ThrowsException()
        {
            var a = new Vector3();
            var v = Vector3.ZAxis;
            var d = 0.0;
            Assert.Throws<ArgumentException>(()=>new Line(a,v,d));
        }
    }
}