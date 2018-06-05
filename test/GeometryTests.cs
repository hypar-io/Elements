using Hypar.Geometry;
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

            var c = a.Negate();
            Assert.True(a.IsParallelTo(c));
        }
    }
}