using Elements.Geometry;
using Elements.Geometry.Profiles;
using Xunit;

namespace Elements.Tests
{
    public class ColumnTest : ModelTest
    {
        private WideFlangeProfileFactory _profileFactory = new WideFlangeProfileFactory();

        [Fact, Trait("Category", "Examples")]
        public void Example()
        {
            this.Name = "Elements_Column";

            // <example>
            // Create a framing type.
            var profile = _profileFactory.GetProfileByType(WideFlangeProfileType.W10x100);

            // Create a column.
            var column = new Column(Vector3.Origin, 3.0, null, profile, material: BuiltInMaterials.Steel);
            // </example>

            this.Model.AddElement(column);
        }

        [Fact]
        public void Transform()
        {
            this.Name = "ColumnTransform";
            var profile = _profileFactory.GetProfileByType(WideFlangeProfileType.W10x100);
            var t = new Transform();
            t.Rotate(45);
            t.Move(2.0);
            var column1 = new Column(Vector3.Origin, 3.0, null, profile, material: BuiltInMaterials.Steel);
            var column2 = new Column(Vector3.Origin, 3.0, null, profile, material: BuiltInMaterials.Steel, transform: t);
            Assert.Equal(new Vector3(5, 5).Unitized(), column2.Transform.XAxis);
            Assert.Equal(2.0, column2.Transform.Origin.X);
            this.Model.AddElements(column1, column2);
        }
    }
}