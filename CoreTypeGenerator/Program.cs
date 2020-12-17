using System;
using System.IO;
using System.Threading.Tasks;
using Elements.Generate;

namespace CoreTypeGenerator
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("You need to specify the output directory.");
                return;
            }

            if (Directory.Exists(args[0]))
            {
                Directory.Delete(args[0], true);
            }
            Directory.CreateDirectory(args[0]);

            if (args.Length > 1)
            {
                TypeGenerator.SchemaBase = args[1];
            }

            await TypeGenerator.GenerateElementTypesAsync(args[0]);
        }
    }
}
