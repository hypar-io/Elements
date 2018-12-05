using Elements;
using Elements.Geometry;
using System.IO;
using System.Linq;
using Xunit;

namespace Hypar.Tests
{
    public class PanelTests
    {
        [Fact]
        public void Example()
        {
            var model = new Model();
            var a = new Vector3(0,0,0);
            var b = new Vector3(1,0,0);
            var c = new Vector3(1,0,1);
            var d = new Vector3(0,0,1);
            var panel = new Panel(new []{a,b,c,d}, BuiltInMaterials.Glass);
            model.AddElement(panel);
            model.SaveGlb("panel.glb");
        }

        [Fact]
        public void Construct()
        {
            var p = Polygon.Rectangle();
            var panel = new Panel(p.Vertices);
            Assert.Equal(BuiltInMaterials.Default, panel.Material);
            Assert.Equal(panel.Perimeter, p.Vertices);
        }
    }
}