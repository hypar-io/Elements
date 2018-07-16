using System;
using System.IO;
using System.Threading.Tasks;
using NJsonSchema.CodeGeneration;
using NJsonSchema.CodeGeneration.CSharp;

namespace generate
{
    class Program
    {
        static void Main(string[] args)
        {
            if(args.Length != 2)
            {
                throw new Exception("You must supply the path to the elements.json schema file, and an output csharp file name.");
            }

            if(!File.Exists(args[0]))
            {
                throw new Exception("The specified schema path does not exist.");
            }

            var schema = Task.Run(()=>NJsonSchema.JsonSchema4.FromFileAsync(args[0])).Result;
            var generator = new CSharpGenerator(schema, new CSharpGeneratorSettings(){
                Namespace = "Hypar.Elements",
            });
            var file = generator.GenerateFile();
            File.WriteAllText(args[1], file);
        }
    }
}
