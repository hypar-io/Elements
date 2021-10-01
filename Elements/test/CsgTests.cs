using System.Collections.Generic;
using Elements.Geometry;
using Elements.Geometry.Profiles;
using Elements.Geometry.Solids;
using System;
using Xunit;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Elements.Tests
{
    public class CsgTests : ModelTest
    {
        private HSSPipeProfileFactory _profileFactory = new HSSPipeProfileFactory();

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
            var profile = _profileFactory.GetProfileByType(HSSPipeProfileType.HSS10_000x0_188);

            var path = new Arc(Vector3.Origin, 5, 0, 270);
            var beam = new Beam(path, profile);

            var s2 = new Extrude(new Circle(Vector3.Origin, 6).ToPolygon(20), 1, Vector3.ZAxis, true);
            beam.Representation.SolidOperations.Add(s2);

            for (var i = 0.0; i < 1.0; i += 0.05)
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
        public void SubstractNotAllowedToGoForInfiniteTime()
        {
            var perimeter = new Polygon(new List<Vector3> {
                (X:6.9609, Y:-18.4036, Z:2.7000), (X:6.9609, Y:-3.2016, Z:2.7000),
                 (X:6.9609, Y:-3.2016, Z:0.0000), (X:6.9609, Y:-3.4816, Z:0.0000),
                  (X:6.9609, Y:-3.4816, Z:-1.2000), (X:6.9609, Y:-18.4036, Z:-1.2000)});
            var centerline = new Line((X: 6.9609, Y: -18.4036, Z: 0.0000), (X: 6.9609, Y: -3.2016, Z: 0.0000));

            const double halfThickness = .33528;
            var openingPerimienter = new Polygon(new List<Vector3> {
                (X:6.8599, Y:-12.2016, Z:-0.5500), (X:6.8599, Y:-6.2016, Z:-0.5500),
                 (X:6.8599, Y:-6.2016, Z:2.1500), (X:6.8599, Y:-12.2016, Z:2.1500) });
            var openings = new List<Opening> { new Opening(openingPerimienter, halfThickness, halfThickness) };


            var wallByProfile = new WallByProfile(perimeter,
                                                  null,
                                                  halfThickness * 2,
                                                  centerline,
                                                  new Transform(),
                                                  BuiltInMaterials.Concrete,
                                                  openings,
                                                  new Representation(new List<SolidOperation>()),
                                                  false,
                                                  Guid.NewGuid(),
                                                  "bad wall");

            var taskFinishedOnTime = Task.Run(() =>
            {
                wallByProfile.UpdateRepresentations();
                var solid = wallByProfile.GetFinalCsgFromSolids();
            }).Wait(2000);
            Assert.True(taskFinishedOnTime);
        }

        [Fact]
        public void TessellatorProducesCorrectVertexNormals()
        {
            Name = nameof(TessellatorProducesCorrectVertexNormals);
            var shape = new Polygon((4.96243, 50.58403), (5.78472, 50.58403), (5.78472, 65.83403), (-7.05727, 65.83403), (-7.05727, 50.57403), (4.96243, 50.57403));

            var geoElem = new GeometricElement(representation: new Extrude(shape, 1, Vector3.ZAxis, false));
            Model.AddElement(geoElem);
            var solid = geoElem.GetFinalCsgFromSolids();
            var mgb = new MockGraphicsBuffer();
            var arrows = new ModelArrows();
            CsgExtensions.Tessellate(new Csg.Solid[] { solid }, mgb);
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

        private class MockGraphicsBuffer : IGraphicsBuffers
        {
            public List<ushort> Indices { get; set; } = new List<ushort>();

            public List<(Vector3 position, Vector3 normal)> Vertices { get; set; } = new List<(Vector3 position, Vector3 normal)>();
            public void AddIndex(ushort index)
            {
                Indices.Add(index);
            }

            public void AddVertex(Vector3 position, Vector3 normal, UV uv, Color? color = null)
            {
                Vertices.Add((position, normal));
            }

            public void AddVertex(double x, double y, double z, double nx, double ny, double nz, double u, double v, Color? color = null)
            {
                Vertices.Add((new Vector3(x, y, z), new Vector3(nx, ny, nz)));
            }
        }
    }
}