using System;
using System.Collections.Generic;
using Elements.Geometry.Profiles;
using Elements.Tests;
using Xunit;
using Xunit.Abstractions;

namespace Elements.Geometry.Tests
{
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

            var propertyValues = new Dictionary<string, double>() {
                {"w", 1},
                {"d", 2}
            };
            var vectorExpressions = new List<VectorExpression>() {
                new VectorExpression(x: "0", y: "0"),
                new VectorExpression(x: "data.w", y: "0"),
                new VectorExpression(x: "data.w", y: "data.d"),
                new VectorExpression(x: "0", y: "data.d")
            };

            var profileData = new ParametericProfileData(vectorExpressions, propertyValues);

            var profile = await ParametricProfile.CreateAsync(profileData);

            Model.AddElement(new ModelCurve(profile.Perimeter));
        }

        [Fact]
        public void ThrowsExceptionWhenExpressionContainsUnknownMember()
        {
            var propertyValues = new Dictionary<string, double>() {
                {"w", 1},
                {"d", 2}
            };
            var vectorExpressions = new List<VectorExpression>() {
                new VectorExpression(x: "foo()", y: "var()"),
            };
            var profileData = new ParametericProfileData(vectorExpressions, propertyValues);
            Assert.ThrowsAsync<AggregateException>(async () => await ParametricProfile.CreateAsync(profileData));
        }

        [Fact]
        public void ThrowsExceptionWhenNoExpressions()
        {
            var propertyValues = new Dictionary<string, double>() {
                {"w", 1},
                {"d", 2}
            };
            var vectorExpressions = new List<VectorExpression>() { };
            var profileData = new ParametericProfileData(vectorExpressions, propertyValues);
            Assert.ThrowsAsync<ArgumentException>(async () => await ParametricProfile.CreateAsync(profileData));
        }

        [Fact]
        public async void SerializesToJSON()
        {
            var propertyValues = new Dictionary<string, double>() {
                {"w", 1},
                {"d", 2}
            };
            var vectorExpressions = new List<VectorExpression>() {
                new VectorExpression(x: "0", y: "0"),
                new VectorExpression(x: "data.w", y: "0"),
                new VectorExpression(x: "data.w", y: "data.d"),
                new VectorExpression(x: "0", y: "data.d")
            };
            var profileData = new ParametericProfileData(vectorExpressions, propertyValues);

            var profile = await ParametricProfile.CreateAsync(profileData);
            Model.AddElement(profile);
            var json = Model.ToJson(true);

            _output.WriteLine(json);
        }

        [Fact]
        public async void BridgeDeck()
        {
            Name = nameof(BridgeDeck);

            var propertyValues = new Dictionary<string, double>() {
                {"w1", 2},
                {"w2", 4},
                {"curbWidth", 1},
                {"deckEdgeThickness", 0.5},
                {"roadCamber", 0.1},
                {"depth", 2}
            };

            var vectorExpressions = new List<VectorExpression>() {
                new VectorExpression(x: "data.w1", y: "-data.depth"),
                new VectorExpression(x: "data.w2", y: "-data.deckEdgeThickness"),
                new VectorExpression(x: "data.w2", y: "0"),
                new VectorExpression(x: "data.w2 - data.curbWidth", y: "0"),
                new VectorExpression(x: "data.w2 - data.curbWidth", y: "-data.roadCamber"),
                new VectorExpression(x: "0", y: "0"),
                new VectorExpression(x: "-data.w2 + data.curbWidth", y: "-data.roadCamber"),
                new VectorExpression(x: "-data.w2 + data.curbWidth", y: "0"),
                new VectorExpression(x: "-data.w2", y: "0"),
                new VectorExpression(x: "-data.w2", y: "-data.deckEdgeThickness"),
                new VectorExpression(x: "-data.w1", y: "-data.depth")
            };

            var voidVectorExpressions = new List<List<VectorExpression>>() {
                new List<VectorExpression>() {
                    new VectorExpression(x: "data.w1 - 0.2", y: "-data.depth + 0.2"),
                    new VectorExpression(x: "-data.w1 + 0.2", y: "-data.depth + 0.2"),
                    new VectorExpression(x: "-data.w1 + 0.2", y: "-data.roadCamber - 0.2"),
                    new VectorExpression(x: "data.w1 - 0.2", y: "-data.roadCamber - 0.2"),
                }
            };
            var profileData = new ParametericProfileData(vectorExpressions, propertyValues, voidVectorExpressions);
            var profile = await ParametricProfile.CreateAsync(profileData);

            var beam = new Beam(new Line(Vector3.Origin, new Vector3(30, 0, 0)), profile, BuiltInMaterials.Concrete);

            Model.AddElement(beam);
        }
    }
}