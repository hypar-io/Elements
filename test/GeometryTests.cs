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
            var a = new Vector3(1,0,0);
            var b = new Vector3(0,1,0);
            Assert.Equal(Math.PI/2, a.AngleTo(b), 5);

            var c = new Vector3(1,1,0);
            Assert.Equal(Math.PI/4, a.AngleTo(c), 5);

            Assert.Equal(0.0, a.AngleTo(a), 5);
        }

        [Fact]
        public void ParallelVectors_AngleBetween_Success()
        {
            var a = new Vector3(1,0,0);
            var b = new Vector3(1,0,0);
            Assert.True(a.IsParallelTo(b));

            var c = new Vector3(-1,0,0);
            Assert.True(a.IsParallelTo(c));
        }
    }
}