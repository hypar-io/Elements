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
        public void Union()
        {
            this.Name = nameof(Union);
            var r = new Transform();
            var a = new Mass(Polygon.Rectangle(5, 5), 5);
            var b = new Mass(Polygon.Rectangle(5, 5).TransformedPolygon(new Transform(2.5, 2.5, 2.5)), 5);
            a.UpdateRepresentations();
            b.UpdateRepresentations();
            var s = SolidBoolean.Union(a.Representation.SolidOperations[0], b.Representation.SolidOperations[0]);

            Assert.Equal(12, s.Faces.Count);

            var i = new GeometricElement(null, BuiltInMaterials.Default, new Representation(new List<SolidOperation> { new ConstructedSolid(s) }));
            this.Model.AddElement(i);
        }

        [Fact]
        public void Difference()
        {
            this.Name = nameof(Difference);
            var r = new Transform();
            r.Move(2.5, 2.5, 2.5);
            var a = new Mass(Polygon.Rectangle(5, 5), 5);
            var b = new Mass(new Circle(2.5).ToPolygon(19).TransformedPolygon(r), 5);

            a.UpdateRepresentations();
            b.UpdateRepresentations();

            var rotate = new Transform();
            rotate.Rotate(Vector3.XAxis, 15);
            b.Representation.SolidOperations[0].LocalTransform = rotate;

            var s = SolidBoolean.Difference(a.Representation.SolidOperations[0], b.Representation.SolidOperations[0]);
            this.Model.AddElement(b);

            Assert.Equal(15, s.Faces.Count);

            var i = new GeometricElement(null, BuiltInMaterials.Steel, new Representation(new List<SolidOperation> { new ConstructedSolid(s) }));
            this.Model.AddElement(i);
        }

        [Fact]
        public void Intersection()
        {
            this.Name = nameof(Intersection);
            var a = new Mass(Polygon.Rectangle(5, 5), 5);
            var b = new Mass(Polygon.Rectangle(5, 5).TransformedPolygon(new Transform(2.5, 2.5, 2.5)), 5);
            a.UpdateRepresentations();
            b.UpdateRepresentations();

            var s = SolidBoolean.Intersection(a.Representation.SolidOperations[0], b.Representation.SolidOperations[0]);

            Assert.Equal(6, s.Faces.Count);

            var i = new GeometricElement(null, BuiltInMaterials.Default, new Representation(new List<SolidOperation> { new ConstructedSolid(s) }));
            this.Model.AddElement(i);
        }
    }

}