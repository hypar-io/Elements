using Hypar.Elements;
using Hypar.Geometry;
using Xunit;
using System.IO;

namespace Hypar.Tests
{
    public class BeamTests
    {
        [Fact]
        public void SquareProfile_Construct_Success()
        {
            var model = new Model();
            var square = new SquareProfile(1.0, 2.0);
            var xLine = new Line(new Vector3(2,0,0), new Vector3(10,0,0));
            var yLine = new Line(new Vector3(0,2,0), new Vector3(0,10,0));
            var zLine = new Line(new Vector3(0,0,2), new Vector3(0,0,10));
            var dLine = new Line(new Vector3(2,2,2), new Vector3(10,10,10));

            model.AddMaterial(new Material("xColor", 1.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f));
            model.AddMaterial(new Material("yColor", 0.0f, 1.0f, 0.0f, 1.0f, 0.0f, 0.0f));
            model.AddMaterial(new Material("zColor", 0.0f, 0.0f, 1.0f, 1.0f, 0.0f, 0.0f));
            model.AddMaterial(new Material("dColor", 1.0f, 0.0f, 1.0f, 1.0f, 0.0f, 0.0f));

            var b1 = new Beam(xLine, square, model.Materials["xColor"]);
            model.AddElement(b1);
            var b2 = new Beam(yLine, square, model.Materials["yColor"]);
            model.AddElement(b2);
            var b3 = new Beam(zLine, square, model.Materials["zColor"]);
            model.AddElement(b3);
            var b4 = new Beam(dLine, square, model.Materials["dColor"]);
            model.AddElement(b4);

            model.SaveGlb("squareBeams.glb");
            Assert.True(File.Exists("squareBeams.glb"));
            Assert.Equal(4, model.Elements.Count);
        }

        [Fact]
        public void WideFlangeProfile_Construct_Success()
        {
            var model = new Model();
            var wf = new WideFlangeProfile(1.0, 2.0, 0.1, 0.1);
            var xLine = new Line(new Vector3(2,0,0), new Vector3(10,0,0));
            var yLine = new Line(new Vector3(0,2,0), new Vector3(0,10,0));
            var zLine = new Line(new Vector3(0,0,2), new Vector3(0,0,10));
            var dLine = new Line(new Vector3(2,2,2), new Vector3(5,5,10));

            model.AddMaterial(new Material("xColor", 1.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f));
            model.AddMaterial(new Material("yColor", 0.0f, 1.0f, 0.0f, 1.0f, 0.0f, 0.0f));
            model.AddMaterial(new Material("zColor", 0.0f, 0.0f, 1.0f, 1.0f, 0.0f, 0.0f));
            model.AddMaterial(new Material("dColor", 1.0f, 0.0f, 1.0f, 1.0f, 0.0f, 0.0f));

            var b1 = new Beam(xLine, wf, model.Materials["xColor"]);
            model.AddElement(b1);
            var b2 = new Beam(yLine, wf, model.Materials["yColor"]);
            model.AddElement(b2);
            var b3 = new Beam(zLine, wf, model.Materials["zColor"]);
            model.AddElement(b3);
            var b4 = new Beam(dLine, wf, model.Materials["dColor"]);
            model.AddElement(b4);

            var slab = new Slab(Profiles.Square(new Vector2(5,5),1.0,1.0), new Polygon2[]{}, 0.0, 1.0, model.Materials[BuiltInMaterials.DEFAULT]);
            model.AddElement(slab);

            model.SaveGlb("wideFlangeBeams.glb");
            Assert.True(File.Exists("wideFlangeBeams.glb"));
            Assert.Equal(5, model.Elements.Count);
        }
    }
}