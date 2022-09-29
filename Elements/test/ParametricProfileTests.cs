using System;
using System.Collections.Generic;
using System.Linq;
using Elements.Geometry.Profiles;
using Elements.Tests;
using Xunit;
using Xunit.Abstractions;

namespace Elements.Geometry.Tests
{
    public class TestParametricProfile : ParametricProfile
    {
        public double w;
        public double d;
        public TestParametricProfile(Polygon perimeter = null,
                                     IList<Polygon> voids = null,
                                     Guid id = default,
                                     string name = null) : base(new List<VectorExpression>() {
                                                                    new VectorExpression(x: "0", y: "0"),
                                                                    new VectorExpression(x: "w", y: "0"),
                                                                    new VectorExpression(x: "w", y: "d"),
                                                                    new VectorExpression(x: "0", y: "d")
                                                                }, null, perimeter, voids, id, name)
        { }
    }

    public class EmptyParametricProfile : ParametricProfile
    {
        public EmptyParametricProfile(Polygon perimeter = null,
                                      IList<Polygon> voids = null,
                                      Guid id = default,
                                      string name = null) : base(null, null, perimeter, voids, id, name)
        {
        }
    }

    public class BadParametricProfile : ParametricProfile
    {
        public double w;
        public double d;
        public BadParametricProfile(Polygon perimeter = null,
                                    IList<Polygon> voids = null,
                                    Guid id = default,
                                    string name = null) : base(new List<VectorExpression>() {
                                                                    new VectorExpression(x: "0", y: "0"),
                                                                    new VectorExpression(x: "w", y: "bar"),
                                                                    new VectorExpression(x: "w", y: "d"),
                                                                    new VectorExpression(x: "foo", y: "d")
                                    }, null, perimeter, voids, id, name)
        { }
    }

    public class BridgeDeckProfile : ParametricProfile
    {
        public double w1;
        public double w2;
        public double curbWidth;
        public double deckEdgeThickness;
        public double roadCamber;
        public double depth;

        public BridgeDeckProfile(Polygon perimeter = null,
                                 IList<Polygon> voids = null,
                                 Guid id = default,
                                 string name = null) : base(new List<VectorExpression>() {
                                                                new VectorExpression(x: "w1", y: "-depth"),
                                                                new VectorExpression(x: "w2", y: "-deckEdgeThickness"),
                                                                new VectorExpression(x: "w2", y: "0"),
                                                                new VectorExpression(x: "w2 - curbWidth", y: "0"),
                                                                new VectorExpression(x: "w2 - curbWidth", y: "-roadCamber"),
                                                                new VectorExpression(x: "0", y: "0"),
                                                                new VectorExpression(x: "-w2 + curbWidth", y: "-roadCamber"),
                                                                new VectorExpression(x: "-w2 + curbWidth", y: "0"),
                                                                new VectorExpression(x: "-w2", y: "0"),
                                                                new VectorExpression(x: "-w2", y: "-deckEdgeThickness"),
                                                                new VectorExpression(x: "-w1", y: "-depth")
                                                            }, new List<List<VectorExpression>>() {
                                                                new List<VectorExpression>() {
                                                                    new VectorExpression(x: "w1 - 0.2", y: "-depth + 0.2"),
                                                                    new VectorExpression(x: "-w1 + 0.2", y: "-depth + 0.2"),
                                                                    new VectorExpression(x: "-w1 + 0.2", y: "-roadCamber - 0.2"),
                                                                    new VectorExpression(x: "w1 - 0.2", y: "-roadCamber - 0.2"),
                                                                }
                                                            }, perimeter, voids, id, name)
        {
        }
    }

    public class ParametricProfileTests : ModelTest
    {
        private ITestOutputHelper _output;

        public ParametricProfileTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async void Create()
        {
            Name = nameof(ParametricProfile);

            var profile = new TestParametricProfile()
            {
                w = 1,
                d = 2
            };
            await profile.SetGeometryAsync();

            Model.AddElement(new ModelCurve(profile.Perimeter));
        }

        [Fact]
        public void ThrowsExceptionWhenExpressionContainsUnknownMember()
        {
            var profile = new BadParametricProfile()
            {
                w = 1,
                d = 2
            };
            Assert.ThrowsAsync<AggregateException>(async () => await profile.SetGeometryAsync());
        }

        [Fact]
        public void ThrowsExceptionWhenNoExpressions()
        {
            Assert.Throws<ArgumentException>(() => new EmptyParametricProfile() { });
        }

        [Fact]
        public async void SerializesToJSON()
        {
            var profile = new TestParametricProfile()
            {
                w = 1,
                d = 2
            };
            await profile.SetGeometryAsync();

            Model.AddElement(profile);
            var json = Model.ToJson(true);

            _output.WriteLine(json);
        }

        [Fact]
        public async void BridgeDeck()
        {
            Name = nameof(BridgeDeck);

            var profile = new BridgeDeckProfile()
            {
                w1 = 2,
                w2 = 4,
                curbWidth = 1,
                deckEdgeThickness = 0.5,
                roadCamber = 0.1,
                depth = 2
            };

            await profile.SetGeometryAsync();

            var beam = new Beam(new Line(Vector3.Origin, new Vector3(30, 0, 0)), profile, material: BuiltInMaterials.Concrete);

            Model.AddElement(beam);
        }

        [Fact]
        public async void CProfiles()
        {
            Name = nameof(CProfiles);

            var factory = new CProfileFactory();
            var profiles = await factory.AllProfilesAsync();

            DrawAllProfiles(Model, profiles.ToList());
        }

        [Fact]
        public async void WTProfiles()
        {
            Name = nameof(WTProfiles);

            var factory = new WTProfileFactory();
            var profiles = await factory.AllProfilesAsync();

            DrawAllProfiles(Model, profiles.ToList());
        }

        [Fact]
        public async void LProfiles()
        {
            Name = nameof(LProfiles);

            var factory = new LProfileFactory();
            var profiles = await factory.AllProfilesAsync();

            DrawAllProfiles(Model, profiles.ToList());
        }

        [Fact]
        public async void STProfiles()
        {
            Name = nameof(STProfiles);

            var factory = new STProfileFactory();
            var profiles = await factory.AllProfilesAsync();

            DrawAllProfiles(Model, profiles.ToList());
        }

        [Fact]
        public async void MCProfiles()
        {
            Name = nameof(MCProfiles);

            var factory = new MCProfileFactory();
            var profiles = await factory.AllProfilesAsync();

            DrawAllProfiles(Model, profiles.ToList());
        }

        [Fact]
        public async void HSSProfiles()
        {
            Name = nameof(HSSProfiles);

            var factory = new HSSProfileFactory();
            var profiles = await factory.AllProfilesAsync();

            DrawAllProfiles(Model, profiles.ToList());
        }

        [Fact]
        public async void WProfiles()
        {
            Name = nameof(WProfiles);

            var factory = new WProfileFactory();
            var profiles = await factory.AllProfilesAsync();

            DrawAllProfiles(Model, profiles.ToList());
        }

        private void DrawAllProfiles(Model model, IEnumerable<Profile> profiles)
        {
            var x = 0.0;
            var z = 0.0;

            foreach (var profile in profiles)
            {
                var line = new Line(new Vector3(x, 0, z), new Vector3(x, 3, z));
                var beam = new Beam(line, profile);
                model.AddElement(beam);
                x += 1.0;
                if (x > 10.0)
                {
                    z += 1.0;
                    x = 0.0;
                }
            }
        }
    }
}