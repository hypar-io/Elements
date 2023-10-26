using Elements;
using Elements.Geometry;
using Elements.Tests;
using Xunit;
using Xunit.Abstractions;

namespace Elements.Geometry.Tests
{
    public class EllipticalArcTests : ModelTest
    {
        ITestOutputHelper _output;

        public EllipticalArcTests(ITestOutputHelper output)
        {
            this._output = output;
            this.GenerateIfc = false;
        }

        [Fact]
        public void EllipticalArcTransforms()
        {
            this.Name = nameof(EllipticalArcTransforms);
            var ellipticalArc = new EllipticalArc(Vector3.Origin, 2.5, 1.5, 0, 210);
            var parameters = ellipticalArc.GetSubdivisionParameters();
            foreach (var p in parameters)
            {
                var t = ellipticalArc.TransformAt(p);
                this.Model.AddElements(t.ToModelCurves());
            }
            this.Model.AddElement(new ModelCurve(ellipticalArc, BuiltInMaterials.ZAxis));
        }

        [Fact]
        public void ToPolyline()
        {
            var arc = new EllipticalArc(Vector3.Origin, 1, 2, 10, 20);
            var p = arc.ToPolyline(10);
            Assert.Equal(10, p.Segments().Length);
            Assert.Equal(arc.Start, p.Vertices[0]);
            Assert.Equal(arc.End, p.Vertices[p.Vertices.Count - 1]);
        }
    }
}