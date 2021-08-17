using Xunit;
using Elements.Geometry;
using Elements.Geometry.Solids;
using Elements.Geometry.Profiles;
using Newtonsoft.Json;
using Xunit.Abstractions;
using System.Collections.Generic;
using System;
using System.IO;
using Elements.Serialization.glTF;
using Elements.Serialization.JSON;
using System.Linq;

namespace Elements.Tests
{
    public class SolidTests : ModelTest
    {
        private readonly ITestOutputHelper output;

        private WideFlangeProfileFactory _profileFactory = new WideFlangeProfileFactory();

        public SolidTests(ITestOutputHelper output)
        {
            this.output = output;
            this.GenerateIfc = false;

            if (!Directory.Exists("models"))
            {
                Directory.CreateDirectory("models");
            }
        }

        [Fact]
        public void SweptSolid()
        {
            var n = 4;
            var outer = Polygon.Ngon(n, 2);
            var inner = Polygon.Ngon(n, 1.75).Reversed();

            var solid = Solid.SweepFace(outer, new[] { inner }, 5);
            foreach (var e in solid.Edges.Values)
            {
                Assert.NotNull(e.Left);
                Assert.NotNull(e.Right);
                Assert.NotSame(e.Left, e.Right);
            }
            Assert.Equal(2 * n + 2, solid.Faces.Count);
            Assert.Equal(n * 6, solid.Edges.Count);
            Assert.Equal(n * 4, solid.Vertices.Count);
            solid.ToGlb("models/SweptSolid.glb");
        }

        [Fact]
        public void Slice()
        {
            var n = 3;
            var outer = Polygon.Ngon(n, 2);
            var solid = Solid.SweepFace(outer, new Polygon[] { }, 5);
            var slicePlane = new Plane(new Vector3(0, 0, 2.5), new Vector3(0.5, 0.5, 0));
            solid.Slice(slicePlane);

            foreach (var e in solid.Edges.Values)
            {
                Assert.NotNull(e.Left);
                Assert.NotNull(e.Right);
                Assert.NotSame(e.Left, e.Right);
            }

            // Console.WriteLine(solid.ToString());
            // Assert.Equal(2 * n + 2, solid.Faces.Count);
            // Assert.Equal(n * 6, solid.Edges.Count);
            // Assert.Equal(n * 4, solid.Vertices.Count);
            solid.ToGlb("models/SliceSolid.glb");
        }

        [Fact]
        public void SweptSolidAngle()
        {
            var n = 4;
            var outer = Polygon.Ngon(n, 2);
            var inner = Polygon.Ngon(n, 1.75).Reversed();

            var solid = Solid.SweepFace(outer, new[] { inner }, new Vector3(0.5, 0.5, 0.5), 5);
            foreach (var e in solid.Edges.Values)
            {
                Assert.NotNull(e.Left);
                Assert.NotNull(e.Right);
                Assert.NotSame(e.Left, e.Right);
            }
            Assert.Equal(2 * n + 2, solid.Faces.Count);
            Assert.Equal(n * 6, solid.Edges.Count);
            Assert.Equal(n * 4, solid.Vertices.Count);
            solid.ToGlb("models/SweptSolidAngle.glb");
        }

        [Fact]
        public void SweptSolidTransformToStart()
        {
            var n = 4;
            var outer = Polygon.Ngon(n, 2);
            var inner = Polygon.Ngon(n, 1.75).Reversed();

            var solid = Solid.SweepFace(outer, new[] { inner }, new Vector3(0.5, 0.5, 0.5), 5);
            foreach (var e in solid.Edges.Values)
            {
                Assert.NotNull(e.Left);
                Assert.NotNull(e.Right);
                Assert.NotSame(e.Left, e.Right);
            }
            Assert.Equal(2 * n + 2, solid.Faces.Count);
            Assert.Equal(n * 6, solid.Edges.Count);
            Assert.Equal(n * 4, solid.Vertices.Count);
            solid.ToGlb("models/SweptSolidTransformToStart.glb");
        }

        [Fact]
        public void SweptSolidCollinearPolyline()
        {
            var profile = _profileFactory.GetProfileByType(WideFlangeProfileType.W10x100);
            var path = new Polyline(new[] { new Vector3(0, 0), new Vector3(0, 2), new Vector3(0, 3, 1), new Vector3(0, 5, 1) });
            var solid = Solid.SweepFaceAlongCurve(profile.Perimeter, null, path);
            foreach (var e in solid.Edges.Values)
            {
                Assert.NotNull(e.Left);
                Assert.NotNull(e.Right);
                Assert.NotSame(e.Left, e.Right);
            }
            solid.ToGlb("models/SweptSolidCollinearPolyline.glb");
        }

