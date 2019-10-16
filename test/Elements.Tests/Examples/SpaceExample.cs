using Elements.Geometry;
using Xunit;

namespace Elements.Tests.Examples
{
    public class SpaceExample : ModelTest
    {
        [Fact]
        public void SpaceExtrude()
        {
            this.Name = "Elements_Space";

            // <example>
            // Create a space.
            var a = new Vector3();
            var b = new Vector3(30, 10);
            var c = new Vector3(20, 50);
            var d = new Vector3(-10, 5);
            var profile = new Profile(new Polygon(new[]{a,b,c,d}));
            var space = new Space(profile, 10, 0);
            // </example>

            this.Model.AddElement(space);
        }
    }
}