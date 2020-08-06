using Elements.Geometry;
using Xunit;

namespace Elements.Tests
{
    public class PanelTests : ModelTest
    {
        [Fact, Trait("Category", "Examples")]
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
            
            this.Model.AddElement(panel);
        }
    }
}