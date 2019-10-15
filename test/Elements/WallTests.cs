using System;
using System.Collections.Generic;
using Elements.Geometry;
using Xunit;

namespace Elements.Tests
{
    public class WallTests : ModelTest
    {
        [Fact]
        public void WallWithAddedOpenings()
        {
            this.Name = "WallWithAddedOpenings";

            var l = new Line(new Vector3(0, 0, 0), new Vector3(10, 10, 0));
           
            var w = new StandardWall(l, 0.1, 3.0, null);
            var openings = new List<Opening>(){
                new Opening(1.0, 2.0, 1.0, 1.0, w.Transform),
                new Opening(3.0, 1.0, 1.0, 2.0, w.Transform),
                new Opening(Polygon.Ngon(3, 2.0), 8,2, w.Transform)
            };

            this.Model.AddElement(w);
            this.Model.AddElements(openings);
        }

        [Fact]
        public void ZeroHeight()
        {
            var a = Vector3.Origin;
            var b = new Vector3(0.0, 5.0);
            var line = new Line(a, b);
            Assert.Throws<ArgumentOutOfRangeException>(() => new StandardWall(line, 0.1, 0.0));
        }

        [Fact]
        public void ZeroThickness()
        {
            var a = Vector3.Origin;
            var b = new Vector3(0.0, 5.0);
            var line = new Line(a, b);
            Assert.Throws<ArgumentOutOfRangeException>(() => new StandardWall(line, 0.0, 3.0));
        }

        [Fact]
        public void NonPlanarCenterLine()
        {
            var a = Vector3.Origin;
            var b = new Vector3(0.0, 5.0, 5.0);
            var line = new Line(a, b);
            Assert.Throws<ArgumentException>(() => new StandardWall(line, 0.1, 5.0));
        }
    }
}