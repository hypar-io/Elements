using Elements.Geometry;
using Xunit;

namespace Elements.Tests
{
    public class LightTests : ModelTest
    {
        [Fact, Trait("Category", "Examples")]
        public void DirectionLight()
        {
            this.Name = "Elements_DirectionalLight";

            // <example>
            // Create a directional light.
            var origin = new Vector3(10, 10, 10);
            var light = new DirectionalLight(Colors.White,
                                             new Transform(origin, origin.Unitized()), 1.0);
            var sunMaterial = new Material("Sun", Colors.Yellow, unlit: true);

            // Create a model curve to visualize the light direction.
            var dirCurve = new ModelCurve(new Line(light.Transform.Origin, light.Transform.Origin + light.Transform.ZAxis.Negate() * 10), sunMaterial);
            var floor = new Floor(Polygon.Rectangle(20, 20), 0.1);
            var column = new Column(new Vector3(5, 5), 5.0, Polygon.Rectangle(0.2, 0.2));
            var mass = new Mass(Polygon.Rectangle(1, 1), 1.0, sunMaterial, new Transform(light.Transform.Origin));
            // </example>

            this.Model.AddElements(light, dirCurve, floor, column, mass);
        }
    }
}