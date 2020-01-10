using System.Collections.Generic;
using Elements.Geometry;
using Xunit;

namespace Elements.Tests.Examples
{
    public class FloorExample : ModelTest
    {
        [Fact]
        public void Floor()
        {
            this.Name = "Elements_Floor";

            // <example>            
            // Create a floor with no elevation.
            var p = Polygon.L(10, 20, 5);
            var floor1 = new Floor(p, 0.1);

            // Create a floor with an elevation.
            var floor2 = new Floor(p, 0.1, new Transform(0,0,3));

            // Create some openings.
            var openings = new List<Opening>(){
                new Opening(1, 1, 1, 1),
                new Opening(3, 3, 1, 3),
            };
            
            // Add the openings to the floor's 
            // openings collection.
            floor1.Openings.AddRange(openings);
            // </example>
            Assert.Equal(0.0, floor1.Elevation);
            Assert.Equal(0.1, floor1.Thickness);
            Assert.Equal(0.0, floor1.Transform.Origin.Z);

            this.Model.AddElement(floor1);
            this.Model.AddElement(floor2);
        }
    }
}