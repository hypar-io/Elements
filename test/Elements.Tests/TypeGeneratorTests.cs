using System;
using System.Collections.Generic;
using Elements.Generate;
using Elements.Geometry;
using Elements.Geometry.Solids;
using Elements.Properties;
using Xunit;

namespace Elements.Tests
{
    public class TypeGeneratorTests
    {
        [Fact]
        public void GeneratesInMemoryAssembly()
        {
            var uris = new []{"https://raw.githubusercontent.com/hypar-io/UserElementSchemaTest/master/FacadeAnchor.json", 
                                "https://raw.githubusercontent.com/hypar-io/UserElementSchemaTest/master/Mullion.json"};
            var asm = TypeGenerator.GenerateInMemoryAssemblyFromUrisAndLoad(uris);
            var mullionType = asm.GetType("Test.Foo.Bar.Mullion");
            var anchorType = asm.GetType("Test.Foo.Bar.FacadeAnchor");
            Assert.NotNull(mullionType);
            Assert.NotNull(anchorType);
            Assert.NotNull(mullionType.GetProperty("CenterLine"));
            Assert.NotNull(mullionType.GetProperty("Profile"));
            Assert.NotNull(anchorType.GetProperty("Location"));

            var ctors = mullionType.GetConstructors();
            Assert.Equal(1, ctors.Length);
            var centerLine = new Line(new Vector3(0,0), new Vector3(5,5));
            var profile = new Profile(Polygon.Rectangle(0.1,0.1));
            // Profile @profile, Line @centerLine, NumericProperty @length, Transform @transform, Material @material, Representation @representation, System.Guid @id, string @name
            var t = new Transform();
            var m = BuiltInMaterials.Steel;
            var mullion = Activator.CreateInstance(mullionType, new object[]{profile, centerLine, new NumericProperty(0, NumericPropertyUnitType.Length), t, m, new Representation(new List<SolidOperation>()), Guid.NewGuid(), "Test Mullion" });
        }
    }
}