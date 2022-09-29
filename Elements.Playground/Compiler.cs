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

        private static Task InitializationTask;
        private static List<MetadataReference> References;

        public static void InitializeMetadataReferences(HttpClient client)
        {
            async Task InitializeInternal()
            {
                var model = new Model();
                Console.WriteLine("Initializing the code editor...");

                // TODO: This loads every assembly that is available. We should
                // see if we can limit this to just the ones that we need.
                var response = await client.GetFromJsonAsync<BlazorBoot>("_framework/blazor.boot.json");
                var assemblies = await Task.WhenAll(response.resources.assembly.Keys.Select(x => client.GetAsync("_framework/" + x)));
                var references = new List<MetadataReference>(assemblies.Length);
                foreach (var asm in assemblies)
                {
                    using var task = await asm.Content.ReadAsStreamAsync();
                    references.Add(MetadataReference.CreateFromStream(task));
                }
                References = references;
            }
            InitializationTask = InitializeInternal();
        }

        public static Task WhenReady(Func<Task> action)
        {
            if (InitializationTask.Status != TaskStatus.RanToCompletion)
            {
                return InitializationTask.ContinueWith(x => action());
            }
            else
            {
                return action();
            }
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
"Elements",
"Elements.Geometry",
"Elements.Geometry.Profiles",
"Elements.Validators"
        }, concurrentBuild: false));

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