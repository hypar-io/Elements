using Hypar.Elements;
using Hypar.Geometry;
using Xunit;
using System;
using System.IO;
using System.Linq;

namespace Hypar.Tests
{
    public class BeamTests
    {
        private static Func<Line,Beam> generateBeam = new Func<Line,Beam>((Line l)=>{
            return new Beam(l, Profiles.WideFlangeProfile());
        });

        [Fact]
        public void Single_AlongLine_Sucess()
        {
            var l = new Line(Vector3.Origin(), Vector3.ByXYZ(5,5,5));
            var b = new Beam(l, Profiles.WideFlangeProfile());
            Assert.Equal(BuiltInMaterials.Default, b.Material);
            Assert.Equal(l, b.Location);
        }

        [Fact]
        public void Collection_AlongLines_Success()
        {
            var l1 = new Line(Vector3.Origin(), Vector3.ByXYZ(5,5,5));
            var l2 = new Line(Vector3.Origin(), Vector3.XAxis());
            var beams = new[]{l1, l2}.Select(l=>{
                return new Beam(l, Profiles.WideFlangeProfile());
            });
            Assert.Equal(2, beams.Count());
        }

        [Fact]
        public void Params_AlongLines_Success()
        {
            var l1 = new Line(Vector3.Origin(), Vector3.ByXYZ(1,1,1));
            var l2 = new Line(Vector3.Origin(), Vector3.XAxis());

            var material = new Material("red", new Color(1.0f, 0.0f, 0.0f, 1.0f), 0.0f, 0.0f);
            var beams = new[]{l1,l2}.Select(l=> {
                var b = new Beam(l, Profiles.WideFlangeProfile());
                b.Material = material;
                return b;
            });
            Assert.Equal(2, beams.Count());
            
            var model = new Model();
            model.AddElements(beams);
            model.SaveGltf("beams.gltf");
        }
    }
}