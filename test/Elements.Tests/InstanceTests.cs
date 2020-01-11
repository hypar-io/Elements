using Elements.Geometry;
using System;
using Xunit;

namespace Elements.Tests
{
    public class InstanceTests : ModelTest
    {
        [Fact]
        public void Instance()
        {
            this.Name = "Instance";
            
            var profile = new Profile(Polygon.Rectangle(1.0, 1.0));
            var material = new Material("yellow", Colors.Yellow);
            var line = new Line(Vector3.Origin, new Vector3(5, 5, 5));
            var m1 = new TestUserElement(line, profile);

            this.Model.AddElement(m1);

            var attractor = new Vector3(30, 20);

            for (var x = 0.0; x < 100; x += 1.5)
            {
                for (var y = 0.0; y < 100; y += 1.5)
                {
                    var m2 = new Instance(m1);
                    // var m2 = new TestUserElement(line, profile);
                    var loc = new Vector3(x, y);
                    var d = loc.DistanceTo(attractor);
                    var s = d == 0 ? 1 : 5 * Math.Sin(1 / d);
                    m2.Transform.Scale(new Vector3(s, s, s));
                    m2.Transform.Move(loc);
                    this.Model.AddElement(m2, false);
                }
            }
        }
    }
}