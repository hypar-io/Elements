using System.Collections.Generic;
using Elements.Geometry;
using Elements.Geometry.Profiles;
using Elements.Geometry.Solids;
using System;
using Xunit;
using System.Linq;
using Newtonsoft.Json;
using Elements.Geometry.Tessellation;
using LibTessDotNet.Double;
using Xunit.Abstractions;

namespace Elements.Tests
{
    public class CsgTests : ModelTest
    {
        private HSSPipeProfileFactory _profileFactory = new HSSPipeProfileFactory();

        private readonly ITestOutputHelper output;

        public CsgTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void Csg()
        {
            this.Name = "Elements_Geometry_Csg";

            var s1 = new Extrude(Polygon.Rectangle(Vector3.Origin, new Vector3(30, 30)), 50, Vector3.ZAxis, false);
            var s2 = new Extrude(Polygon.Rectangle(30, 30), 30, Vector3.ZAxis, true);
            var s3 = new Sweep(Polygon.Rectangle(Vector3.Origin, new Vector3(5, 5)), new Line(new Vector3(0, 0, 45), new Vector3(30, 0, 45)), 0, 0, 0, false);
            var poly = new Polygon(new List<Vector3>(){
                new Vector3(0,0,0), new Vector3(20,50,0), new Vector3(0,50,0)
            });
            var s4 = new Sweep(poly, new Line(new Vector3(0, 30, 0), new Vector3(30, 30, 0)), 0, 0, 0, true);

            var geom = new GeometricElement(new Transform(), new Material("Mod", Colors.White, 0.0, 0.0, "./Textures/UV.jpg"), new Representation(new List<SolidOperation>(){
                s1, s2, s3, s4
            }), false, Guid.NewGuid(), null);
            this.Model.AddElement(geom);
        }

        [Fact]
        public void Union()
        {
            this.Name = "CSG_Union";
            var s1 = new Extrude(Polygon.Rectangle(1, 1), 1, Vector3.ZAxis, false);
            var csg = s1.Solid.ToCsg();

            var s2 = new Extrude(Polygon.L(1.0, 2.0, 0.5), 1, Vector3.ZAxis, false);
            csg = csg.Union(s2.Solid.ToCsg());

            var result = new Mesh();
            csg.Tessellate(ref result);

            var me = new MeshElement(result);
            this.Model.AddElement(me);
        }


        [Fact]
        public void Difference()
        {
            this.Name = "CSG_Difference";
            var profile = Polygon.Rectangle(0.5, 0.5);

            var path = new Arc(Vector3.Origin, 5, 0, 270);
            var beam = new Beam(path, profile);

            var s2 = new Extrude(new Circle(Vector3.Origin, 6).ToPolygon(20), 1, Vector3.ZAxis, true);
            beam.Representation.SolidOperations.Add(s2);

            for (var i = path.Domain.Min; i < path.Domain.Max; i += 0.1)
            {
                var pt = path.PointAt(i);
                var hole = new Extrude(new Circle(Vector3.Origin, 0.05).ToPolygon(), 3, Vector3.ZAxis, true)
                {
                    LocalTransform = new Transform(pt + new Vector3(0, 0, -2))
                };
                beam.Representation.SolidOperations.Add(hole);
            }

            this.Model.AddElement(beam);
        }

        [Fact]
        public void SubtractWithProblematicPolygons()
        {
            var wallPerimeter = new Elements.Geometry.Polygon(new Vector3(-48.41, 0, 0), new Vector3(-48.53, 0, 0), new Vector3(-51.48, 0, 0), new Vector3(-51.48, 0, 3.81), new Vector3(-46.27, 0, 3.81));
            var wall = new WallByProfile(wallPerimeter,
                    0.2, new Line(new Vector3(0, 0, 0), new Vector3(1, 0, 0)));
            var oP = new Elements.Geometry.Polygon(new Vector3(-46.27, 0, 0.00), new Vector3(-46.42, 0, 0.00), new Vector3(-46.42, 0, 2.13), new Vector3(-47.34, 0, 2.13), new Vector3(-47.34, 0, 0.00));
            wall.AddOpening(oP);

            wall.UpdateRepresentations();
            var solid = wall.GetFinalCsgFromSolids();
        }

