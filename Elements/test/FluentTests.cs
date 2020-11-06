using Elements.Geometry;
using Xunit;
using Xunit.Abstractions;

namespace Elements.Tests.FluentTests
{
    public class FluentTests
    {
        private readonly ITestOutputHelper _output;

        public FluentTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void BuildABeamWithDefaults()
        {
            var polygon = Polygon.Star(5, 3, 7);
            foreach (var c in polygon.Segments())
            {
                var pt = Build.PointAlongCurve(c)
                              .Build();

                var circle = Build.Circle()
                                  .AtOrigin(pt)
                                  .OfRadius(-1)
                                  .Build();

                Build.ModelCurve()
                     .FromCurve(circle)
                     .Build();

                Build.Beam()
                     .AlongCurve(c)
                     .WithStartSetback(5)
                     .Build();
            }

            Build.ToGltf("testModel.glb");

            foreach (var e in Build.Errors)
            {
                _output.WriteLine(e);
            }
        }
    }
}