using Hypar.Elements;
using Hypar.Geometry;
using System.IO;
using System.Linq;
using Xunit;

namespace Hypar.Tests
{
    public class BeamSystemTests
    {
        [Fact]
        public void Example()
        {
            var profile = Polygon.WideFlange(1.0, 2.0, 0.1, 0.1);
            var a = new Vector3(0,0,0);
            var b = new Vector3(20,0,0);
            var d = new Vector3(20,20,10);
            var c = new Vector3(0,20,0);
            var polygon = new Polygon(new[]{a,b,c,d});
            var system = new BeamSystem(5, profile, new Line(a,b), new Line(c,d), BuiltInMaterials.Steel);
            var model = new Model();
            model.AddElements(system.Beams);
            model.SaveGlb("beam_system.glb");
        }

        [Fact]
        public void BeamSystem()
        {
            var profile = Polygon.WideFlange(1.0, 2.0, 0.1, 0.1);
            var a = new Vector3(0,0,0);
            var b = new Vector3(20,0,0);
            var d = new Vector3(20,20,10);
            var c = new Vector3(0,20,0);
            var polygon = new Polygon(new[]{a,b,c,d});
            var system = new BeamSystem(5, profile, new Line(a,b), new Line(c,d), BuiltInMaterials.Steel);
            Assert.True(system.Beams.Count() == 5);
        }
    }
}