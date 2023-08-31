using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Elements.Generate;
using Elements.Geometry;
using Elements.Geometry.Solids;
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
        /// <summary>
        /// The embeddedSchemaTest2 demonstrates a schema that references the same
        /// schema that will be referenced in embeddedSchemaTest above at the same time.
        /// </summary>
        const string embeddedSchemaTest2 = @"{
    ""$id"": ""https://hypar.io/Schemas/Truss.json"",
    ""$schema"": ""http://json-schema.org/draft-07/schema#"",
    ""description"": ""A test of schema embedding."",
    ""title"": ""EmbeddedSchemaTestSecond"",
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

        [MultiFact(typeof(IgnoreOnTravisFact))]
        public async Task GeneratesCodeFromSchema()
        {
            var tmpPath = Path.GetTempPath();
            var schemaPath = Path.Combine(tmpPath, "beam.json");
            File.WriteAllText(schemaPath, beamSchema);
            var relPath = Path.GetRelativePath(Assembly.GetExecutingAssembly().Location, schemaPath);
            await TypeGenerator.GenerateUserElementTypeFromUriAsync(relPath, tmpPath);
            var code = File.ReadAllText(Path.Combine(tmpPath, "beam.g.cs"));
        }

        [MultiFact(typeof(IgnoreOnTravisFact))]
        public async Task GeneratesInMemoryAssembly()
        {
            var uris = new[]{"https://raw.githubusercontent.com/hypar-io/Schemas/master/FacadeAnchor.json",
                                "https://raw.githubusercontent.com/hypar-io/Schemas/master/Mullion.json"};
            var asm = await TypeGenerator.GenerateInMemoryAssemblyFromUrisAndLoadAsync(uris);
            var mullionType = asm.Assembly.GetType("Test.Foo.Bar.Mullion");
            var anchorType = asm.Assembly.GetType("Test.Foo.Bar.FacadeAnchor");
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
            var mullion = Activator.CreateInstance(mullionType, new object[] { profile, centerLine, null, t, m, new Representation(new List<SolidOperation>()), false, Guid.NewGuid(), "Test Mullion" });
        }

        [IgnoreOnTravisFact]
        public async Task FailsWithBadSchema()
        {
            var uris = new[]{"https://raw.githubusercontent.com/hypar-io/Schemas/master/ThisDoesntExist.json",
                                "https://raw.githubusercontent.com/hypar-io/Schemas/master/Mullion.json"};
            var result = await TypeGenerator.GenerateInMemoryAssemblyFromUrisAndLoadAsync(uris);
            Assert.False(result.Success);
            Assert.NotEmpty(result.DiagnosticResults);
        }

        [Fact]
        public async Task CodeGenerationOfTypeWithEmbeddedType()
        {
            var tmpPath = Path.Combine(Path.GetTempPath(), "HyparModels");
            string relEmbeddedSchemaTestPath = RelativeSavedSchemaPath(embeddedSchemaTest, tmpPath, "embeddedSchemaTest.json");
            string relEmbeddedSchemaTestPath2 = RelativeSavedSchemaPath(embeddedSchemaTest2, tmpPath, "embeddedSchemaTest2.json");

            // Generate the truss type which contains the beam type.
            await TypeGenerator.GenerateUserElementTypesFromUrisAsync(new[] { relEmbeddedSchemaTestPath, relEmbeddedSchemaTestPath2, "https://raw.githubusercontent.com/hypar-io/Schemas/master/FacadePanel.json" }, tmpPath);
            // Ensure that there is only one beam.g.cs in the output.
            Assert.Equal(3, Directory.GetFiles(tmpPath, "*.g.cs").Length);
        }

        [Fact]
        public async Task CodeGenerationOfNullableVectorAndColorProperties()
        {
            var schemaPath = "../../TestData/NullableAndNonNullableTypesSchema.json";

            var tmpPath = Path.GetTempPath();            
            var relPath = Path.GetRelativePath(Assembly.GetExecutingAssembly().Location, schemaPath);
            await TypeGenerator.GenerateUserElementTypeFromUriAsync(relPath, tmpPath);
            var code = File.ReadAllText(Path.Combine(tmpPath, "NullableAndNonNullableTypes.g.cs"));
            Assert.Contains("public Vector3 NonNullableVector", code);
            Assert.Contains("public Vector3? NullableVector", code);
            Assert.Contains("public Color NonNullableColor", code);
            Assert.Contains("public Color? NullableColor", code);
        }

        [Fact]
        public async Task InMemoryCodeGenOfTypeWithEmbeddedType()
        {
            var tmpPath = Path.Combine(Path.GetTempPath(), "HyparModels");
            string relEmbeddedSchemaTestPath = RelativeSavedSchemaPath(embeddedSchemaTest, tmpPath, "embeddedSchemaTest.json");
            string relEmbeddedSchemaTestPath2 = RelativeSavedSchemaPath(embeddedSchemaTest2, tmpPath, "embeddedSchemaTest2.json");

            // Load schemas in memeory.
            var asm = await TypeGenerator.GenerateInMemoryAssemblyFromUrisAndLoadAsync(new[] { relEmbeddedSchemaTestPath, relEmbeddedSchemaTestPath2 });
            Assert.True(asm.Success);
            var facadeType = asm.Assembly.GetType("Elements.FacadePanel");
            Assert.NotNull(facadeType);
        }

        [Fact]
        public async Task SaveDllCodeGenOfTypeWithEmbeddedType()
        {
            var tmpPath = Path.Combine(Path.GetTempPath(), "HyparModels");
            string relEmbeddedSchemaTestPath = RelativeSavedSchemaPath(embeddedSchemaTest, tmpPath, "embeddedSchemaTest.json");
            string relEmbeddedSchemaTestPath2 = RelativeSavedSchemaPath(embeddedSchemaTest2, tmpPath, "embeddedSchemaTest2.json");

            // Save dll for schemas.
            var dllPath = Path.Combine(tmpPath, "userElements.dll");
            var asm2 = await TypeGenerator.GenerateInMemoryAssemblyFromUrisAndSaveAsync(new[] { relEmbeddedSchemaTestPath, relEmbeddedSchemaTestPath2 }, dllPath);
            Assert.True(asm2.Success);
        }

        private static string RelativeSavedSchemaPath(string schema, string tmpPath, string fileName)
        {
            if (!Directory.Exists(tmpPath))
            {
                Directory.CreateDirectory(tmpPath);
            }
            var embeddedSchemaTestPath = Path.Combine(tmpPath, "embeddedSchemaTest.json");
            var relEmbeddedSchemaTestPath = Path.GetRelativePath(Assembly.GetExecutingAssembly().Location, embeddedSchemaTestPath);
            File.WriteAllText(embeddedSchemaTestPath, schema);
            return relEmbeddedSchemaTestPath;
        }
    }
}