        [Fact]
        public void UnionWithPolygonsWhichCreateZeroAreaTessElement()
        {
            var profile1 = JsonConvert.DeserializeObject<Polygon>(
                @"{
                ""Vertices"": [
                    {
                        ""X"": -348.82036858497275,
                        ""Y"": 246.02759072077453,
                        ""Z"": 30.25
                    },
                    {
                        ""X"": -348.85572392403208,
                        ""Y"": 246.06294605983385,
                        ""Z"": 30.25
                    },
                    {
                        ""X"": -350.60244308413706,
                        ""Y"": 244.31622689972886,
                        ""Z"": 30.25
                    },
                    {
                        ""X"": -350.56708774507774,
                        ""Y"": 244.28087156066954,
                        ""Z"": 30.25
                    },
                    {
                        ""X"": -350.53173240601842,
                        ""Y"": 244.24551622161022,
                        ""Z"": 30.25
                    },
                    {
                        ""X"": -348.78501324591343,
                        ""Y"": 245.9922353817152,
                        ""Z"": 30.25
                    }
                ]}
                ");
            var profile2 = JsonConvert.DeserializeObject<Polygon>(
                @"{
                ""Vertices"": [
                    {
                        ""X"": -350.56708774507774,
                        ""Y"": 244.28087156066954,
                        ""Z"": 30.25
                    },
                    {
                        ""X"": -350.51708774507773,
                        ""Y"": 244.28087156066954,
                        ""Z"": 30.25
                    },
                    {
                        ""X"": -350.51708774507773,
                        ""Y"": 247.77430988087954,
                        ""Z"": 30.25
                    },
                    {
                        ""X"": -350.56708774507774,
                        ""Y"": 247.77430988087954,
                        ""Z"": 30.25
                    },
                    {
                        ""X"": -350.61708774507775,
                        ""Y"": 247.77430988087954,
                        ""Z"": 30.25
                    },
                    {
                        ""X"": -350.61708774507775,
                        ""Y"": 244.28087156066954,
                        ""Z"": 30.25
                    }
                    ]}"
                    );
            var element = new GeometricElement
            {
                Representation = new Representation(new List<SolidOperation>{
                    new Extrude(profile1, 1, Vector3.ZAxis, false),
                    new Extrude(profile2, 1, Vector3.ZAxis, false)
                })
            };

            element.UpdateRepresentations();
            element.GetFinalCsgFromSolids();
        }

        [Fact]
        public void TessellatorProducesCorrectVertexNormals()
        {
            Name = nameof(TessellatorProducesCorrectVertexNormals);
            var shape = new Polygon((4.96243, 50.58403), (5.78472, 50.58403), (5.78472, 65.83403), (-7.05727, 65.83403), (-7.05727, 50.57403), (4.96243, 50.57403));

            var geoElem = new GeometricElement(representation: new Extrude(shape, 1, Vector3.ZAxis, false));
            Model.AddElement(geoElem);
            var solid = geoElem.GetFinalCsgFromSolids();
            var arrows = new ModelArrows();
            var mgb = Tessellation.Tessellate<MockGraphicsBuffer>(new Csg.Solid[] { solid }.Select(s => new CsgTessellationTargetProvider(solid, 0)));
            for (int i = 0; i < mgb.Indices.Count; i += 3)
            {
                var a = mgb.Indices[i];
                var b = mgb.Indices[i + 1];
                var c = mgb.Indices[i + 2];
                var verts = new[] { mgb.Vertices[a], mgb.Vertices[b], mgb.Vertices[c] };
                verts.ToList().ForEach((v) =>
                {
                    arrows.Vectors.Add((v.position, v.normal, 0.2, Colors.Blue));
                });
                var triangle = new Polygon(verts.Select(v => v.position).ToList());
                var normal = verts[0].normal;
                Assert.True(triangle.Normal().Dot(normal.Unitized()) > 0, "The vertex normals are pointing in the opposite direction as their triangles' winding should suggest");
                Model.AddElement(triangle.TransformedPolygon(new Transform(normal * 0.2)));
            }
            Model.AddElement(arrows);
        }

