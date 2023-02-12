using System;
using Elements.Geometry;
using Xunit;

namespace Elements.Tests
{
    public class WallTests : ModelTest
    {
        [Fact, Trait("Category", "Examples")]
        public void Example()
        {
            this.Name = "Elements_StandardWall";

            // <example>
            // Create a wall.
            var line = new Line(new Vector3(0, 0, 0), new Vector3(10, 10, 0));
            var wall = new StandardWall(line, 0.1, 3.0);
            wall.AddOpening(1, 2, 1, 1);
            wall.AddOpening(3, 1, 3.5, 1.5);
            // </example>

            this.Model.AddElement(wall);
        }

        [Fact]
        public void WallWithAddedOpenings()
        {
            this.Name = "WallWithAddedOpenings";

            var p = Polygon.Ngon(5, 10);
            foreach (var l in p.Segments())
            {
                var w = new StandardWall(l, 0.1, 3.0, null);
                w.AddOpening(1, 1, 1, 2, 1.0, 1.0);
                w.AddOpening(1, 2, 3, 1, 1.0, 1.0);
                w.AddOpening(Polygon.Ngon(3, 2.0), 8, 2, 1.0, 0.0);
                this.Model.AddElement(w);
            }
        }

        [Fact]
        public void WallByProfileFromProfileCreatesOpenings()
        {
            this.Name = nameof(WallByProfileFromProfileCreatesOpenings);

            var boundary = new Polygon(new[] {  new Vector3(),
                                                new Vector3(0,0,10),
                                                new Vector3(0,10,10),
                                                new Vector3(0,10,0) });
            var window = new Polygon(new[] {new Vector3(0,2,2),
                                            new Vector3(0,2,4),
                                            new Vector3(0,4,4),
                                            new Vector3(0,4,2)});
            var wallProfile = new Profile(boundary, window);
            var wall = new WallByProfile(wallProfile, 0.1, new Line(Vector3.Origin, new Vector3(0, 10)));

            Assert.Single(wall.Openings);
            Assert.True(wall.GetProfile().Equals(wallProfile));

            this.Model.AddElement(wall);
            this.Model.AddElement(wall.Centerline);
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
            var line = new Line(a, b);
            var wall1 = new StandardWall(line, 0.1, 3.0, BuiltInMaterials.Mass);
            this.Model.AddElement(wall1);

            var c = new Vector3(0, 0, 3.0);
            var d = new Vector3(0, 5, 3.0);
            var line1 = new Line(c, d);
            var wall2 = new StandardWall(line1, 0.1, 3.0, BuiltInMaterials.Void);
            wall2.Transform.Rotate(Vector3.ZAxis, 45);
            this.Model.AddElement(wall2);

            var json = this.Model.ToJson();

            var newModel = Model.FromJson(json);

            // Add the new walls back to the original model
            // and transform them
            var walls = newModel.AllElementsOfType<StandardWall>();
            foreach (var w in walls)
            {
                w.Id = Guid.NewGuid();
                w.Transform.Move(new Vector3(5, 0));
                this.Model.AddElement(w);
            }
        }
    }
}