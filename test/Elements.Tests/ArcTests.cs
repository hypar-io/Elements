using Elements.Geometry;
using System;
using Xunit;
using Xunit.Abstractions;

namespace Hypar.Tests
{
    public class ArcTests
    {
        ITestOutputHelper _output;

        public ArcTests(ITestOutputHelper output)
        {
            this._output = output;
        }

        [Fact]
        public void Arc()
        {
            var arc = new Arc(Vector3.Origin, 2.0, 0.0, 90.0);
            Assert.True(new Vector3(2, 0, 0).IsAlmostEqualTo(arc.Start));
            Assert.True(new Vector3(0, 2, 0).IsAlmostEqualTo(arc.End));

            var arc1 = new Arc(Vector3.Origin, 2.0, 0.0, -90.0);
            Assert.True(new Vector3(2, 0, 0).IsAlmostEqualTo(arc1.Start));
            Assert.True(new Vector3(0, -2, 0).IsAlmostEqualTo(arc1.End));
        }

        [Fact]
        public void ZeroSweep_ThrowsException()
        {
            Assert.Throws<ArgumentException>(() => new Arc(Vector3.Origin, 2.0, 0.0, 0.0));
        }

        [Fact]
        public void ZeroRadius_ThrowsException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Arc(Vector3.Origin, 0.0, 0.0, 90.0));
        }

        [Fact]
        public void PointAt()
        {
            var arc = new Arc(Vector3.Origin, 5.0, 0.0, 90.0);
            Assert.Equal(new Vector3(0, 5, 0), arc.End);
            Assert.Equal(new Vector3(0, 5, 0), arc.PointAt(1.0));
            Assert.Equal(new Vector3(5 * Math.Cos(Math.PI / 4), 5 * Math.Sin(Math.PI / 4), 0), arc.PointAt(0.5));
            Assert.Equal(new Vector3(5 * Math.Cos(Math.PI / 2), 5 * Math.Sin(Math.PI / 2), 0), arc.PointAt(1.0));

            arc = new Arc(Vector3.Origin, 5.0, 0.0, 180.0);
            Assert.Equal(new Vector3(-5, 0, 0), arc.PointAt(1.0));
            Assert.Equal(new Vector3(0, 5, 0), arc.PointAt(0.5));
            Assert.Equal(new Vector3(5, 0, 0), arc.PointAt(0.0));
        }

        [Fact]
        public void TransformAt()
        {
            var arc = new Arc(Vector3.Origin, 5.0, 0.0, 180.0);
            var t = arc.TransformAt(0.5);
            Assert.Equal(new Vector3(0, 1, 0), t.XAxis);
            Assert.Equal(new Vector3(0, 0, 1), t.YAxis);
            Assert.Equal(new Vector3(1, 0, 0), t.ZAxis);
        }

        [Fact]
        public void Frames()
        {
            var arc = new Arc(Vector3.Origin, 5.0, 0.0, 180.0);
            var frames = arc.Frames();

            var arc1 = new Arc(Vector3.Origin, 5.0, 0.0, 180.0);
            var frames1 = arc.Frames(0.1, 0.1);
        }
    }
}