        [Fact]
        public void Tessellate_WithCombinedContourVertices_DoesNotThrowAndProducesValidBuffer()
        {
            // LibTess synthesizes new ContourVertices at intersection / T-junction points.
            // When the upstream Tess wasn't configured with a CombineCallback, those synthetic
            // vertices have Data == null, which previously NREd in
            // Tessellation.PackTessellationsIntoBuffers when it unboxed v.Data.
            // This adapter intentionally omits the CombineCallback to exercise that path.
            var adapter = new SelfIntersectingNoCombineTessAdapter();
            var provider = new InlineTessTargetProvider(adapter);

            var ex = Record.Exception(() =>
            {
                var buffer = Tessellation.Tessellate<MockGraphicsBuffer>(new[] { provider });
                Assert.NotNull(buffer);
                Assert.True(buffer.Indices.Count > 0, "Buffer should contain at least one triangle index.");
                Assert.True(buffer.Vertices.Count > 0, "Buffer should contain at least one vertex.");
            });
            Assert.Null(ex);
        }

        [Fact]
        public void CsgPolygonTessAdapter_RegistersCombineCallback_AllVerticesHaveData()
        {
            // Build a simple csg polygon and tessellate via the adapter. With the combine
            // callback registered, every vertex - including any synthesized by LibTess -
            // must carry the Hypar 4-tuple Data shape.
            var verts = new List<Csg.Vertex>
            {
                new Csg.Vertex(new Csg.Vector3D(0, 0, 0), new Csg.Vector2D(0, 0)),
                new Csg.Vertex(new Csg.Vector3D(10, 0, 0), new Csg.Vector2D(1, 0)),
                new Csg.Vertex(new Csg.Vector3D(10, 10, 0), new Csg.Vector2D(1, 1)),
                new Csg.Vertex(new Csg.Vector3D(0, 10, 0), new Csg.Vector2D(0, 1)),
            };
            var poly = new Csg.Polygon(verts);

            var adapter = new CsgPolygonTessAdapter(poly, faceId: 7, solidId: 3);
            var tess = adapter.GetTess();
            foreach (var v in tess.Vertices)
            {
                Assert.True(v.Data is CsgVertexData,
                    $"All ContourVertex.Data entries must carry the CsgVertexData shape; got {v.Data?.GetType().FullName ?? "null"}.");
            }
        }

        [Theory]
        [InlineData(0.35)]
        [InlineData(0.55)]
        [InlineData(0.6)]
        public void RoofDrainLikeUnion_GraphicsBufferTessellation_ProducesValidMesh(double diameter)
        {
            const int segments = 20;
            var cylinder = new Extrude(new Circle(Vector3.Origin, diameter / 2).ToPolygon(segments), 0.1, Vector3.ZAxis, false);
            var profile = new Circle(Vector3.Origin, 0.075 / 2).ToPolygon(segments);
            var elbow = Vector3.Origin + Vector3.ZAxis.Negate() * 0.2764999948978424;
            var connectorPoint = elbow + new Vector3(0.5, 0, 0);
            var connectorPipe = new Sweep(profile, new Polyline(new List<Vector3> { Vector3.Origin, elbow, connectorPoint }), 0, 0, 0, false);

            var geom = new GeometricElement(
                new Transform(),
                BuiltInMaterials.Steel,
                new Representation(new List<SolidOperation> { cylinder, connectorPipe }),
                false,
                Guid.NewGuid(),
                "roof-drain-like");

            geom.UpdateBoundsAndComputeSolid();
            var unionCsg = geom.GetFinalCsgFromSolids();
            Assert.NotNull(unionCsg);

            var unionBuffer = Tessellation.Tessellate<MockGraphicsBuffer>(
                new[] { new CsgTessellationTargetProvider(unionCsg, 0) });
            var unionTriangleCount = unionBuffer.Indices.Count / 3;
            var unionBounds = new BBox3(unionBuffer.Vertices.Select(v => v.position));

            uint solidId = 0;
            var providers = new List<SolidTesselationTargetProvider>();
            foreach (var so in geom.Representation.SolidOperations)
            {
                providers.Add(new SolidTesselationTargetProvider(so.Solid, solidId, so.LocalTransform));
                solidId++;
            }
            var skipBuffer = Tessellation.Tessellate<MockGraphicsBuffer>(providers);
            var skipTriangleCount = skipBuffer.Indices.Count / 3;
            var skipBounds = new BBox3(skipBuffer.Vertices.Select(v => v.position));

            Assert.True(unionTriangleCount > 0);
            Assert.True(skipTriangleCount > 0);
            Assert.True(unionBounds.Volume.ApproximatelyEquals(skipBounds.Volume, 1e-6));
            Assert.True(unionBounds.Min.Z.ApproximatelyEquals(skipBounds.Min.Z, 1e-3));
            Assert.True(unionBounds.Max.Z.ApproximatelyEquals(skipBounds.Max.Z, 1e-3));
            Assert.True(unionTriangleCount >= skipTriangleCount * 0.85,
                $"Union mesh ({unionTriangleCount} tris) lost too many triangles vs per-op reference ({skipTriangleCount} tris).");
        }

