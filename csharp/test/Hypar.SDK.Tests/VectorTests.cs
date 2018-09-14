using Hypar.Geometry;
using Hypar.Elements;
using System;
using System.Linq;
using Xunit;

namespace Hypar.Tests
{
    public class VectorTests
    {
        [Fact]
        public void Vector3_TwoVectors_Equal()
        {
            var v1 = new Vector3(5,5,5);
            var v2 = new Vector3(5,5,5);
            Assert.True(v1.Equals(v2));

            var v3 = new Vector3();
            Assert.True(!v1.Equals(v3));
        }

        [Fact]
        public void Vector3_AnglesBetween_Success()
        {
            var a = Vector3.XAxis;
            var b = Vector3.YAxis;
            Assert.Equal(Math.PI/2, a.AngleTo(b), 5);

            var c = new Vector3(1,1,0);
            Assert.Equal(Math.PI/4, a.AngleTo(c), 5);

            Assert.Equal(0.0, a.AngleTo(a), 5);
        }

        [Fact]
        public void Vector3_Parallel_AngleBetween_Success()
        {
            var a = Vector3.XAxis;
            var b = Vector3.XAxis;
            Assert.True(a.IsParallelTo(b));

            var c = a.Negated();
            Assert.True(a.IsParallelTo(c));
        }
    }
}