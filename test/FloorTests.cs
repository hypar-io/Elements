using Elements.Serialization.glTF;
using Elements.Geometry;
using System;
using System.Collections.Generic;
using Xunit;

namespace Elements.Tests
{
    public class FloorTests : ModelTest
    {
        [Fact]
        public void FloorWithAddedOpenings()
        {
            this.Name = "FloorWithAddedOpenings";
            var p = Polygon.L(10, 20, 5);
            var floorType = new FloorType("test", new List<MaterialLayer> { new MaterialLayer(new Material("green", Colors.Green, 0.0f, 0.0f), 0.1) });
            var openings = new List<Opening>(){
                new Opening(1, 1, 1, 1),
                new Opening(3, 3, 1, 3),
            };
            var floor1 = new Floor(p, floorType, 0.5, null, null);

            var model = new Model();

            Assert.Empty(floor1.Openings);
            Assert.Equal(0.5, floor1.Elevation);
            Assert.Equal(0.1, floor1.ElementType.Thickness());
            Assert.Equal(0.5, floor1.Transform.Origin.Z);
            Assert.Equal(Vector3.ZAxis * -1, floor1.ExtrudeDirection);

            this.Model.AddElement(floor1);

            List<Floor> updatedFloors = new List<Floor>(this.Model.ElementsOfType<Floor>());

            foreach (Floor floor in updatedFloors)
            {
                floor.Openings.AddRange(openings);
            }

            this.Model.UpdateElements(updatedFloors);

            Assert.Equal(2, floor1.Openings.Count);
        }

        [Fact]
        public void ZeroThickness()
        {
            var model = new Model();
            var poly = Polygon.Rectangle(width: 20, height: 20);
            Assert.Throws<ArgumentOutOfRangeException>(() => { var floorType = new FloorType("test", 0.0); });
        }

        [Fact]
        public void Area()
        {
            // A floor with two holes punched in it.
            var p1 = Polygon.Rectangle(1, 1);
            var p2 = Polygon.Rectangle(1, 1);
            var o1 = new Opening(p1, 1, 1);
            var o2 = new Opening(p2, 3, 3);
            var floorType = new FloorType("test", 0.2);
            var floor = new Floor(Polygon.Rectangle(10, 10), floorType, 0.0, null, new List<Opening>() { o1, o2 });
            Assert.Equal(100.0, floor.Area());
        }
    }
}