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
            this.Name = "WallLinear";
            var testWallType = new WallType("test", 0.1);

            var ngon = Polygon.Ngon(7, 15.0);
            var openings = new Opening[]{
                new Opening(1.0, 2.0, 1.0, 1.0),
                new Opening(3.0, 1.0, 1.0, 2.0),
                // new Opening(7.0, 1.25, 3.0, 2.5),
                new Opening(Polygon.Ngon(5, 2.0), 8,2)
            };

            var frameProfile = new Profile(Polygon.Rectangle(0.075, 0.01));
            foreach(var l in ngon.Segments())
            {
                var w = new Wall(l, testWallType, 3.0, BuiltInMaterials.Default, openings);
                this.Model.AddElement(w);

                // Draw some frames and panels in the openings.
                foreach(var o in w.Openings)
                {
                    var f = new Frame(o.Perimeter, frameProfile, 0.0375, null, w.Transform);
                    this.Model.AddElement(f);
                    var p = new Panel(o.Perimeter, BuiltInMaterials.Glass, w.Transform);
                    this.Model.AddElement(p);
                }
            }
        }

        [Fact]
        public void ZeroHeight()
        {
            var a = Vector3.Origin;
            var b = new Vector3(0.0, 5.0);
            var line = new Line(a,b);
            var testWallType = new WallType("test", 0.1);
            Assert.Throws<ArgumentOutOfRangeException>(()=>new Wall(line, testWallType, 0.0));
        }

        [Fact]
        public void ZeroThickness()
        {
            var a = Vector3.Origin;
            var b = new Vector3(0.0, 5.0);
            var line = new Line(a,b);
            Assert.Throws<ArgumentOutOfRangeException>(()=>{var testWallType = new WallType("test", 0.0);});
        }

        [Fact]
        public void NonPlanarCenterLine()
        {
            var a = Vector3.Origin;
            var b = new Vector3(0.0, 5.0, 5.0);
            var line = new Line(a,b);
            var testWallType = new WallType("test", 0.1);
            Assert.Throws<ArgumentException>(()=>new Wall(line, testWallType, 5.0));
        }

        [Fact]
        public void ProfileWithNoVoids()
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