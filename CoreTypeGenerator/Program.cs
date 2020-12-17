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

            if (!Directory.Exists(args[0]))
            {
                Console.WriteLine("The specified output directory does not exist.");
                return;
            }

            await TypeGenerator.GenerateElementTypesAsync(args[0]);
        }
    }
}
