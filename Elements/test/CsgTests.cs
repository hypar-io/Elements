using Elements.Geometry;
using Elements.Geometry.Solids;
using Xunit;

namespace Elements.Tests
{
    public class CsgTests : ModelTest
    {
        [Fact]
        public void Csg()
        {
            this.Name = "CSG_Union";
            var s1 = new Extrude(Polygon.Rectangle(1, 1), 1, Vector3.ZAxis, false);
            var s2 = new Extrude(Polygon.L(1.0, 2.0, 0.5), 1, Vector3.ZAxis, false);
            var result = s1.Solid.Union(s2.Solid);
            var me = new MeshElement(result);
            this.Model.AddElement(me);

            var s3 = new Extrude(new Circle(Vector3.Origin, 0.25).ToPolygon(20), 1, Vector3.ZAxis, false);
            var result1 = s1.Solid.Difference(s3.Solid);
            var me1 = new MeshElement(result1, transform: new Transform(3, 0, 0));
            this.Model.AddElement(me1);
        }
    }
}