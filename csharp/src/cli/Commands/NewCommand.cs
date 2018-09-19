#pragma warning disable CS0067

using Hypar.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
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
                Logger.LogError("Hypar new expects a function id parameter.");
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
            Logger.LogInfo("Create a new Hypar function.");
            Logger.LogInfo("Usage: hypar new <function_id>");
        }

        private void New(string functionName)
        {
            var name = SanitizeFunctionName(functionName);
            var newDir = Path.Combine(Directory.GetCurrentDirectory(), name);
            CloneStarterRepo(name);
            UpdateHyparJson(newDir, name);
            Logger.LogSuccess($"{functionName} created successfully.");
            return;
        }

        private void CloneStarterRepo(string name)
        {
            Logger.LogInfo($"Cloning the starter repo...");
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    FileName="git",
                    Arguments=$"clone https://github.com/hypar-io/starter {name}"
                }
            };
            process.Start();
            process.WaitForExit();
        }

        private void UpdateHyparJson(string directory, string name)
        {
            Logger.LogInfo("Updating the hypar.json...");
            var configPath = Path.Combine(directory, Program.HYPAR_CONFIG);
            var config = HyparConfig.FromJson(File.ReadAllText(configPath));
            config.FunctionId = name;
            config.Description = $"The {name} generator.";
            var json = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(configPath, json);
        }
        
        private string SanitizeFunctionName(string functionName)
        {
            var clean = functionName.Replace("_","-").ToLower();
            if(clean != functionName)
            {
                Logger.LogInfo($"The function name has been updated to {clean}.");
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