using Elements.Geometry;
using Elements.Geometry.Profiles;
using Xunit;

namespace Elements.Tests
{
    public class ColumnTest : ModelTest
    {        
        [Fact, Trait("Category", "Examples")]
        public void Example()
        {
            this.Name = "Elements_Column";

            // <example>
            // Create a framing type.
            var profile = WideFlangeProfileServer.Instance.GetProfileByType(WideFlangeProfileType.W10x100);

            // Create a column.
            var column = new Column(Vector3.Origin, 3.0, profile, BuiltInMaterials.Steel);
            // </example>

            this.Model.AddElement(column);
        }
    }
}