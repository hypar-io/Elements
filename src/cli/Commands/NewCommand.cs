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
            Console.ForegroundColor = ConsoleColor.Gray;
            var name = SanitizeFunctionName(functionName);
            var newDir = Path.Combine(Directory.GetCurrentDirectory(), name);
            CreateProject(name);       
            CreateHyparReference(name);
            CreateLambdaReferences(name);
            CreateHyparJson(newDir, name);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{functionName} project created successfully.");
            Console.ResetColor();
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
            Console.WriteLine($"\tCreating {functionName} project...");
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    FileName="dotnet",
                    Arguments=$"new classlib -n {functionName} --target-framework-override netcoreapp2.0"
                }
            };
            process.Start();
            process.WaitForExit();
        }

        private void CreateHyparReference(string functionName)
        {
            Console.WriteLine($"\tReferencing Hypar SDK...");
            var project = $"./{functionName}/{functionName}.csproj";
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    FileName="dotnet",
                    Arguments=$"add {project} package Hypar -v 0.0.1-beta4"
                }
            };
            process.Start();
            process.WaitForExit();
        }

        private static void CreateLambdaReferences(string functionName)
        {
            var project = $"./{functionName}/{functionName}.csproj";
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    FileName="dotnet",
                    Arguments=$"add {project} package Amazon.Lambda.Core"
                }
            };
            process.Start();
            process.WaitForExit();

            project = $"./{functionName}/{functionName}.csproj";
            process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    FileName="dotnet",
                    Arguments=$"add {project} package Amazon.Lambda.Serialization.Json"
                }
            };
            process.Start();
            process.WaitForExit();
        }

        private void CreateHyparJson(string dir, string functionId)
        {
            Console.WriteLine("\tCreating hypar configuration file...");
            var hyparPath = Path.Combine(dir, Program.HYPAR_CONFIG);
            var className = ClassName(functionId);
            var config = new HyparConfig();
            config.Description = $"The {functionId} function.";
            config.FunctionId = functionId;
            config.Function = $"{functionId}::Hypar.Function::Handler";
            config.Runtime = "dotnetcore2.0";
            config.Parameters.Add("param1", new NumberParameter("The first parameter.", 0.0, 1.0, 0.1));
            config.Parameters.Add("param2", new PointParameter("The second parameter"));

            var json = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(hyparPath, json);
        }
        
        private void DeleteDefaultClass(string functionName)
        {
            var classPath = Path.Combine(System.Environment.CurrentDirectory, $"{functionName}/Class1.cs");
            if(File.Exists(classPath))
            {
                File.Delete(classPath);
            }

            var progPath = Path.Combine(System.Environment.CurrentDirectory, $"{functionName}/Program.cs");
            if(File.Exists(progPath))
            {
                File.Delete(progPath);
            }
        }

        private void CreateHyparDefaultClass(string functionName)
        {
            Console.WriteLine("\tCreating default function class...");
            var className = ClassName(functionName);
            
            string classStr = $@"using Hypar.Elements;
using Amazon.Lambda.Core;
using System.Collections.Generic;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
namespace Hypar
{{
    public class Function
    {{
        public Dictionary<string,object> Handler(Dictionary<string,object> input, ILambdaContext context)
        {{
            var profile = Profiles.Rectangular();

            var mass = Mass.WithBottomProfile(profile)
                            .WithBottomAtElevation(0)
                            .WithTopAtElevation(1);

            var model = new Model();
            model.AddElement(mass);
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
                Console.WriteLine($"\tThe function name has been updated to {clean}.");
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