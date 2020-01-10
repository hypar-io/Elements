using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Newtonsoft.Json;
using NJsonSchema;
using NJsonSchema.CodeGeneration.CSharp;

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

    /// <summary>
    /// TypeGenerator contains logic for generating element types from JSON schemas.
    /// </summary>
    public static class TypeGenerator
    {
        /// <summary>
        /// These are all the 'base' schemas defined for Elements.
        /// </summary>
        private static readonly string[] _hyparSchemas = new string[]{
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
                "https://hypar.io/Schemas/GeometricElement.json",
                "https://hypar.io/Schemas/Element.json",
                "https://hypar.io/Schemas/Material.json",
                "https://hypar.io/Schemas/Model.json",
                "https://hypar.io/Schemas/Geometry/Matrix.json",
            };

        private const string NAMESPACE_PROPERTY = "x-namespace";
        private static string[] _coreTypeNames;
        
        /// <summary>
        /// Generate a user-defined type in a .cs file from a schema.
        /// </summary>
        /// <param name="uri">The uri to the schema which defines the type. This can be a url or a relative file path.</param>
        /// <param name="outputBaseDir">The base output directory.</param>
        /// <param name="isUserElement">Is the type a user-defined element?</param>
        public static void GenerateUserElementTypeFromUri(string uri, string outputBaseDir, bool isUserElement = false)
        {
            var schema = GetSchema(uri);

            string ns;
            if(!GetNamespace(schema, out ns))
            {
                return;
            }

            var typeName = schema.Title;
            var filePath = Path.Combine(outputBaseDir, GetFileNameFromTypeName(typeName));
            if(_coreTypeNames == null)
            {
                _coreTypeNames = GetCoreTypeNames();
            }
            var localExcludes = _coreTypeNames.Where(n=>n != typeName).ToArray();
            
            WriteTypeFromSchemaToDisk(schema, filePath, typeName, ns, isUserElement, localExcludes);
        }

        /// <summary>
        /// Generate an in-memory assembly containing all the types generated from the supplied uris.
        /// </summary>
        /// <param name="uris">A collection of uris to JSON schema. These can be public urls or relative file paths.</param>
        /// <returns>An assembly containing the generated types or null if no Assembly could be generated.</returns>
        public static Assembly GenerateInMemoryAssemblyFromUrisAndLoad(string[] uris)
        {
            // https://docs.microsoft.com/en-us/archive/msdn-magazine/2017/may/net-core-cross-platform-code-generation-with-roslyn-and-net-core

            var code = new List<string>();
            foreach(var uri in uris)
            {
                try
                {
                    var schema = GetSchema(uri);

                    string ns;
                    if(!GetNamespace(schema, out ns))
                    {
                        return null;
                    }

                    var typeName = schema.Title;
                    if(_coreTypeNames == null)
                    {
                        _coreTypeNames = GetCoreTypeNames();
                    }
                    var localExcludes = _coreTypeNames.Where(n=>n != typeName).ToArray();

                    var csharp = WriteTypeFromSchema(schema, typeName, ns,  true, localExcludes);
                    code.Add(csharp);
                }
                catch
                {
                    throw new Exception($"There was an error reading the schema at {uri}. Type generation will not continue.");
                }
            }

            // Generate the assembly from the various code files.
            var options = new CSharpParseOptions(LanguageVersion.CSharp7_3,
                                                 kind: Microsoft.CodeAnalysis.SourceCodeKind.Regular,
                                                 documentationMode: Microsoft.CodeAnalysis.DocumentationMode.Diagnose);
            var syntaxTrees = new List<Microsoft.CodeAnalysis.SyntaxTree>();
            foreach(var cs in code)
            {
                var tree = CSharpSyntaxTree.ParseText(cs, options);
                syntaxTrees.Add(tree);
                
            }

            var assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location);
            var elementsAssemblyPath = Path.GetDirectoryName(typeof(Model).Assembly.Location);
            var newtonSoftPath = Path.GetDirectoryName(typeof(JsonConverter).Assembly.Location);

            IEnumerable<MetadataReference> defaultReferences = new[]
            {
                // MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "mscorlib.dll")),
                // MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.dll")),
                // MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Core.dll")),
                MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "netstandard.dll")),
                MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.ComponentModel.Annotations.dll")),
                MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Diagnostics.Tools.dll")),
                MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Runtime.dll")),
                MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Runtime.Serialization.Primitives.dll")),
                MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Private.CoreLib.dll")),
                MetadataReference.CreateFromFile(Path.Combine(elementsAssemblyPath, "Hypar.Elements.dll")),
                MetadataReference.CreateFromFile(Path.Combine(newtonSoftPath, "Newtonsoft.Json.dll"))
            };

            var compileOptions = new CSharpCompilationOptions(Microsoft.CodeAnalysis.OutputKind.DynamicallyLinkedLibrary,
                                                              optimizationLevel: Microsoft.CodeAnalysis.OptimizationLevel.Release);
            var compilation = CSharpCompilation.Create("UserElements",
                                                       syntaxTrees,
                                                       defaultReferences,
                                                       compileOptions);
            
            Assembly assembly = null;
            using(var ms = new MemoryStream())
            {
                var emitResult = compilation.Emit(ms);
                if(emitResult.Success)
                {
                    ms.Seek(0, SeekOrigin.Begin);
                    assembly = Assembly.Load(ms.ToArray());
                }
                else
                {
                    foreach(var d in emitResult.Diagnostics)
                    {
                        Console.WriteLine(d.ToString());
                    }
                    throw new Exception("There was an error creating and assembly for the user defined types. See the console for more information.");
                }
            }
            return assembly;
        }

        /// <summary>
        /// Generate the core element types as .cs files to the specified output directory. 
        /// </summary>
        /// <param name="outputBaseDir">The root directory into which generated files will be written.</param>
        public static void GenerateElementTypes(string outputBaseDir)
        {
            var typeNames = _hyparSchemas.Select(u=>u.Split(new[]{"/"}, StringSplitOptions.RemoveEmptyEntries).Last().Replace(".json", "")).ToList();

            foreach(var uri in _hyparSchemas)
            {
                var split = uri.Split(new[]{"/"}, StringSplitOptions.RemoveEmptyEntries).Skip(3);
                var outDir = Path.Combine(outputBaseDir, string.Join("/", split.Take(split.Count()-1)).TrimEnd('.'));
                if(!Directory.Exists(outDir))
                {
                    Directory.CreateDirectory(outDir);
                }

                GenerateUserElementTypeFromUri(uri, outDir);
            }
        }

        private static string[] GetCoreTypeNames()
        {
            return _hyparSchemas.Select(u=>u.Split(new[]{"/"}, StringSplitOptions.RemoveEmptyEntries).Last().Replace(".json", "")).ToArray();
        }

        private static string GetFileNameFromTypeName(string typeName)
        {
            return $"{typeName}.g.cs";
        }

        private static JsonSchema GetSchema(string uri)
        {
            if(uri.StartsWith("http://") || uri.StartsWith("https://"))
            {
                return Task.Run(()=>JsonSchema.FromUrlAsync(uri)).Result;;
            }
            else
            {
                var path = Path.GetFullPath(Path.Combine(System.Environment.CurrentDirectory, uri));
                if(!File.Exists(path))
                {
                    throw new Exception($"The specified schema, {uri}, can not be found as a relative file or a url.");
                }
                return Task.Run(()=> JsonSchema.FromJsonAsync(File.ReadAllText(path))).Result;;
            }
        }

        private static bool GetNamespace(JsonSchema schema, out string @namespace)
        {
            if(!schema.ExtensionData.ContainsKey(NAMESPACE_PROPERTY))
            {
                Console.WriteLine($"The provided schema does not contain the required 'x-namespace' property.");
                @namespace  = null;
                return false;
            }
            @namespace = (string)schema.ExtensionData[NAMESPACE_PROPERTY];
            return true;
        }

        private static string WriteTypeFromSchema(JsonSchema schema, string typeName, string ns, bool isUserElement = false, string[] excludedTypes = null)
        {
            var templates = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "./Templates"));
            
            var generator = new CSharpGenerator(schema, new CSharpGeneratorSettings(){
                Namespace = ns, 
                ArrayType = "IList",
                ArrayInstanceType = "List",
                ExcludedTypeNames = excludedTypes == null ? new string[]{} : excludedTypes,
                TemplateDirectory = templates,
                GenerateJsonMethods = false,
                ClassStyle = CSharpClassStyle.Poco, // Use pocos but with constructors. 
                TypeNameGenerator = new ElementsTypeNameGenerator()
            });
            var file = generator.GenerateFile();

            if(isUserElement)
            {
                // Insert the UserElement attribute directly before
                // 'public partial class <typeName>'
                var start = file.IndexOf($"public partial class {typeName}");
                file = file.Insert(start, $"[UserElement]\n\t");
            }

            // JSON schema only allows us to generate Dictionary<string,Element>
            // so we replace those entries here with Dictionary<Guid,Element>
            if(typeName == "Model")
            {
                file = file.Replace("System.Collections.Generic.IDictionary<string, Element>", "System.Collections.Generic.IDictionary<Guid, Element>");
                file = file.Replace("System.Collections.Generic.Dictionary<string, Element>", "System.Collections.Generic.Dictionary<Guid, Element>");
            }
            
            return file;
        }

        private static void WriteTypeFromSchemaToDisk(JsonSchema schema, string outPath, string typeName, string ns, bool isUserElement = false, string[] excludedTypes = null)
        {
            Console.WriteLine($"Generating type {@ns}.{typeName} in {outPath}...");
            var type = WriteTypeFromSchema(schema, typeName, ns, isUserElement, excludedTypes);
            File.WriteAllText(outPath, type);
        }
    }
}