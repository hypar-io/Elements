using Elements;
using Elements.Geometry;
using System;
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
            var floorType = new FloorType("test", new List<MaterialLayer> { new MaterialLayer(new Material("green", Colors.Green, 0.0f, 0.0f), 0.1) });
            var openings = new List<Opening>(){
                new Opening(1, 1, 1, 1),
                new Opening(3, 3, 1, 3),
            };
            var floor = new Floor(p, floorType, 0.5, null, openings);

            this.Model.AddElement(floor);
        }
    }
}