using BenchmarkDotNet.Attributes;
using Elements.Geometry;
using Elements.Geometry.Solids;

namespace Elements.Serialization
{
    [SimpleJob]
    public class Solids
    {
        [Benchmark(Description = "Star shaped thing")]
        public void StarShapedThing()
        {
            var s1 = new Extrude(Polygon.Star(10, 7, 10), 10, Vector3.ZAxis, false);
            var s2 = new Extrude(Polygon.Star(10, 7, 10).TransformedPolygon(new Transform(new Vector3(1, 1, -1))), 8, Vector3.ZAxis, false);
            _ = Solid.Difference(s1._solid, null, s2._solid, null);
        }
    }
}