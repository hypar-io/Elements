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
    }
}