using System;
using Elements.Geometry;
using Xunit;
using System.Linq;
using System.Collections.Generic;
using Elements.Spatial;
using System.IO;
using Newtonsoft.Json;

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
        public void ProfileComplexUnionAll()
        {
            this.Name = "ProfileComplexUnionAll";
            var profile1 = new Profile(new Polygon(new Vector3[] {
                new Vector3(19.3124, 32.0831),
                new Vector3(19.3072, 42.1189),
                new Vector3(19.3124, 42.1189),
                new Vector3(19.3124, 50.1435),
                new Vector3(70.7491, 50.1435),
                new Vector3(70.7491, 43.6189),
                new Vector3(72.2491, 43.6189),
                new Vector3(72.2491, 51.6435),
                new Vector3(17.8124, 51.6435),
                new Vector3(17.8124, 22.5907),
                new Vector3(19.3124, 22.5907)
            }));
            var profile2 = new Profile(new Polygon(new Vector3[] {
                new Vector3(72.2491, 30.6189),
                new Vector3(70.7491, 30.6189),
                new Vector3(70.7491, 10.8835),
                new Vector3(72.2491, 10.8835)
            }));
            var profile3 = new Profile(new Polygon(new Vector3[] {
                new Vector3(70.7491, 24.0907),
                new Vector3(19.3124, 24.0907),
                new Vector3(19.3124, 22.5907),
                new Vector3(70.7491, 22.5907)
            }));
            var profile4 = new Profile(new Polygon(new Vector3[] {
                new Vector3(19.3072, 42.1189),
                new Vector3(19.3124, 42.1189),
                new Vector3(19.3124, 43.5831),
                new Vector3( 8.0830, 43.5831),
                new Vector3( 8.0830, 42.0831),
                new Vector3(19.3072, 42.0831)
            }));
            var profile5 = new Profile(new Polygon(new Vector3[] {
                new Vector3(17.8124, 51.6435),
                new Vector3( 8.0830, 51.6435),
                new Vector3( 8.0830, 50.1435),
                new Vector3(17.8124, 50.1435)
            }));
            var profile6 = new Profile(new Polygon(new Vector3[] {
                new Vector3(86.4220, 43.6189),
                new Vector3(75.3654, 43.6189),
                new Vector3(75.3654, 42.1189),
                new Vector3(86.4220, 42.1189)
            }));
            var profile7 = new Profile(new Polygon(new Vector3[] {
                new Vector3(32.5299, 30.6189),
                new Vector3(31.0299, 30.6189),
                new Vector3(31.0299, 22.5907),
                new Vector3(32.5299, 22.5907)
            }));
            var profile8 = new Profile(new Polygon(new Vector3[] {
                new Vector3(58.8651, 30.6189),
                new Vector3(56.7315, 30.6189),
                new Vector3(56.7315, 24.0907),
                new Vector3(58.8651, 24.0907)
            }));
            var profile9 = new Profile(new Polygon(new Vector3[] {
                new Vector3(81.0193, 56.0046),
                new Vector3(81.0193, 31.7802),
                new Vector3(75.3654, 31.7802),
                new Vector3(75.3654, 30.6189),
                new Vector3(82.1806, 30.6189),
                new Vector3(82.1806, 56.0046)
            }));
            var profile10 = new Profile(new Polygon(new Vector3[] {
                new Vector3(75.3654, 43.6189),
                new Vector3(17.8124, 43.6189),
                new Vector3(17.8124, 30.6189),
                new Vector3(75.3654, 30.6189)
                /*void*/            }), new Polygon(new Vector3[] {
                new Vector3(73.8707, 32.0831),
                new Vector3(19.3124, 32.0831),
                new Vector3(19.3072, 42.1189),
                new Vector3(73.8654, 42.1189)
            }));
            var unions = Geometry.Profile.UnionAll(new[] {
                profile1, profile2, profile3, profile4, profile5,
                profile6, profile7, profile8, profile9, profile10 });
            Assert.Single(unions);
            var union = unions.First();
            Assert.Equal(6, union.Voids.Count);
            Assert.DoesNotContain(union.Perimeter.Vertices, v => union.Perimeter.Segments().Any(s => s.PointOnLine(v)));
            Assert.DoesNotContain(union.Voids, vp => vp.Vertices.Any(v => vp.Segments().Any(s => s.PointOnLine(v))));
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
            var star = Polygon.Star(5, 3, 5).TransformedPolygon(new Transform(new Vector3(10, 10)));
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
            Name = "Profile_Split";
            var random = new Random(4);
            var profile = new Profile(
                Polygon.Rectangle(20, 20),
                new[] {
                    Polygon.Rectangle(5, 5),
                    Polygon.Rectangle(new Vector3(8, -8), new Vector3(9, 9))
                    },
                Guid.NewGuid(), null);
            // this one crosses all the way thru, and should split the profile.
            var polyline1 = new Polyline(new[] {
                new Vector3(-21, 0),
                new Vector3(0, 5),
                new Vector3(21, -4),
            });
            // this one doesn't and should be ignored by the split
            var polyline2 = new Polyline(new[] {
                new Vector3(-21, -6),
                new Vector3(0, -8),
            });
            //this one is totally internal and should also be ignored by the split
            var polyline3 = new Polyline(new[] {
                new Vector3(2, -4),
                new Vector3(-2, -7)
            });
            Model.AddElement(polyline1);
            Model.AddElement(polyline2);
            Model.AddElement(polyline3);
            Model.AddElements(profile.ToModelCurves());
            var split = Elements.Geometry.Profile.Split(new[] { profile }, new[] { polyline1, polyline2, polyline3 });
            Model.AddElements(split.SelectMany(s => s.ToModelCurves()));
            Model.AddElements(split.Select(s => new Floor(s, 0.1, new Transform(), random.NextMaterial())));
            Assert.Equal(2, split.Count);
            Assert.Equal(20 * 20 - 5 * 5 - 17, split.Sum(s => s.Area()), 4);

            // these splitters only cross into a void, but not all the way across.
            var donutProfile = new Profile(Polygon.Rectangle(5, 5), new[] { Polygon.Rectangle(2.5, 2.5), Polygon.Rectangle(0.5, 0.5).TransformedPolygon(new Transform(1.875, 1.875, 0)) }, Guid.NewGuid(), null);
            var splitters = new[] {
                new Line(new Vector3(-3, 0), new Vector3(-1,0)),
                new Line(new Vector3(0, 3), new Vector3(0, 1)),
                new Line(new Vector3(3, 0), new Vector3(1,0)),
                new Line(new Vector3(0, -3), new Vector3(0,-1))
                };
            Model.AddElements(splitters.Select(s => new ModelCurve(s)));
            var donutSplitResults = Elements.Geometry.Profile.Split(new[] { donutProfile }, splitters.Select(s => s.ToPolyline(1)));
            Model.AddElements(donutSplitResults.Select(s => new Floor(s, 0.1, new Transform(0, 0, 1), random.NextMaterial())));
            Assert.Equal(4, donutSplitResults.Count);
            Assert.Equal(donutProfile.Area(), donutSplitResults.Sum(s => s.Area()), 5);


            // two splitters cross in partially and intersect.
            var pos = new Transform(0, 30, 0);
            var partialIntersectProfile = new Profile(Polygon.Rectangle(8, 8).TransformedPolygon(pos), Polygon.Rectangle(3, 3).TransformedPolygon(pos));
            var lines = new[] {
                    new Line(new Vector3(-2,-1), new Vector3(-5, 4)),
                    new Line(new Vector3(-2,1), new Vector3(-5, -4)),
                };
            var partialIntersectSplitResults = Elements.Geometry.Profile.Split(new[] { partialIntersectProfile }, lines.Select(s => s.TransformedLine(pos).ToPolyline(1)));
            Model.AddElements(partialIntersectSplitResults.Select(s => new Floor(s, 0.1, new Transform(0, 0, 1), random.NextMaterial())));
            Assert.Equal(2, partialIntersectSplitResults.Count);
            Assert.Equal(partialIntersectProfile.Area(), partialIntersectSplitResults.Sum(s => s.Area()), 5);

            // splitters cross between two voids
            var pos2 = new Transform(10, 30, 0);
            var doubleVoidProfile = new Profile(
                Polygon.Rectangle(8, 8).TransformedPolygon(pos2),
                new[] {
                    Polygon.Rectangle(new Vector3(-3, -3), new Vector3(-1, -1)).TransformedPolygon(pos2),
                    Polygon.Rectangle(new Vector3(1, 1), new Vector3(3, 3)).TransformedPolygon(pos2),
                },
                 Guid.NewGuid(), null);
            var doubleVoidSplitters = new[] {
                    new Line(new Vector3(-3,-2), new Vector3(2, 3)),
                    new Line(new Vector3(-2,-3), new Vector3(3, 2)),
                };
            var doubleVoidSplitResults = Elements.Geometry.Profile.Split(new[] { doubleVoidProfile }, doubleVoidSplitters.Select(s => s.TransformedLine(pos2).ToPolyline(1)));
            Model.AddElements(doubleVoidSplitResults.Select(s => new Floor(s, 0.1, new Transform(0, 0, 1), random.NextMaterial())));
            Assert.Equal(2, doubleVoidSplitResults.Count);
            Assert.Equal(doubleVoidProfile.Area(), doubleVoidSplitResults.Sum(s => s.Area()), 5);
        }

        [Fact]
        public void CreateProfilesFromPolygons()
        {
            Name = nameof(CreateProfilesFromPolygons);
            var polygons = new List<Polygon> {
                Polygon.Rectangle(10,10),
                Polygon.Star(4,2,5),
                Polygon.Ngon(3,1),
                Polygon.Rectangle(new Vector3(6, -5), new Vector3(11,0))
            };
            var profiles = Elements.Geometry.Profile.CreateFromPolygons(polygons);
            Assert.True(profiles.Count == 3, $"There were {profiles.Count} profiles, 3 expected.");
            var extrudes = profiles.Select(p => new Geometry.Solids.Extrude(p, 1, Vector3.ZAxis, false));
            var rep = new Representation(new List<Geometry.Solids.SolidOperation>(extrudes));
            var geoElem = new GeometricElement(new Transform(), BuiltInMaterials.Mass, rep, false, Guid.NewGuid(), null);
            Model.AddElement(geoElem);
        }

        [Fact]
        public void DifferenceFail()
        {
            // This test used to fail with an exception due to a very small polygon produced from the boolean.
            var aShapeJson = "{\"discriminator\":\"Elements.Geometry.Profile\",\"Perimeter\":{\"discriminator\":\"Elements.Geometry.Polygon\",\"Vertices\":[{\"X\":-149.51680916003411,\"Y\":-545.6788101645068,\"Z\":0.0},{\"X\":-181.13449678692302,\"Y\":-508.57118930829984,\"Z\":0.0},{\"X\":-185.00173212061222,\"Y\":-476.0495417216826,\"Z\":0.0},{\"X\":-211.47915563014516,\"Y\":-479.19804234633307,\"Z\":0.0},{\"X\":-213.43484799507593,\"Y\":-627.6323068549372,\"Z\":0.0},{\"X\":-169.59832064900132,\"Y\":-624.8068240421294,\"Z\":0.0},{\"X\":-144.53329588528302,\"Y\":-620.4717620034767,\"Z\":0.0}]},\"Voids\":[{\"discriminator\":\"Elements.Geometry.Polygon\",\"Vertices\":[{\"X\":-174.65966226538052,\"Y\":-581.4103523143159,\"Z\":0.0},{\"X\":-174.89675190025594,\"Y\":-599.4087908161704,\"Z\":0.0},{\"X\":-184.89588440128614,\"Y\":-599.2770743523507,\"Z\":0.0},{\"X\":-184.65879476641072,\"Y\":-581.2786358504962,\"Z\":0.0}]}],\"Id\":\"5b65b6a5-b0a4-4373-ba47-76380d379893\",\"Name\":null}";
            var p1 = Newtonsoft.Json.JsonConvert.DeserializeObject<Profile>(aShapeJson);
            var otherPolysJson = "[{\"discriminator\":\"Elements.Geometry.Profile\",\"Perimeter\":{\"discriminator\":\"Elements.Geometry.Polygon\",\"Vertices\":[{\"X\":-170.30455,\"Y\":-618.83988,\"Z\":0.0},{\"X\":-150.8792,\"Y\":-615.48022,\"Z\":0.0},{\"X\":-155.37154,\"Y\":-548.05882,\"Z\":0.0},{\"X\":-186.87827,\"Y\":-511.08143,\"Z\":0.0},{\"X\":-190.01511,\"Y\":-484.70206,\"Z\":0.0},{\"X\":-205.57554,\"Y\":-486.55239,\"Z\":0.0},{\"X\":-207.34994,\"Y\":-621.22765,\"Z\":0.0}]},\"Voids\":[{\"discriminator\":\"Elements.Geometry.Polygon\",\"Vertices\":[{\"X\":-205.82871,\"Y\":-619.62649,\"Z\":0.0},{\"X\":-204.09299,\"Y\":-487.88667,\"Z\":0.0},{\"X\":-191.3275,\"Y\":-486.36869,\"Z\":0.0},{\"X\":-188.31421,\"Y\":-511.70899,\"Z\":0.0},{\"X\":-156.83522,\"Y\":-548.65382,\"Z\":0.0},{\"X\":-152.46567,\"Y\":-614.23233,\"Z\":0.0},{\"X\":-170.48117,\"Y\":-617.34816,\"Z\":0.0}]}],\"Id\":\"0fbf2cac-126b-45a3-b32e-e53c32c9e05a\",\"Name\":\"Corridor\"},{\"discriminator\":\"Elements.Geometry.Profile\",\"Perimeter\":{\"discriminator\":\"Elements.Geometry.Polygon\",\"Vertices\":[{\"X\":-153.1705,\"Y\":-580.19402,\"Z\":0.0},{\"X\":-206.8,\"Y\":-579.48763,\"Z\":0.0},{\"X\":-206.81976,\"Y\":-580.98749,\"Z\":0.0},{\"X\":-184.6588,\"Y\":-581.27939,\"Z\":0.0},{\"X\":-184.65879,\"Y\":-581.27864,\"Z\":0.0},{\"X\":-174.65966,\"Y\":-581.41035,\"Z\":0.0},{\"X\":-174.65967,\"Y\":-581.41109,\"Z\":0.0},{\"X\":-153.19026,\"Y\":-581.69388,\"Z\":0.0}]},\"Voids\":[],\"Id\":\"a02d7813-883b-4bd4-8bd5-47aa0ce8ac05\",\"Name\":null},{\"discriminator\":\"Elements.Geometry.Profile\",\"Perimeter\":{\"discriminator\":\"Elements.Geometry.Polygon\",\"Vertices\":[{\"X\":-184.89513,\"Y\":-599.27708,\"Z\":0.0},{\"X\":-184.89588,\"Y\":-599.27707,\"Z\":0.0},{\"X\":-184.65879,\"Y\":-581.27864,\"Z\":0.0},{\"X\":-184.65805,\"Y\":-581.27865,\"Z\":0.0},{\"X\":-183.76997,\"Y\":-513.85917,\"Z\":0.0},{\"X\":-185.26983,\"Y\":-513.83941,\"Z\":0.0},{\"X\":-186.66607,\"Y\":-619.83625,\"Z\":0.0},{\"X\":-185.16621,\"Y\":-619.85601,\"Z\":0.0}]},\"Voids\":[],\"Id\":\"59c1f71d-7c88-4fa3-8b79-6d799f6f80f0\",\"Name\":null},{\"discriminator\":\"Elements.Geometry.Profile\",\"Perimeter\":{\"discriminator\":\"Elements.Geometry.Polygon\",\"Vertices\":[{\"X\":-151.87004,\"Y\":-599.71136,\"Z\":0.0},{\"X\":-174.89674,\"Y\":-599.40804,\"Z\":0.0},{\"X\":-174.89675,\"Y\":-599.40879,\"Z\":0.0},{\"X\":-184.89588,\"Y\":-599.27707,\"Z\":0.0},{\"X\":-184.89587,\"Y\":-599.27633,\"Z\":0.0},{\"X\":-207.05688,\"Y\":-598.98441,\"Z\":0.0},{\"X\":-207.07664,\"Y\":-600.48427,\"Z\":0.0},{\"X\":-151.8898,\"Y\":-601.21122,\"Z\":0.0}]},\"Voids\":[],\"Id\":\"b320c3f1-eac5-46a9-9ed5-3bb49a5e38e9\",\"Name\":null},{\"discriminator\":\"Elements.Geometry.Profile\",\"Perimeter\":{\"discriminator\":\"Elements.Geometry.Polygon\",\"Vertices\":[{\"X\":-172.44555,\"Y\":-527.14991,\"Z\":0.0},{\"X\":-173.94541,\"Y\":-527.13015,\"Z\":0.0},{\"X\":-174.66041,\"Y\":-581.41034,\"Z\":0.0},{\"X\":-174.65966,\"Y\":-581.41035,\"Z\":0.0},{\"X\":-174.89675,\"Y\":-599.40879,\"Z\":0.0},{\"X\":-174.89749,\"Y\":-599.40878,\"Z\":0.0},{\"X\":-175.1568,\"Y\":-619.09442,\"Z\":0.0},{\"X\":-173.65694,\"Y\":-619.11418,\"Z\":0.0}]},\"Voids\":[],\"Id\":\"82060be1-c38b-4eaf-aa5f-6792f6839f37\",\"Name\":null}]";
            var otherPolys = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Profile>>(otherPolysJson);
            var result = Elements.Geometry.Profile.Difference(new[] { p1 }, otherPolys);
        }

        [Fact]
        public void DifferenceFail2()
        {
            this.Name = nameof(DifferenceFail2);
            var json = File.ReadAllText("../../../models/Geometry/differenceFail2.json");
            var profiles = JsonConvert.DeserializeObject<Dictionary<string, IEnumerable<Profile>>>(json);
            var firstSet = profiles["levelBoundaryCleaned"];
            var secondSet = profiles["insetProfiles"];
            var diff = Elements.Geometry.Profile.Difference(firstSet, secondSet);
            Model.AddElements(firstSet.SelectMany(p => p.ToModelCurves(material: BuiltInMaterials.XAxis)));
            Model.AddElements(secondSet.SelectMany(p => p.ToModelCurves(material: BuiltInMaterials.YAxis)));
            Model.AddElements(diff.SelectMany(p => p.ToModelCurves(new Transform(0, 0, 0.4), BuiltInMaterials.ZAxis)));

        }

        [Fact]
        public void SplitDonutProfileFromFunction()
        {
            Name = "SplitDonutProfileFromFunction";

            var perim = Newtonsoft.Json.JsonConvert.DeserializeObject<Polygon>("{\"discriminator\":\"Elements.Geometry.Polygon\",\"Vertices\":[{\"X\":76.30921,\"Y\":46.63686,\"Z\":0.0},{\"X\":-75.42933,\"Y\":46.63686,\"Z\":0.0},{\"X\":-75.42933,\"Y\":-30.00397,\"Z\":0.0},{\"X\":76.30921,\"Y\":-30.00397,\"Z\":0.0}]}");
            var voids = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Polygon>>("[{\"discriminator\":\"Elements.Geometry.Polygon\",\"Vertices\":[{\"X\":-69.42933,\"Y\":-24.00397,\"Z\":0.0},{\"X\":-69.42933,\"Y\":40.63686,\"Z\":0.0},{\"X\":70.30921,\"Y\":40.63686,\"Z\":0.0},{\"X\":70.30921,\"Y\":-24.00397,\"Z\":0.0}]}]");
            var profileToSplit = new Profile(perim, voids, Guid.NewGuid(), null);
            var splitters = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Polyline>>("[{\"discriminator\":\"Elements.Geometry.Polyline\",\"Vertices\":[{\"X\":76.40920999999999,\"Y\":46.63686,\"Z\":0.0},{\"X\":-75.52932999999997,\"Y\":46.63686,\"Z\":0.0}]},{\"discriminator\":\"Elements.Geometry.Polyline\",\"Vertices\":[{\"X\":-75.42933,\"Y\":46.83686,\"Z\":0.0},{\"X\":-75.42933,\"Y\":-30.20397,\"Z\":0.0}]},{\"discriminator\":\"Elements.Geometry.Polyline\",\"Vertices\":[{\"X\":-75.52932999999999,\"Y\":-30.00397,\"Z\":0.0},{\"X\":76.40920999999997,\"Y\":-30.00397,\"Z\":0.0}]},{\"discriminator\":\"Elements.Geometry.Polyline\",\"Vertices\":[{\"X\":76.30921,\"Y\":-30.20397,\"Z\":0.0},{\"X\":76.30921,\"Y\":46.83686,\"Z\":0.0}]},{\"discriminator\":\"Elements.Geometry.Polyline\",\"Vertices\":[{\"X\":-69.42933,\"Y\":-30.103969999999997,\"Z\":0.0},{\"X\":-69.42933,\"Y\":46.73686,\"Z\":0.0}]},{\"discriminator\":\"Elements.Geometry.Polyline\",\"Vertices\":[{\"X\":-69.52932999999999,\"Y\":40.63686,\"Z\":0.0},{\"X\":70.40920999999997,\"Y\":40.63686,\"Z\":0.0}]},{\"discriminator\":\"Elements.Geometry.Polyline\",\"Vertices\":[{\"X\":70.30921,\"Y\":46.73686000000001,\"Z\":0.0},{\"X\":70.30921,\"Y\":-30.103969999999997,\"Z\":0.0}]},{\"discriminator\":\"Elements.Geometry.Polyline\",\"Vertices\":[{\"X\":70.40920999999999,\"Y\":-24.00397,\"Z\":0.0},{\"X\":-69.52932999999997,\"Y\":-24.00397,\"Z\":0.0}]}]");
            Model.AddElement(perim);
            voids.ForEach((v) => Model.AddElement(v));
            Model.AddElements(splitters.Select(s => new ModelCurve(s, BuiltInMaterials.XAxis)));
            var splitResults = Elements.Geometry.Profile.Split(new[] { profileToSplit }, splitters);
            var random = new Random();
            Model.AddElements(splitResults.Select(s => new Floor(s, 0.1, new Transform(0, 0, random.NextDouble()), random.NextMaterial())));
            Assert.Equal(4, splitResults.Count);
        }

        [Fact]
        public void ProfileOffset()
        {
            Name = nameof(ProfileOffset);

            var profile = new Profile(Polygon.Rectangle((0, 0), (10, 10)), Polygon.Rectangle((0.5, 0.5), (5, 5)));
            Model.AddElements(profile.ToModelCurves(material: BuiltInMaterials.XAxis));
            // offset in
            var offset = Elements.Geometry.Profile.Offset(new[] { profile }, -0.5);
            Assert.Equal(56, offset.Sum(o => o.Area()));
            Model.AddElements(offset.SelectMany(o => o.ToModelCurves(material: BuiltInMaterials.YAxis)));
            // offset out
            var offset2 = Elements.Geometry.Profile.Offset(new[] { profile }, 1);
            Assert.Equal(137.75, offset2.Sum(o => o.Area()), 3);
            Model.AddElements(offset2.SelectMany(o => o.ToModelCurves(material: BuiltInMaterials.ZAxis)));
            // offset in and back out again
            var offsetProfiles = Elements.Geometry.Profile.Offset(new[] { profile }, -1);
            var offsetOut = Elements.Geometry.Profile.Offset(offsetProfiles, 1);
            Assert.Equal(75, offsetOut.Sum(o => o.Area()));
            Model.AddElements(offsetOut.Select(p => new Panel(p.Perimeter, BuiltInMaterials.Void)));
        }

        [Fact]
        public void PolygonCleanupIssue()
        {
            Name = nameof(PolygonCleanupIssue);
            var rect = Polygon.Rectangle(10, 10);
            var splitters = Polygon.Rectangle(4, 15).Segments().Select(s => s.ToPolyline(1));
            Model.AddElements(rect);

            var heg = HalfEdgeGraph2d.Construct(new[] { rect }, splitters);
            var polygons = heg.Polygonize();
            Assert.Equal(3, polygons.Count);
            Assert.True(polygons.All(p => p.Segments().Count() == 4), "All polygons should be simple rectangles");
            var rand = new Random();
            foreach (var s in polygons)
            {
                Model.AddElement(new ModelCurve(s, rand.NextMaterial(), new Transform(0, 0, rand.NextDouble())));
            }
        }

        [Fact]
        public void ProfileInvalidWontThrow()
        {
            //This test shows that two non overlapping Polygons can still for a Profile
            //without throwing exception, this is NOT showcase of correct behavior.
            var p1 = Polygon.Rectangle(new Vector3(0, 0), new Vector3(2, 2));
            var p2 = Polygon.Rectangle(new Vector3(3, 0), new Vector3(5, 2));
            var profile = new Profile(p1, p2);
            Assert.DoesNotContain(profile.Perimeter.Vertices, v => profile.Voids.First().Contains(v));
            Assert.DoesNotContain(profile.Voids.First().Vertices, v => profile.Perimeter.Contains(v));
        }

        [Fact]
        public void DifferenceToleratesBadGeometry()
        {
            Name = nameof(DifferenceToleratesBadGeometry);
            var cp = JsonConvert.DeserializeObject<List<Profile>>(File.ReadAllText("../../../models/Geometry/corridorProfiles.json"));
            var lb = JsonConvert.DeserializeObject<Profile>(File.ReadAllText("../../../models/Geometry/levelBoundary.json"));
            var results = Elements.Geometry.Profile.Difference(new[] { lb }, cp);
            Model.AddElements(results);
            Model.AddElements(results.SelectMany(r => r.ToModelCurves()));
            Assert.Equal(2115.667, results.Sum(r => Math.Abs(r.Area())), 3);
        }

        [Fact]
        public void SplitComplexProfileWithInnerVoids()
        {
            Name = nameof(SplitComplexProfileWithInnerVoids);
            var profileJson = File.ReadAllText("../../../models/Geometry/complex-profile-w-voids.json");
            var segmentsJson = File.ReadAllText("../../../models/Geometry/splitsegments.json");
            var profiles = JsonConvert.DeserializeObject<List<Profile>>(profileJson);
            var segments = JsonConvert.DeserializeObject<List<Line>>(segmentsJson);
            var splits = Elements.Geometry.Profile.Split(profiles, segments.Select(l => l.ToPolyline(1)));
            // Value determined experimentally. If this test breaks, verify output visually —
            // it's not necessarily the end of the world if the number changes slightly, but we want to
            // make sure the results look sensible.
            Assert.Equal(444, splits.Count);
            var random = new Random(11);
            foreach (var s in splits)
            {
                var ge = new GeometricElement()
                {
                    Representation = new Geometry.Solids.Lamina(s),
                    Material = random.NextMaterial()
                };
                Model.AddElement(ge);
            }
        }

        [Fact]
        public void ProfilesWoundClockwiseSplitCorrectly()
        {
            Name = nameof(ProfilesWoundClockwiseSplitCorrectly);
            var polygon = new Polygon((0, 0, 0), (0, 10, 0), (10, 10, 0), (10, 0, 0));
            var polylineOffsetInside = new Polygon((3, 3), (3, 7), (7, 7), (7, 3));
            var extendedLines = polylineOffsetInside.Segments().Select(s => s.ExtendTo(polygon, true).ToPolyline(1)).ToList();
            var splitResults = Elements.Geometry.Profile.Split(new[] { new Profile(polygon) }, extendedLines);
            Assert.Equal(9, splitResults.Count);
        }

        [Fact]
        public void CleanProfilesSplitsAdjacentEdges()
        {
            Name = nameof(CleanProfilesSplitsAdjacentEdges);

            //   e----d----g
            //   |    |    |
            //   h----c    |
            //   |    |    |
            //   a----b----f

            var a = (0, 0);
            var b = (10, 0);
            var c = (10, 5);
            var d = (10, 10);
            var e = (0, 10);
            var f = (20, 0);
            var g = (20, 10);
            var h = (0, 5);
            var profiles = new Profile[] {
                new Profile(new Polygon(a,b,c,h)),
                new Profile(new Polygon(h,c,d,e)),
                new Profile(new Polygon(b,f,g,d)) // does not include c
            };
            var cleaned = profiles.Cleaned();
            // the "c" point should be present in all profiles
            Assert.True(cleaned.Count((p) => p.Perimeter.Vertices.Count == 4) == 2);
            Assert.True(cleaned.Count((p) => p.Perimeter.Vertices.Count == 5) == 1);
            Assert.True(cleaned.All(p => p.Perimeter.Vertices.Any((v) => v.DistanceTo(c) < 0.00001)));
        }

        [Fact]
        public void CleanProfilesMergesNearbyEdgesAndVertices()
        {
            Name = nameof(CleanProfilesMergesNearbyEdgesAndVertices);

            //   e----d----g
            //   |    |    |
            //   h----c    |
            //   |    |    |
            //   a----b----f

            var a = (0, 0);
            var b = (10, 0);
            var b2 = (10.0001, -0.0001);
            var c = (10, 5);
            var d = (10, 10);
            var d2 = (10.0001, 10.0001);
            var e = (0, 10);
            var f = (20, 0);
            var g = (20, 10);
            var h = (0, 5);
            var h2 = (-0.0001, 5);

            var profiles = new Profile[] {
                new Profile(new Polygon(a,b,c,h)),
                new Profile(new Polygon(h2,c,d,e)),
                new Profile(new Polygon(b2,f,g,d2))
            };
            var cleaned = profiles.Cleaned();
            // for the purposes of this test we are doing something "illegal" —
            // using `Distinct` on a set of points. Since for this test we only
            // care about exact equality, not "equality within tolerance," this
            // is OK.
            var uniquePoints = cleaned.SelectMany(c => c.Perimeter.Vertices).Distinct().ToList();
            Assert.True(uniquePoints.Count == 8);
        }

        [Fact]
        public void CleanProfilesPreservesValidProfiles()
        {
            var profiles = new Profile[] {
                new Profile(new Polygon((20.83686, 24.77025), (-6.27588, 24.77025), (-6.27588, -1.78971), (20.83686, -1.78971)), new List<Polygon> {
                    new Polygon((2.28049, 6.49027), (2.28049, 16.49027), (12.28049, 16.49027), (12.28049, 6.49027))
                }),
            };
            var cleaned = profiles.Cleaned();
            Assert.Single(cleaned);
        }

        [Fact]
        public void ThickenedProfilesProduceValidBoundaries()
        {
            var profile = new Profile
            {
                Perimeter = Polygon.Rectangle(10, 10),
            };
            profile.SetEdgeThickness(1, 1);
            var innerBoundary = profile.ThickenedInteriorProfile();
            Assert.Equal(4, innerBoundary.Perimeter.Vertices.Count);
            // (10 - 1 - 1)^2 = 64
            Assert.Equal(64, innerBoundary.Area());

            var outerBoundary = profile.ThickenedExteriorProfile();
            Assert.Equal(4, outerBoundary.Perimeter.Vertices.Count);
            // (10 + 1 + 1)^2 = 144
            Assert.Equal(144, outerBoundary.Area());

            var boundaryPolygons = profile.ThickenedEdgePolygons();
            Assert.Equal(4, boundaryPolygons.Count);
            var areaSum = boundaryPolygons.Sum(p => p.Area());
            Assert.Equal(144 - 64, areaSum);
        }

        [Fact]
        public void InvalidEdgeThicknessThrows()
        {
            var profile = new Profile
            {
                Perimeter = new Polygon((0, 0), (10, 0), (15, 20), (10, 24), (-2, 10))
            };
            Assert.Throws(typeof(ArgumentException), () =>
            {
                profile.SetEdgeThickness((1, 1), (0, 1), (1, 0), (2, 2));
            });

            Assert.Throws(typeof(ArgumentException), () =>
            {
                profile.SetEdgeThickness(4, -1);
            });
        }

        [Fact]
        public void ProfileWithVariableEdgeThickness()
        {
            Name = nameof(ProfileWithVariableEdgeThickness);
            var profile = new Profile
            {
                Perimeter = new Polygon((0, 0), (10, 0), (15, 20), (10, 24), (-2, 10))
            };
            profile.SetEdgeThickness((1, 1), (0, 1), (1, 0), (2, 2), (0, 0));
            Model.AddElements(profile.ToModelCurves(new Transform(0, 0, 2), BuiltInMaterials.XAxis));
            Model.AddElements(profile.ThickenedExteriorProfile().ToModelCurves(new Transform(0, 0, 1), BuiltInMaterials.YAxis));
            Model.AddElements(profile.ThickenedInteriorProfile().ToModelCurves(new Transform(), BuiltInMaterials.ZAxis));
            var boundaryPolygons = profile.ThickenedEdgePolygons();
            Model.AddElements(boundaryPolygons.Select(p => new ModelCurve(p, BuiltInMaterials.Mass, new Transform(0, 0, -1))));
        }

    }
}