        [Fact]
        public void MultiSweepAssemblyAtSurveyCoordinates_UnionMatchesOriginUnion()
        {
            const int segmentCount = 20;
            const double surveyX = -49256;
            const int profileSegments = 12;
            var profile = new Circle(Vector3.Origin, 0.05).ToPolygon(profileSegments);

            List<SolidOperation> BuildOps(Vector3 origin)
            {
                var ops = new List<SolidOperation>();
                for (var i = 0; i < segmentCount; i++)
                {
                    var start = origin + new Vector3(i * 0.8, 0, 0);
                    var mid = start + new Vector3(0.4, (i % 2 == 0 ? 0.3 : -0.3), 0);
                    var end = mid + new Vector3(0.4, 0, 0);
                    ops.Add(new Sweep(profile, new Polyline(new List<Vector3> { start, mid, end }), 0, 0, 0, false));
                }
                return ops;
            }

            int UnionTriangleCount(IList<SolidOperation> ops)
            {
                var geom = new GeometricElement(
                    new Transform(),
                    BuiltInMaterials.Steel,
                    new Representation(ops),
                    false,
                    Guid.NewGuid(),
                    "pipe-assembly");

                geom.UpdateBoundsAndComputeSolid();
                var unionCsg = geom.GetFinalCsgFromSolids();
                Assert.NotNull(unionCsg);
                var unionBuffer = Tessellation.Tessellate<MockGraphicsBuffer>(
                    new[] { new CsgTessellationTargetProvider(unionCsg, 0) });
                return unionBuffer.Indices.Count / 3;
            }

            var originUnionTriangles = UnionTriangleCount(BuildOps(Vector3.Origin));
            var surveyUnionTriangles = UnionTriangleCount(BuildOps(new Vector3(surveyX, 12000, 5)));

            Assert.True(originUnionTriangles > 0);
            Assert.True(surveyUnionTriangles > 0);
            Assert.True(Math.Abs(originUnionTriangles - surveyUnionTriangles) <= 2,
                $"Survey-coordinate union ({surveyUnionTriangles} tris) should match origin union ({originUnionTriangles} tris).");
        }

        private void AssertNoTagCollisions(string label, global::Csg.Solid unionCsg)
        {
            // Mesh.FindOrCreateVertex (used by AddToMesh path) keys dedup on tag alone.
            // If any Csg.Vertex.Tag appeared at multiple positions post-union, that path
            // would weld unrelated corners. This canary asserts current CSG output
            // doesn't produce such collisions; if it ever does, fix Mesh.FindOrCreateVertex
            // (see TODO there) before this assertion is relaxed.
            var tagToPositions = new Dictionary<int, HashSet<(double, double, double)>>();
            foreach (var p in unionCsg.Polygons)
            {
                foreach (var v in p.Vertices)
                {
                    var pos = (Math.Round(v.Pos.X, 6), Math.Round(v.Pos.Y, 6), Math.Round(v.Pos.Z, 6));
                    if (!tagToPositions.TryGetValue(v.Tag, out var positions))
                    {
                        positions = new HashSet<(double, double, double)>();
                        tagToPositions[v.Tag] = positions;
                    }
                    positions.Add(pos);
                }
            }
            var collisions = tagToPositions.Where(kv => kv.Value.Count > 1).ToList();
            output.WriteLine($"[{label}] {tagToPositions.Count} distinct tags, {collisions.Count} colliding across positions");
            Assert.Empty(collisions);
        }

