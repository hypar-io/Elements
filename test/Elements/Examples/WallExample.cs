using System.Collections.Generic;
using Elements.Geometry;
using Xunit;

namespace Elements.Tests.Examples
{
    public class WallExample : ModelTest
    {
        [Fact]
        public void Example()
        {
            this.Name = "Elements_Wall";

            // <example>
            // Create some openings.
            var openings = new List<Opening>(){
                new Opening(1.0, 2.0, 1.0, 1.0),
                new Opening(3.0, 1.0, 1.0, 2.0)
            };
            
            var line = new Line(new Vector3(0, 0, 0), new Vector3(10, 10, 0));
            var wall = new StandardWall(line, 0.1, 3.0, openings);
            // </example>

            this.Model.AddElement(wall);
        }
    }
}