        [Fact]
        public void SweptSolidPolyline()
        {
            var profile = _profileFactory.GetProfileByType(WideFlangeProfileType.W10x100);
            var path = new Polyline(new[] { new Vector3(-2, 2, 0), new Vector3(0, 2, 0), new Vector3(0, 3, 1), new Vector3(0, 5, 1), new Vector3(-2, 6, 0) });
            var solid = Solid.SweepFaceAlongCurve(profile.Perimeter, null, path);
            foreach (var e in solid.Edges.Values)
            {
                Assert.NotNull(e.Left);
                Assert.NotNull(e.Right);
                Assert.NotSame(e.Left, e.Right);
            }
            solid.ToGlb("models/SweptSolidPolyline.glb");
        }

        [Fact]
        public void SweptSolidArc()
        {
            var profile = _profileFactory.GetProfileByType(WideFlangeProfileType.W10x100);
            var path = new Arc(Vector3.Origin, 5, 0, 90);
            var solid = Solid.SweepFaceAlongCurve(profile.Perimeter, null, path);
            foreach (var e in solid.Edges.Values)
            {
                Assert.NotNull(e.Left);
                Assert.NotNull(e.Right);
                Assert.NotSame(e.Left, e.Right);
            }
            solid.ToGlb("models/SweptSolidArc.glb");
        }

        [Fact]
        public void SweptSolidPolygon()
        {
            var profile = _profileFactory.GetProfileByType(WideFlangeProfileType.W10x100);
            var path = Polygon.Ngon(12, 5);
            var solid = Solid.SweepFaceAlongCurve(profile.Perimeter, null, path);
            foreach (var e in solid.Edges.Values)
            {
                Assert.NotNull(e.Left);
                Assert.NotNull(e.Right);
                Assert.NotSame(e.Left, e.Right);
            }
            solid.ToGlb("models/SweptSolidPolygon.glb");
        }

        [Fact]
        public void SolidToMesh()
        {
            var n = 4;
            var outer = Polygon.Ngon(n, 2);
            var inner = Polygon.Ngon(n, 1.75).Reversed();

            var solid = Solid.SweepFace(outer, new[] { inner }, new Vector3(0.5, 0.5, 0.5), 5);
            var mesh = solid.ToMesh();
            var subtraction = mesh.ToCsg().Substract(solid.ToCsg());
            Assert.Empty(subtraction.Polygons);
        }

        [Fact]
        public void Serialization()
        {
            var n = 4;
            var outer = Polygon.Ngon(n, 2);
            var solid = Solid.SweepFace(outer, null, 2.0);
            var materials = new Dictionary<Guid, Material>();
            var defMaterial = BuiltInMaterials.Default;
            materials.Add(defMaterial.Id, defMaterial);
            var json = JsonConvert.SerializeObject(solid, new JsonSerializerSettings()
            {
                Converters = new[] { new SolidConverter(materials) },
                Formatting = Formatting.Indented
            });
            var newSolid = JsonConvert.DeserializeObject<Solid>(json, new JsonSerializerSettings()
            {
                Converters = new[] { new SolidConverter(materials) }
            });
            Assert.Equal(8, newSolid.Vertices.Count);
            Assert.Equal(12, newSolid.Edges.Count);
            Assert.Equal(6, newSolid.Faces.Count);
            newSolid.ToGlb("models/SweptSolidDeserialized.glb");
        }

        [Fact]
        public void ConstructedSolid()
        {
            GenerateIfc = false;
            Name = nameof(ConstructedSolid);
            var solid = new Solid();
            var A = new Vector3(0, 0, 0);
            var B = new Vector3(1, 0, 0);
            var C = new Vector3(1, 1, 0);
            var D = new Vector3(0, 1, 0);
            var E = new Vector3(0, 0, 1);
            var F = new Vector3(1, 0, 1);
            var G = new Vector3(1, 1, 1);
            var H = new Vector3(0, 1, 1);
            solid.AddFace(new Polygon(new[] { A, B, C, D }), mergeVerticesAndEdges: true);
            solid.AddFace(new Polygon(new[] { E, F, G, H }), mergeVerticesAndEdges: true);
            solid.AddFace(new Polygon(new[] { A, B, F, E }), mergeVerticesAndEdges: true);
            solid.AddFace(new Polygon(new[] { B, C, G, F }), mergeVerticesAndEdges: true);
            solid.AddFace(new Polygon(new[] { C, D, H, G }), mergeVerticesAndEdges: true);
            solid.AddFace(new Polygon(new[] { D, A, E, H }), mergeVerticesAndEdges: true);

            var emptySolid = new ConstructedSolid(new Solid(), false);
            var import = new ConstructedSolid(solid, false);
            var representation = new Representation(new[] { import });
            var emptyRep = new Representation(new[] { emptySolid });
            var userElement = new GeometricElement(new Transform(), BuiltInMaterials.Default, representation, false, Guid.NewGuid(), "Import");
            var userElementWithEmptySolid = new GeometricElement(new Transform(), BuiltInMaterials.Default, emptyRep, false, Guid.NewGuid(), "Import Empty");
            Model.AddElement(userElement);
            Model.AddElement(userElementWithEmptySolid);

            // ensure serialized solid
            var modelJson = Model.ToJson();

            var deserializedModel = Model.FromJson(modelJson);
            var userElemDeserialized = deserializedModel.GetElementByName<GeometricElement>("Import");
            var opDeserialized = userElemDeserialized.Representation.SolidOperations.First();
            var solidDeserialized = opDeserialized?.Solid;
            Assert.NotNull(solidDeserialized);
        }

