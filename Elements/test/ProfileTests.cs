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
        public void ProfileUnionAll()
        {
            this.Name = "ProfileUnionAll";
            var outer1 = new Circle(Vector3.Origin, 5).ToPolygon(10);
            var inner1 = new Circle(Vector3.Origin, 4).ToPolygon(10);
            var outer2 = new Circle(new Vector3(9, 0, 0), 5).ToPolygon(10);
            var inner2 = new Circle(new Vector3(9, 0, 0), 4).ToPolygon(10);
            var outer3 = new Circle(new Vector3(4.5, 12, 0), 5).ToPolygon(10);
            var inner3 = new Circle(new Vector3(4.5, 12, 0), 4).ToPolygon(10);
            var p1 = new Profile(outer1, inner1);
            var p2 = new Profile(outer2, inner2);
            var p3 = new Profile(outer3, inner3);
            var union = Elements.Geometry.Profile.UnionAll(new[] { p1, p2, p3 });
            foreach (var profile in union)
            {
                var floor = new Floor(profile, 1);
                this.Model.AddElement(floor);
            }
            Assert.Equal(2, union.Count);

        }

        [Fact]
        public void ProfileDifference()
        {
            Name = "Profile Difference";
            var rect = Polygon.Rectangle(10, 10);
            var rect2 = rect.TransformedPolygon(new Transform(11, 0, 0));
            var difference = Elements.Geometry.Profile.Difference(new[] { new Profile(rect) }, new[] { new Profile(rect2) });

            var innerRect = Polygon.Rectangle(3, 3);
            var grid2d = new Elements.Spatial.Grid2d(new[] { rect, innerRect });
            grid2d.U.DivideByCount(10);
            grid2d.V.DivideByCount(10);
            var secondSet = new[] {
                new Profile(new Circle(new Vector3(4,4), 4).ToPolygon(), new Circle(new Vector3(4,4), 2).ToPolygon()),
                new Profile(new Circle(new Vector3(-4,-4), 4).ToPolygon(), new Circle(new Vector3(-4,-4), 2).ToPolygon())
            };

            foreach (var cell in grid2d.GetCells())
            {
                var crvs = cell.GetTrimmedCellGeometry().OfType<Polygon>().ToList();
                var profiles = crvs.Select(p => new Profile(p));
                var differenceResult = Elements.Geometry.Profile.Difference(profiles, secondSet);
                var floors = differenceResult.Select(p => new Floor(p, 1));
                var mcs = differenceResult.Select(p => new ModelCurve(p.Perimeter, transform: new Transform(0, 0, 1.1)));
                Model.AddElements(floors);
                Model.AddElements(mcs);
            }
            Assert.Equal(84, Model.AllElementsOfType<Floor>().Count());
        }

        [Fact]
        public void ProfileIntersection()
        {
            Name = "Profile Intersection";
            var firstSet = new List<Profile>();
            for (int i = 0; i < 10; i++)
            {
                var angle = (i / 10.0) * Math.PI * 2;
                var center = new Vector3(4 * Math.Cos(angle), 4 * Math.Sin(angle));
                var outerCircle = new Circle(center, 5).ToPolygon(20);
                var innerCircle = new Circle(center, 4).ToPolygon(20);
                var location = new Transform(1, 0, 0);
                var profile = new Profile(outerCircle, innerCircle);
                firstSet.Add(profile);
                Model.AddElement(new Floor(profile, 0.04));
            }
            var clipProfile = new Profile(Polygon.Rectangle(20, 10), Polygon.Rectangle(5, 5));
            Model.AddElement(new Floor(clipProfile, 0.1));
            var secondSet = new List<Profile> {
                clipProfile,
            };
            var intersection = Elements.Geometry.Profile.Intersection(firstSet, secondSet);
            var floors = intersection.Select(p => new Floor(p, 0.4, material: BuiltInMaterials.XAxis));
            Model.AddElements(floors);
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
            var frames = outerL.Frames(0, 0);

            Model.AddElements(frames.SelectMany(f => f.ToModelCurves()));
            var star = Polygon.Star(5, 3, 5).Transformed(new Transform(new Vector3(10, 10)));
            var starBeam = new Beam(star, l);
            this.Model.AddElement(starBeam);

            // Test that the X axes do not invert from one to the next.
            for (var i = 0; i < frames.Count(); i++)
            {
                var a = frames[i];
                var b = frames[(i + 1) % frames.Count()];
                var dot = a.XAxis.Dot(b.XAxis);
                Assert.True(dot >= 0);
            }
        }

        [Fact]
        public void DeeplyNestedProfileBooleans()
        {
            Name = "Deeply Nested Profile Booleans";
            var perimeter1 = Polygon.Rectangle(new Vector3(0, 0), new Vector3(50, 50));
            var void1 = Polygon.Rectangle(new Vector3(24, 24), new Vector3(26, 26));
            var profile1 = new Profile(perimeter1, void1);

            var perimeter2 = Polygon.Rectangle(new Vector3(10, 10), new Vector3(40, 40));
            var void2 = Polygon.Rectangle(new Vector3(12, 12), new Vector3(38, 38));
            var profile2 = new Profile(perimeter2, void2);

            var perimeter3 = Polygon.Rectangle(new Vector3(15, 15), new Vector3(35, 35));
            var void3 = Polygon.Rectangle(new Vector3(17, 17), new Vector3(32, 32));
            var profile3 = new Profile(perimeter3, void3);

            var difference = Elements.Geometry.Profile.Difference(new[] { profile1 }, new[] { profile2, profile3 });
            Model.AddElements(difference.Select(d => (new Floor(d, 0.1))));
            Assert.Equal(3, difference.Count);
        }

        [Fact]
        public void SplitProfiles()
        {
            Name = "SplitProfiles";
            var profile = new Profile(
                Polygon.Rectangle(20, 20),
                new[] {
                    Polygon.Rectangle(5, 5),
                    Polygon.Rectangle(new Vector3(8, -8), new Vector3(9, 9))
                    },
                Guid.NewGuid(), null);
            var polyline = new Polyline(new[] {
                new Vector3(-21, 0),
                new Vector3(0, 5),
                new Vector3(21, -4),
            });
            Model.AddElement(polyline);
            Model.AddElements(profile.ToModelCurves());
            var split = Elements.Geometry.Profile.Split(new[] { profile }, new[] { polyline });
            var random = new Random(4);
            Model.AddElements(split.Select(s => new Floor(s, 0.1, new Transform(), random.NextMaterial())));
            Assert.Equal(2, split.Count);
            Assert.Equal(20 * 20 - 5 * 5 - 17, split.Sum(s => s.Area()), 4);
        }
    }
}
