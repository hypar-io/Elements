using Hypar.Elements;
using Hypar.Geometry;
using Xunit;
using System.IO;
using System.Linq;

namespace Hypar.Tests
{
    public class BeamTests
    {
        [Fact]
        public void Single_AlongLine_Sucess()
        {
            var l = Line.FromStart(Vector3.Origin()).ToEnd(Vector3.ByXYZ(5,5,5));
            var b = Beam.AlongLine(l);
            Assert.Equal(BuiltIntMaterials.Default, b.Material);
            Assert.Equal(l, b.CenterLine);
            Assert.Equal(Profiles.WideFlangeProfile(), b.Profile);
        }

        [Fact]
        public void Collection_AlongLines_Success()
        {
            var l1 = Line.FromStart(Vector3.Origin()).ToEnd(Vector3.ByXYZ(5,5,5));
            var l2 = Line.FromStart(Vector3.Origin()).ToEnd(Vector3.XAxis());
            var beams = Beam.AlongLines(new[]{l1, l2});
            Assert.Equal(2, beams.Count());
        }

        [Fact]
        public void Params_AlongLines_Success()
        {
            var l1 = Line.FromStart(Vector3.Origin()).ToEnd(Vector3.ByXYZ(5,5,5));
            var l2 = Line.FromStart(Vector3.Origin()).ToEnd(Vector3.XAxis());
            var beams = Beam.AlongLines(l1,l2);
            Assert.Equal(2, beams.Count());
        }

        // [Fact]
        // public void SquareProfile_Construct_Success()
        // {
        //     var model = new Model();
        //     var square = new SquareProfile(1.0, 2.0);
        //     var xLine = Line.FromStart(new Vector3(2,0,0)).ToEnd(new Vector3(10,0,0));
        //     var yLine = Line.FromStart(new Vector3(0,2,0)).ToEnd(new Vector3(0,10,0));
        //     var zLine = Line.FromStart(new Vector3(0,0,2)).ToEnd(new Vector3(0,0,10));
        //     var dLine = Line.FromStart(new Vector3(2,2,2)).ToEnd(new Vector3(10,10,10));

        //     var xMat = new Material("xColor", 1.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f);
        //     var yMat = new Material("yColor", 0.0f, 1.0f, 0.0f, 1.0f, 0.0f, 0.0f);
        //     var zMat = new Material("zColor", 0.0f, 0.0f, 1.0f, 1.0f, 0.0f, 0.0f);
        //     var dMat = new Material("dColor", 1.0f, 0.0f, 1.0f, 1.0f, 0.0f, 0.0f);

        //     var b1 = Beam.AlongLine(xLine)
        //                     .WithProfile(square)
        //                     .OfMaterial(xMat);
        //     var b2 = Beam.AlongLine(yLine)
        //                     .WithProfile(square)
        //                     .OfMaterial(yMat);
        //     var b3 = Beam.AlongLine(zLine)
        //                     .WithProfile(square)
        //                     .OfMaterial(zMat);
        //     var b4 = Beam.AlongLine(dLine)
        //                     .WithProfile(square)
        //                     .OfMaterial(dMat);
                            
        //     model.SaveGlb("squareBeams.glb");
        //     Assert.True(File.Exists("squareBeams.glb"));
        //     Assert.Equal(4, model.Elements.Count);
        // }

        // [Fact]
        // public void WideFlangeProfile_Construct_Success()
        // {
        //     var model = new Model();
        //     var wf = new WideFlangeProfile(1.0, 2.0, 0.1, 0.1);
        //     var xLine = Line.FromStart(new Vector3(2,0,0)).ToEnd(new Vector3(10,0,0));
        //     var yLine = Line.FromStart(new Vector3(0,2,0)).ToEnd(new Vector3(0,10,0));
        //     var zLine = Line.FromStart(new Vector3(0,0,2)).ToEnd(new Vector3(0,0,10));
        //     var dLine = Line.FromStart(new Vector3(2,2,2)).ToEnd(new Vector3(5,5,10));

        //     var xMat = new Material("xColor", 1.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f);
        //     var yMat = new Material("yColor", 0.0f, 1.0f, 0.0f, 1.0f, 0.0f, 0.0f);
        //     var zMat = new Material("zColor", 0.0f, 0.0f, 1.0f, 1.0f, 0.0f, 0.0f);
        //     var dMat = new Material("dColor", 1.0f, 0.0f, 1.0f, 1.0f, 0.0f, 0.0f);

        //     var b1 = Beam.AlongLine(xLine)
        //                     .WithProfile(wf)
        //                     .OfMaterial(model.Materials["xColor"]);

        //     var b2 = Beam.AlongLine(yLine)
        //                     .WithProfile(wf)
        //                     .OfMaterial(model.Materials["yColor"]);

        //     var b3 = Beam.AlongLine(zLine)
        //                     .WithProfile(wf)
        //                     .OfMaterial(model.Materials["zColor"]);

        //     var b4 = Beam.AlongLine(dLine)
        //                     .WithProfile(wf)
        //                     .OfMaterial(model.Materials["dColor"]);
                            
        //     model.AddElements(new[]{b1,b2,b3,b4});
            
        //     var slab = Slab.WithinPerimeter(Profiles.Square(new Vector3(5,5),1.0,1.0))
        //                     .WithHoles(new Polyline[]{})
        //                     .AtElevation(0.0)
        //                     .WithThickness(1.0)
        //                     .OfMaterial(BuiltIntMaterials.Default);

        //     model.AddElement(slab);

        //     model.SaveGlb("wideFlangeBeams.glb");
        //     Assert.True(File.Exists("wideFlangeBeams.glb"));
        //     Assert.Equal(5, model.Elements.Count);
        // }
    }
}