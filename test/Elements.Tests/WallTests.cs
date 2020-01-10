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
                new Opening(1.0, 2.0, 1.0, 1.0),
                new Opening(3.0, 1.0, 1.0, 2.0),
                new Opening(Polygon.Ngon(3, 2.0), 8,2)
            };
            w.Openings.AddRange(openings);

            this.Model.AddElement(w);
        }

        [Fact]
        public void ZeroHeightThrows()
        {
            var a = Vector3.Origin;
            var b = new Vector3(0.0, 5.0);
            var line = new Line(a, b);
            Assert.Throws<ArgumentOutOfRangeException>(() => new StandardWall(line, 0.1, 0.0));
        }

        [Fact]
        public void ZeroThicknessThrows()
        {
            var a = Vector3.Origin;
            var b = new Vector3(0.0, 5.0);
            var line = new Line(a, b);
            Assert.Throws<ArgumentOutOfRangeException>(() => new StandardWall(line, 0.0, 3.0));
        }

        [Fact]
        public void NonPlanarCenterLineThrows()
        {
            var a = Vector3.Origin;
            var b = new Vector3(0.0, 5.0, 5.0);
            var line = new Line(a, b);
            Assert.Throws<ArgumentException>(() => new StandardWall(line, 0.1, 5.0));
        }

        [Fact]
        public void TransformedAndSerializedWalls()
        {
            this.Name = "TransformedSerializedWalls";

            var a = Vector3.Origin;
            var b = new Vector3(0, 5.0, 0.0);
            var line = new Line(a,b);
            var wall1 = new StandardWall(line, 0.1, 3.0, BuiltInMaterials.Mass);
            this.Model.AddElement(wall1);

            var c = new Vector3(0, 0, 3.0);
            var d = new Vector3(0, 5, 3.0);
            var line1 = new Line(c,d);
            var wall2 = new StandardWall(line1, 0.1, 3.0, BuiltInMaterials.Void);
            wall2.Transform.Rotate(Vector3.ZAxis, 45);
            this.Model.AddElement(wall2);

            var json = this.Model.ToJson();

            var newModel = Model.FromJson(json);

            // Add the new walls back to the original model
            // and transform them
            var walls = newModel.AllElementsOfType<StandardWall>();
            foreach(var w in walls)
            {
                w.Id = Guid.NewGuid();
                w.Transform.Move(new Vector3(5,0));
                this.Model.AddElement(w);
            }
        }
    }
}