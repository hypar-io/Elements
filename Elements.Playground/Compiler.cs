using System;
using System.Collections;
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
using Elements.Geometry;
using Elements.Geometry.Profiles;

namespace Elements.Playground
{
    public abstract class InputBase
    {
        public string Name { get; set; }
    }

    public class Input<T> : InputBase
    {
        public T Value { get; set; }

        public Input(string name, T value)
        {
            Name = name;
            Value = value;
        }
    }

    public class WideFlangeProfileInput : Input<WideFlangeProfile>
    {
        public WideFlangeProfileFactory Factory { get; } = new WideFlangeProfileFactory();

        public WideFlangeProfileInput(string name, WideFlangeProfile value) : base(name, value) { }
    }

    public class Globals
    {
        public Inputs Inputs { get; set; } = new Inputs();
    }

    public class Inputs
    {
        public List<InputBase> Values { get; set; } = new List<InputBase>();

        public double GetNumberInput(string name)
        {
            var input = Values.FirstOrDefault(i => i.Name == name);
            if (input is Input<double> numberInput)
            {
                return numberInput.Value;
            }
            return 0.0;
        }

        public Material GetMaterialInput(string name)
        {
            var input = Values.FirstOrDefault(i => i.Name == name);
            if (input is Input<Material> materialInput)
            {
                return materialInput.Value;
            }
            return null;
        }

        public Vector3 GetVectorInput(string name)
        {
            var input = Values.FirstOrDefault(i => i.Name == name);
            if (input is Input<Vector3> vectorInput)
            {
                return vectorInput.Value;
            }
            return default;
        }

        public WideFlangeProfile GetProfileInput(string name)
        {
            var input = Values.FirstOrDefault(i => i.Name == name);
            if (input is WideFlangeProfileInput wideFlangeProfileInput)
            {
                return wideFlangeProfileInput.Value;
            }
            return default;
        }
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

        public static async Task InitializeMetadataReferences(HttpClient client)
        {
            // async Task InitializeInternal()
            // {
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
"Elements.Geometry.Solids",
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