using Elements.Geometry;
using System.Collections.Generic;
using Xunit;

namespace Elements.Tests
{
    public class FloorTest : ModelTest
    {
        [Fact]
        public void Floor()
        {
            this.Name = "Elements_Floor";

            var p = Polygon.L(10, 20, 5);
            var openings = new List<Opening>(){
                new Opening(1, 1, 1, 1),
                new Opening(3, 3, 1, 3),
            };
            var floor = new Floor(p, 0.1, 0.5, null, openings, new Material("green", Colors.Green, 0.0f, 0.0f));

            this.Model.AddElement(floor);
        }
    }
}