using Xunit;
using Elements.Geometry;

namespace Elements.Tests
{
    public class PlaneTests
    {
        [Fact]
        public void IsCoplanar()
        {
            var p = new Plane(Vector3.Origin, Vector3.XAxis);
            var p1 = new Plane(new Vector3(0, 5, 0), Vector3.XAxis);
            Assert.True(p.IsCoplanar(p1));

            var p2 = new Plane(Vector3.Origin, new Vector3(0.1, 0.1, 0.1).Unitized());
            Assert.False(p.IsCoplanar(p2));
        }

        [Fact]
        public void ThreePlanesIntersect()
        {
            var origin = new Vector3(2, 2, 2);
            var a = new Plane(origin, Vector3.XAxis);
            var b = new Plane(origin, Vector3.ZAxis);
            var c = new Plane(origin, Vector3.YAxis);
            var intersects = a.Intersects(b, c, out Vector3 result);
            Assert.True(intersects);
            Assert.True(result.IsAlmostEqualTo(origin));
        }

        [Fact]
        public void ThreePlanesDoNotIntersect()
        {
            var origin = new Vector3(2, 2, 2);
            var a = new Plane(origin, Vector3.XAxis);
            var b = new Plane(new Vector3(1, 1, 1), Vector3.XAxis);
            var c = new Plane(origin, Vector3.YAxis);
            var intersects = a.Intersects(b, c, out Vector3 result);
            Assert.False(intersects);
        }

        [Fact]
        public void TwoPlanesIntersect()
        {
            var a = new Plane(new Vector3(2, 2, 2), Vector3.XAxis);
            var b = new Plane(new Vector3(0, 0, 0), Vector3.ZAxis);
            var intersects = a.Intersects(b, out InfiniteLine result);
            Assert.True(intersects);
            Assert.True(result.Direction.IsParallelTo(Vector3.YAxis));
            Assert.True(result.ParameterAt(new Vector3(2, 0, 0), out _));
        }

        [Fact]
        public void TwoPlanesDoNotIntersect()
        {
            var a = new Plane(new Vector3(2, 2, 2), Vector3.XAxis);
            var b = new Plane(new Vector3(0, 0, 0), Vector3.XAxis);
            var intersects = a.Intersects(b, out InfiniteLine result);
            Assert.False(intersects);
        }
    }
}