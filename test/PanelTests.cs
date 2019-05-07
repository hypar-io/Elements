using Elements;
using Elements.Geometry;
using System.IO;
using System.Linq;
using Xunit;

namespace Elements.Tests
{
    public class PanelTests : ModelTest
    {
        [Fact]
        public void Panel()
        {
            this.Name = "Panel";
            var a = new Vector3(0,0,0);
            var b = new Vector3(1,0,0);
            var c = new Vector3(1,0,1);
            var d = new Vector3(0,0,1);
            var panel = new Panel(new []{a,b,c,d}, BuiltInMaterials.Glass);
            Assert.Equal(BuiltInMaterials.Glass, panel.Material);
            // Assert.Equal(panel.Geometry[0].Faces[0].Vertices, panel.Perimeter);
            this.Model.AddElement(panel);
        }
    }
}