using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Elements.Generate;
using Elements.Geometry;
using Elements.Geometry.Solids;
using Elements.Properties;
using Xunit;

namespace Elements.Tests
{
    public sealed class IgnoreOnTravisFact : FactAttribute
    {
        public IgnoreOnTravisFact() {
            if(IsTravis()) 
            {
                Skip = "Ignore on Travis.";
            }
        }
        
        private static bool IsTravis()
        {
            return Environment.GetEnvironmentVariable("TRAVIS") != null;
        }
    }

    public sealed class IgnoreOnMacFact : FactAttribute
    {
        public IgnoreOnMacFact() 
        {
            if(IsMac())
            {
                Skip = "Ignore on mac.";
            }
        }

        private static bool IsMac()
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
        }
    }

    public class MultiFact : FactAttribute
    {
        public MultiFact(params Type[] types)
        {
            var result = types.Select(Activator.CreateInstance).Cast<FactAttribute>().ToList();

            if (result.Any(it => !string.IsNullOrEmpty(it.Skip)))
            {
                Skip = string.Join(", ", result.Where(it => !string.IsNullOrEmpty(it.Skip)).Select(it => it.Skip));
            }
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
        // We've started ignoring these tests on mac because on 
        // Catalina we receive an System.Net.Http.CurlException : Login denied.
        [MultiFact(typeof(IgnoreOnMacFact), typeof(IgnoreOnTravisFact))]
        public void GeneratesCodeFromSchema()
        {
            var tmpPath = Path.GetTempPath();
            var schemaPath = Path.Combine(tmpPath, "beam.json");
            File.WriteAllText(schemaPath, schema);
            var relPath = Path.GetRelativePath(Assembly.GetExecutingAssembly().Location, schemaPath);
            TypeGenerator.GenerateUserElementTypeFromUri(relPath, tmpPath, true);
            var code = File.ReadAllText(Path.Combine(tmpPath, "beam.g.cs"));
        }
        
        [MultiFact(typeof(IgnoreOnMacFact), typeof(IgnoreOnTravisFact))]
        public void GeneratesInMemoryAssembly()
        {
            var uris = new []{"https://raw.githubusercontent.com/hypar-io/Schemas/master/FacadeAnchor.json", 
                                "https://raw.githubusercontent.com/hypar-io/Schemas/master/Mullion.json"};
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
            var uris = new []{"https://raw.githubusercontent.com/hypar-io/Schemas/master/ThisDoesn'tExist.json", 
                                "https://raw.githubusercontent.com/hypar-io/Schemas/master/Mullion.json"};
            Assert.Throws<Exception>(()=>TypeGenerator.GenerateInMemoryAssemblyFromUrisAndLoad(uris));
        }
    }
}