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
            return Beam.AlongLine(l);
        });

        [Fact]
        public void Single_AlongLine_Sucess()
        {
            var l = Line.FromStart(Vector3.Origin()).ToEnd(Vector3.ByXYZ(5,5,5));
            var b = Beam.AlongLine(l);
            Assert.Equal(BuiltInMaterials.Default, b.Material);
            Assert.Equal(l, b.CenterLine);
            // Assert.Equal(Profiles.WideFlangeProfile(), b.Profile);
        }

        [Fact]
        public void Collection_AlongLines_Success()
        {
            var l1 = Line.FromStart(Vector3.Origin()).ToEnd(Vector3.ByXYZ(5,5,5));
            var l2 = Line.FromStart(Vector3.Origin()).ToEnd(Vector3.XAxis());
            var beams = new[]{l1, l2}.AlongEachCreate<Beam>(l=>{
                return Beam.AlongLine(l);
            });
            Assert.Equal(2, beams.Count());
        }

        [Fact]
        public void Params_AlongLines_Success()
        {
            var l1 = Line.FromStart(Vector3.Origin()).ToEnd(Vector3.ByXYZ(1,1,1));
            var l2 = Line.FromStart(Vector3.Origin()).ToEnd(Vector3.XAxis());

            var material = new Material("red",1.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f);
            var beams = new[]{l1,l2}.AlongEachCreate<Beam>(l => {
                return Beam.AlongLine(l).OfMaterial(material);
            });
            Assert.Equal(2, beams.Count());
            
            var model = new Model();
            model.AddElements(beams);
            model.SaveGltf("beams.gltf");
        }
    }
}