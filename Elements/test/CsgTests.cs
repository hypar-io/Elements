using Elements.Geometry;
using Elements.Geometry.Profiles;
using Elements.Geometry.Solids;
using Xunit;

namespace Elements.Tests
{
    public class CsgTests : ModelTest
    {
        [Fact]
        public void Union()
        {
            this.Name = "CSG_Union";
            var s1 = new Extrude(Polygon.Rectangle(1, 1), 1, Vector3.ZAxis, false);
            var csg = new CSG(s1.Solid);

            var s2 = new Extrude(Polygon.L(1.0, 2.0, 0.5), 1, Vector3.ZAxis, false);
            csg.Union(s2.Solid);

            var result = new Mesh();
            csg.Tessellate(ref result);

            var me = new MeshElement(result);
            this.Model.AddElement(me);
        }

        [Fact]
        public void Difference()
        {
            this.Name = "CSG_Difference";
            // var profile = WideFlangeProfileServer.Instance.GetProfileByType(WideFlangeProfileType.W10x100);
            var profile = HSSPipeProfileServer.Instance.GetProfileByType(HSSPipeProfileType.HSS10_000x0_188);

            var path = new Arc(Vector3.Origin, 5, 0, 270);
            var s1 = new Sweep(profile, path, 0, 0, true);
            var csg = new CSG(s1.Solid);

            var s2 = new Extrude(new Circle(Vector3.Origin, 6).ToPolygon(20), 1, Vector3.ZAxis, false);
            csg.Difference(s2.Solid);

            for (var i = 0.0; i < 1.0; i += 0.05)
            {
                var pt = path.PointAt(i);
                var hole = new Extrude(new Circle(Vector3.Origin, 0.05).ToPolygon(), 3, Vector3.ZAxis, false);
                csg.Difference(hole.Solid, new Transform(pt + new Vector3(0, 0, -2)));
            }

            var result = new Mesh();
            csg.Tessellate(ref result);

            var me2 = new MeshElement(result);
            this.Model.AddElement(me2);
        }

        [Fact]
        public void Merge()
        {
            var panel = new Lamina(Polygon.Rectangle(1, 1), false);
            var result = new Mesh();
            panel.Solid.Tessellate(ref result);
            Assert.Equal(4, result.Triangles.Count);
            Assert.Equal(8, result.Vertices.Count);

            var box = new Extrude(Polygon.Rectangle(1, 1), 1, Vector3.ZAxis, false);
            var boxResult = new Mesh();
            box.Solid.Tessellate(ref boxResult);
            Assert.Equal(24, boxResult.Vertices.Count);
            Assert.Equal(12, boxResult.Triangles.Count);

            var pipe = new Extrude(new Profile(new Circle(1.0).ToPolygon(10), new Circle(0.5).ToPolygon(10)).Reversed(), 1.0, Vector3.ZAxis, false);
            var pipeResult = new Mesh();
            pipe.Solid.Tessellate(ref pipeResult);
            Assert.Equal(40, pipeResult.Vertices.Count);
        }
    }
}