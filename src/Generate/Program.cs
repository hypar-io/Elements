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
            var excludedTypes = new string[]{"Vector3", "Line", "Plane", "Polygon", "Profile", "Component", "Curve"};
            await WriteTypes(schemaPath, elementOutPath, "Elements.Generate", null);

            var di = new DirectoryInfo(schemaRoot);
            foreach(var fi in di.EnumerateFiles("*.json", SearchOption.AllDirectories))
            {
                if(fi.Name == "Element.json")
                {
                    continue;
                }

                var ns = $"Elements.{fi.Directory.Name}";
                var outDir = Path.Combine(outRoot, fi.Directory.Name);
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
                ClassStyle = CSharpClassStyle.Poco, 
                ExcludedTypeNames = excludedTypes == null ? new string[]{} : excludedTypes, 
            });
        
            var file = generator.GenerateFile();
            file = file.Insert(0, "using Elements.Geometry;\n");
            File.WriteAllText(outPath, file);
        }
    }
}