        [Fact]
        public void RealCsgUnion_OverlappingBoxes_TagCollisionProbe()
        {
            var s1 = new Extrude(Polygon.Rectangle(2, 2), 1, Vector3.ZAxis, false);
            var s2 = new Extrude(Polygon.Rectangle(2, 2), 1, Vector3.ZAxis, false)
            {
                LocalTransform = new Transform(new Vector3(0.5, 0.5, 0.3)),
            };
            var geom = new GeometricElement(
                new Transform(), BuiltInMaterials.Steel,
                new Representation(new List<SolidOperation> { s1, s2 }),
                false, Guid.NewGuid(), "boxes");
            geom.UpdateBoundsAndComputeSolid();
            AssertNoTagCollisions("boxes", geom.GetFinalCsgFromSolids());
        }

        [Fact]
        public void RealCsgUnion_RoofDrainShape_TagCollisionProbe()
        {
            const int segments = 20;
            var cylinder = new Extrude(new Circle(Vector3.Origin, 0.55 / 2).ToPolygon(segments), 0.1, Vector3.ZAxis, false);
            var profile = new Circle(Vector3.Origin, 0.075 / 2).ToPolygon(segments);
            var elbow = Vector3.Origin + Vector3.ZAxis.Negate() * 0.2764999948978424;
            var connectorPoint = elbow + new Vector3(0.5, 0, 0);
            var connectorPipe = new Sweep(profile, new Polyline(new List<Vector3> { Vector3.Origin, elbow, connectorPoint }), 0, 0, 0, false);
            var geom = new GeometricElement(
                new Transform(), BuiltInMaterials.Steel,
                new Representation(new List<SolidOperation> { cylinder, connectorPipe }),
                false, Guid.NewGuid(), "roof-drain");
            geom.UpdateBoundsAndComputeSolid();
            AssertNoTagCollisions("roof-drain", geom.GetFinalCsgFromSolids());
        }

        [Fact]
        public void RealCsgUnion_PipeChain_TagCollisionProbe()
        {
            const int segmentCount = 20;
            const int profileSegments = 12;
            var profile = new Circle(Vector3.Origin, 0.05).ToPolygon(profileSegments);
            var ops = new List<SolidOperation>();
            for (var i = 0; i < segmentCount; i++)
            {
                var start = new Vector3(i * 0.8, 0, 0);
                var mid = start + new Vector3(0.4, (i % 2 == 0 ? 0.3 : -0.3), 0);
                var end = mid + new Vector3(0.4, 0, 0);
                ops.Add(new Sweep(profile, new Polyline(new List<Vector3> { start, mid, end }), 0, 0, 0, false));
            }
            var geom = new GeometricElement(
                new Transform(), BuiltInMaterials.Steel,
                new Representation(ops),
                false, Guid.NewGuid(), "pipe-chain");
            geom.UpdateBoundsAndComputeSolid();
            AssertNoTagCollisions("pipe-chain", geom.GetFinalCsgFromSolids());
        }

        [Fact]
        public void AddToMesh_PentagramPolygon_CsgTexTagCombineKeepsSynthesizedVerticesDistinct()
        {
            // CsgExtensions.AddToMesh's >4-vertex branch invokes LibTess with the
            // CsgTexTagCombine callback. The callback assigns a unique synthetic tag
            // to each LibTess-synthesized vertex. Without it, synthesized vertices
            // all fall back to tag=0 in TexTagOrDefault, and FindOrCreateVertex (keyed
            // solely on tag) welds them all into one mesh vertex.
            var pts = new[]
            {
                new Csg.Vector3D(0, 10, 0),
                new Csg.Vector3D(6, -8, 0),
                new Csg.Vector3D(-10, 3, 0),
                new Csg.Vector3D(10, 3, 0),
                new Csg.Vector3D(-6, -8, 0),
            };
            var verts = new List<Csg.Vertex>(pts.Length);
            for (var i = 0; i < pts.Length; i++)
            {
                verts.Add(new Csg.Vertex(pts[i], new Csg.Vector2D(pts[i].X * 0.05, pts[i].Y * 0.05)));
            }
            var poly = new global::Csg.Polygon(verts);
            var csg = global::Csg.Solid.FromPolygons(new List<global::Csg.Polygon> { poly });

            var mesh = new Mesh();
            csg.Tessellate(ref mesh);

            // Pentagram should yield 5 input + 5 synthesized = 10 distinct mesh vertices.
            // If the callback is missing or broken, synthesized vertices share tag=0
            // and FindOrCreateVertex welds them, dropping vertex count well below 10.
            Assert.True(mesh.Vertices.Count >= 9,
                $"Expected ~10 distinct vertices from pentagram tessellation; got {mesh.Vertices.Count}. " +
                "Missing CsgTexTagCombine would weld all synthesized vertices via tag=0 in FindOrCreateVertex.");
        }

