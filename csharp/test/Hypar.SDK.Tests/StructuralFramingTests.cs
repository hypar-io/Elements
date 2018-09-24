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
            var l1= new Line(Vector3.Origin, new Vector3(2,0,0));
            var b1 = new Beam(l1, Polygon.WideFlange(), new Material("x", Colors.Red));

            var l2 = new Line(Vector3.Origin, new Vector3(0,0,2));
            var b2 = new Beam(l2, Polygon.Circle(0.2, 10), new Material("z", Colors.Blue));

            var l3 = new Line(Vector3.Origin, new Vector3(0,2,0));
            var b3 = new Beam(l3, Polygon.Rectangle(Vector3.Origin, 0.2, 0.2), new Material("y", Colors.Green));

            var l4 = new Line(Vector3.Origin, new Vector3(2,2,2));
            var b4 = new Beam(l4, Polygon.Rectangle(Vector3.Origin, 0.2, 0.2), new Material("d", Colors.Pink));

            var model = new Model();
            model.AddElements(new[]{b1,b2,b3,b4});
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