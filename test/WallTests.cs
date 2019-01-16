using System;
using System.Collections.Generic;
using Elements;
using Elements.Geometry;
using Xunit;

namespace Elements.Tests
{
    public class WallTests : ModelTest
    {
        [Fact]
        public void Wall()
        {
            this.Name = "Wall";
            var testWallType = new WallType("test", 0.1);

            var triangle = Polygon.Ngon(7, 15.0);
            var openings = new Opening[]{
                new Opening(Polygon.Rectangle(Vector3.Origin, 1.0, 1.0), 0.1,  new Transform(new Vector3(2.0, 1.0))),
                new Opening(Polygon.Rectangle(Vector3.Origin, 1.0, 2.0), 0.1, new Transform(new Vector3(4.0, 0.0)))
            };
            foreach(var l in triangle.Segments())
            {
                var w = new Wall(l, testWallType, 5.0, BuiltInMaterials.Default, openings);
                this.Model.AddElement(w);
            }
        }

        [Fact]
        public void ZeroHeight_ThrowsException()
        {
            var a = Vector3.Origin;
            var b = new Vector3(0.0, 5.0);
            var line = new Line(a,b);
            var testWallType = new WallType("test", 0.1);
            Assert.Throws<ArgumentOutOfRangeException>(()=>new Wall(line, testWallType, 0.0));
        }

        [Fact]
        public void ZeroThickness_ThrowsException()
        {
            var a = Vector3.Origin;
            var b = new Vector3(0.0, 5.0);
            var line = new Line(a,b);
            Assert.Throws<ArgumentOutOfRangeException>(()=>{var testWallType = new WallType("test", 0.0);});
        }

        [Fact]
        public void NonPlanarCenterLine_ThrowsException()
        {
            var a = Vector3.Origin;
            var b = new Vector3(0.0, 5.0, 5.0);
            var line = new Line(a,b);
            var testWallType = new WallType("test", 0.1);
            Assert.Throws<ArgumentException>(()=>new Wall(line, testWallType, 5.0));
        }

        [Fact]
        public void NullOpenings_ProfileWithNoVoids()
        {
            var a = Vector3.Origin;
            var b = new Vector3(0.0, 5.0);
            var line = new Line(a,b);
            var testWallType = new WallType("test", 0.1);
            var wall = new Wall(line, testWallType, 4.0);
            Assert.Null(wall.Profile.Voids);
        }
    }
}