
using System;
using System.Collections.Generic;
using System.Linq;
using Elements;
using Elements.Flow;
using Elements.Geometry;
using Elements.Geometry.Solids;
using Elements.Serialization.glTF;
using Xunit;

namespace Elements.MEP.Tests
{
    public partial class Tests
    {
        [Fact]
        public void PortAndNodeTests()
        {
            var model = new Model();
            model.AddElement(new Trunk(10, "", Vector3.Origin, Guid.NewGuid(), ""));
            var json = model.ToJson();
            var deserialized = Model.FromJson(json);
            Assert.NotEmpty(deserialized.Elements);
            Assert.Contains("Elements.Flow.Trunk", json);
        }

        [Fact]
        public void TreeIsInitializedWhenDeserialized()
        {
            var tree = FittingsTests.GetSampleTreeWithTrunkBelow();
            var model = new Model();
            model.AddElement(tree);

            var deserialized = Model.FromJson(model.ToJson());
            Assert.NotEmpty(deserialized.Elements);
            var deserializedTree = deserialized.AllElementsOfType<Tree>().First();

            Assert.True(deserializedTree._alreadyTriedInit);
        }

        [Fact]
        public void RoofDrain_UpdateRepresentations_UnionTessellatesToGlb()
        {
            var drain = CreateRoofDrainForUnionTest(0.55);
            drain.UpdateRepresentations();

            Assert.False(drain.Representation.SkipCSGUnion);
            Assert.Equal(2, drain.Representation.SolidOperations.Count);

            var model = new Model();
            model.AddElement(drain);
            var glb = model.ToGlTF(updateElementsRepresentations: false);
            Assert.NotNull(glb);
            Assert.NotEmpty(glb);
        }

        [Fact]
        public void Tree_UpdateRepresentations_UnionTessellates()
        {
            var tree = FittingsTests.GetSampleTreeWithTrunkBelow();
            tree.UpdateRepresentations();

            Assert.False(tree.Representation.SkipCSGUnion);
            Assert.NotEmpty(tree.Representation.SolidOperations);

            tree.UpdateBoundsAndComputeSolid();
            Assert.True(tree.Bounds.Max.Z - tree.Bounds.Min.Z > 0.01);

            var model = new Model();
            model.AddElement(tree);
            var glb = model.ToGlTF(updateElementsRepresentations: false);
            Assert.NotNull(glb);
            Assert.NotEmpty(glb);
        }

        private static RoofDrain CreateRoofDrainForUnionTest(double diameter)
        {
            var drain = new RoofDrain(
                diameter,
                0,
                0,
                0,
                false,
                0,
                Guid.NewGuid().ToString(),
                id: Guid.NewGuid(),
                name: "RD-test");
            drain.ConnectorLength = 0.2764999948978424;
            drain.ConnectorOuterDiameter = 0.075;
            drain.HorizontalConnectorVector = new Vector3(0.5, 0, 0);
            return drain;
        }

        [Fact]
        public void RoofDrainTests()
        {
            var model = new Model();
            var section = new DrainableRoofSection(Polygon.Rectangle(2, 2), null, null, 0, null, null, id: Guid.NewGuid(), name: "");
            var circle = new Circle(new Vector3(), 0.3 / 2).ToPolygon(FlowSystemConstants.CIRCLE_SEGMENTS);
            circle = circle.TransformedPolygon(new Transform(0, 0, section.Boundary.Start.Z));
            var cylinder = new Extrude(new Profile(circle, new List<Polygon>(), Guid.NewGuid(), ""), .1, Vector3.ZAxis, false);
            var drain = new RoofDrain(
                                      0.2,
                                      1,
                                      0,
                                      0.2,
                                      false,
                                      0,
                                      section.Id.ToString(),
                                      new Transform(),
                                      BuiltInMaterials.Steel,
                                      new Representation(new List<SolidOperation>{
                                          cylinder
                                      }),
                                      false,
                                      Guid.NewGuid(),
                                      "");
            model.AddElement(section);
            model.AddElement(drain);
            var json = model.ToJson();

            var mdl = Model.FromJson(json, out var errors);
            Assert.Empty(errors);
        }
    }
}