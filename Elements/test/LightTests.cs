using System;
using Elements.Geometry;
using Xunit;

namespace Elements.Tests
{
    public class LightTests : ModelTest
    {
        [Fact, Trait("Category", "Examples")]
        public void DirectionalLight()
        {
            this.Name = "Elements_DirectionalLight";

            // <directional_example>
            // Create a directional light.
            var origin = new Vector3(10, 10, 10);
            var light = new DirectionalLight(Colors.White,
                                             new Transform(origin, origin.Unitized()), 1.0);
            var sunMaterial = new Material("Sun", Colors.Yellow, unlit: true);

            // Create a model curve to visualize the light direction.
            var dirCurve = new ModelCurve(new Line(light.Transform.Origin, light.Transform.Origin + light.Transform.ZAxis.Negate() * 10), sunMaterial);
            var floor = new Floor(Polygon.Rectangle(20, 20), 0.1);
            var column = new Column(new Vector3(5, 5), 5.0, null, Polygon.Rectangle(0.2, 0.2));
            var mass = new Mass(Polygon.Rectangle(1, 1), 1.0, sunMaterial, new Transform(light.Transform.Origin));
            // </directional_example>

            this.Model.AddElements(light, dirCurve, floor, column, mass);
        }

        [Fact, Trait("Category", "Examples")]
        public void PointLight()
        {
            this.Name = "Elements_PointLight";

            // <point_example>
            var lightMaterial = new Material("Light", Colors.White, unlit: true);
            var t = new Transform(0, 0, 5);
            var lightBulb = new Mass(Polygon.Rectangle(0.1, 0.1), 0.1, lightMaterial, transform: t);

            var floor = new Floor(Polygon.Rectangle(20, 20), 0.1);
            var pointLight = new PointLight(Colors.White, t, 20);
            // </point_example>
            this.Model.AddElements(lightBulb, pointLight, floor);
        }

        [Fact, Trait("Category", "Examples")]
        public void SpotLight()
        {
            this.Name = "Elements_SpotLight";

            // <spot_example>
            // Visualize the light with a small, constant-colored "light bulb".
            var lightMaterial = new Material("Light", Colors.White, unlit: true);
            var t = new Transform(0, 0, 5);
            var lightBulb = new Mass(Polygon.Rectangle(0.1, 0.1), 0.1, lightMaterial, transform: t);

            var floor = new Floor(Polygon.Rectangle(20, 20), 0.1);
            var spotLight = new SpotLight(Colors.White, t, 20, 0, 1.0);
            // </spot_example>
            this.Model.AddElements(lightBulb, spotLight, floor);
        }

        [Fact]
        public void SpotLightInnerConeAngleLessThanOuterConeAngle()
        {
            Assert.Throws<ArgumentException>(() => new SpotLight(Colors.White, new Transform(), innerConeAngle: 1.0, outerConeAngle: 0.5));
        }

        [Fact]
        public void SpotLightOuterConeAngleBetweenOneAndHalfPi()
        {
            Assert.Throws<ArgumentException>(() => new SpotLight(Colors.White, new Transform(), outerConeAngle: 1.6));
            Assert.Throws<ArgumentException>(() => new SpotLight(Colors.White, new Transform(), outerConeAngle: -1));
        }

        [Fact]
        public void CanCreateModelWithOnlyLights()
        {
            // Ensure that a model containing only lights, without any geometry,
            // can be created and written to glTF.
            this.Name = "Lights";
            for (var x = 0; x < 20; x += 3)
            {
                for (var y = 0; y < 20; y += 3)
                {
                    var spotLight = new SpotLight(Colors.White, new Transform(x, y, 3), 10, 0, 0.4);
                    this.Model.AddElement(spotLight);
                }
            }
        }
    }
}