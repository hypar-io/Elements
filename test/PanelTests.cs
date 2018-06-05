using Hypar.Elements;
using Hypar.Geometry;
using System.IO;
using System.Linq;
using Xunit;

namespace Hypar.Tests
{
    public class PanelTests
    {
        [Fact]
        public void Single_WithinPerimeter_Success()
        {
            var p = Profiles.Square();
            var panel = Panel.WithinPerimeter(p);
            Assert.Equal(BuiltIntMaterials.Default, panel.Material);
            Assert.Equal(Vector3.ZAxis(), panel.Normal);
            Assert.Equal(p, panel.Perimeter);
        }

        [Fact]
        public void Collection_WithinPerimeter_Success()
        {
            var p1 = Profiles.Square();
            var p2 = Profiles.Square(width:10, height:5);
            var panels = Panel.WithinPerimeters(new[]{p1,p2});
            Assert.Equal(2, panels.Count());
        }

        [Fact]
        public void Params_WithinPerimeter_Success()
        {
            var p1 = Profiles.Square();
            var p2 = Profiles.Square(width:10, height:5);
            var panels = Panel.WithinPerimeters(p1,p2);
            Assert.Equal(2, panels.Count());
        }

        // [Fact]
        // public void Default_Panel()
        // {
        //     var model = QuadPanelModel();
        //     model.SaveGlb("quadPanel.glb");
        //     Assert.True(File.Exists("quadPanel.glb"));
        //     Assert.Equal(1, model.Elements.Count);
        // }

        // private Model QuadPanelModel()
        // {
        //     var model = new Model();
        //     var a = new Vector3(0,0,0);
        //     var b = new Vector3(1,0,0);
        //     var c = new Vector3(1,0,1);
        //     var d = new Vector3(0,0,1);
        //     var panel = Panel.WithinPerimeter(new Polyline(new[]{a,b,c,d}))
        //                         .OfMaterial(BuiltIntMaterials.Glass);
        //     model.AddElement(panel);
        //     return model;
        // }
    }
}