        [Fact]
        public void CreateLaminaWithHoles()
        {
            Name = nameof(CreateLaminaWithHoles);
            var profile = new Profile(Polygon.Rectangle(15, 15), Polygon.Star(7, 4, 5));
            var geoElem = new GeometricElement(
                new Transform(),
                BuiltInMaterials.XAxis,
                new Representation(new[] {
                    new Lamina(profile)
                    }),
                false, Guid.NewGuid(), "Planar Shape Test");
            Model.AddElement(geoElem);
        }


        [Fact]
        public void ConstructedSolidProducesValidGlb()
        {
            Name = nameof(ConstructedSolidProducesValidGlb);
            var allPolygons = JsonConvert.DeserializeObject<List<(Polygon outerLoop, List<Polygon> innerLoops)>>(File.ReadAllText("../../../models/Geometry/ExampleConstructedSolidPolygons.json"));
            var solid = new Solid();
            foreach (var face in allPolygons)
            {
                solid.AddFace(face.outerLoop, face.innerLoops, true);
            }
            var solidOp = new Elements.Geometry.Solids.ConstructedSolid(solid, false);
            solidOp.LocalTransform = new Transform();
            var geoElem = new GeometricElement(new Transform(), BuiltInMaterials.Concrete, new Representation(new[] { solidOp }), false, Guid.NewGuid(), null);
            var model = new Model();
            model.AddElement(geoElem);
            var bytes = model.ToGlTF();
            Assert.True(bytes != null && bytes.Length > 3000);
            Model.AddElement(geoElem);
        }

        [Fact]
        public void ImplicitRepresentationOperator()
        {
            Name = nameof(ImplicitRepresentationOperator);
            // implicitly convert single solid operation to a representation
            var element = new GeometricElement(new Transform(), BuiltInMaterials.ZAxis, new Extrude(Polygon.Rectangle(5, 5), 1, Vector3.ZAxis, false), false, Guid.NewGuid(), null);
            // params constructor for Representation
            var element2 = new GeometricElement(
                new Transform(),
                BuiltInMaterials.XAxis,
                new Representation(
                    new Extrude(Polygon.Rectangle(7, 7), 1, Vector3.ZAxis, false),
                    new Extrude(Polygon.Rectangle(6, 6), 2, Vector3.ZAxis, true)
                ),
                false,
                Guid.NewGuid(),
                null);
            Model.AddElements(element, element2);
        }

        [Fact]
        public void SolidIntersectsWithPlane()
        {
            this.Name = nameof(SolidIntersectsWithPlane);
            var n = 4;
            var outer = Polygon.Ngon(n, 2);
            var inner = Polygon.Ngon(n, 1.75).Reversed();
            var profile = new Profile(outer, new[] { inner });
            var sweep = new Extrude(profile, 5, Vector3.ZAxis, false);

            var plane1 = new Plane(new Vector3(0, 0, 1), new Vector3(0.5, 0.5, 0.5));
            var plane2 = new Plane(new Vector3(0, 0, 2), new Vector3(0.1, 0, 1));
            var plane3 = new Plane(new Vector3(0, 0, 5), Vector3.ZAxis);

            Plane[] planes = new Plane[] { plane1, plane2, plane3 };
            var r = new Random();
            foreach (var plane in planes)
            {
                if (sweep.Solid.Intersects(plane, out List<Polygon> result))
                {
                    if (result.Count > 1)
                    {
                        Assert.Equal(2, result.Count);
                        var cutProfile = new Profile(result[0], result.Skip(1).ToArray());
                        var lam = new Lamina(cutProfile, false);
                        var cutRep = new Representation(new List<SolidOperation>() { lam });
                        this.Model.AddElement(new GeometricElement(representation: cutRep, material: r.NextMaterial()));
                    }
                    else
                    {
                        Assert.Single(result);
                        this.Model.AddElement(new Panel(result[0], r.NextMaterial()));
                    }
                }
            }

            var rep = new Representation(new List<SolidOperation>() { sweep });
            var solidElement = new GeometricElement(representation: rep, material: BuiltInMaterials.Mass);
            this.Model.AddElement(solidElement);
        }

