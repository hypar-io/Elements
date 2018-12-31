using System;
using System.Collections.Generic;
using Elements;
using Elements.Geometry;
using Xunit;

namespace Hypar.Tests
{
    public class WallTests
    {
        [Fact]
        public void Example()
        {
            var model = new Model();
            var testWallType = new WallType("test", 0.1);

            var triangle = Polygon.Ngon(7, 15.0);
            var openings = new Opening[]{
                new Opening(1.0, 1.0, 2.0, 1.0),
                new Opening(4.0, 0.0, 2.0, 1.0)
            };
            foreach(var l in triangle.Segments())
            {
                var w = new Wall(l, testWallType, 5.0, openings);
                model.AddElement(w);
            }
            
            model.SaveGlb("wall.glb");
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

        [Fact]
        public void TwoOpenings_ProfileWithTwoOpenings()
        {
            var a = Vector3.Origin;
            var b = new Vector3(0.0, 5.0);
            var line = new Line(a,b);
            var o1 = new Opening(1.0, 0.0, 2.0, 1.0);
            var o2 = new Opening(3.0, 1.0, 1.0, 1.0);
            var testWallType = new WallType("test", 0.1);
            var wall = new Wall(line, testWallType, 4.0, new []{o1,o2});
            var model = new Model();
            model.AddElement(wall);
            model.SaveGlb("wall_twoHoles.glb");
            Assert.Equal(2, wall.Profile.Voids.Length);
        }
    }
}