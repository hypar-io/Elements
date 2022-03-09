using Elements.Geometry;
using Xunit;

namespace Elements.Tests
{
    public class MeshPrimitivesTests : ModelTest
    {
        [Fact]
        public void Sphere()
        {
            Name = nameof(Sphere);
            var m = new Material("Test", Colors.White, unlit: true, texture: "./Textures/UV.jpg");
            var s = Mesh.Sphere(3, 20);
            Assert.Equal(401, s.Vertices.Count);
            Assert.Equal(760, s.Triangles.Count);
            Model.AddElement(new MeshElement(s, m));
        }
    }
}