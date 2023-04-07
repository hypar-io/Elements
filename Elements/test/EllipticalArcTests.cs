using Elements;
using Elements.Geometry;
using Elements.Tests;
using Xunit;
using Xunit.Abstractions;

namespace Hypar.Tests
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
            var ellipticalArc = new EllipticalArc(Vector3.Origin, 2.5,1.5, 0, 210);
            var parameters = ellipticalArc.GetSampleParameters();
            foreach(var p in parameters)
            {
                var t = ellipticalArc.TransformAt(p);
                this.Model.AddElements(t.ToModelCurves());
            }
            this.Model.AddElement(new ModelCurve(ellipticalArc, BuiltInMaterials.ZAxis));
        }
    }
}