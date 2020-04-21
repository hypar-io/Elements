using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Elements.Generate;
using Elements.Geometry;
using Elements.Geometry.Solids;
using Elements.Properties;
using Xunit;
using Xunit.Abstractions;

namespace Elements.Tests
{
    public sealed class IgnoreOnTravisFact : FactAttribute
    {
        public IgnoreOnTravisFact()
        {
            if (IsTravis())
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
            if (IsMac())
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
        const string beamSchema = @"{
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

        /// <summary>
        /// The embeddedSchemaTest demonstrates a schema that has another 
        /// schema that will be read at the same time specified for one of
        /// its properties 
        /// </summary>
        const string embeddedSchemaTest = @"{
    ""$id"": ""https://hypar.io/Schemas/Truss.json"",
    ""$schema"": ""http://json-schema.org/draft-07/schema#"",
    ""description"": ""A test of schema embedding."",
    ""title"": ""EmbeddedSchemaTest"",
    ""x-namespace"": ""Elements"",
    ""type"": [""object"", ""null""],
    ""allOf"": [{""$ref"": ""https://hypar.io/Schemas/GeometricElement.json""}],
    ""required"": [""Panels""],
    ""properties"": {
        ""Panels"": {
            ""type"": ""array"",
            ""items"": {
                ""$ref"": ""https://raw.githubusercontent.com/hypar-io/Schemas/master/FacadePanel.json""
            }
        }
    },
    ""additionalProperties"": false
}";
        private ITestOutputHelper output;

        public TypeGeneratorTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        // We've started ignoring these tests on mac because on 
        // Catalina we receive an System.Net.Http.CurlException : Login denied.
        [MultiFact(typeof(IgnoreOnMacFact), typeof(IgnoreOnTravisFact))]
        public async Task GeneratesCodeFromSchema()
        {
            var tmpPath = Path.GetTempPath();
            var schemaPath = Path.Combine(tmpPath, "beam.json");
            File.WriteAllText(schemaPath, beamSchema);
            var relPath = Path.GetRelativePath(Assembly.GetExecutingAssembly().Location, schemaPath);
            await TypeGenerator.GenerateUserElementTypeFromUriAsync(relPath, tmpPath, true);
            var code = File.ReadAllText(Path.Combine(tmpPath, "beam.g.cs"));
        }

        [MultiFact(typeof(IgnoreOnMacFact), typeof(IgnoreOnTravisFact))]
        public async Task GeneratesInMemoryAssembly()
        {
            var uris = new[]{"https://raw.githubusercontent.com/hypar-io/Schemas/master/FacadeAnchor.json",
                                "https://raw.githubusercontent.com/hypar-io/Schemas/master/Mullion.json"};
            var asm = await TypeGenerator.GenerateInMemoryAssemblyFromUrisAndLoadAsync(uris);
            var mullionType = asm.GetType("Test.Foo.Bar.Mullion");
            var anchorType = asm.GetType("Test.Foo.Bar.FacadeAnchor");
            Assert.NotNull(mullionType);
            Assert.NotNull(anchorType);
            Assert.NotNull(mullionType.GetProperty("CenterLine"));
            Assert.NotNull(mullionType.GetProperty("Profile"));
            Assert.NotNull(anchorType.GetProperty("Location"));

            var ctors = mullionType.GetConstructors();
            Assert.Single<ConstructorInfo>(ctors);
            var centerLine = new Line(new Vector3(0, 0), new Vector3(5, 5));
            var profile = new Profile(Polygon.Rectangle(0.1, 0.1));
            // Profile @profile, Line @centerLine, NumericProperty @length, Transform @transform, Material @material, Representation @representation, System.Guid @id, string @name
            var t = new Transform();
            var m = BuiltInMaterials.Steel;
            var mullion = Activator.CreateInstance(mullionType, new object[] { profile, centerLine, new NumericProperty(0, NumericPropertyUnitType.Length), t, m, new Representation(new List<SolidOperation>()), Guid.NewGuid(), "Test Mullion" });
        }

        [IgnoreOnTravisFact]
        public async Task ThrowsWithBadSchema()
        {
            var uris = new[]{"https://raw.githubusercontent.com/hypar-io/Schemas/master/ThisDoesn'tExist.json",
                                "https://raw.githubusercontent.com/hypar-io/Schemas/master/Mullion.json"};
            await Assert.ThrowsAsync<Exception>(async () => await TypeGenerator.GenerateInMemoryAssemblyFromUrisAndLoadAsync(uris));
        }

        [Fact]
        public async Task CodeGenerationOfTypeWithEmbeddedType()
        {
            var tmpPath = Path.Combine(Path.GetTempPath(), "HyparModels");
            if (!Directory.Exists(tmpPath))
            {
                Directory.CreateDirectory(tmpPath);
            }

            var embeddedSchemaTestPath = Path.Combine(tmpPath, "embeddedSchemaTest.json");
            var relEmbeddedSchemaTestPath = Path.GetRelativePath(Assembly.GetExecutingAssembly().Location, embeddedSchemaTestPath);
            File.WriteAllText(embeddedSchemaTestPath, embeddedSchemaTest);

            // Generate the truss type which contains the beam type
            await TypeGenerator.GenerateUserElementTypesFromUrisAsync(new[] { relEmbeddedSchemaTestPath, "https://raw.githubusercontent.com/hypar-io/Schemas/master/FacadePanel.json" }, tmpPath, true);

            // Ensure that there is only one beam.g.cs in the output.
            Assert.Equal(2, Directory.GetFiles(tmpPath, "*.g.cs").Length);
        }
    }
}