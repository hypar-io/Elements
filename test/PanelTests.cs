using Hypar.Elements;
using Hypar.Geometry;
using System.IO;
using Xunit;

namespace Hypar.Tests
{
    public class PanelTests
    {
        [Fact]
        public void NonPlanarPoints_Construction_ThrowsException()
        {
            Assert.True(false, "Implement this test.");
        }

        [Fact]
        public void Default_Panel()
        {
            var model = QuadPanelModel();
            model.SaveGlb("quadPanel.glb");
            Assert.True(File.Exists("quadPanel.glb"));
            Assert.Equal(1, model.Elements.Count);
        }

        private Model QuadPanelModel()
        {
            var model = new Model();
            var a = new Vector3(0,0,0);
            var b = new Vector3(1,0,0);
            var c = new Vector3(1,0,1);
            var d = new Vector3(0,0,1);
            var panel = new Panel(new Polygon3(new[]{a,b,c,d}),model.Materials[BuiltInMaterials.GLASS]);
            model.AddElement(panel);
            return model;
        }
    }
}