using Elements.Geometry;
using Elements.Geometry.Solids;
using Xunit;

namespace Elements.Tests
{
    public class MeshTests
    {
        [Fact]
        public void Volume()
        {
            // A simple extrusion.
            var extrude = new Extrude(Polygon.Rectangle(1, 1), 1, Vector3.ZAxis, false);
            var mesh = extrude.Solid.Tessellate();
            Assert.Equal(1.0, mesh.Volume(), 5);

            // A more complicated extrusion.
            var l = Polygon.L(20, 10, 5);
            var lExtrude = new Extrude(l, 5, Vector3.ZAxis, false);
            var lMesh = lExtrude.Solid.Tessellate();
            Assert.Equal(l.Area() * 5, lMesh.Volume(), 5);

            // A boolean.
            var l1 = l.Offset(-1)[0].Reversed();
            var l1Extrude = new Extrude(new Profile(l, l1), 5, Vector3.ZAxis, false);
            var l1Mesh = l1Extrude.Solid.Tessellate();
            Assert.Equal((l.Area() + l1.Area()) * 5, l1Mesh.Volume(), 5);
        }
    }
}