        [Fact]
        public void SolidFaceTessAdapter_PentagramOuterLoop_AdapterWiringCausesSynthesizedVerticesToCarryCsgVertexData()
        {
            // Construct a Face whose outer Loop is a 5-pointed self-intersecting
            // pentagram. WindingRule.Positive forces LibTess to synthesize vertices
            // at the 5 edge crossings; with the callback wired, each must carry
            // CsgVertexData. Mirror of the CsgPolygonTessAdapter pentagram test.
            var points = new[]
            {
                new Vector3(0, 10, 0),
                new Vector3(6, -8, 0),
                new Vector3(-10, 3, 0),
                new Vector3(10, 3, 0),
                new Vector3(-6, -8, 0),
            };
            var vertices = new Elements.Geometry.Solids.Vertex[points.Length];
            for (var i = 0; i < points.Length; i++)
            {
                vertices[i] = new Elements.Geometry.Solids.Vertex((uint)i, points[i]);
            }
            var halfEdges = new HalfEdge[points.Length];
            for (var i = 0; i < points.Length; i++)
            {
                halfEdges[i] = new HalfEdge(vertices[i]);
            }
            var loop = new Loop(halfEdges);
            var face = new Face(0, loop, null);

            var adapter = new SolidFaceTessAdapter(face, solidId: 9);
            var tess = adapter.GetTess();

            Assert.True(tess.Vertices.Length > points.Length,
                $"Expected LibTess to synthesize vertices for pentagram outer loop; got {tess.Vertices.Length} for {points.Length} input. " +
                "If this fails, the test no longer exercises the SolidFaceTessAdapter callback wiring.");

            foreach (var v in tess.Vertices)
            {
                Assert.True(v.Data is CsgVertexData,
                    $"Every vertex from SolidFaceTessAdapter must carry CsgVertexData; got {v.Data?.GetType().FullName ?? "null"} at ({v.Position.X},{v.Position.Y},{v.Position.Z}).");
            }
        }

        [Fact]
        public void CsgPolygonTessAdapter_PentagramContour_AdapterWiringCausesSynthesizedVerticesToCarryCsgVertexData()
        {
            // Build a pentagram (5-pointed star) as a single self-intersecting CSG
            // polygon contour. With WindingRule.Positive, LibTess synthesizes vertices
            // at the 5 edge crossings. This test routes through the REAL
            // CsgPolygonTessAdapter to prove its callback wiring (not the
            // CombineCallbacks internals) is the load-bearing piece.
            var verts = new List<Csg.Vertex>
            {
                new Csg.Vertex(new Csg.Vector3D(0, 10, 0), new Csg.Vector2D(0.5, 1)),
                new Csg.Vertex(new Csg.Vector3D(6, -8, 0), new Csg.Vector2D(0.8, 0.1)),
                new Csg.Vertex(new Csg.Vector3D(-10, 3, 0), new Csg.Vector2D(0.0, 0.7)),
                new Csg.Vertex(new Csg.Vector3D(10, 3, 0), new Csg.Vector2D(1.0, 0.7)),
                new Csg.Vertex(new Csg.Vector3D(-6, -8, 0), new Csg.Vector2D(0.2, 0.1)),
            };
            var poly = new Csg.Polygon(verts);

            var adapter = new CsgPolygonTessAdapter(poly, faceId: 42, solidId: 7);
            var tess = adapter.GetTess();

            // Pentagram crossings force LibTess to synthesize vertices.
            Assert.True(tess.Vertices.Length > verts.Count,
                $"Expected LibTess to synthesize vertices for the pentagram contour; got {tess.Vertices.Length} for {verts.Count} input. " +
                "If this fails, the test no longer exercises the adapter's callback wiring.");

            foreach (var v in tess.Vertices)
            {
                Assert.True(v.Data is CsgVertexData,
                    $"Every vertex from CsgPolygonTessAdapter must carry CsgVertexData; got {v.Data?.GetType().FullName ?? "null"} at ({v.Position.X},{v.Position.Y},{v.Position.Z}).");
            }
        }

