using Elements.Geometry;
using Xunit;

namespace Elements.Tests
{
    public class PanelExample : ModelTest
    {
        [Fact]
        public void Example()
        {
            this.Name = "Elements_Panel";

            // <example>
            var a = new Vector3(0,0,0);
            var b = new Vector3(1,0,0);
            var c = new Vector3(1,0,1);
            var d = new Vector3(0,0,1);
            
            // Create a panel.
            var panel = new Panel(new Polygon(new []{a,b,c,d}), BuiltInMaterials.Glass);
            // </example>
            
            Assert.Equal(BuiltInMaterials.Glass, panel.Material);
            
            this.Model.AddElement(panel);
        }
    }
}