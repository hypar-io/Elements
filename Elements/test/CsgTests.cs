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
            var s2 = new Extrude(Polygon.L(1.0, 2.0, 0.5), 1, Vector3.ZAxis, false);
            var result = s1.Solid.Union(s2.Solid);
            var me = new MeshElement(result);
            this.Model.AddElement(me);

        }

        [Fact]
        public void Difference()
        {
            this.Name = "CSG_Difference";
            var profile = WideFlangeProfileServer.Instance.GetProfileByType(WideFlangeProfileType.W10x100);
            var path = new Arc(Vector3.Origin, 5, 0, 90);
            var s1 = new Sweep(profile, path, 0, 0, true);

            // var result1 = new Mesh();
            // s1.Solid.Tessellate(ref result1);
            // result1.ComputeNormals();

            // var me1 = new MeshElement(result1, transform: new Transform());
            // this.Model.AddElement(me1);

            var s2 = new Extrude(new Circle(Vector3.Origin, 6).ToPolygon(20), 1, Vector3.ZAxis, false);
            var result1 = s1.Solid.Difference(s2.Solid);

            // var result2 = new Mesh();
            // s2.Solid.Tessellate(ref result2);
            // result2.ComputeNormals();

            var me2 = new MeshElement(result1);
            this.Model.AddElement(me2);
        }
    }
}