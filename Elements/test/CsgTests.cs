using System.Collections.Generic;
using Elements.Geometry;
using Elements.Geometry.Profiles;
using Elements.Geometry.Solids;
using Xunit;

namespace Elements.Tests
{
    public class CsgTests : ModelTest
    {
        [Fact]
        public void Csg()
        {
            this.Name = "Elements_Geometry_Csg";
            var s1 = new Extrude(Polygon.Rectangle(Vector3.Origin, new Vector3(30, 30)), 50, Vector3.ZAxis, false);
            var csg = s1.Solid.ToCsg();

            var s2 = new Extrude(Polygon.Rectangle(30, 30), 30, Vector3.ZAxis, false);
            csg = csg.Substract(s2.Solid.ToCsg());

            var s3 = new Sweep(Polygon.Rectangle(Vector3.Origin, new Vector3(5, 5)), new Line(new Vector3(0, 0, 45), new Vector3(30, 0, 45)), 0, 0, false);
            csg = csg.Union(s3.Solid.ToCsg());

            var poly = new Polygon(new List<Vector3>(){
                new Vector3(0,0,0), new Vector3(20,50,0), new Vector3(0,50,0)
            });
            var s4 = new Sweep(poly, new Line(new Vector3(0, 30, 0), new Vector3(30, 30, 0)), 0, 0, false);
            csg = csg.Substract(s4.Solid.ToCsg());

            var result = new Mesh();
            csg.Tessellate(ref result);
            this.Model.AddElement(new MeshElement(result, new Material("Mod", Colors.Red, 0.5, 0.5)));
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
            // var profile = WideFlangeProfileServer.Instance.GetProfileByType(WideFlangeProfileType.W10x100);
            var profile = HSSPipeProfileServer.Instance.GetProfileByType(HSSPipeProfileType.HSS10_000x0_188);

            var path = new Arc(Vector3.Origin, 5, 0, 270);
            var s1 = new Sweep(profile, path, 0, 0, true);
            var csg = s1.Solid.ToCsg();

            var s2 = new Extrude(new Circle(Vector3.Origin, 6).ToPolygon(20), 1, Vector3.ZAxis, false);
            csg = csg.Substract(s2.Solid.ToCsg());

            for (var i = 0.0; i < 1.0; i += 0.05)
            {
                var pt = path.PointAt(i);
                var hole = new Extrude(new Circle(Vector3.Origin, 0.05).ToPolygon(), 3, Vector3.ZAxis, false);
                csg = csg.Substract(hole.Solid.ToCsg().Transform(new Transform(pt + new Vector3(0, 0, -2)).ToMatrix4x4()));
            }

            var result = new Mesh();
            csg.Tessellate(ref result);

            var me2 = new MeshElement(result);
            this.Model.AddElement(me2);
        }
    }
}