        [Fact]
        public void SolidIntersectsPlaneAtFaceReturningFaceLoop()
        {
            this.Name = nameof(SolidIntersectsPlaneAtFaceReturningFaceLoop);

            var r = new Random();
            var profile = Polygon.Rectangle(5, 5);
            var extrude = new Extrude(profile, 5, Vector3.ZAxis, false);
            var plane = new Plane(new Vector3(0, -2.5, 2.5), Vector3.YAxis.Negate());
            Assert.True(extrude.Solid.Intersects(plane, out List<Polygon> result));

            Assert.Single(result);
            var p = result[0];
            Assert.Equal(4, p.Vertices.Count);

            this.Model.AddElement(new Panel(p, r.NextMaterial()));

            var rep = new Representation(new List<SolidOperation>() { extrude });
            var solidElement = new GeometricElement(representation: rep, material: BuiltInMaterials.Mass);
            this.Model.AddElement(solidElement);
        }

        [Fact]
        public void SolidIntersectsPlaneAtVertexWithNoResult()
        {
            var profile = Polygon.Rectangle(5, 5);
            var extrude = new Extrude(profile, 5, Vector3.ZAxis, false);
            var plane = new Plane(new Vector3(2.5, 2.5, 5.0), new Vector3(0.5, 0.5, 0.5));
            Assert.False(extrude.Solid.Intersects(plane, out List<Polygon> result));
        }

        [Fact]
        public void SolidIntersectsPlaneAtEdgeWithNoResult()
        {
            var profile = Polygon.Rectangle(5, 5);
            var extrude = new Extrude(profile, 5, Vector3.ZAxis, false);
            var plane = new Plane(new Vector3(2.5, 2.5, 0.0), new Vector3(0.5, 0.5, 0.0));
            Assert.False(extrude.Solid.Intersects(plane, out List<Polygon> result));
        }

        [Fact]
        public void SolidIntersectPlaneTwice()
        {
            this.Name = nameof(SolidIntersectPlaneTwice);
            var r = new Random();
            var l = Polygon.L(5, 5, 1);
            var profile = new Profile(l, l.Offset(-0.1).Reversed());

            var arc = new Arc(Vector3.Origin, 5, 0, 180);
            var sweep = new Sweep(profile, arc, 0, 0, 0, false);
            var plane = new Plane(Vector3.Origin, Vector3.YAxis.Negate());
            Assert.True(sweep.Solid.Intersects(plane, out List<Polygon> result));

            Assert.Equal(4, result.Count);
            foreach (var p in result)
            {
                Assert.Equal(6, p.Vertices.Count);
                this.Model.AddElement(new Panel(p, r.NextMaterial()));
            }

            var rep = new Representation(new List<SolidOperation>() { sweep });
            var solidElement = new GeometricElement(representation: rep, material: BuiltInMaterials.Mass);
            this.Model.AddElement(solidElement);
        }

        [Theory]
        [InlineData("SolidIntersectionTest1", "../../../models/Geometry/SolidPlaneIntersection/debug-case-1.json")]
        [InlineData("SolidIntersectionTest2", "../../../models/Geometry/SolidPlaneIntersection/debug-case-2.json")]
        [InlineData("SolidIntersectionTest3", "../../../models/Geometry/SolidPlaneIntersection/debug-case-3.json")]
        public void SolidPlaneIntersectionTests(string name, string path)
        {
            this.Name = name;

            var r = new Random();

            var di = JsonConvert.DeserializeObject<DebugInfo>(File.ReadAllText(path), new[] { new SolidConverter() });
            foreach (var solid in di.Solid)
            {
                Assert.True(di.Plane.Normal.IsUnitized());
                Assert.True(solid.Intersects(di.Plane, out var results));

                foreach (var p in results)
                {
                    this.Model.AddElement(new Panel(p, r.NextMaterial()));
                }
                var rep = new Representation(new List<SolidOperation>() { new ConstructedSolid(solid, false) });
                var solidElement = new GeometricElement(representation: rep, material: BuiltInMaterials.Mass);
                this.Model.AddElement(solidElement);
            }
        }

        private class DebugInfo
        {
            public List<Solid> Solid { get; set; }
            public Plane Plane { get; set; }
            public string Exception { get; set; }
        }
    }

}