        [Fact]
        public void CombineCallbacks_TwoOverlappingContours_SynthesizedVerticesCarryCsgVertexData()
        {
            // Two overlapping rectangular contours. With WindingRule.Positive their
            // shared region forces LibTess to synthesize vertices at the boundary
            // crossings. With DataCombine wired, every synthetic vertex must
            // carry CsgVertexData; without the callback it'd be null.
            var contourA = new[]
            {
                MakeRawVertex(0, 0, 0, faceId: 1, solidId: 0, tag: 10),
                MakeRawVertex(10, 0, 0, faceId: 1, solidId: 0, tag: 11),
                MakeRawVertex(10, 10, 0, faceId: 1, solidId: 0, tag: 12),
                MakeRawVertex(0, 10, 0, faceId: 1, solidId: 0, tag: 13),
            };
            var contourB = new[]
            {
                MakeRawVertex(5, 5, 0, faceId: 1, solidId: 0, tag: 20),
                MakeRawVertex(15, 5, 0, faceId: 1, solidId: 0, tag: 21),
                MakeRawVertex(15, 15, 0, faceId: 1, solidId: 0, tag: 22),
                MakeRawVertex(5, 15, 0, faceId: 1, solidId: 0, tag: 23),
            };

            var adapter = new MultiContourTessAdapter(new[] { contourA, contourB });
            var tess = adapter.GetTess();

            // Confirm LibTess actually synthesized at least one vertex.
            Assert.True(tess.Vertices.Length > contourA.Length + contourB.Length,
                $"Expected LibTess to synthesize vertices at contour crossings; got {tess.Vertices.Length} for {contourA.Length + contourB.Length} input vertices. " +
                "If this fails, the test no longer exercises CombineCallback.");

            foreach (var v in tess.Vertices)
            {
                Assert.True(v.Data is CsgVertexData,
                    $"Synthetic and input vertices must all carry CsgVertexData; got {v.Data?.GetType().FullName ?? "null"} at ({v.Position.X},{v.Position.Y},{v.Position.Z}).");
            }
        }

        [Fact]
        public void PackTessellations_TagCollisionAtDifferentPositions_DoesNotWeldUnrelatedCorners()
        {
            // Two separate contours sharing identical Data tag values but located at
            // different positions. Without the position-matched reuse guard, the pack
            // dedup map would weld them by key alone, collapsing distinct corners
            // into one vertex index and producing degenerate triangles.
            var contourA = new[]
            {
                MakeRawVertex(0, 0, 0, faceId: 5, solidId: 0, tag: 100),
                MakeRawVertex(1, 0, 0, faceId: 5, solidId: 0, tag: 101),
                MakeRawVertex(1, 1, 0, faceId: 5, solidId: 0, tag: 102),
                MakeRawVertex(0, 1, 0, faceId: 5, solidId: 0, tag: 103),
            };
            var contourB = new[]
            {
                MakeRawVertex(10, 10, 0, faceId: 5, solidId: 0, tag: 100), // same key as A's first vertex, different position
                MakeRawVertex(11, 10, 0, faceId: 5, solidId: 0, tag: 101),
                MakeRawVertex(11, 11, 0, faceId: 5, solidId: 0, tag: 102),
                MakeRawVertex(10, 11, 0, faceId: 5, solidId: 0, tag: 103),
            };

            var provider = new InlineTessTargetProvider(new MultiContourTessAdapter(new[] { contourA, contourB }));
            var buffer = Tessellation.Tessellate<MockGraphicsBuffer>(new[] { provider });

            // Without position-matched reuse: contourB's 4 vertices weld to contourA's,
            // collapsing 8 distinct corners to 4 and producing zero-area triangles.
            // With the guard: contourB allocates fresh synthetic tags, all 8 corners survive.
            var distinctPositions = buffer.Vertices.Select(v => v.position).Distinct().Count();
            Assert.True(distinctPositions >= 8,
                $"Expected 8 distinct vertex positions across two non-overlapping contours; got {distinctPositions}. " +
                "Vertex welding across unrelated keys would collapse them.");
        }

