using System;
using System.Collections.Generic;
using System.Linq;
using Elements.Geometry;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Elements.Tests
{
    public class WorldTransformResolverTests
    {
        private static readonly Guid BuildingId = new Guid("00000000-0000-0000-0000-00000000000b");
        private static readonly Guid MassId = new Guid("00000000-0000-0000-0000-00000000000a");
        private static readonly Guid WallId = new Guid("00000000-0000-0000-0000-000000000001");
        private static readonly Guid DoorId = new Guid("00000000-0000-0000-0000-000000000002");
        private static readonly Guid SpaceId = new Guid("00000000-0000-0000-0000-000000000003");
        private static readonly Guid V3WallId = new Guid("00000000-0000-0000-0000-000000000004");

        // A building moved to (10, 20, 0) and turned 90 degrees about Z.
        private static readonly Transform MovedRotatedBuilding =
            new Transform(new Vector3(10, 20, 0), Vector3.YAxis, Vector3.XAxis.Negate(), Vector3.ZAxis);
        private static readonly Transform MassLocal = new Transform(new Vector3(5, 0, 3));
        private static readonly Transform WallLocal = new Transform(new Vector3(1, 2, 0), 30);
        private static readonly Transform DoorLocal = new Transform(new Vector3(0.5, 0, 0));

        /// <summary>A transform as the editor writes it: { "Matrix": { "Components": [12 numbers] } }.</summary>
        private static JObject TransformJson(Transform t)
        {
            return new JObject(new JProperty("Matrix",
                new JObject(new JProperty("Components", new JArray(t.Matrix.Components.Cast<object>().ToArray())))));
        }

        private static JObject Building(Transform t)
        {
            // Elements.Building is a group: Id, Name, Transform only.
            return new JObject
            {
                ["discriminator"] = "Elements.Building",
                ["Id"] = BuildingId.ToString(),
                ["Name"] = "Building 1",
                ["Transform"] = TransformJson(t),
            };
        }

        private static JObject Mass(Transform t, Guid? parent)
        {
            return new JObject
            {
                ["discriminator"] = "Elements.ConceptualMass",
                ["Id"] = MassId.ToString(),
                ["Name"] = "Mass 1",
                ["Transform"] = TransformJson(t),
                ["Height"] = 6.0,
                ["Parent"] = parent?.ToString(),
            };
        }

        private static JObject Wall(Transform t, string wallsVersion, Guid? parent, Guid? id = null)
        {
            return new JObject
            {
                ["discriminator"] = "Elements.StandardWall",
                ["Id"] = (id ?? WallId).ToString(),
                ["Name"] = "Wall 1",
                ["CenterLine"] = JObject.FromObject(new Line(Vector3.Origin, new Vector3(4, 0, 0))),
                ["Thickness"] = 0.2,
                ["Height"] = 3.0,
                ["Transform"] = TransformJson(t),
                ["WallsVersion"] = wallsVersion,
                ["Parent"] = parent?.ToString(),
            };
        }

        private static JObject Door(Transform t, Guid parent)
        {
            return new JObject
            {
                ["discriminator"] = "Elements.Door",
                ["Id"] = DoorId.ToString(),
                ["Name"] = "Door 1",
                ["Transform"] = TransformJson(t),
                ["Parent"] = parent.ToString(),
            };
        }

        private static Model ModelFrom(params JObject[] elements)
        {
            var json = new JObject
            {
                ["Transform"] = TransformJson(new Transform()),
                ["Elements"] = new JObject(elements.Select(e => new JProperty((string)e["Id"], e))),
            };
            var model = Model.FromJson(json.ToString(), out var errors);
            Assert.Empty(errors);
            Assert.Equal(elements.Length, model.Elements.Count);
            return model;
        }

        private static void AssertSame(Transform expected, Transform actual)
        {
            Assert.NotNull(actual);
            for (var i = 0; i < 12; i++)
            {
                Assert.Equal(expected.Matrix.Components[i], actual.Matrix.Components[i], 9);
            }
        }

        [Fact]
        public void MassInIdentityBuildingIsItsLocalTransform()
        {
            var model = ModelFrom(Building(new Transform()), Mass(MassLocal, BuildingId));
            var resolver = new WorldTransformResolver(model);

            Assert.True(WorldTransformResolver.IsTransformRelativeToParent(model.Elements[MassId]));
            AssertSame(MassLocal, resolver.GetWorldTransform(MassId));
            AssertSame(new Transform(), resolver.GetWorldTransform(BuildingId));
        }

        [Fact]
        public void MassInMovedRotatedBuildingComposesWithTheBuilding()
        {
            var model = ModelFrom(Building(MovedRotatedBuilding), Mass(MassLocal, BuildingId));

            var world = new WorldTransformResolver(model).GetWorldTransform(MassId);

            // The mass's local (5, 0, 3) turned 90 degrees is (0, 5, 3), then moved by (10, 20, 0).
            Assert.True(world.Origin.IsAlmostEqualTo(new Vector3(10, 25, 3)));
            Assert.True(world.XAxis.IsAlmostEqualTo(Vector3.YAxis));
            AssertSame(MassLocal.Concatenated(MovedRotatedBuilding), world);
        }

        [Fact]
        public void V4WallOnMassInBuildingComposesTheWholeChain()
        {
            var model = ModelFrom(
                Building(MovedRotatedBuilding),
                Mass(MassLocal, BuildingId),
                Wall(WallLocal, "4", MassId),
                Door(DoorLocal, WallId));
            var resolver = new WorldTransformResolver(model);
            // This door has no Anchor; one the editor placed in the wall does (see DoorAnchoredInWallIsFlagged).
            Assert.False(WorldTransformResolver.HasAnchor(model.Elements[DoorId]));

            var massWorld = MassLocal.Concatenated(MovedRotatedBuilding);
            var wallWorld = WallLocal.Concatenated(massWorld);
            Assert.IsType<StandardWall>(model.Elements[WallId]);
            AssertSame(wallWorld, resolver.GetWorldTransform(WallId));
            AssertSame(DoorLocal.Concatenated(wallWorld), resolver.GetWorldTransform(DoorId));

            // A point at the wall's local origin lands where the building, mass and wall frames put it.
            var expected = MovedRotatedBuilding.OfPoint(MassLocal.OfPoint(WallLocal.OfPoint(Vector3.Origin)));
            Assert.True(resolver.GetWorldTransform(WallId).OfPoint(Vector3.Origin).IsAlmostEqualTo(expected));
        }

        [Fact]
        public void DoorAnchoredInWallIsFlagged()
        {
            var door = Door(DoorLocal, WallId);
            door["Anchor"] = new JObject
            {
                ["AnchorType"] = "UVW",
                ["UVW"] = new JObject { ["X"] = 0.5, ["Y"] = 0.5, ["Z"] = 0 },
            };
            var model = ModelFrom(Wall(WallLocal, "4", null), door);

            Assert.True(WorldTransformResolver.HasAnchor(model.Elements[DoorId]));

            var unanchored = Door(DoorLocal, WallId);
            unanchored["Anchor"] = null;
            Assert.False(WorldTransformResolver.HasAnchor(ModelFrom(unanchored).Elements[DoorId]));
            Assert.False(WorldTransformResolver.HasAnchor(null));
        }

        [Fact]
        public void V3WallWithParentStaysWorld()
        {
            var model = ModelFrom(Building(MovedRotatedBuilding), Mass(MassLocal, BuildingId), Wall(WallLocal, "3", MassId));

            Assert.False(WorldTransformResolver.IsTransformRelativeToParent(model.Elements[WallId]));
            AssertSame(WallLocal, new WorldTransformResolver(model).GetWorldTransform(WallId));
        }

        [Fact]
        public void SpaceWithParentStaysWorld()
        {
            var spaceTransform = new Transform(new Vector3(3, 4, 0));
            var space = new JObject
            {
                ["discriminator"] = "Elements.SpaceBoundary",
                ["Id"] = SpaceId.ToString(),
                ["Name"] = "Office",
                ["Transform"] = TransformJson(spaceTransform),
                ["Parent"] = MassId.ToString(),
            };
            var model = ModelFrom(Building(MovedRotatedBuilding), Mass(MassLocal, BuildingId), space);

            Assert.Equal("Elements.SpaceBoundary", WorldTransformResolver.GetDiscriminator(model.Elements[SpaceId]));
            Assert.False(WorldTransformResolver.IsTransformRelativeToParent(model.Elements[SpaceId]));
            AssertSame(spaceTransform, model.GetWorldTransform(model.Elements[SpaceId]));
        }

        [Fact]
        public void MassWithoutParentIsWorld()
        {
            var model = ModelFrom(Building(MovedRotatedBuilding), Mass(MassLocal, null));

            Assert.False(WorldTransformResolver.IsTransformRelativeToParent(model.Elements[MassId]));
            AssertSame(MassLocal, model.GetWorldTransform(model.Elements[MassId]));
        }

        [Fact]
        public void MissingParentLeavesTheElementsOwnTransform()
        {
            var model = ModelFrom(Mass(MassLocal, BuildingId));

            Assert.True(WorldTransformResolver.IsTransformRelativeToParent(model.Elements[MassId]));
            AssertSame(MassLocal, new WorldTransformResolver(model).GetWorldTransform(MassId));
        }

        [Fact]
        public void ParentLoopResolvesEachMemberToItsOwnTransform()
        {
            // A mass parented to a v4 wall parented to the mass, and a door hanging off the loop.
            var model = ModelFrom(Mass(MassLocal, WallId), Wall(WallLocal, "4", MassId), Door(DoorLocal, WallId));

            foreach (var first in new[] { MassId, WallId, DoorId })
            {
                var resolver = new WorldTransformResolver(model);
                resolver.GetWorldTransform(first);
                AssertSame(MassLocal, resolver.GetWorldTransform(MassId));
                AssertSame(WallLocal, resolver.GetWorldTransform(WallId));
                AssertSame(DoorLocal.Concatenated(WallLocal), resolver.GetWorldTransform(DoorId));
            }
        }

        [Fact]
        public void ParentWithoutTransformComposesAsIdentity()
        {
            var building = Building(MovedRotatedBuilding);
            building.Remove("Transform");
            var model = ModelFrom(building, Mass(MassLocal, BuildingId));
            var resolver = new WorldTransformResolver(model);

            Assert.Null(resolver.GetWorldTransform(BuildingId));
            AssertSame(MassLocal, resolver.GetWorldTransform(MassId));
        }

        [Fact]
        public void LookupResolverAndUnknownIds()
        {
            var model = ModelFrom(Building(MovedRotatedBuilding), Mass(MassLocal, BuildingId));
            var resolver = new WorldTransformResolver(id => model.Elements.TryGetValue(id, out var e) ? e : null);

            AssertSame(MassLocal.Concatenated(MovedRotatedBuilding), resolver.GetWorldTransform(MassId));
            Assert.Null(resolver.GetWorldTransform(Guid.NewGuid()));
        }

        [Fact]
        public void ReturnedTransformsAreCopies()
        {
            var model = ModelFrom(Building(MovedRotatedBuilding), Mass(MassLocal, BuildingId));
            var resolver = new WorldTransformResolver(model);

            resolver.GetWorldTransform(MassId).Move(new Vector3(100, 0, 0));
            resolver.GetWorldTransform(BuildingId).Move(new Vector3(100, 0, 0));

            AssertSame(MassLocal.Concatenated(MovedRotatedBuilding), resolver.GetWorldTransform(MassId));
            AssertSame(MovedRotatedBuilding, resolver.GetWorldTransform(BuildingId));
        }

        [Fact]
        public void TypedElementsBuiltInCodeResolve()
        {
            // No JSON: a C# StandardWall and a fallback-style Element carrying Parent in AdditionalProperties.
            var mass = new Element(MassId, "Mass");
            mass.AdditionalProperties["discriminator"] = "Elements.ConceptualMass";
            mass.AdditionalProperties["Transform"] = MovedRotatedBuilding;
            var wall = new StandardWall(new Line(Vector3.Origin, new Vector3(4, 0, 0)), 0.2, 3, transform: WallLocal, wallsVersion: "4", id: WallId);
            wall.AdditionalProperties["Parent"] = MassId;
            var model = new Model();
            model.AddElement(mass);
            model.AddElement(wall);

            Assert.Equal(MassId, WorldTransformResolver.GetParentId(wall));
            AssertSame(WallLocal.Concatenated(MovedRotatedBuilding), model.GetWorldTransform(wall));
        }

        /// <summary>A function's own mass type whose Transform hides GeometricElement.Transform.</summary>
        private class HidingMass : GeometricElement
        {
            public new Transform Transform { get; set; }
        }

        private class DerivedHidingMass : HidingMass
        {
        }

        [Fact]
        public void HiddenTransformPropertyIsRead()
        {
            var building = new Element(BuildingId, "Building");
            building.AdditionalProperties["discriminator"] = "Elements.Building";
            building.AdditionalProperties["Transform"] = TransformJson(MovedRotatedBuilding);
            var expected = MassLocal.Concatenated(MovedRotatedBuilding);

            foreach (var mass in new HidingMass[] { new HidingMass(), new DerivedHidingMass() })
            {
                mass.Id = MassId;
                mass.Transform = MassLocal;
                mass.AdditionalProperties["discriminator"] = "Elements.ConceptualMass";
                mass.AdditionalProperties["Parent"] = BuildingId.ToString();
                Assert.True(((GeometricElement)mass).Transform.Equals(new Transform()));

                var elements = new Dictionary<Guid, Element> { { BuildingId, building }, { MassId, mass } };
                var resolver = new WorldTransformResolver(id => elements.TryGetValue(id, out var e) ? e : null);
                AssertSame(MassLocal, WorldTransformResolver.GetLocalTransform(mass));
                AssertSame(expected, resolver.GetWorldTransform(mass));
            }
        }

        [Fact]
        public void LookupSpansSeveralModels()
        {
            // Building and mass in one input model, a v2 column on the mass in another.
            var buildingAt = new Transform(new Vector3(100, 0, 0), Vector3.YAxis, Vector3.XAxis.Negate(), Vector3.ZAxis);
            var massModel = ModelFrom(Building(buildingAt), Mass(new Transform(new Vector3(10, 0, 0)), BuildingId));
            // As a function that does not register Elements.Column reads it: a plain Element.
            var column = new Element(WallId, "Column 1");
            column.AdditionalProperties["discriminator"] = "Elements.Column";
            column.AdditionalProperties["Transform"] = TransformJson(new Transform());
            column.AdditionalProperties["ColumnsVersion"] = "2";
            column.AdditionalProperties["Parent"] = MassId.ToString();
            var columnModel = new Model();
            columnModel.AddElement(column);
            var models = new[] { massModel, columnModel };
            var resolver = new WorldTransformResolver(id => models.Select(m => m.Elements.TryGetValue(id, out var e) ? e : null).FirstOrDefault(e => e != null));

            Assert.True(resolver.GetWorldTransform(WallId).Origin.IsAlmostEqualTo(new Vector3(100, 10, 0)));
        }

        [Fact]
        public void ToLocalTransformRoundTrips()
        {
            var model = ModelFrom(
                Building(MovedRotatedBuilding),
                Mass(MassLocal, BuildingId),
                Wall(WallLocal, "4", MassId),
                Wall(WallLocal, "3", MassId, V3WallId));
            var resolver = new WorldTransformResolver(model);
            var newWorld = new Transform(new Vector3(-3, 7, 2), 45);

            // A relative element: the stored transform composes back to the requested world.
            var wallLocal = resolver.ToLocalTransform(model.Elements[WallId], newWorld);
            var massWorld = MassLocal.Concatenated(MovedRotatedBuilding);
            AssertSame(newWorld, wallLocal.Concatenated(massWorld));

            // A world-space element and a root store the world transform as it is.
            AssertSame(newWorld, resolver.ToLocalTransform(model.Elements[V3WallId], newWorld));
            AssertSame(newWorld, resolver.ToLocalTransform(model.Elements[BuildingId], newWorld));
        }

        public static IEnumerable<object[]> RelativityCases()
        {
            // discriminator, version property, version value, relative?
            yield return new object[] { "Elements.StandardWall", "WallsVersion", "4", true };
            yield return new object[] { "Elements.StandardWall", "WallsVersion", "5", true };
            yield return new object[] { "Elements.StandardWall", "WallsVersion", "3", false };
            yield return new object[] { "Elements.StandardWall", "WallsVersion", null, false };
            yield return new object[] { "Elements.GridLine", "GridlinesVersion", "2", true };
            yield return new object[] { "Elements.GridLine", "GridlinesVersion", "1", false };
            yield return new object[] { "Elements.GridLine", null, null, false };
            yield return new object[] { "Elements.Column", "ColumnsVersion", "2", true };
            yield return new object[] { "Elements.Column", null, null, false };
            yield return new object[] { "Elements.Door", null, null, true };
            yield return new object[] { "Elements.Window", null, null, true };
            yield return new object[] { "Elements.Floor", null, null, true };
            yield return new object[] { "Elements.Roof", null, null, true };
            yield return new object[] { "Elements.Stair", null, null, true };
            yield return new object[] { "Elements.Elevator", null, null, true };
            yield return new object[] { "Elements.Annotations.Anchor", null, null, true };
            yield return new object[] { "Elements.Component", null, null, true };
            yield return new object[] { "Elements.Clearance", null, null, true };
            yield return new object[] { "Elements.Circulation", null, null, true };
            yield return new object[] { "Elements.PlanningGrid", null, null, true };
            yield return new object[] { "Elements.CadComponent", null, null, true };
            yield return new object[] { "Elements.FlexibleComponent", null, null, true };
            yield return new object[] { "Elements.Building", null, null, false };
            yield return new object[] { "Elements.Space", null, null, false };
            yield return new object[] { "Elements.SpaceBoundary", null, null, false };
        }

        [Theory]
        [MemberData(nameof(RelativityCases))]
        public void RelativityFollowsTheEditorsTable(string discriminator, string versionProperty, string version, bool relative)
        {
            var element = new Element(Guid.NewGuid());
            element.AdditionalProperties["discriminator"] = discriminator;
            if (versionProperty != null)
            {
                element.AdditionalProperties[versionProperty] = version;
            }
            Assert.False(WorldTransformResolver.IsTransformRelativeToParent(element));

            element.AdditionalProperties["Parent"] = Guid.NewGuid().ToString();
            Assert.Equal(relative, WorldTransformResolver.IsTransformRelativeToParent(element));

            element.AdditionalProperties["Parent"] = "";
            Assert.False(WorldTransformResolver.IsTransformRelativeToParent(element));
        }

        [Fact]
        public void NumericVersionIsNotRelative()
        {
            // The editor's rule reads the version only when it is a string.
            var model = ModelFrom(Mass(MassLocal, null), Wall(WallLocal, "4", MassId));
            var wall = (StandardWall)model.Elements[WallId];
            Assert.True(WorldTransformResolver.IsTransformRelativeToParent(wall));

            var element = new Element(Guid.NewGuid());
            element.AdditionalProperties["discriminator"] = "Elements.StandardWall";
            element.AdditionalProperties["WallsVersion"] = 4;
            element.AdditionalProperties["Parent"] = MassId.ToString();
            Assert.False(WorldTransformResolver.IsTransformRelativeToParent(element));
        }
    }
}
