using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using NJsonSchema;
using NJsonSchema.CodeGeneration.CSharp;
using System.Linq;
using System.Collections.Generic;

namespace Elements.Generate
{
    class ElementsTypeNameGenerator : ITypeNameGenerator
    {
        // TODO(Ian): This type name generator is only required because njsonschema
        // calls dependencies 'Json2', 'Json3', etc. We use their title to label
        // them and then they get excluded by that title. This is fragile because
        // if a user gives their schema a title that is different than its id, 
        // this will break. We need to figure out why njson schema has this bizarre
        // behavior. This behavior does not exist when the schemas are loaded
        // from disk, only when they are referenced by urls.
        public string Generate(JsonSchema schema, string typeNameHint, IEnumerable<string> reservedTypeNames)
        {
            // Console.WriteLine(typeNameHint + ":" + schema.InheritedSchema ?? "none");
            if(schema.IsEnumeration)
            {
                return typeNameHint;
            }
            else
            {
                return schema.Title;
            }
        }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            var asmDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var schemaRoot = Path.GetFullPath(Path.Combine(asmDir, "../../../../../Schemas"));
            var outRoot = Path.GetFullPath(Path.Combine(asmDir, "../../../../Elements/Generate"));

            await WriteTypesFromUrls(outRoot);
        }

        private static async Task WriteTypesFromUrls(string outRoot)
        {
            var urls = new string[]{
                "https://hypar.io/Schemas/GeoJSON/Position.json",
                "https://hypar.io/Schemas/Geometry/Solids/Extrude.json",
                "https://hypar.io/Schemas/Geometry/Solids/Lamina.json",
                "https://hypar.io/Schemas/Geometry/Solids/SolidOperation.json",
                "https://hypar.io/Schemas/Geometry/Solids/Sweep.json",
                "https://hypar.io/Schemas/Geometry/Arc.json",
                "https://hypar.io/Schemas/Geometry/Color.json",
                "https://hypar.io/Schemas/Geometry/Curve.json",
                "https://hypar.io/Schemas/Geometry/Line.json",
                "https://hypar.io/Schemas/Geometry/Plane.json",
                "https://hypar.io/Schemas/Geometry/Polygon.json",
                "https://hypar.io/Schemas/Geometry/Polyline.json",
                "https://hypar.io/Schemas/Geometry/Profile.json",
                "https://hypar.io/Schemas/Geometry/Representation.json",
                "https://hypar.io/Schemas/Geometry/Transform.json",
                "https://hypar.io/Schemas/Geometry/Vector3.json",
                "https://hypar.io/Schemas/Properties/NumericProperty.json",
                "https://hypar.io/Schemas/Element.json",
                "https://hypar.io/Schemas/GeometricElement.json",
                "https://hypar.io/Schemas/Identifiable.json",
                "https://hypar.io/Schemas/Material.json",
                "https://hypar.io/Schemas/Model.json"
            };

            var typeNames = urls.Select(u=>u.Split("/", StringSplitOptions.RemoveEmptyEntries).Last().Replace(".json", "")).ToList();

            foreach(var url in urls)
            {
                var split = url.Split("/", StringSplitOptions.RemoveEmptyEntries).Skip(3);
                var ns = "Elements";
                if(split.Count() > 1)
                {
                    ns += "." + string.Join('.', split.SkipLast(1));
                }
                var outDir = Path.Combine(outRoot, string.Join('/', split.SkipLast(1)).TrimEnd('.'));
                if(!Directory.Exists(outDir))
                {
                    Directory.CreateDirectory(outDir);
                }
                var outPath = Path.Combine(outDir, split.Last().Replace(".json",".g.cs"));

                var thisTypeName = split.Last().Replace(".json","");
                var localExcludes = typeNames.Where(n=>n != thisTypeName).ToArray();

                // Exclude all types that aren't this type so that we only 
                // get one type declaration per file.
                await WriteTypesFromSchemasOnline(url, outPath, ns, localExcludes);
            }
        }

        private static async Task WriteTypesFromDisk(string schemaRoot, string outRoot, string[] excludedTypes = null)
        {
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
                await WriteTypesFromSchemasOnDisk(fi.FullName, outPath, ns, excludedTypes.Where(t=>t != fi.Name.Replace(".json","")).ToArray());
            }
        }

        private static async Task WriteTypesFromSchemasOnline(string schemaUrl, string outPath, string ns, string[] excludedTypes = null)
        {
            Console.WriteLine($"Generating type in {outPath}...");
            var schema = await JsonSchema.FromUrlAsync(schemaUrl);
            var generator = new CSharpGenerator(schema, new CSharpGeneratorSettings(){
                Namespace = ns, 
                ArrayType = "IList",
                ArrayInstanceType = "List",
                ExcludedTypeNames = excludedTypes == null ? new string[]{} : excludedTypes,
                TemplateDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "../../../Templates"),
                GenerateJsonMethods = false,
                ClassStyle = CSharpClassStyle.Poco,
                TypeNameGenerator = new ElementsTypeNameGenerator()
            });
            var file = generator.GenerateFile();
            File.WriteAllText(outPath, file);
        }

        private static async Task WriteTypesFromSchemasOnDisk(string schemaPath, string outPath, string ns, string[] excludedTypes = null)
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
