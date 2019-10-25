using Elements.Geometry;
using Xunit;

namespace Elements.Tests.Examples
{
    public class TransformExample : ModelTest
    {
        [Fact]
        public void Example()
        {
            this.Name = "Elements_Geometry_Transform";
            // <example>
            var m1 = new Mass(Polygon.Rectangle(1.0, 1.0),1.0, new Material("yellow", Colors.Yellow));
            this.Model.AddElement(m1);

            Profile prof = Polygon.Rectangle(1.0, 1.0);

            var j = 1.0;
            var count = 10;
            for(var i=0.0; i<360.0; i+= 360.0/(double)count)
            {
                var m2 = new Mass(prof, 1.0, new Material($"color_{j}", new Color((float)j - 1.0f, 0.0f, 0.0f, 1.0f)), new Transform());
                
                // Scale the mass.
                m2.Transform.Scale(new Vector3(j,j,j));

                // Move the mass.
                m2.Transform.Move(new Vector3(3, 0, 0));

                // Rotate the mass.
                m2.Transform.Rotate(Vector3.ZAxis, i);
                this.Model.AddElement(m2);
                j += 1.0/(double)count;
            }
            // </example>
        }
    }
}