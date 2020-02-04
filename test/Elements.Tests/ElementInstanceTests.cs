using Elements.Geometry;
using System;
using Xunit;

namespace Elements.Tests
{
    public class ElementInstanceTests : ModelTest
    {
        [Fact]
        public void Instance()
        {
            this.Name = "Elements_ElementInstance";
            
            // <example>
            // Create a test user element.
            var profile = new Profile(Polygon.Rectangle(1.0, 1.0));
            var material = new Material("yellow", Colors.Yellow);
            var line = new Line(Vector3.Origin, new Vector3(5, 5, 5));
            var testUserElement = new TestUserElement(line, profile, material);
            testUserElement.IsElementDefintion = true;

            // Create instances of that element
            // with varied transforms.
            var attractor = new Vector3(30, 20);
            for (var x = 0.0; x < 50; x += 1.5)
            {
                for (var y = 0.0; y < 50; y += 1.5)
                {
                    var loc = new Vector3(x, y);
                    var d = loc.DistanceTo(attractor);
                    var s = d == 0 ? 1 : 5 * Math.Sin(1 / d);
                    var t = new Transform();
                    t.Scale(new Vector3(s, s, s));
                    t.Move(loc);
                    var instance = testUserElement.CreateInstance(t, $"Test User Element {x}:{y}");
                    this.Model.AddElement(instance);
                }
            }
            // </example>
        }
    }
}