using Elements.Geometry;
using Xunit;

namespace Elements.Tests
{
    public class GeometricElementTests : ModelTest
    {
        [Fact]
        public void ToMesh()
        {
            var profile = Polygon.Rectangle(1.0, 1.0);
            var mass = new Mass(profile, 5.0, BuiltInMaterials.Mass, new Transform());
            var mesh = mass.ToMesh();

            Assert.Equal(24, mesh.Vertices.Count);
            Assert.Equal(12, mesh.Triangles.Count);
        }
    }
}