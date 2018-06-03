using Hypar.Elements;
using Hypar.Geometry;
using System.IO;
using Xunit;

namespace Hypar.Tests
{
    public class BeamSystemTests
    {
        [Fact]
        public void BeamSystem()
        {
            var model = new Model();
            var profile = new WideFlangeProfile(1.0, 2.0, 0.1, 0.1);
            var edge1 = new Line(new Vector3(0,0,0), new Vector3(20,0,0));
            var edge2 = new Line(new Vector3(0,20,0), new Vector3(20,20,10));
            var system = new BeamSystem(5, profile, edge1, edge2, model.Materials[BuiltInMaterials.STEEL]);
            foreach(var b in system.Beams)
            {
                model.AddElement(b);
            }
            model.SaveGlb("beamSystem.glb");
            Assert.True(File.Exists("beamSystem.glb"));
            Assert.Equal(5, model.Elements.Count);
        }
    }
}