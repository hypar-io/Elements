using Hypar.Geometry;
using Hypar.Elements;
using System;
using Xunit;

namespace Hypar.Tests
{
    public class GeometryTests
    {
        [Fact]
        public void Vector3_AnglesBetweenVectors_Success()
        {
            var a = Vector3.XAxis();
            var b = Vector3.YAxis();
            Assert.Equal(Math.PI/2, a.AngleTo(b), 5);

            var c = Vector3.ByXYZ(1,1,0);
            Assert.Equal(Math.PI/4, a.AngleTo(c), 5);

            Assert.Equal(0.0, a.AngleTo(a), 5);
        }

        [Fact]
        public void ParallelVectors_AngleBetween_Success()
        {
            var a = Vector3.XAxis();
            var b = Vector3.XAxis();
            Assert.True(a.IsParallelTo(b));

            var c = a.Negated();
            Assert.True(a.IsParallelTo(c));
        }

        [Fact]
        public void Polyline_Area()
        {
            var a = Profiles.Rectangular();
            Assert.Equal(1.0, a.Area);

            var b = Profiles.Rectangular(Vector3.Origin(), 2.0,2.0);
            Assert.Equal(4.0, b.Area);

            var p1 = Vector3.Origin();
            var p2 = Vector3.XAxis();
            var p3 = new Vector3(1.0, 0.0, 1.0);
            var p4 = new Vector3(0.0, 0.0, 1.0);
            var pp = new Polyline(new[]{p1,p2,p3,p4});
            Assert.Equal(1.0, pp.Area);
        }
    }
}