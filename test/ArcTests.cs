using Elements.Geometry;
using System;
using Xunit;

namespace Hypar.Tests
{
    public class ArcTests
    {
        [Fact]
        public void Arc()
        {
            var arc = new Arc(Vector3.Origin, 2.0, 0.0, 90.0);
            Assert.True(new Vector3(2,0,0).IsAlmostEqualTo(arc.Start));
            Assert.True(new Vector3(0,2,0).IsAlmostEqualTo(arc.End));

            var arc1 = new Arc(Vector3.Origin, 2.0, 0.0, -90.0);
            Assert.True(new Vector3(2,0,0).IsAlmostEqualTo(arc1.Start));
            Assert.True(new Vector3(0,-2,0).IsAlmostEqualTo(arc1.End));
        }

        [Fact]
        public void ZeroSweep_ThrowsException()
        {
            Assert.Throws<ArgumentException>(()=>new Arc(Vector3.Origin, 2.0, 0.0, 0.0));
        }

        [Fact]
        public void ZeroRadius_ThrowsException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(()=>new Arc(Vector3.Origin, 0.0, 0.0, 90.0));
        }
    }
}