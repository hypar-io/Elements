using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Elements.Playground
{
    public class Globals
    {
        public string InputJson { get; set; }
    }

    public static class Compiler
    {
        class BlazorBoot
        {
            public bool cacheBootResources { get; set; }
            public object[] config { get; set; }
            public bool debugBuild { get; set; }
            public string entryAssembly { get; set; }
            public bool linkerEnabled { get; set; }
            public Resources resources { get; set; }
        }

        class Resources
        {
            public Dictionary<string, string> assembly { get; set; }
            public Dictionary<string, string> pdb { get; set; }
            public Dictionary<string, string> runtime { get; set; }
        }
        private static List<MetadataReference> References;

        private static bool _isInitialized = false;

        public static bool IsReady()
        {
            return _isInitialized;
        }

        public static async Task InitializeMetadataReferences(HttpClient client)
        {
            if (_isInitialized)
            {
                return;
            }
            var model = new Model();
            Console.WriteLine("Loading metadata references...");
            // TODO: Make this conditional on some build flag, so we can easily
            // deploy to prod or dev, or allow others cloning the project to
            // spec the URL where they'll be hosting it.
            var rootUrl = "https://elements.hypar.io/";
            // TODO: This loads every assembly that is available. We should
            // see if we can limit this to just the ones that we need.
            var response = await client.GetFromJsonAsync<BlazorBoot>($"{rootUrl}blazor.boot.json");
            var assemblies = await Task.WhenAll(response.resources.assembly.Keys.Select(x => client.GetAsync($"{rootUrl}{x}")));
            var references = new List<MetadataReference>(assemblies.Length);
            foreach (var asm in assemblies)
            {
                using var task = await asm.Content.ReadAsStreamAsync();
                references.Add(MetadataReference.CreateFromStream(task));
            }
            References = references;
            _isInitialized = true;
        }

        public static (bool success, Assembly asm, Compilation compilation) LoadSource(string source)
        {
            // Use ConcurrentBuild to avoid the issue in
            // TODO: https://github.com/dotnet/runtime/issues/43411
            var compilation = CSharpCompilation.CreateScriptCompilation(
        Path.GetRandomFileName(),
        CSharpSyntaxTree.ParseText(source,
        CSharpParseOptions.Default.WithKind(SourceCodeKind.Script).WithLanguageVersion(LanguageVersion.Preview)),
        References,
        new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, usings: new[]
        {
"System",
"System.Collections.Generic",
"System.Console",
"System.Linq",
"System.Text.Json",
"System.Text.Json.Serialization",
"Elements",
"Elements.Geometry",
"Elements.Geometry.Solids",
"Elements.Spatial",
"Elements.Geometry.Profiles",
"Elements.Validators"
        }, concurrentBuild: false), globalsType: typeof(Globals));

            ImmutableArray<Diagnostic> diagnostics = compilation.GetDiagnostics();

            bool error = false;
            foreach (Diagnostic diag in diagnostics)
            {
                switch (diag.Severity)
                {
                    case DiagnosticSeverity.Info:
                        Console.WriteLine(diag.ToString());
                        break;
                    case DiagnosticSeverity.Warning:
                        Console.WriteLine(diag.ToString());
                        break;
                    case DiagnosticSeverity.Error:
                        error = true;
                        Console.WriteLine(diag.ToString());
                        break;
                }
            }

            if (error)
            {
                return (false, null, null);
            }

            Assembly assembly;
            using (var outputAssembly = new MemoryStream())
            {
                var result = compilation.Emit(outputAssembly);
                if (!result.Success)
                {
                    foreach (var resultD in result.Diagnostics)
                    {
                        Console.WriteLine(resultD.ToString());
                    }
                }
                assembly = Assembly.Load(outputAssembly.ToArray());
            }

            return (true, assembly, compilation);
        }
    }
}