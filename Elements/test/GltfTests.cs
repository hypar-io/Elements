using Xunit;
using Elements.Geometry;
using Elements.Serialization.glTF;

namespace Elements.Tests
{
    public class GltfTests
    {
        [Fact]
        public void EmptyModelSerializesWithoutThrowingException()
        {
            var model = new Model();
            model.ToGlTF("./empty_model.glb", true);
        }

        [Fact]
        public void ModelSerializesToBase64String()
        {
            var model = new Model();
            var beam = new Beam(new Line(Vector3.Origin, new Vector3(5, 5, 5)), Polygon.Rectangle(0.1, 0.2));
            model.AddElement(beam);
            var str = model.ToBase64String();
        }
    }
}