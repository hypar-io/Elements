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
            var floor1 = new Floor(p, 0.1, 0.5, material: new Material("green", Colors.Green, 0.0f, 0.0f));

            var transRotate = new Transform();
            transRotate.Rotate(Vector3.ZAxis, 20.0);
            var floor2 = new Floor(p, 0.1, 2.0, transRotate, material: new Material("blue", Colors.Blue, 0.0f, 0.0f));
            var openings = new List<Opening>(){
                new Opening(1, 1, 1, 1, floor1.Transform),
                new Opening(3, 3, 1, 3, floor1.Transform),
            };

            Assert.Equal(0.5, floor1.Elevation);
            Assert.Equal(0.1, floor1.Thickness);
            Assert.Equal(0.5, floor1.Transform.Origin.Z);
            
            this.Model.AddElements(new[]{floor1, floor2});
        }

        [Fact]
        public void ZeroThickness()
        {
            var model = new Model();
            var poly = Polygon.Rectangle(width: 20, height: 20);
            Assert.Throws<ArgumentOutOfRangeException>(() => new Floor(poly, 0.0));
        }

        [Fact]
        public void Area()
        {
            // A floor with two holes punched in it.
            var p1 = Polygon.Rectangle(1, 1);
            var p2 = Polygon.Rectangle(1, 1);
            var o1 = new Opening(p1, 1, 1);
            var o2 = new Opening(p2, 3, 3);
            var floor = new Floor(Polygon.Rectangle(10, 10), 0.2, 0.0);
            Assert.Equal(100.0, floor.Area());
        }
    }
}