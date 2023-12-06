using Xunit;
using Elements.Geometry;
using Elements.Geometry.Solids;
using Elements.Geometry.Profiles;
using Xunit.Abstractions;
using System.Collections.Generic;
using System;
using System.IO;
using Elements.Serialization.glTF;
using Elements.Serialization.JSON;
using System.Linq;
using System.Text.Json;
using Elements.Geometry.Tessellation;

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
            var subtraction = mesh.ToCsg().Subtract(solid.ToCsg());
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

            var options = new JsonSerializerOptions()
            {
                WriteIndented = true
            };
            options.Converters.Add(new SolidConverter());

            var json = JsonSerializer.Serialize(solid, options);
            var newSolid = JsonSerializer.Deserialize<Solid>(json, options);

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

            var options = new JsonSerializerOptions()
            {
                IncludeFields = true
            };
            var allPolygons = JsonSerializer.Deserialize<List<(Polygon outerLoop, List<Polygon> innerLoops)>>(File.ReadAllText("../../../models/Geometry/ExampleConstructedSolidPolygons.json"), options);
            var solid = new Solid();
            foreach (var (outerLoop, innerLoops) in allPolygons)
            {
                solid.AddFace(outerLoop, innerLoops, true);
            }
            var solidOp = new ConstructedSolid(solid, false)
            {
                LocalTransform = new Transform()
            };
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

            CreateTestSolids(out GeometricElement a, out GeometricElement b);

            var s = Solid.Union(a.Representation.SolidOperations[0], b.Representation.SolidOperations[0]);

            Assert.Equal(28, s.Faces.Count);

            var i = new GeometricElement(null, BuiltInMaterials.Steel, new Representation(new List<SolidOperation> { new ConstructedSolid(s) }));
            this.Model.AddElement(i);
        }

        [Fact]
        public void Difference()
        {
            this.Name = nameof(Difference);

            CreateTestSolids(out GeometricElement a, out GeometricElement b);
            var s = Solid.Difference(a.Representation.SolidOperations[0], b.Representation.SolidOperations[0]);

            Assert.Equal(15, s.Faces.Count);

            var i = new GeometricElement(null, BuiltInMaterials.Steel, new Representation(new List<SolidOperation> { new ConstructedSolid(s) }));
            this.Model.AddElement(i);
        }

        [Fact]
        public void Intersection()
        {
            this.Name = nameof(Intersection);

            CreateTestSolids(out GeometricElement a, out GeometricElement b);

            var s = Solid.Intersection(a.Representation.SolidOperations[0], b.Representation.SolidOperations[0]);

            Assert.Equal(12, s.Faces.Count);

            var i = new GeometricElement(null, BuiltInMaterials.Steel, new Representation(new List<SolidOperation> { new ConstructedSolid(s) }));
            this.Model.AddElement(i);
        }

        [Fact]
        public void AllBooleans()
        {
            this.Name = nameof(AllBooleans);

            CreateTestSolids(out GeometricElement a, out GeometricElement b);

            var t1 = new Transform(new Vector3(10, 0));
            var t2 = new Transform(new Vector3(15, 0));
            var s1 = Solid.Union(a.Representation.SolidOperations[0], b.Representation.SolidOperations[0]);
            var s2 = Solid.Difference(a.Representation.SolidOperations[0], b.Representation.SolidOperations[0]);
            var s3 = Solid.Intersection(a.Representation.SolidOperations[0], b.Representation.SolidOperations[0]);
            var i1 = new GeometricElement(null, BuiltInMaterials.Steel, new Representation(new List<SolidOperation> { new ConstructedSolid(s1) }));
            var i2 = new GeometricElement(t1, BuiltInMaterials.Steel, new Representation(new List<SolidOperation> { new ConstructedSolid(s2) }));
            var i3 = new GeometricElement(t2, BuiltInMaterials.Steel, new Representation(new List<SolidOperation> { new ConstructedSolid(s3) }));

            this.Model.AddElements(DrawEdges(s1, null));
            this.Model.AddElements(DrawEdges(s2, t1));
            this.Model.AddElements(DrawEdges(s3, t2));
            this.Model.AddElements(i1, i2, i3);
        }

        private static List<ModelCurve> DrawEdges(Solid s, Transform t)
        {
            var modelCurves = new List<ModelCurve>();
            foreach (var e in s.Edges)
            {
                var from = e.Value.Right.Vertex.Point;
                var to = e.Value.Left.Vertex.Point;
                modelCurves.Add(new ModelCurve(new Line(from, to).TransformedLine(t)));
            }
            return modelCurves;
        }

        private static void CreateTestSolids(out GeometricElement a, out GeometricElement b)
        {
            var r = new Transform();
            r.Move(2.5, 2.5, 2.5);
            a = new Mass(Polygon.Rectangle(5, 5), 5);
            b = new Mass(new Circle(r, 2.5).ToPolygon(19), 5);

            a.UpdateRepresentations();
            b.UpdateRepresentations();

            var rotate = new Transform();
            rotate.Rotate(Vector3.XAxis, 15);
            b.Representation.SolidOperations[0].LocalTransform = rotate;
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

        [Fact]
        public void SweepWithSetbacksRegressionTest()
        {
            Name = nameof(SweepWithSetbacksRegressionTest);
            Polygon crossSection = Polygon.Rectangle(0.25, 0.25);

            Polyline curve = new(new List<Vector3>
            {
                    new Vector3(x: 20.0, y: 15.0, z:0.0),
                    new Vector3(x: 20.0, y: 15.0, z:1.0),
                    new Vector3(x: 20.0, y: 14.5, z:1.5),
                    new Vector3(x: 19.5, y: 14.5, z:1.5),
            }
            );

            var sweep = new Sweep(
                new Profile(crossSection),
                curve,
                startSetback: 1,
                endSetback: 1,
                profileRotation: 0,
                isVoid: false
            );
            var rep = new Representation(new List<SolidOperation>() { sweep });
            Model.AddElement(new GeometricElement(representation: rep, material: BuiltInMaterials.Black));
        }

        [Theory]
        [InlineData("SolidIntersectionTest1", "../../../models/Geometry/SolidPlaneIntersection/debug-case-1.json")]
        [InlineData("SolidIntersectionTest2", "../../../models/Geometry/SolidPlaneIntersection/debug-case-2.json")]
        [InlineData("SolidIntersectionTest3", "../../../models/Geometry/SolidPlaneIntersection/debug-case-3.json")]
        public void SolidPlaneIntersectionTests(string name, string path)
        {
            this.Name = name;

            var r = new Random();

            var options = new JsonSerializerOptions();
            options.Converters.Add(new SolidConverter());
            var di = JsonSerializer.Deserialize<DebugInfo>(File.ReadAllText(path), options);
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

        [Fact]
        public void CoplanarSolidFacesUnionCorrectly()
        {
            this.Name = nameof(CoplanarSolidFacesUnionCorrectly);

            var s1 = new Extrude(Polygon.Rectangle(2, 2), 2, Vector3.ZAxis, false);
            var s2 = new Extrude(Polygon.Rectangle(2, 2).TransformedPolygon(new Transform(new Vector3(1, 1))), 2, Vector3.ZAxis, false);
            var result = Solid.Union(s1._solid, null, s2._solid, null);

            var rep = new Representation(new List<SolidOperation>() { new ConstructedSolid(result) });
            var solidElement = new GeometricElement(representation: rep);
            this.Model.AddElement(solidElement);
            this.Model.AddElements(DrawEdges(result, null));

            Assert.Equal(10, result.Faces.Count);

            var t = new Transform(new Vector3(5, 0));
            var result1 = Solid.Difference(s1._solid, t, s2._solid, t);
            var rep1 = new Representation(new List<SolidOperation>() { new ConstructedSolid(result1) });
            var solidElement1 = new GeometricElement(representation: rep1);
            this.Model.AddElement(solidElement1);
            this.Model.AddElements(DrawEdges(result1, null));

            Assert.Equal(8, result1.Faces.Count);

            var t1 = new Transform(new Vector3(10, 0));
            var result2 = Solid.Intersection(s1._solid, t1, s2._solid, t1);
            var rep2 = new Representation(new List<SolidOperation>() { new ConstructedSolid(result2) });
            var solidElement2 = new GeometricElement(representation: rep2);
            this.Model.AddElement(solidElement2);
            this.Model.AddElements(DrawEdges(result2, null));

            Assert.Equal(6, result2.Faces.Count);
        }

        [Fact]
        public void DifferenceInCenterOfFaceCreatesVoid()
        {
            this.Name = nameof(DifferenceInCenterOfFaceCreatesVoid);

            var s1 = new Extrude(Polygon.Rectangle(2, 2), 2, Vector3.ZAxis, false);
            var s2 = new Extrude(Polygon.Star(0.5, 0.25, 5), 6, Vector3.ZAxis, false);
            var result1 = Solid.Difference(s1.Solid, new Transform(new Vector3(0, 0, -1)), s2.Solid, new Transform(new Vector3(0, 0, -3)));

            // var t = new Transform();
            // t.Move(new Vector3(0, 0, -3));
            // t.Rotate(Vector3.XAxis, 90);
            // var s3 = new Extrude(Polygon.Rectangle(0.6, 0.6), 6, Vector3.ZAxis, false);
            // result1 = Solid.Difference(result1, null, s3.Solid, t);

            var rep = new Representation(new List<SolidOperation>() { new ConstructedSolid(result1) });
            var solidElement = new GeometricElement(representation: rep);
            this.Model.AddElement(solidElement);
            this.Model.AddElements(DrawEdges(result1, null));
        }

        [Fact]
        public void DifferenceWhichSplitsVolumeSucceeds()
        {
            this.Name = nameof(DifferenceWhichSplitsVolumeSucceeds);

            var s1 = new Extrude(Polygon.Rectangle(2, 2), 2, Vector3.ZAxis, false);
            var s2 = new Extrude(Polygon.Rectangle(0.5, 4), 6, Vector3.ZAxis, false);
            var result1 = Solid.Difference(s1.Solid, new Transform(new Vector3(0, 0, -1)), s2.Solid, new Transform(new Vector3(0, 0, -3)));

            var s3 = new Extrude(Polygon.Rectangle(4, 0.5), 3, Vector3.ZAxis, false);
            result1 = Solid.Difference(result1, null, s3.Solid, new Transform(new Vector3(0, 0, -3.25)));

            var rep = new Representation(new List<SolidOperation>() { new ConstructedSolid(result1) });
            var solidElement = new GeometricElement(representation: rep);
            this.Model.AddElement(solidElement);

            this.Model.AddElements(DrawEdges(result1, null));
            Assert.Equal(20, result1.Faces.Count);
            Assert.Equal(32, result1.Vertices.Count);
            Assert.Equal(48, result1.Edges.Count);
        }

        [Fact]
        public void UnionAcrossVolumeSucceeds()
        {
            // This tests the union of two crossing volumes which
            // share top and bottom faces.
            this.Name = nameof(UnionAcrossVolumeSucceeds);

            var s1 = new Extrude(Polygon.Rectangle(2, 2), 2, Vector3.ZAxis, false);
            var s2 = new Extrude(Polygon.Rectangle(0.5, 4), 2, Vector3.ZAxis, false);
            var result1 = Solid.Union(s1.Solid, null, s2.Solid, null);

            var rep = new Representation(new List<SolidOperation>() { new ConstructedSolid(result1) });
            var solidElement = new GeometricElement(representation: rep);
            this.Model.AddElement(solidElement);

            this.Model.AddElements(DrawEdges(result1, null));
            Assert.Equal(14, result1.Faces.Count);
            Assert.Equal(24, result1.Vertices.Count);
        }

        [Fact]
        public void BlindHoleHasBottomFace()
        {
            // The bottom face of a blind hole is inside the main solid,
            // but does not intersect with any of the faces of the main solid.
            // Ensure that the bottom face is not excluded.
            this.Name = nameof(BlindHoleHasBottomFace);

            var s1 = new Extrude(Polygon.Rectangle(2, 2), 2, Vector3.ZAxis, false);
            var s2 = new Extrude(Polygon.Rectangle(0.5, 0.5), 6, Vector3.ZAxis, false);
            var result = Solid.Difference(s1.Solid, null, s2.Solid, new Transform(new Vector3(0, 0, 0.5)));

            var rep = new Representation(new List<SolidOperation>() { new ConstructedSolid(result) });
            var solidElement = new GeometricElement(representation: rep);
            this.Model.AddElement(solidElement);

            this.Model.AddElements(DrawEdges(result, null));
            Assert.Equal(11, result.Faces.Count);
        }

        [Fact]
        public void HorizontalThroughHole()
        {
            this.Name = nameof(HorizontalThroughHole);

            var s1 = new Extrude(Polygon.Rectangle(2, 2), 2, Vector3.ZAxis, false);
            var s2 = new Extrude(Polygon.Rectangle(0.5, 0.5), 6, Vector3.ZAxis, false);
            var t = new Transform();
            t.Rotate(Vector3.YAxis, 90);
            t.Move(new Vector3(-2, 0, 1));
            var result = Solid.Difference(s1.Solid, null, s2.Solid, t);

            var rep = new Representation(new List<SolidOperation>() { new ConstructedSolid(result) });
            var solidElement = new GeometricElement(representation: rep);
            this.Model.AddElement(solidElement);

            this.Model.AddElements(DrawEdges(result, null));
            Assert.Equal(10, result.Faces.Count);
        }

        [Fact]
        public void ThroughHoleWithEqualFaces()
        {
            // The bottom face of a blind hole is inside the main solid,
            // but does not intersect with any of the faces of the main solid.
            // Ensure that the bottom face is not excluded.
            this.Name = nameof(ThroughHoleWithEqualFaces);

            var s1 = new Extrude(Polygon.Rectangle(2, 2), 2, Vector3.ZAxis, false);
            var s2 = new Extrude(Polygon.Rectangle(0.5, 0.5), 2, Vector3.ZAxis, false);
            var result = Solid.Difference(s1.Solid, null, s2.Solid, null);

            var rep = new Representation(new List<SolidOperation>() { new ConstructedSolid(result) });
            var solidElement = new GeometricElement(representation: rep);
            this.Model.AddElement(solidElement);

            this.Model.AddElements(DrawEdges(result, null));
            Assert.Equal(10, result.Faces.Count);
        }

        [Fact]
        public void PassingHoleWithEqualFaces()
        {
            // The bottom face of a blind hole is inside the main solid,
            // but does not intersect with any of the faces of the main solid.
            // Ensure that the bottom face is not excluded.
            this.Name = nameof(PassingHoleWithEqualFaces);

            var s1 = new Extrude(Polygon.Rectangle(2, 2), 2, Vector3.ZAxis, false);
            var s2 = new Extrude(Polygon.Rectangle(0.5, 0.5).TransformedPolygon(new Transform(new Vector3(0, 0.8))), 2, Vector3.ZAxis, false);
            var result = Solid.Difference(s1.Solid, null, s2.Solid, null);

            var rep = new Representation(new List<SolidOperation>() { new ConstructedSolid(result) });
            var solidElement = new GeometricElement(representation: rep);
            this.Model.AddElement(solidElement);

            this.Model.AddElements(DrawEdges(result, null));
            Assert.Equal(10, result.Faces.Count);
        }

        [Fact]
        public void TwoHoles()
        {
            this.Name = nameof(TwoHoles);

            var s1 = new Extrude(Polygon.Rectangle(2, 2), 2, Vector3.ZAxis, false);
            var s2 = new Extrude(Polygon.Rectangle(0.25, 0.25).TransformedPolygon(new Transform(new Vector3(0, 0.5))), 2.5, Vector3.ZAxis, false);
            var s3 = new Extrude(Polygon.Rectangle(0.25, 0.25).TransformedPolygon(new Transform(new Vector3(0, -0.5))), 2.5, Vector3.ZAxis, false);
            var result = Solid.Union(s2.Solid, null, s3.Solid, null);

            result = Solid.Difference(s1.Solid, null, result, null);

            var rep = new Representation(new List<SolidOperation>() { new ConstructedSolid(result) });
            var solidElement = new GeometricElement(representation: rep);
            this.Model.AddElement(solidElement);

            this.Model.AddElements(DrawEdges(result, null));
            Assert.Equal(14, result.Faces.Count);
        }

        [Fact]
        public void TessellationHasCorrectNumberOfVertices()
        {
            var panel = new Panel(Polygon.L(5, 5, 2));
            panel.UpdateRepresentations();
            var buffer = Tessellation.Tessellate<GraphicsBuffers>(panel.Representation.SolidOperations.Select(so => new SolidTesselationTargetProvider(so.Solid, 0, so.LocalTransform)));
            Assert.Equal(12, buffer.VertexCount); // Two faces of 6 vertices each
            Assert.Equal(8, buffer.FacetCount); // Two faces of 4 facets each.
        }

        [Fact]
        public void TesselationOfModelThatProducesEmptyTrianles()
        {
            var model = Model.FromJson(File.ReadAllText("../../../models/Geometry/WallFromBasicModel.json"));
            var wall = model.AllElementsOfType<WallByProfile>().First();
            wall.UpdateRepresentations();
            wall.UpdateBoundsAndComputeSolid();
            var buffer = wall._csg.Tessellate(modifyVertexAttributes: wall.ModifyVertexAttributes);
            Assert.True(buffer.VertexCount > 0);
        }

        [Fact]
        public void FlippedExtrude()
        {
            Name = nameof(FlippedExtrude);
            var normalExtrude = new Extrude(Polygon.Rectangle(1, 1), 1, Vector3.ZAxis);
            var flippedExtrude = new Extrude(Polygon.Rectangle(1, 1).TransformedPolygon(new Transform(2, 0, 0)), 1, Vector3.ZAxis, false, true);
            var geo = new GeometricElement
            {
                Representation = new Representation(normalExtrude, flippedExtrude)
            };
            Model.AddElement(geo);
            var centroid1 = new Vector3(0, 0, 0.5);
            var centroid2 = new Vector3(2, 0, 0.5);
            Assert.All(normalExtrude._solid.Faces, (face) =>
            {
                var faceCenter = face.Value.Outer.ToPolygon().Centroid();
                var centroidToFaceCenter = faceCenter - centroid1;
                var faceNormal = face.Value.Plane().Normal;
                Assert.True(centroidToFaceCenter.Unitized().Dot(faceNormal.Unitized()) == 1.0);
            });
            Assert.All(flippedExtrude._solid.Faces, (face) =>
            {
                var faceCenter = face.Value.Outer.ToPolygon().Centroid();
                var centroidToFaceCenter = faceCenter - centroid2;
                var faceNormal = face.Value.Plane().Normal;
                Assert.True(centroidToFaceCenter.Unitized().Dot(faceNormal.Unitized()) == -1.0);
            });
            // Visually verify that flipped geometry is flipped.
        }

        private class DebugInfo
        {
            public List<Solid> Solid { get; set; }
            public Plane Plane { get; set; }
            public string Exception { get; set; }
        }
    }

}