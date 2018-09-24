using Hypar.Elements;
using Hypar.Geometry;
using Xunit;

namespace Hypar.Tests
{
    public class BeamTests
    {
        [Fact]
        public void Example()
        {
            var l = new Line(Vector3.Origin, new Vector3(2,0,0));
            var b = new Beam(l, Polygon.WideFlange());

            var model = new Model();
            model.AddElement(b);
            model.SaveGlb("beam.glb");
        }

        [Fact]
        public void Construct_Beam()
        {
            var l = new Line(Vector3.Origin, new Vector3(5,5,5));
            var b = new Beam(l, Polygon.WideFlange());
            Assert.Equal(BuiltInMaterials.Steel, b.Material);
            Assert.Equal(l, b.CenterLine);
        }

        [Fact]
        public void Construct_Column()
        {
            var c = new Column(Vector3.Origin, 10.0, Polygon.WideFlange());
            Assert.Equal(BuiltInMaterials.Steel, c.Material);
            Assert.Equal(10.0, c.CenterLine.Length);
        }

        [Fact]
        public void Construct_Brace()
        {
            var l = new Line(Vector3.Origin, new Vector3(5,5,5));
            var b = new Brace(l, Polygon.WideFlange());
            Assert.Equal(BuiltInMaterials.Steel, b.Material);
            Assert.Equal(l, b.CenterLine);
        }
    }
}