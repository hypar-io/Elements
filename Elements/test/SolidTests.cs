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

namespace Elements.Tests
{
    public class SolidTests : ModelTest
    {
        private readonly ITestOutputHelper output;

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
            var solid = Solid.SweepFace(outer, new Polygon[] { Polygon.Ngon(n, 0.5).Reversed() }, 5);
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
        public void SolidPlaneIntersection()
        {
            this.Name = "SolidPlaneIntersection";

            var n = 3;
            var outer = Polygon.Ngon(n, 2);
            var solid = Solid.SweepFace(outer, new Polygon[] { Polygon.Ngon(n, 0.5).Reversed() }, 5);
            var mesh = new Mesh();
            solid.Tessellate(ref mesh);
            mesh.ComputeNormals();
            var meshElement = new MeshElement(mesh, BuiltInMaterials.Mass);
            this.Model.AddElement(meshElement);

            var rand = new Random();

            var slicePlane1 = new Plane(new Vector3(0, 0, 2.5), new Vector3(0.5, 0.5, 0));
            if (solid.TryIntersect(slicePlane1, out var pgons))
            {
                foreach (var p in pgons)
                {
                    this.Model.AddElement(new Panel(p, new Material(Guid.NewGuid().ToString(), new Color(rand.NextDouble(), rand.NextDouble(), rand.NextDouble(), 1.0))));
                }
            }

            var slicePlane2 = new Plane(new Vector3(0, 0, 1), new Vector3(0.3, 0.0, 1.0).Unitized());
            if (solid.TryIntersect(slicePlane2, out var pgons2))
            {
                foreach (var p in pgons2)
                {
                    this.Model.AddElement(new Panel(p, new Material(Guid.NewGuid().ToString(), new Color(rand.NextDouble(), rand.NextDouble(), rand.NextDouble(), 1.0))));
                }
            }

            Assert.Equal(2, pgons.Count);
            Assert.Equal(2, pgons2.Count);
        }

        [Fact]
        public void SolidOperationsPlaneIntersection()
        {
            this.Name = "SolidOperationsPlaneIntersection";

            var leg1 = new Extrude(Polygon.Rectangle(10, 10), 30, Vector3.ZAxis, false);
            var t = new Transform(0, 0, 0);
            t.Rotate(Vector3.ZAxis, 45);
            t.Move(30, 0, 0);
            var leg2 = new Extrude((Polygon)Polygon.Rectangle(10, 10).Transformed(t), 50, new Vector3(0.1, 0.1, 1.0).Unitized(), false);
            var t1 = new Transform(15, 0, 0);
            var podium = new Extrude((Polygon)Polygon.Star(30, 25, 5).Transformed(t1), 10, Vector3.ZAxis, false);
            var atrium = new Extrude((Polygon)Polygon.Rectangle(20, 20).Transformed(t1), 30, Vector3.ZAxis, true);

            var skyBridge = new Sweep(Polygon.Rectangle(10, 10), new Line(new Vector3(0, 0, 30), new Vector3(30, 0, 40)), 0, 0, 0.0, false);
            var geom = new GeometricElement(new Transform(), BuiltInMaterials.Trans, new Representation(new List<SolidOperation> { leg1, leg2, podium, atrium, skyBridge }), false, Guid.NewGuid(), "Building Mass");

            this.Model.AddElement(geom);

            for (var i = 0.0; i < 100; i += 1.0)
            {
                var slicePlane = new Plane(new Vector3(0, 0, i), new Vector3(0, 0, 1));
                if (geom.Representation.SolidOperations.TryIntersect(slicePlane, out var result))
                {
                    foreach (var profile in result)
                    {
                        this.Model.AddElement(new Floor(profile, 0.01));
                    }
                }
            }
        }

        [Fact]
        public void SolidPlaneIntersectionReturnsFalseWhenNotIntersecting()
        {
            var box = new Extrude(Polygon.Rectangle(10, 10), 30, Vector3.ZAxis, false);
            var plane = new Plane(new Vector3(0, 0, 31), Vector3.ZAxis);
            var intersect = new[] { box }.TryIntersect(plane, out _);
            Assert.False(intersect);
        }

        [Fact]
        public void SolidPlaneIntersectionThrowsExceptionWhenPlaneNotHoriztonal()
        {
            var box = new Extrude(Polygon.Rectangle(10, 10), 30, Vector3.ZAxis, false);
            var plane = new Plane(new Vector3(0, 0, 0), Vector3.XAxis);
            Assert.Throws<Exception>(() => new[] { box }.TryIntersect(plane, out _));
        }

        [Fact]
        public void SolidPlaneIntersectionReturnsDisjointProfiles()
        {
            var leg1 = new Extrude(Polygon.Rectangle(10, 10), 30, Vector3.ZAxis, false);
            var t = new Transform(0, 0, 0);
            t.Move(30, 0, 0);
            var leg2 = new Extrude((Polygon)Polygon.Rectangle(10, 10).Transformed(t), 50, Vector3.ZAxis, false);
            var plane = new Plane(new Vector3(0, 0, 15), Vector3.ZAxis);
            var intersect = new[] { leg1, leg2 }.TryIntersect(plane, out var result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void SolidPlaneIntersectionReturnsProfileWithOneHole()
        {
            var outer = new Extrude(Polygon.Rectangle(10, 10), 30, Vector3.ZAxis, false);
            var inner = new Extrude((Polygon)Polygon.Rectangle(5, 5), 30, Vector3.ZAxis, true);
            var plane = new Plane(new Vector3(0, 0, 15), Vector3.ZAxis);
            var intersect = new[] { outer, inner }.TryIntersect(plane, out var result);
            Assert.True(intersect);
            Assert.Equal(1, result.Count);
            Assert.Equal(1, result[0].Voids.Count);
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
            var profile = WideFlangeProfileServer.Instance.GetProfileByType(WideFlangeProfileType.W10x100);
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
            var profile = WideFlangeProfileServer.Instance.GetProfileByType(WideFlangeProfileType.W10x100);
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
            var profile = WideFlangeProfileServer.Instance.GetProfileByType(WideFlangeProfileType.W10x100);
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
            var profile = WideFlangeProfileServer.Instance.GetProfileByType(WideFlangeProfileType.W10x100);
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
    }

}