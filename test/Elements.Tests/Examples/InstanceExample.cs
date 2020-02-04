using Elements.Geometry;
using Xunit;

namespace Elements.Tests.Examples
{
    public class InstanceExample : ModelTest
    {
        [Fact]
        public void Instance()
        {
            this.Name = "Elements_Instance";

            // <example>
            // Create a mass type from which to create instances.
            var m = new Mass(new Profile(Polygon.Rectangle(1.0, 1.0)), 1.5);

            var j = 1.0;
            var count = 20;
            for (var i = 0.0; i < 360.0; i += 360.0 / (double)count)
            {
                var t = new Transform();
                t.Scale(new Vector3(j, j, j));
                t.Move(new Vector3(3, 0, 0));
                t.Rotate(Vector3.ZAxis, i);
                var instance = m.CreateInstance(t, $"mass {i}");
                this.Model.AddElement(instance);

                j += 1.0 / (double)count;
            }
            //</example>
        }
    }
}