#pragma warning disable CS0067

using Hypar.API;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;

namespace Hypar.Commands
{
    internal class ModelCommand : IHyparCommand
    {
        private Execution _execution;

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            var args = (string[])parameter;

            if(args[0] != "model")
            {
                return false;
            }

            if(args.Length == 2 && args[1] == "help")
            {
                return true;
            }

            if(args.Length != 2)
            {
                Console.WriteLine("The 'model' command requires an output directory to be specified.");
                return false;
            }

            if(!Directory.Exists((string)args[1]))
            {
                Console.WriteLine("The specified output directory does not exist.");
                return false;
            }

            if(Console.IsInputRedirected)
            {
                var input = Console.In.ReadToEnd();
                try
                {
                    _execution = JsonConvert.DeserializeObject<Execution>(input);
                }
                catch
                {
                    Console.WriteLine("The input could not be deserialized to an array of executions.");
                    return false;
                }
            }
            else
            {
                Console.WriteLine("Hypar results requires an array of executions.");
                return false;
            }

            return true;
        }

        public void Execute(object parameter)
        {
            var args = (string[])parameter;
            Model(args[1]);
        }

        public void Help()
        {
            Console.WriteLine("Read executions from stdin and write models to 'output_directory'.");
            Console.WriteLine("Usage: hypar model <output_directory>");
        }

        private void Model(string outputDirectory)
        {
            var outDir = outputDirectory;

            if(!Path.IsPathRooted(outDir)) 
            {
                outDir = Path.Combine(System.Environment.CurrentDirectory, outDir);
            }

            if(!Directory.Exists(outDir))
            {
                Directory.CreateDirectory(outDir);
            }

            var client = new RestClient("https://s3-us-west-1.amazonaws.com");
            var request = new RestRequest($"hypar-executions-dev/{_execution.Id}_model.zip", Method.GET);
            var response = client.DownloadData(request);
            var output = Path.Combine(outDir, $"{_execution.Id}.zip");
            File.WriteAllBytes(output, response);
            
            return;
        }
    }
}