        private static ContourVertex MakeRawVertex(double x, double y, double z, uint faceId, uint solidId, uint tag)
        {
            return new ContourVertex
            {
                Position = new Vec3 { X = x, Y = y, Z = z },
                Data = new CsgVertexData(new UV(x, y), tag, faceId, solidId)
            };
        }

        private class MultiContourTessAdapter : ITessAdapter
        {
            private readonly ContourVertex[][] _contours;
            public MultiContourTessAdapter(ContourVertex[][] contours) { _contours = contours; }
            public Tess GetTess()
            {
                var tess = new Tess { NoEmptyPolygons = true };
                foreach (var c in _contours) { tess.AddContour(c); }
                tess.Tessellate(WindingRule.Positive, ElementType.Polygons, 3, CombineCallbacks.DataCombine);
                return tess;
            }
        }

        private class InlineTessTargetProvider : ITessellationTargetProvider
        {
            private readonly ITessAdapter _adapter;
            public InlineTessTargetProvider(ITessAdapter adapter) { _adapter = adapter; }
            public IEnumerable<ITessAdapter> GetTessellationTargets() { yield return _adapter; }
        }

        /// <summary>
        /// Builds a Tess from a self-intersecting contour without registering a
        /// CombineCallback, so LibTess will produce synthetic vertices with Data == null.
        /// </summary>
        private class SelfIntersectingNoCombineTessAdapter : ITessAdapter
        {
            public Tess GetTess()
            {
                var tess = new Tess { NoEmptyPolygons = true };
                // Bow-tie / self-crossing quad whose two diagonals force LibTess to
                // synthesize a vertex at the crossing point.
                var contour = new[]
                {
                    MakeVertex(0, 0, faceId: 1, solidId: 0, tag: 1),
                    MakeVertex(10, 10, faceId: 1, solidId: 0, tag: 2),
                    MakeVertex(10, 0, faceId: 1, solidId: 0, tag: 3),
                    MakeVertex(0, 10, faceId: 1, solidId: 0, tag: 4),
                };
                tess.AddContour(contour);
                tess.Tessellate(WindingRule.Positive, ElementType.Polygons, 3);
                return tess;
            }

            private static ContourVertex MakeVertex(double x, double y, uint faceId, uint solidId, uint tag)
            {
                return new ContourVertex
                {
                    Position = new Vec3 { X = x, Y = y, Z = 0 },
                    Data = new CsgVertexData(new UV(x, y), tag, faceId, solidId)
                };
            }
        }

        private class MockGraphicsBuffer : IGraphicsBuffers
        {
            public List<ushort> Indices { get; set; } = new List<ushort>();

            public List<(Vector3 position, Vector3 normal, UV uv, Color? color)> Vertices { get; set; } = new List<(Vector3 position, Vector3 normal, UV uv, Color? color)>();

            public void AddIndex(ushort index)
            {
                Indices.Add(index);
            }

            public void AddIndices(IList<ushort> indices)
            {
                Indices.AddRange(indices);
            }

            public void AddVertex(Vector3 position, Vector3 normal, UV uv, Color? color = null)
            {
                Vertices.Add((position, normal, uv, color));
            }

            public void AddVertex(double x, double y, double z, double nx, double ny, double nz, double u, double v, Color? color = null)
            {
                Vertices.Add((new Vector3(x, y, z), new Vector3(nx, ny, nz), new UV(u, v), color));
            }

            public void AddVertices(IList<(Vector3 position, Vector3 normal, UV uv, Color? color)> vertices)
            {
                Vertices.AddRange(vertices);
            }

            public void Initialize(int vertexCount, int indexCount)
            {
                Indices = new List<ushort>();
                Vertices = new List<(Vector3 position, Vector3 normal, UV uv, Color? color)>();
            }
        }
    }
}