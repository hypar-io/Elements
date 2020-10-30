using System;
using Elements.Geometry;
using Xunit;
using System.Linq;
using System.Collections.Generic;

namespace Elements.Tests
{
    public class ProfileTests : ModelTest
    {
        [Fact]
        public void Profile()
        {
            // Use the generated constructor to test that voids will
            // be set to a default in the validator.
            var profile = new Profile(Polygon.Rectangle(1, 1), null, default(Guid), null);
            Assert.NotNull(profile.Voids);
            profile.Voids.Add(Polygon.Rectangle(0.5, 0.5));
        }

        [Fact]
        public void ProfileMultipleUnion()
        {
            this.Name = "MultipleProfileUnion";
            // small grid of rough circles is unioned
            // 2x3 grid shoud produce 2 openings
            var circle = new Circle(Vector3.Origin, 3).ToPolygon(4);
            var smallCircle = new Circle(Vector3.Origin, 1).ToPolygon(4);

            var seed = new Profile(circle, new List<Polygon> { smallCircle }, Guid.NewGuid(), "");
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    Transform t = new Transform(5 * i, 5 * j, 0);
                    var newCircle = new Profile(new Polygon(circle.Vertices.Select(v => t.OfPoint(v)).ToArray()),
                                                new List<Polygon> { new Polygon(smallCircle.Vertices.Select(v => t.OfPoint(v)).ToArray()) },
                                                Guid.NewGuid(),
                                                "");

                    seed = seed.Union(newCircle);

                    if (seed == null) throw new Exception("Making union failed");
                }
            }
            var floor = new Floor(seed, 1);
            this.Model.AddElement(floor);

            Assert.Equal(8, seed.Voids.Count());
        }

        [Fact]
        public void VoidsOrientedCorrectly()
        {
            this.Name = "VoidsOrientedCorrectly";
            var outerRing = new Polygon(new[]
            {
                new Vector3(0,0,0),
                new Vector3(10,0,0),
                new Vector3(10,10,0),
                new Vector3(0,10,0),
            });
            var innerRing1 = new Polygon(new[]
            {
                new Vector3(2,2,0),
                new Vector3(4,2,0),
                new Vector3(4,4,0),
                new Vector3(2,4,0),
            });
            var innerRing2 = new Polygon(new[]
            {
                new Vector3(8,8,0),
                new Vector3(8,6,0),
                new Vector3(6,6,0),
                new Vector3(6,8,0)
            });

            var profile1 = new Profile(new Polygon[] { innerRing2, outerRing, innerRing1 });
            var profile2 = new Profile(outerRing.Reversed(), new[] { innerRing1, innerRing2 }, Guid.NewGuid(), null);
            foreach (var profile in new[] { profile1, profile2 })
            {
                foreach (var curve in profile.Voids)
                {
                    Assert.NotEqual(curve.IsClockWise(), profile.Perimeter.IsClockWise());
                }
            }

            var mass1 = new Mass(profile1, 1);
            var mass2 = new Mass(profile2, 1, null, new Transform(0, 0, 10));
            Model.AddElement(mass1);
            Model.AddElement(mass2);
        }

        [Fact]
        public void ProfileDoesNotReverseWhenWound()
        {
            this.Name = "ProfileDoesNotReverseWhenWound";

            // This model should show an L and a star shape
            // each with an L profile sweep. The sweep should not
            // invert at any point and should be of constant thickness.

            var l = new Profile(Polygon.L(1.0, 2.0, 0.5));
            var outerL = Polygon.L(5.0, 10.0, 2.0);
            var beam = new Beam(outerL, l);
            this.Model.AddElement(beam);

            Model.AddElements(outerL.Frames(0, 0).SelectMany(f => f.ToModelCurves()));

            var c = new Circle(outerL.Vertices[0], 0.1);
            Model.AddElements(new ModelCurve(c, BuiltInMaterials.XAxis));

            var c1 = new Circle(outerL.Vertices[1], 0.1);
            Model.AddElements(new ModelCurve(c1, BuiltInMaterials.YAxis));

            var star = Polygon.Star(5, 3, 5).Transformed(new Transform(new Vector3(10, 10)));
            var starBeam = new Beam(star, l);
            this.Model.AddElement(starBeam);
        }
    }
}
