using System;
using Hypar.Elements;
using Hypar.Geometry;
using Xunit;

namespace Hypar.Tests
{
    public class WallTests
    {
        [Fact]
        public void Example()
        {
            var a = Vector3.Origin;
            var b = new Vector3(0.0, 5.0);
            var line = new Line(a,b);
            var wall = new Wall(line, 0.1, 5.0);

            var model = new Model();
            model.AddElement(wall);
            model.SaveGlb("wall.glb");
        }

        [Fact]
        public void ZeroHeight_ThrowsException()
        {
            var a = Vector3.Origin;
            var b = new Vector3(0.0, 5.0);
            var line = new Line(a,b);
            Assert.Throws<ArgumentOutOfRangeException>(()=>new Wall(line, 0.1, 0.0));
        }

        [Fact]
        public void ZeroThickness_ThrowsException()
        {
            var a = Vector3.Origin;
            var b = new Vector3(0.0, 5.0);
            var line = new Line(a,b);
            Assert.Throws<ArgumentOutOfRangeException>(()=>new Wall(line, 0.0, 5.0));
        }

        [Fact]
        public void NonPlanarCenterLine_ThrowsException()
        {
            var a = Vector3.Origin;
            var b = new Vector3(0.0, 5.0, 5.0);
            var line = new Line(a,b);
            Assert.Throws<ArgumentException>(()=>new Wall(line, 0.1, 5.0));
        }
    }
}