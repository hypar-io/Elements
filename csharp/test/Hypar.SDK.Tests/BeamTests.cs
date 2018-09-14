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
            return new Beam(l, new[]{Profiles.WideFlangeProfile()});
        });

        [Fact]
        public void Example()
        {
            var l = new Line(Vector3.Origin(), new Vector3(5,5,5));
            var b = new Beam(l, new[]{Profiles.WideFlangeProfile()});
            var model = new Model();
            model.AddElement(b);
            model.SaveGlb("beam.glb");
        }

        [Fact]
        public void Construct()
        {
            var l = new Line(Vector3.Origin(), new Vector3(5,5,5));
            var b = new Beam(l, new[]{Profiles.WideFlangeProfile()});
            Assert.Equal(BuiltInMaterials.Steel, b.Material);
            Assert.Equal(l, b.CenterLine);
        }
    }
}