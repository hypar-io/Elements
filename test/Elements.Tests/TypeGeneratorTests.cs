using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Elements;
using Elements.Generate;
using Elements.Geometry;
using Elements.Geometry.Solids;
using Elements.Properties;
using Elements.Tests;
using Xunit;

namespace Elements.Tests
{
    public sealed class IgnoreOnTravisFact : FactAttribute
    {
        public IgnoreOnTravisFact() {
            if(IsTravis()) {
                Skip = "Ignore on Travis.";
            }
        }
        
        private static bool IsTravis()
            => Environment.GetEnvironmentVariable("TRAVIS") != null;
        }

    }

    public class TypeGeneratorTests
    {
        const string schema = @"{
    ""$id"": ""https://hypar.io/Schemas/Beam.json"",
    ""$schema"": ""http://json-schema.org/draft-07/schema#"",
    ""description"": ""A beam."",
    ""title"": ""Beam"",
    ""x-namespace"": ""Elements"",
    ""type"": [""object"", ""null""],
    ""allOf"": [{""$ref"": ""https://hypar.io/Schemas/GeometricElement.json""}],
    ""required"": [""CenterLine"", ""Profile""],
    ""properties"": {
        ""CenterLine"": {
            ""description"": ""The center line of the beam."",
            ""$ref"": ""https://hypar.io/Schemas/Geometry/Line.json""
        },
        ""Profile"": {
            ""description"": ""The beam's cross section."",
            ""$ref"": ""https://hypar.io/Schemas/Geometry/Profile.json""
        }
    },
    ""additionalProperties"": false
}";

        [IgnoreOnTravisFact]
        public void GeneratesCodeFromSchema()
        {
            var tmpPath = Path.GetTempPath();
            var schemaPath = Path.Combine(tmpPath, "beam.json");
            File.WriteAllText(schemaPath, schema);
            var relPath = Path.GetRelativePath(Assembly.GetExecutingAssembly().Location, schemaPath);
            TypeGenerator.GenerateUserElementTypeFromUri(relPath, tmpPath, true);
            var code = File.ReadAllText(Path.Combine(tmpPath, "beam.g.cs"));
        }
        
        [IgnoreOnTravisFact]
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

        [IgnoreOnTravisFact]
        public void ThrowsWithBadSchema()
        {
            var uris = new []{"https://raw.githubusercontent.com/hypar-io/UserElementSchemaTest/master/ThisDoesn'tExist.json", 
                                "https://raw.githubusercontent.com/hypar-io/UserElementSchemaTest/master/Mullion.json"};
            Assert.Throws<Exception>(()=>TypeGenerator.GenerateInMemoryAssemblyFromUrisAndLoad(uris));
        }
    }
}