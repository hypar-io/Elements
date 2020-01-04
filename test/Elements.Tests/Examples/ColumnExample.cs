using Elements.Geometry;
using Elements.Geometry.Profiles;
using Xunit;

namespace Elements.Tests.Examples
{
    public class ColumnExample : ModelTest
    {
        [Fact]
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