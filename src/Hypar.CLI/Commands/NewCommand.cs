#pragma warning disable CS0067

using Hypar.Configuration;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Hypar.Commands
{
    internal class NewCommand : IHyparCommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            var args = (string[])parameter;
            if(args[0] != "new")
            {
                return false;
            }

            if(args.Length < 2)
            {
                Console.WriteLine("Hypar new expects a function id parameter.");
                return false;
            }

            return true;
        }

        public void Execute(object parameter)
        {
            var args = (string[])parameter;
            var functionId = args[1];
            New(functionId);
        }

        public void Help()
        {
            Console.WriteLine("Create a new Hypar function.");
            Console.WriteLine("Usage: hypar new <function_id>");
        }

        private void New(string functionName)
        {
            var name = SanitizeFunctionName(functionName);
            var newDir = Path.Combine(Directory.GetCurrentDirectory(), name);
            CreateProject(name);       
            CreateHyparReference(name);
            CreateLambdaReference(name);
            CreateHyparJson(newDir, name);
            return;
        }

        private void CreateProject(string functionName)
        {
            CreateDotnetProject(functionName);
            DeleteDefaultClass(functionName);
            CreateHyparDefaultClass(functionName);
        }

        private void CreateDotnetProject(string functionName)
        {
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    CreateNoWindow = true,
                    RedirectStandardOutput = false,
                    FileName="dotnet",
                    Arguments=$"new classlib -n {functionName}"
                }
            };
            process.Start();
            process.WaitForExit();
        }

        private void CreateHyparReference(string functionName)
        {
            var project = $"./{functionName}/{functionName}.csproj";
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    CreateNoWindow = true,
                    RedirectStandardOutput = false,
                    FileName="dotnet",
                    Arguments=$"add {project} package Hypar -v 0.0.1-beta4"
                }
            };
            process.Start();
            process.WaitForExit();
        }

        private static void CreateLambdaReference(string functionName)
        {
            var project = $"./{functionName}/{functionName}.csproj";
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    CreateNoWindow = true,
                    RedirectStandardOutput = false,
                    FileName="dotnet",
                    Arguments=$"add {project} package Amazon.Lambda.Core"
                }
            };
            process.Start();
            process.WaitForExit();
        }

        private void CreateHyparJson(string dir, string functionId)
        {
            var hyparPath = Path.Combine(dir, Program.HYPAR_CONFIG);
            var className = ClassName(functionId);
            var config = new HyparConfig();
            config.Description = "A description of your Hypar function.";
            config.FunctionId = functionId;
            config.Function = $"{functionId}::{className}.{className}::Handler";
            config.Runtime = "dotnetcore2.0";
            config.Parameters.Add("param1", new NumberParameter("The first parameter.", 0.0, 1.0, 0.1));
            config.Parameters.Add("param2", new PointParameter("The second parameter"));

            var json = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(hyparPath, json);
        }
        
        private void DeleteDefaultClass(string functionName)
        {
            var classPath = Path.Combine(System.Environment.CurrentDirectory, $"{functionName}/Class1.cs");
            Console.WriteLine(classPath);
            if(File.Exists(classPath))
            {
                File.Delete(classPath);
            }
        }

        private void CreateHyparDefaultClass(string functionName)
        {
            var className = ClassName(functionName);
            
            string classStr = $@"using Hypar.Elements;
using Amazon.Lambda.Core;
using System.Collections.Generic;

namespace {className}
{{
    public class {className}
    {{
        public Dictionary<string,object> Handler(Dictionary<string,object> input, ILambdaContext context)
        {{
            var model = new Model();

            // Insert your code here.

            return model.ToHypar();
        }}
    }}
}}";
            var classPath = Path.Combine(System.Environment.CurrentDirectory, $"{functionName}/{className}.cs");
            File.WriteAllText(classPath, classStr);
        }

        private string SanitizeFunctionName(string functionName)
        {
            var clean = functionName.Replace("_","-").ToLower();
            if(clean != functionName)
            {
                Console.WriteLine($"The function name has been updated to {clean}.");
            }
            return clean;
        }

        private string ClassName(string functionName)
        {
            var splits = functionName.Split('-');
            var sb = new StringBuilder();
            foreach(var split in splits)
            {
                sb.Append(char.ToUpper(split[0]) + split.Substring(1));
            }
            return sb.ToString();
        }
    }
}