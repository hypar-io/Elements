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
            var mass = new Mass(Polygon.Rectangle(1.0, 1.0),1.0, new Material("yellow", Colors.Yellow));
            this.Model.AddElement(mass);

            var j = 1.0;
            var count = 10;
            for(var i=0.0; i<360.0; i+= 360.0/(double)count)
            {
                var m2 = new Instance(mass);
                m2.Transform.Scale(new Vector3(j,j,j));
                m2.Transform.Move(new Vector3(3, 0, 0));
                m2.Transform.Rotate(Vector3.ZAxis, i);
                this.Model.AddElement(m2);

                j += 1.0/(double)count;
            }
            //</example>
        }
     }
 }