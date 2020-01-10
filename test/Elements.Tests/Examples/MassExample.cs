using Elements.Geometry;
using Xunit;

namespace Elements.Tests.Examples
 {
     public class MassExample : ModelTest
     {
        [Fact]
        public void Mass()
        {
            this.Name = "Elements_Mass";

            // <example>
            var a = new Vector3();
            var b = new Vector3(30, 10);
            var c = new Vector3(20, 50);
            var d = new Vector3(-10, 5);
            var poly = new Polygon(new[] { a, b, c, d });
            
            // Create a mass.
            var mass = new Mass(poly, 5.0);
            //</example>
            
            this.Model.AddElement(mass);
        }
     }
 }