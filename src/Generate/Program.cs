using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using NJsonSchema;
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

            // By passing a set of excluded types, we can tell the code 
            // generator not to generate inline type definitions if types
            // reference those types.
            var excludedTypes = new string[]{"Vector3", "Line", "Plane", "Polyline", 
                    "Polygon", "Profile", "Curve", "Solid", 
                    "Color", "Property", "Transform", "Element", "Material",
                    "Position", "Elements", "GeometricElement", "Geometry",
                    "Identifiable", "SolidOperation", "Representation"};

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
                ArrayType = "IList",
                ArrayInstanceType = "List",
                ExcludedTypeNames = excludedTypes == null ? new string[]{} : excludedTypes,
                TemplateDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "../../../Templates"),
                GenerateJsonMethods = false,
                ClassStyle = CSharpClassStyle.Poco
            });
        
            var file = generator.GenerateFile();
            File.WriteAllText(outPath, file);
        }
    }
}
