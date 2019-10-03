using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using NJsonSchema;
using NJsonSchema.CodeGeneration;
using NJsonSchema.CodeGeneration.CSharp;
using System.Linq;

namespace Elements.Generate
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var asmDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var schemaRoot = Path.GetFullPath(Path.Combine(asmDir, "../../../../../Schemas"));
            var outRoot = Path.GetFullPath(Path.Combine(asmDir, "../../../../Elements/Generate"));

            var elementOutPath = Path.Combine(outRoot, "Element.g.cs");
            var schemaPath = Path.Combine(schemaRoot, "Element.json");

            // By passing a set of excluded types, we can tell the code 
            // generator not to generate inline type definitions if types
            // reference those types.
            var excludedTypes = new string[]{"Vector3", "Line", "Plane", "Polyline", "Polygon", "Profile", "Component", "Curve", "Solid", "Color", "Property", "Transform"};

            var typesDict = new Dictionary<string, CodeArtifact>();
            var di = new DirectoryInfo(schemaRoot);
            foreach(var fi in di.EnumerateFiles("*.json", SearchOption.AllDirectories))
            {
                var index = fi.Directory.FullName.IndexOf("Schemas");
                var subDir = fi.Directory.FullName.Substring(index + 7).Replace(fi.Name, "").TrimStart('/');
                
                var ns = $"Elements{fi.Directory.FullName.Substring(index + 7).Replace(fi.Name, "").Replace("/",".")}";

                var outDir = Path.Combine(outRoot, subDir);
                if(!Directory.Exists(outDir))
                {
                    Directory.CreateDirectory(outDir);
                }

                var outPath = Path.Combine(outDir, fi.Name.Replace(".json",".g.cs"));
                await WriteTypes(fi.FullName, outPath, ns, excludedTypes.Where(t=>t != fi.Name.Replace(".json","")).ToArray());
            }
        }

        private static async Task WriteTypes(string schemaPath, string outPath, string ns, string[] excludedTypes = null)
        {
            Console.WriteLine($"Generating types in {outPath}...");
            var schema = await JsonSchema.FromJsonAsync(File.ReadAllText(schemaPath), schemaPath);
            var generator = new CSharpGenerator(schema, new CSharpGeneratorSettings(){
                Namespace = ns, 
                ArrayType = "System.Collections.Generic.List",
                ArrayInstanceType = "System.Collections.Generic.List",
                ExcludedTypeNames = excludedTypes == null ? new string[]{} : excludedTypes, 
                GenerateDefaultValues = false,
                GenerateDataAnnotations = false, 
                PropertySetterAccessModifier = "internal", 
                HandleReferences = true
            });
        
            var file = generator.GenerateFile();
            file = file.Insert(0, "using Elements.Geometry;\nusing Elements.Geometry.Solids;\nusing Elements.Properties;\n");
            File.WriteAllText(outPath, file);
        }
    }
}
