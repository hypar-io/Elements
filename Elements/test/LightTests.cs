using Elements.Geometry;
using Xunit;

namespace Elements.Tests
{
    public class LightTests : ModelTest
    {
        [Fact]
        public void DirectionLightConstruction()
        {
            this.Name = "DirectionalLight";
            var origin = new Vector3(10, 10, 10);
            var light = new DirectionalLight(Colors.White,
                                             new Transform(origin, origin.Unitized()), 1.0);
            this.Model.AddElement(light);

            var sunMaterial = new Material("Sun", Colors.Yellow, unlit: true);
            var dirCurve = new ModelCurve(new Line(light.Transform.Origin, light.Transform.Origin + light.Transform.ZAxis.Negate() * 10), sunMaterial);
            this.Model.AddElement(dirCurve);

            var floor = new Floor(Polygon.Rectangle(20, 20), 0.1);
            this.Model.AddElement(floor);

            var column = new Column(new Vector3(5, 5), 5.0, Polygon.Rectangle(0.2, 0.2));
            this.Model.AddElement(column);

            var mass = new Mass(Polygon.Rectangle(1, 1), 1.0, sunMaterial, new Transform(light.Transform.Origin));
            this.Model.AddElement(mass);
        }
    }
}