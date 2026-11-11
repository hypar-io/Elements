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

        // A building moved to (10, 20, 0) and turned 90 degrees about Z.
        private static readonly Transform MovedRotatedBuilding =
            new Transform(new Vector3(10, 20, 0), Vector3.YAxis, Vector3.XAxis.Negate(), Vector3.ZAxis);
        private static readonly Transform MassLocal = new Transform(new Vector3(5, 0, 3));
        private static readonly Transform WallLocal = new Transform(new Vector3(1, 2, 0), 30);
        private static readonly Transform DoorLocal = new Transform(new Vector3(0.5, 0, 0));

        /// <summary>A pringle-shaped transform: { "Matrix": { "Components": [12 numbers] } }.</summary>
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

        private static JObject Wall(Transform t, string wallsVersion, Guid? parent)
        {
            return new JObject
            {
                ["discriminator"] = "Elements.StandardWall",
                ["Id"] = WallId.ToString(),
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
            yield return new object[] { "Elements.Building", null, null, false };
            yield return new object[] { "Elements.Space", null, null, false };
            yield return new object[] { "Elements.SpaceBoundary", null, null, false };
        }

        [Theory]
        [MemberData(nameof(RelativityCases))]
        public void RelativityFollowsPringlesTable(string discriminator, string versionProperty, string version, bool relative)
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
        public void NumericVersionIsNotRelativeAsInPringle()
        {
            // Pringle reads the version only when it is a string.
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
